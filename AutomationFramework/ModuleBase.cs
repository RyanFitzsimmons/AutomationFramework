using System;
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
        public ModuleBase(IStageBuilder builder) => Builder = builder;

        private readonly CancellationTokenSource _cancellationSource = new CancellationTokenSource();
        internal CancellationToken Token => _cancellationSource.Token;

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

        /// <summary>
        /// If the module is cancelled it will try to update the status of the stage
        /// in the datalayer to `Cancelled`. This is the ammount of time the task will wait before 
        /// cancelling the data layer request. Default = 10000 (10 seconds)
        /// </summary>
        public int CancelStatusCancelAfter { get; init; } = 10000;

        public event Action<IModule, LogLevels, object> OnLog;
        public event Action<IModule> OnBuild;
        public event Action<IModule> OnCompletion;
        public event Action<IModule> OnCancellation;
        public event Action<IModule> OnError;
        /// <summary>
        /// Be aware this is called from the kernel context
        /// </summary>
        public event Action<IModule> PreCancellation;

        public void Log(LogLevels level, object message) => OnLog?.Invoke(this, level, message);

        public async Task Build()
        {
            try
            {
                Log(LogLevels.Information, $"{Name} Building");
                await DataLayer?.CreateStage(this, Token);
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

        public async Task Run()
        {
            try
            {
                if (IsEnabled) await RunWork(Token);
                else await SetStatus(StageStatuses.Disabled, Token);
            }
            catch (OperationCanceledException)
            {
                /// We catch here to set the status of the stage                
                /// We log here and in the kernel so the 
                /// OnLog event gets a full view
                /// the exception is thrown again so the kernel knows 
                /// not to create the children
                await SetStatus(StageStatuses.Cancelled, Token);
                OnCancellation?.Invoke(this);
                throw;
            }
            catch (Exception ex)
            {
                /// We catch here to set the status of the stage
                /// We log here and in the kernel so the 
                /// OnLog event gets a full view
                /// the exception is thrown again so the kernel knows 
                /// not to create the children
                await SetStatus(StageStatuses.Errored, Token);
                Log(LogLevels.Error, ex);
                OnError?.Invoke(this);
                throw;
            }
            finally
            {
                OnCompletion?.Invoke(this);
            }
        }

        internal abstract Task RunWork(CancellationToken token);

        protected async Task SetStatus(StageStatuses status, CancellationToken token)
        {
            Log(LogLevels.Information, status);
            if (status == StageStatuses.Cancelled) await SetCancelledStatus();
            else await DataLayer?.SetStatus(this, status, token);
        }

        private async Task SetCancelledStatus()
        {
            try
            {
                var cancelStatusCancellationSource = new CancellationTokenSource(CancelStatusCancelAfter);
                await DataLayer?.SetStatus(this, StageStatuses.Cancelled, cancelStatusCancellationSource.Token);
            }
            catch (OperationCanceledException)
            {
                Log(LogLevels.Error, "Failed to set cancelled status in data layer");
            }
            catch (Exception ex)
            {
                Log(LogLevels.Error, "Failed to set cancelled status in data layer");
                Log(LogLevels.Error, ex);
            }
        }

        internal bool MeetsRunCriteria() =>
            RunInfo.Type switch
            {
                RunType.Standard => true,
                RunType.From => StagePath == RunInfo.Path || StagePath.IsDescendantOf(RunInfo.Path),
                RunType.Single => StagePath == RunInfo.Path,
                _ => throw new Exception("Unknown Run Type: " + RunInfo.Path),
            };

        public abstract Task<IModule[]> InvokeCreateChildren();

        public void Cancel()
        {
            PreCancellation?.Invoke(this);
            _cancellationSource.Cancel();
        }

        public override string ToString() => StagePath.ToString() + " - " + Name;
    }
}
