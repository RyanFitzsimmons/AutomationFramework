using System;
using System.Threading.Tasks;

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

        public Func<IModule, Task<TResult>> Work { get; init; }

        internal override async Task RunWork()
        {
            if (MeetsRunCriteria())
            {
                CheckForCancellation();
                await OnRunStart();
                CheckForCancellation();
                var result = await DoWork();
                CheckForCancellation();
                await DataLayer?.SaveResult(this, result, GetCancellationToken());
                CheckForCancellation();
                await OnRunFinish(result);
            }
            else
            {
                await SetStatus(StageStatuses.Bypassed);
            }
        }

        protected virtual async Task OnRunStart() => await SetStatus(StageStatuses.Running);

        protected virtual async Task OnRunFinish(TResult result) => await SetStatus(StageStatuses.Completed);

        protected virtual async Task<TResult> DoWork() => 
            Work == null ? default : await Work.Invoke(this);

        public override async Task<IModule[]> InvokeCreateChildren()
        {
            CheckForCancellation();
            var result = await GetResult();
            CreateChildren?.Invoke(Builder, result);
            return Builder.Build();
        }

        private async Task<TResult> GetResult()
        {
            switch(RunInfo.Type)
            {
                case RunType.Standard:
                    return await DataLayer?.GetCurrentResult<TResult>(this, GetCancellationToken());
                case RunType.From:
                    if (RunInfo.Path == StagePath || RunInfo.Path.IsDescendantOf(StagePath))
                        return await DataLayer?.GetCurrentResult<TResult>(this, GetCancellationToken());
                    else return await DataLayer?.GetPreviousResult<TResult>(this, GetCancellationToken());
                case RunType.Single:
                    if (RunInfo.Path == StagePath)
                        return await DataLayer?.GetCurrentResult<TResult>(this, GetCancellationToken());
                    else return await DataLayer?.GetPreviousResult<TResult>(this, GetCancellationToken());
                default:
                    throw new Exception($"Unknown RunType {RunInfo.Type}");
            }
        }
    }
}
