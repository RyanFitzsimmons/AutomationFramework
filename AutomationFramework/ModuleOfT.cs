using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutomationFramework
{
    public abstract class Module<TId, TDataLayer, TResult> : ModuleBase<TId, TDataLayer> where TDataLayer : IModuleDataLayer<TId> where TResult : class
    {
        /// <summary>
        /// Takes the stage module and stage module result as arguments and returns an IEnumerable of child stage modules
        /// </summary>
        public Func<IModule<TId>, TResult, IEnumerable<IModule<TId>>> CreateChildren { get; set; }

        public Func<IModule<TId>, RunInfo<TId>, StagePath, TResult> Work { get; set; }

        internal protected override void Run()
        {
            if (MeetsRunCriteria())
            {
                CheckForCancellation();
                OnRunStart();
                CheckForCancellation();
                var result = DoWork();
                CheckForCancellation();
                DataLayer.SaveResult(this, result);
                CheckForCancellation();
                OnRunFinish(result);
            }
            else
            {
                SetStatusBase(StageStatuses.Bypassed);
            }
        }

        protected virtual void OnRunStart() => SetStatusBase(StageStatuses.Running);

        protected virtual void OnRunFinish(TResult result) => SetStatusBase(StageStatuses.Completed);

        protected virtual TResult DoWork() => 
            Work == null ? default : Work.Invoke(this, RunInfo, StagePath);

        public override IModule<TId>[] InvokeCreateChildren()
        {
            List<IModule<TId>> children = new List<IModule<TId>>();
            CheckForCancellation();
            var result = GetResult();
            if (result == default(TResult))
            {
                Logger.Warning($"{this} no result found");
            }
            else
            {
                children = CreateChildren?.Invoke(this, result)?.ToList() ?? new List<IModule<TId>>();
                if (!IsEnabled)
                    foreach (var child in children) child.IsEnabled = false;
            }
            return children.ToArray();
        }

        private TResult GetResult()
        {
            switch(RunInfo.Type)
            {
                case RunType.Run:
                    return DataLayer.GetCurrentResult<TResult>(this);
                case RunType.RunFrom:
                    if (RunInfo.Path == StagePath || RunInfo.Path.IsDescendantOf(StagePath))
                        return DataLayer.GetCurrentResult<TResult>(this);
                    else return DataLayer.GetPreviousResult<TResult>(this);
                case RunType.RunSingle:
                    if (RunInfo.Path == StagePath)
                        return DataLayer.GetCurrentResult<TResult>(this);
                    else return DataLayer.GetPreviousResult<TResult>(this);
                default:
                    throw new Exception($"Unknown RunType {RunInfo.Type}");
            }
        }
    }
}
