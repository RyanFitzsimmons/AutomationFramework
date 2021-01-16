using System;
using System.Threading.Tasks;

namespace AutomationFramework
{
    public class Module : ModuleBase 
    {
        public Module(IStageBuilder builder) : base(builder)
        {
        }

        public Action<IStageBuilder> CreateChildren { get; init; }
        public Func<IModule, Task> Work { get; init; }
        public override string Name { get; init; } = "Default Module";

        internal override async Task RunWork()
        {
            if (MeetsRunCriteria())
            {
                CheckForCancellation();
                await OnRunStart();
                CheckForCancellation();
                await DoWork();
                CheckForCancellation();
                await OnRunFinish();
            }
            else
            {
                await SetStatus(StageStatuses.Bypassed);
            }
        }

        protected virtual async Task OnRunStart() => await SetStatus(StageStatuses.Running);

        protected virtual async Task OnRunFinish() => await SetStatus(StageStatuses.Completed);

        protected virtual async Task DoWork() => await Work?.Invoke(this);

        public override async Task<IModule[]> InvokeCreateChildren()
        {
            CheckForCancellation();
            CreateChildren?.Invoke(Builder);
            return await Task.FromResult(Builder.Build());
        }
    }
}
