using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutomationFramework
{
    public abstract class Module<TResult> : ModuleBase where TResult : class
    {
        protected Module(IDataLayer dataLayer, IRunInfo runInfo, StagePath stagePath) : base(dataLayer, runInfo, stagePath)
        {
        }

        /// <summary>
        /// Takes the stage module result and the child stage module as input.
        /// The main use of this is for a module with a result to pass
        /// information onto its children.
        /// </summary>
        public Action<TResult, IModule> ConfigureChildWithResult { get; init; }

        public Func<IModule, TResult> Work { get; init; }

        internal protected override void RunWork()
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
            Work == null ? default : Work.Invoke(this);

        public override void InvokeConfigureChild(IModule child)
        {
            CheckForCancellation();
            var result = GetResult();
            if (result == default(TResult)) Log(LogLevels.Warning, $"{this} no result found");
            else ConfigureChildWithResult?.Invoke(result, child); 
        }

        private TResult GetResult()
        {
            switch(RunInfo.Type)
            {
                case RunType.Standard:
                    return DataLayer.GetCurrentResult<TResult>(this);
                case RunType.From:
                    if (RunInfo.Path == Path || RunInfo.Path.IsDescendantOf(Path))
                        return DataLayer.GetCurrentResult<TResult>(this);
                    else return DataLayer.GetPreviousResult<TResult>(this);
                case RunType.Single:
                    if (RunInfo.Path == Path)
                        return DataLayer.GetCurrentResult<TResult>(this);
                    else return DataLayer.GetPreviousResult<TResult>(this);
                default:
                    throw new Exception($"Unknown RunType {RunInfo.Type}");
            }
        }
    }
}
