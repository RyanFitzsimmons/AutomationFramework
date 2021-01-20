using System;
using System.Threading;
using System.Threading.Tasks;

namespace AutomationFramework
{
    public class Module : ModuleBase 
    {
        public Module(IStageBuilder builder) : base(builder)
        {
        }

        public Action<IStageBuilder> CreateChildren { get; init; }
        public Func<IModule, CancellationToken, Task> Work { get; init; }
        public override string Name { get; init; } = "Default Module";

        internal override async Task RunWork(CancellationToken token)
        {
            if (MeetsRunCriteria())
            {
                await OnRunStart(token);
                await DoWork(token);
                await OnRunFinish(token);
            }
            else
            {
                await SetStatus(StageStatuses.Bypassed, token);
            }
        }

        /// <summary>
        /// Sets the status to running, can be overriden if a different status is required.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        protected virtual async Task OnRunStart(CancellationToken token) =>
            await SetStatus(StageStatuses.Running, token);

        /// <summary>
        /// Sets the status to completed, can be overriden if a different status is required.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        protected virtual async Task OnRunFinish(CancellationToken token) =>
            await SetStatus(StageStatuses.Completed, token);

        protected virtual async Task DoWork(CancellationToken token) => 
            await (Work?.Invoke(this, token) ?? Task.CompletedTask);

        public override async Task<IModule[]> InvokeCreateChildren()
        {
            try
            {
                CreateChildren?.Invoke(Builder);
                return await Task.FromResult(Builder.Build());
            }
            catch (OperationCanceledException)
            {
                /// We log here and in the kernel so the 
                /// OnLog event gets a full view
                Log(LogLevels.Warning, "Unable to create children. The stage has been cancelled.");
                throw;
            }
            catch (Exception ex)
            {
                /// We log here and in the kernel so the 
                /// OnLog event gets a full view
                Log(LogLevels.Error, "Unable to create children.");
                Log(LogLevels.Error, ex);
                throw;
            }
        }
    }
}
