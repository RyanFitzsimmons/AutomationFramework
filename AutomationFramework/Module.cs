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
        /// Takes the stage module as an argument and returns an IEnumerable of child stage modules
        /// </summary>
        public Func<IModule, IEnumerable<IModule>> CreateChildren { get; set; }

        public Action<IModule, RunInfo, StagePath> Work { get; set; }

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

        public override IModule[] InvokeCreateChildren()
        {
            CheckForCancellation();
            var children = CreateChildren?.Invoke(this)?.ToArray() ?? Array.Empty<IModule>();
            if (!IsEnabled)
                foreach (var child in children) child.IsEnabled = false;
            return children;
        }
    }
}
