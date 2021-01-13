using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutomationFramework
{
    public class Module<TResult> : ModuleBase where TResult : class
    {
        public Module(IStageBuilder builder) : base(builder)
        {
        }

        public override string Name { get; init; } = "Default Module With Result";

        /// <summary>
        /// Takes the stage module result and the child stage module as input.
        /// The main use of this is for a module with a result to pass
        /// information onto its children.
        /// </summary>
        public Action<IStageBuilder, TResult> CreateChildren { get; init; }

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

        internal override IModule[] InvokeCreateChildren()
        {
            CheckForCancellation();
            var result = GetResult();
            CreateChildren?.Invoke(Builder, result);
            return Builder.Build();
        }

        private TResult GetResult()
        {
            switch(RunInfo.Type)
            {
                case RunType.Standard:
                    return DataLayer?.GetCurrentResult<TResult>(this);
                case RunType.From:
                    if (RunInfo.Path == StagePath || RunInfo.Path.IsDescendantOf(StagePath))
                        return DataLayer?.GetCurrentResult<TResult>(this);
                    else return DataLayer?.GetPreviousResult<TResult>(this);
                case RunType.Single:
                    if (RunInfo.Path == StagePath)
                        return DataLayer?.GetCurrentResult<TResult>(this);
                    else return DataLayer?.GetPreviousResult<TResult>(this);
                default:
                    throw new Exception($"Unknown RunType {RunInfo.Type}");
            }
        }
    }
}
