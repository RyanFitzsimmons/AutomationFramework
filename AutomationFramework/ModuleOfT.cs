using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutomationFramework
{
    public abstract class Module<TDataLayer, TResult> : ModuleBase<TDataLayer> where TDataLayer : IModuleDataLayer where TResult : class
    {
        /// <summary>
        /// Takes the stage module and stage module result as arguments and returns an IEnumerable of child stage modules
        /// </summary>
        public Func<IModule, TResult, IEnumerable<IModule>> CreateChildren { get; set; }

        public Func<IModule, RunInfo, StagePath, TResult> Work { get; set; }

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

        public override IModule[] InvokeCreateChildren()
        {
            List<IModule> children = new List<IModule>();
            CheckForCancellation();
            var result = GetResult();
            if (result == default(TResult))
            {
                Logger.Warning($"{this} no result found");
            }
            else
            {
                children = CreateChildren?.Invoke(this, result)?.ToList() ?? new List<IModule>();
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
