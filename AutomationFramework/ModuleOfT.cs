﻿using System;
using System.Threading;
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

        public Func<IModule, CancellationToken, Task<TResult>> Work { get; init; }

        public event Action<IModule, TResult> OnResult;

        internal override async Task RunWork(CancellationToken token)
        {
            if (MeetsRunCriteria())
            {                
                await OnRunStart(token);
                var result = await DoWork(token);
                OnResult?.Invoke(this, result);
                await (DataLayer?.SaveResult(this, result, token)
                    ?? Task.CompletedTask);
                await OnRunFinish(result, token);
            }
            else
            {
                await SetStatus(StageStatuses.Bypassed, token);
            }
        }

        /// <summary>
        /// Sets the status to running, can be overriden if a different status is required.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        protected virtual async Task OnRunStart(CancellationToken token) => 
            await SetStatus(StageStatuses.Running, token);

        /// <summary>
        /// Sets the status to completed, can be overriden if a different status is required.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        protected virtual async Task OnRunFinish(TResult result, CancellationToken token) =>
            await SetStatus(StageStatuses.Completed, token);

        protected virtual async Task<TResult> DoWork(CancellationToken token) => 
            Work == null ? await Task.FromResult(default(TResult)) : await Work.Invoke(this, token);

        public override async Task<IModule[]> InvokeCreateChildren()
        {
            try
            {
                var result = await GetResult(Token);
                CreateChildren?.Invoke(Builder, result);
                return Builder.Build();
            }
            catch (OperationCanceledException)
            {
                /// We log here and in the kernel so the 
                /// OnLog event gets a full view
                Log(LogLevels.Warning, "Unable to create children. The stage has been cancelled.");
                throw;
            }
            catch (Exception ex)
            {
                /// We log here and in the kernel so the 
                /// OnLog event gets a full view
                Log(LogLevels.Error, "Unable to create children.");
                Log(LogLevels.Error, ex);
                throw;
            }
        }

        private async Task<TResult> GetResult(CancellationToken token)
        {
            switch(RunInfo.Type)
            {
                case RunType.Standard:
                    {
                        return await (DataLayer?.GetCurrentResult<TResult>(this, token) 
                            ?? Task.FromResult<TResult>(default));
                    }
                case RunType.From:
                    {
                        if (RunInfo.Path == StagePath || RunInfo.Path.IsDescendantOf(StagePath))
                        {
                            return await (DataLayer?.GetCurrentResult<TResult>(this, token)
                                ?? Task.FromResult<TResult>(default));
                        }
                        else
                        {
                            return await (DataLayer?.GetPreviousResult<TResult>(this, token)
                                ?? Task.FromResult<TResult>(default));
                        }
                    }
                case RunType.Single:
                    {
                        if (RunInfo.Path == StagePath)
                        {
                            return await (DataLayer?.GetCurrentResult<TResult>(this, token) 
                                ?? Task.FromResult<TResult>(default));
                        }
                        else
                        {
                            return await (DataLayer?.GetPreviousResult<TResult>(this, token) 
                                ?? Task.FromResult<TResult>(default));
                        }
                    }
                default:
                    throw new Exception($"Unknown RunType {RunInfo.Type}");
            }
        }
    }
}
