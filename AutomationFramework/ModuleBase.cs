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
        public RunInfo RunInfo { get; private set; }
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
        public Func<object> GetMetaDataFunc { get; private set; }

        internal void SetProperties(RunInfo runInfo, StagePath path, Func<object> getMetaData, ILogger logger)
        {
            RunInfo = runInfo;
            StagePath = path;
            GetMetaDataFunc = getMetaData;
            Logger = logger;
            Logger?.Information(path, $"{Name}");
        }

        protected TMetaData GetMetaData<TMetaData>() where TMetaData : class
        {
            try
            {
                return GetMetaDataFunc?.Invoke() as TMetaData;
            }
            catch (Exception ex)
            {
                var message = "Failed to retrieve meta data";
                Logger?.Error(StagePath.Empty, message);
                throw new Exception(message, ex);
            }
        }

        public void Run(RunInfo runInfo, StagePath path, Func<object> getMetaData, ILogger logger)
        {
            try
            {
                SetProperties(runInfo, path, getMetaData, logger);
                DataLayer.CreateStage(this);
                if (IsEnabled) Run();
                else SetStatusBase(StageStatuses.Disabled);
            }
            catch (OperationCanceledException)
            {
                SetStatusBase(StageStatuses.Cancelled);
                throw;
            }
            catch (Exception ex)
            {
                SetStatusBase(StageStatuses.Errored);
                Logger?.Error(StagePath, ex.ToString());
                throw;
            }
        }

        internal protected abstract void Run();

        internal protected void SetStatusBase(StageStatuses status)
        {
            Logger.Information(StagePath, status.ToString());
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

        public abstract IModule[] InvokeCreateChildren();

        public CancellationToken GetCancellationToken() => CancellationSource.Token;

        public void Cancel()
        {
            PreCancellation();
            CancellationSource.Cancel();
        }

        protected void CheckForCancellation()
        {
            var token = GetCancellationToken();
            if (token.IsCancellationRequested)
            {
                OnCancellation();
                token.ThrowIfCancellationRequested();
            }
        }

        protected virtual void OnCancellation()
        {

        }

        protected virtual void PreCancellation()
        {

        }

        public override string ToString()
        {
            return StagePath.ToString() + " - " + Name;
        }
    }
}
