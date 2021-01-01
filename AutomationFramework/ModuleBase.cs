using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutomationFramework
{
    /// <summary>
    /// Modules are not thread safe and because of this no variables should be 
    /// accessed from outside the module while the stage is in progress.
    /// </summary>
    public abstract class ModuleBase : IModule
    {
        public ModuleBase(IDataLayer dataLayer, IRunInfo runInfo, StagePath path)
        {
            RunInfo = runInfo;
            Path = path;            
            DataLayer = dataLayer;
        }

        private readonly CancellationTokenSource CancellationSource = new CancellationTokenSource();
        private IMetaData _MetaData;

        public IRunInfo RunInfo { get; }
        public StagePath Path { get; }
        protected IDataLayer DataLayer { get; }

        private IMetaData MetaData => _MetaData ??= DataLayer.GetMetaData(RunInfo);

        /// <summary>
        /// Is enabled by default
        /// </summary>
        public virtual bool IsEnabled { get; init; } = true;
        public abstract string Name { get; init; }

        /// <summary>
        /// If zero all child stages will run in parallel. 
        /// If Set to one the child stages will run one at a time.
        /// Set to 1 by default.
        /// WARNING: If this is set to run in parallel, the Work and CreateChildren functions of this stage and any child stages need to be thread safe.
        /// </summary>
        public virtual int MaxParallelChildren { get; init; } = 1;

        public event Action<IModule, LogLevels, object> OnLog;
        public event Action<IModule, IMetaData> OnBuild;
        public event Action<IModule, IMetaData> OnCompletion;
        public event Action<IModule, IMetaData> OnCancellation;
        /// <summary>
        /// Be aware this is called from the kernel thread
        /// </summary>
        public event Action<IModule> PreCancellation;

        protected virtual void Log(LogLevels level, object message) => OnLog?.Invoke(this, level, message);

        protected TMetaData GetMetaData<TMetaData>() where TMetaData : class, IMetaData
        {
            return MetaData as TMetaData;
        }

        public void Build()
        {
            try
            {
                Log(LogLevels.Information, $"{Name} Building");
                DataLayer.CreateStage(this);
                OnBuild?.Invoke(this, MetaData);
            }
            catch (Exception ex)
            {
                /// We log here and in the kernel so the 
                /// OnLog event gets a full view
                /// the exception is thrown again so the kernel knows 
                /// to stop building
                Log(LogLevels.Error, ex);
                throw;
            }
        }

        public void Run()
        {
            try
            {
                if (IsEnabled) RunWork();
                else SetStatusBase(StageStatuses.Disabled);
            }
            catch (OperationCanceledException)
            {
                /// We catch here to set the status of the stage
                /// We log here and in the kernel so the 
                /// OnLog event gets a full view
                /// the exception is thrown again so the kernel knows 
                /// not to create the children
                SetStatusBase(StageStatuses.Cancelled);
                throw;
            }
            catch (Exception ex)
            {
                /// We catch here to set the status of the stage
                /// We log here and in the kernel so the 
                /// OnLog event gets a full view
                /// the exception is thrown again so the kernel knows 
                /// not to create the children
                SetStatusBase(StageStatuses.Errored);
                Log(LogLevels.Error, ex);
                throw;
            }
            finally
            {
                OnCompletion?.Invoke(this, MetaData);
            }
        }

        internal protected abstract void RunWork();

        internal protected void SetStatusBase(StageStatuses status)
        {
            Log(LogLevels.Information, status);
            DataLayer.SetStatus(this, status);
        }

        protected bool MeetsRunCriteria() =>
            RunInfo.Type switch
            {
                RunType.Standard => true,
                RunType.From => Path == RunInfo.Path || Path.IsDescendantOf(RunInfo.Path),
                RunType.Single => Path == RunInfo.Path,
                _ => throw new Exception("Unknown Run Type: " + RunInfo.Path),
            };

        public abstract void InvokeConfigureChild(IModule child);

        public CancellationToken GetCancellationToken() => CancellationSource.Token;

        public void Cancel()
        {
            PreCancellation?.Invoke(this);
            CancellationSource.Cancel();
        }

        protected void CheckForCancellation()
        {
            var token = GetCancellationToken();
            if (token.IsCancellationRequested)
            {
                OnCancellation?.Invoke(this, MetaData);
                token.ThrowIfCancellationRequested();
            }
        }

        public override string ToString()
        {
            return Path.ToString() + " - " + Name;
        }
    }
}
