using System;
using System.Threading;
using System.Threading.Tasks;

namespace AutomationFramework
{
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
        /// If zero all child stages awaits will run in parallel. 
        /// If set to one the child stages will run one at a time.
        /// If greater than one this will be the limit of child awaits.
        /// Set to 1 by default.
        /// </summary>
        public virtual int MaxParallelChildren { get; init; } = 1;

        /// <summary>
        /// If the module is cancelled it will try to update the status of the stage
        /// in the datalayer to `Cancelled`. This is the ammount of time the task will wait before 
        /// cancelling the data layer request. Default = 10000 (10 seconds)
        /// </summary>
        public int CancelStatusCancelAfter { get; init; } = 10000;

        /// <summary>
        /// Raised everytime the module Log method is called
        /// </summary>
        public event Action<IModule, LogLevels, object> OnLog;
        /// <summary>
        /// Raised at the end of the build method
        /// </summary>
        public event Action<IModule> OnBuild;
        /// <summary>
        /// Raised at the end of the Run method
        /// </summary>
        public event Action<IModule> OnCompletion;
        /// <summary>
        /// Raised at the end of Run cancellation
        /// </summary>
        public event Action<IModule> OnCancellation;
        /// <summary>
        /// Raised at the end of Run Error
        /// </summary>
        public event Action<IModule> OnError;
        /// <summary>
        /// Raised just before the CancellationSource is cancelled. 
        /// Be aware this is called from the kernel context.
        /// </summary>
        public event Action<IModule> PreCancellation;

        /// <summary>
        /// Raises the OnLog event
        /// </summary>
        /// <param name="level">Log level</param>
        /// <param name="message">Message</param>
        public void Log(LogLevels level, object message) => OnLog?.Invoke(this, level, message);

        /// <summary>
        /// Creates the stage in the data layer. 
        /// </summary>
        /// <returns>Task</returns>
        public async Task Build()
        {
            try
            {
                Log(LogLevels.Information, $"{Name} Building");
                await (DataLayer?.CreateStage(this, Token) ?? Task.CompletedTask);
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

        /// <summary>
        /// Runs the work if the criteria is met
        /// </summary>
        /// <returns>Task</returns>
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

        /// <summary>
        /// Allows for run differences in inherited module classes
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Task</returns>
        internal abstract Task RunWork(CancellationToken token);

        /// <summary>
        /// Sets the stage status in the data layer
        /// </summary>
        /// <param name="status">Status</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Task</returns>
        protected async Task SetStatus(StageStatuses status, CancellationToken token)
        {
            Log(LogLevels.Information, status);
            if (status == StageStatuses.Cancelled) await SetCancelledStatus();
            else await (DataLayer?.SetStatus(this, status, token) ?? Task.CompletedTask);
        }

        /// <summary>
        /// Handles cancellation data layer update
        /// </summary>
        /// <returns>Task</returns>
        private async Task SetCancelledStatus()
        {
            try
            {
                var cancelStatusCancellationSource = new CancellationTokenSource(CancelStatusCancelAfter);
                await (DataLayer?.SetStatus(this, StageStatuses.Cancelled, cancelStatusCancellationSource.Token) ?? Task.CompletedTask);
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

        /// <summary>
        /// Checks if the stage can run
        /// </summary>
        /// <returns>True if the stage can run</returns>
        internal bool MeetsRunCriteria() =>
            RunInfo.Type switch
            {
                RunType.Standard => true,
                RunType.From => StagePath == RunInfo.Path || StagePath.IsDescendantOf(RunInfo.Path),
                RunType.Single => StagePath == RunInfo.Path,
                _ => throw new Exception("Unknown Run Type: " + RunInfo.Path),
            };

        /// <summary>
        /// Invokes create children delegates of inherited module classes
        /// </summary>
        /// <returns></returns>
        public abstract Task<IModule[]> InvokeCreateChildren();

        /// <summary>
        /// Cancels the stage
        /// </summary>
        public void Cancel()
        {
            PreCancellation?.Invoke(this);
            _cancellationSource.Cancel();
        }

        /// <summary>
        /// A string of the stage path and name
        /// </summary>
        /// <returns>StagePath - Name</returns>
        public override string ToString() => StagePath.ToString() + " - " + Name;
    }
}
