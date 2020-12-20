using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutomationFramework
{
    public abstract class ModuleBase<TDataLayer> : IModule where TDataLayer : IModuleDataLayer
    {
        public ModuleBase()
        {
            DataLayer = Activator.CreateInstance<TDataLayer>();
        }

        private readonly CancellationTokenSource CancellationSource = new CancellationTokenSource();

        protected TDataLayer DataLayer { get; }

        protected ILogger Logger { get; private set; }
        public IRunInfo RunInfo { get; private set; }
        public StagePath StagePath { get; private set; }

        /// <summary>
        /// Is enabled by default
        /// </summary>
        public virtual bool IsEnabled { get; set; } = true;
        public abstract string Name { get; set; }

        /// <summary>
        /// If zero all child stages will run in parallel. 
        /// If Set to one the child stages will run one at a time.
        /// Set to 1 by default.
        /// WARNING: If this is set to run in parallel, the Work and CreateChildren functions of this stage and any child stages need to be thread safe.
        /// </summary>
        public virtual int MaxParallelChildren { get; set; } = 1;
        protected internal IMetaData MetaData { get; set; }

        public virtual Action<IModule, IMetaData> OnInitialize { get; set; }
        public virtual Action<IModule, IMetaData> OnCompletion { get; set; }
        public virtual Action<IModule, IMetaData> PreCancellation { get; set; }
        public virtual Action<IModule, IMetaData> OnCancellation { get; set; }

        internal void SetProperties(IRunInfo runInfo, StagePath path, IMetaData metaData, ILogger logger)
        {
            RunInfo = runInfo;
            StagePath = path;
            MetaData = metaData;
            Logger = logger;
            Logger?.Information(path, $"{Name}");
        }

        protected TMetaData GetMetaData<TMetaData>() where TMetaData : class, IMetaData
        {
            return MetaData as TMetaData;
        }

        public void Initialize(IRunInfo runInfo, StagePath path, IMetaData metaData, ILogger logger)
        {
            try
            {
                SetProperties(runInfo, path, metaData, logger);
                DataLayer.CreateStage(this);
                OnInitialize?.Invoke(this, metaData);
            }
            catch 
            {
                /// We log here to capture which stage faulted
                /// The exception is then thrown and logged within the kernel
                Logger?.Error(path, "Failed to initialize");
                throw;
            }
        }

        public void Run()
        {
            try
            {
                if (IsEnabled) Run();
                else SetStatusBase(StageStatuses.Disabled);
            }
            catch (OperationCanceledException)
            {
                /// We catch here to set the status of the stage
                /// The exception is logged within the kernel
                SetStatusBase(StageStatuses.Cancelled);
                throw;
            }
            catch
            {
                /// We catch here to set the status of the stage
                /// The exception is logged within the kernel
                SetStatusBase(StageStatuses.Errored);
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
            Logger?.Information(StagePath, status);
            DataLayer.SetStatus(this, status);
        }

        protected bool MeetsRunCriteria() =>
            RunInfo.Type switch
            {
                RunType.Run => true,
                RunType.RunFrom => StagePath == RunInfo.Path || StagePath.IsDescendantOf(RunInfo.Path),
                RunType.RunSingle => StagePath == RunInfo.Path,
                _ => throw new Exception("Unknown Run Type: " + RunInfo.Path),
            };

        public abstract void InvokeConfigureChild(IModule child);

        public CancellationToken GetCancellationToken() => CancellationSource.Token;

        public void Cancel()
        {
            PreCancellation?.Invoke(this, MetaData);
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
            return StagePath.ToString() + " - " + Name;
        }
    }
}
