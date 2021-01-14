using System;

namespace AutomationFramework
{
    public class Module : ModuleBase 
    {
        public Module(IStageBuilder builder) : base(builder)
        {
        }

        public Action<IStageBuilder> CreateChildren { get; init; }
        public Action<IModule> Work { get; init; }
        public override string Name { get; init; } = "Default Module";

        internal override void RunWork()
        {
            if (MeetsRunCriteria())
            {
                CheckForCancellation();
                OnRunStart();
                CheckForCancellation();
                DoWork();
                CheckForCancellation();
                OnRunFinish();
            }
            else
            {
                SetStatus(StageStatuses.Bypassed);
            }
        }

        protected virtual void OnRunStart() => SetStatus(StageStatuses.Running);

        protected virtual void OnRunFinish() => SetStatus(StageStatuses.Completed);

        protected virtual void DoWork() =>
            Work?.Invoke(this);

        internal override IModule[] InvokeCreateChildren()
        {
            CheckForCancellation();
            CreateChildren?.Invoke(Builder);
            return Builder.Build();
        }
    }
}
