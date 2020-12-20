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
        /// Takes the child stage module and meta data as input.
        /// The main use of this is for a module with a result to pass
        /// information onto its children.
        /// </summary>
        public Action<IModule, IMetaData> ConfigureChild { get; set; }
        public Action<IModule, IMetaData> Work { get; set; }

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
            Work?.Invoke(this, MetaData);

        public override void InvokeConfigureChild(IModule child)
        {
            ConfigureChild?.Invoke(child, MetaData);
            if (!IsEnabled) child.IsEnabled = false;
        }
    }
}
