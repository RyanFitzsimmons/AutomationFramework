using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutomationFramework
{
    public abstract class Module<TId, TDataLayer> : ModuleBase<TId, TDataLayer> where TDataLayer : IModuleDataLayer<TId>
    {
        /// <summary>
        /// Takes the stage module as an argument and returns an IEnumerable of child stage modules
        /// </summary>
        public Func<IModule<TId>, IEnumerable<IModule<TId>>> CreateChildren { get; set; }

        public Action<IModule<TId>, RunInfo<TId>, StagePath> Work { get; set; }

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

        public override IModule<TId>[] InvokeCreateChildren()
        {
            CheckForCancellation();
            var children = CreateChildren?.Invoke(this)?.ToArray() ?? Array.Empty<IModule<TId>>();
            if (!IsEnabled)
                foreach (var child in children) child.IsEnabled = false;
            return children;
        }
    }
}
