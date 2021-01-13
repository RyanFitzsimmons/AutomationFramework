using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public Action<IModule> Work { get; init; }
        public override string Name { get; init; } = "Default Module";

        internal protected override void RunWork()
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
                SetStatusBase(StageStatuses.Bypassed);
            }
        }

        protected virtual void OnRunStart() => SetStatusBase(StageStatuses.Running);

        protected virtual void OnRunFinish() => SetStatusBase(StageStatuses.Completed);

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
