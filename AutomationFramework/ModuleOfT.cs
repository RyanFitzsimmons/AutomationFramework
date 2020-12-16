using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutomationFramework
{
    public abstract class Module<TDataLayer, TResult> : ModuleBase<TDataLayer> where TDataLayer : IModuleDataLayer where TResult : class
    {
        /// <summary>
        /// Takes the stage module, stage module result and an IEnumerable of child stage modules
        /// </summary>
        public Action<IModule, TResult, IModule> ConfigureChildWithResult { get; set; }

        public Func<IModule, IRunInfo, StagePath, TResult> Work { get; set; }

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

        public override void InvokeConfigureChild(IModule child)
        {
            CheckForCancellation();
            var result = GetResult();
            if (result == default(TResult)) Logger.Warning($"{this} no result found");
            else ConfigureChildWithResult?.Invoke(this, result, child); 

            if (!IsEnabled) child.IsEnabled = false;
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
