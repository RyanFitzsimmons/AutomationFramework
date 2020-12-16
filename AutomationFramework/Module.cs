using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutomationFramework
{
    public abstract class Module<TDataLayer> : ModuleBase<TDataLayer> where TDataLayer : IModuleDataLayer
    {
        /// <summary>
        /// Takes the stage module and an IEnumerable of child stage modules
        /// </summary>
        public Action<IModule, IModule> ConfigureChild { get; set; }
        public Action<IModule, IRunInfo, StagePath> Work { get; set; }

        internal protected override void Run()
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
            Work?.Invoke(this, RunInfo, StagePath);

        public override void InvokeConfigureChild(IModule child)
        {
            ConfigureChild?.Invoke(this, child);
            if (!IsEnabled) child.IsEnabled = false;
        }
    }
}
