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
        public Module(IDataLayer dataLayer, IRunInfo runInfo, StagePath stagePath) : base(dataLayer, runInfo, stagePath)
        {
        }

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

        protected virtual void OnRunStart()
        {
            SetStatusBase(StageStatuses.Running);
        }

        protected virtual void OnRunFinish()
        {
            SetStatusBase(StageStatuses.Completed);
        }

        protected virtual void DoWork() =>
            Work?.Invoke(this);

        internal override bool InvokeConfigureChild(IModule child)
        {
            // Only needed for module with result
            return true;
        }
    }
}
