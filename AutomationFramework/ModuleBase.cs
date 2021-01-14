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
        public ModuleBase(IStageBuilder builder)
        {
            Builder = builder;
        }

        private readonly CancellationTokenSource CancellationSource = new CancellationTokenSource();

        protected internal IStageBuilder Builder { get; }
        public IRunInfo RunInfo => Builder.RunInfo;
        public StagePath StagePath => Builder.StagePath;
        protected IDataLayer DataLayer => Builder.DataLayer;

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
        public event Action<IModule> OnBuild;
        public event Action<IModule> OnCompletion;
        public event Action<IModule> OnCancellation;
        /// <summary>
        /// Be aware this is called from the kernel thread
        /// </summary>
        public event Action<IModule> PreCancellation;

        public virtual void Log(LogLevels level, object message) => OnLog?.Invoke(this, level, message);

        public void Build()
        {
            try
            {
                Log(LogLevels.Information, $"{Name} Building");
                DataLayer?.CreateStage(this);
                OnBuild?.Invoke(this);
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
                OnCompletion?.Invoke(this);
            }
        }

        internal protected abstract void RunWork();

        internal protected void SetStatusBase(StageStatuses status)
        {
            Log(LogLevels.Information, status);
            DataLayer?.SetStatus(this, status);
        }

        protected bool MeetsRunCriteria() =>
            RunInfo.Type switch
            {
                RunType.Standard => true,
                RunType.From => StagePath == RunInfo.Path || StagePath.IsDescendantOf(RunInfo.Path),
                RunType.Single => StagePath == RunInfo.Path,
                _ => throw new Exception("Unknown Run Type: " + RunInfo.Path),
            };

        internal abstract IModule[] InvokeCreateChildren();

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
                OnCancellation?.Invoke(this);
                token.ThrowIfCancellationRequested();
            }
        }

        public override string ToString()
        {
            return StagePath.ToString() + " - " + Name;
        }
    }
}
