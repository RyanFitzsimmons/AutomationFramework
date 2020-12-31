using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutomationFramework
{
    public abstract class Module<TDataLayer, TResult> : ModuleBase<TDataLayer> where TDataLayer : IModuleDataLayer where TResult : class
    {
        protected Module(IRunInfo runInfo, StagePath stagePath, IMetaData metaData) : base(runInfo, stagePath, metaData)
        {
        }

        /// <summary>
        /// Takes the stage module result and an IEnumerable of child stage modules
        /// The main use of this is for a module with a result to pass
        /// information onto its children.
        /// </summary>
        public Action<TResult, IModule, IMetaData> ConfigureChildWithResult { get; set; }

        public Func<IModule, IMetaData, TResult> Work { get; set; }

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
            Work == null ? default : Work.Invoke(this, MetaData);

        public override void InvokeConfigureChild(IModule child)
        {
            CheckForCancellation();
            var result = GetResult();
            if (result == default(TResult)) Log(LogLevels.Warning, $"{this} no result found");
            else ConfigureChildWithResult?.Invoke(result, child, MetaData); 
        }

        private TResult GetResult()
        {
            switch(RunInfo.Type)
            {
                case RunType.Standard:
                    return DataLayer.GetCurrentResult<TResult>(this);
                case RunType.From:
                    if (RunInfo.Path == StagePath || RunInfo.Path.IsDescendantOf(StagePath))
                        return DataLayer.GetCurrentResult<TResult>(this);
                    else return DataLayer.GetPreviousResult<TResult>(this);
                case RunType.Single:
                    if (RunInfo.Path == StagePath)
                        return DataLayer.GetCurrentResult<TResult>(this);
                    else return DataLayer.GetPreviousResult<TResult>(this);
                default:
                    throw new Exception($"Unknown RunType {RunInfo.Type}");
            }
        }
    }
}
