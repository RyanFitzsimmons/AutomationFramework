using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationFramework.UnitTests.TestSetup
{
    public class TestParallelDataLayer : IDataLayer
    {
        private readonly ConcurrentDictionary<StagePath, object> Results = new ConcurrentDictionary<StagePath, object>();

        public bool GetIsNewJob(IRunInfo runInfo)
        {
            return string.IsNullOrWhiteSpace((runInfo as RunInfo<string>).JobId);
        }

        public async Task<IRunInfo> CreateJob(IKernel kernel, IRunInfo runInfo)
        {
            return await Task.FromResult(new RunInfo<string>(runInfo.Type, "Test Job ID", (runInfo as RunInfo<string>).RequestId, runInfo.Path));
        }

        public async Task ValidateExistingJob(IRunInfo runInfo, string version)
        {
            await Task.CompletedTask;
        }

        public async Task<IRunInfo> CreateRequest(IRunInfo runInfo, IMetaData metaData)
        {
            return await Task.FromResult(new RunInfo<string>(runInfo.Type, (runInfo as RunInfo<string>).JobId, "Test Request ID", runInfo.Path));
        }

        public async Task<IRunInfo> CreateRequest(IRunInfo runInfo)
        {
            return await Task.FromResult(new RunInfo<string>(runInfo.Type, (runInfo as RunInfo<string>).JobId, "Test Request ID", runInfo.Path));
        }

        public async Task CreateStage(IModule module)
        {
            await Task.CompletedTask;
        }

        public async Task<TResult> GetCurrentResult<TResult>(IModule module) where TResult : class
        {
            Results.TryGetValue(module.StagePath, out object value);
            return await Task.FromResult(value as TResult);
        }

        public async Task<TResult> GetPreviousResult<TResult>(IModule module) where TResult : class
        {
            return await Task.FromResult<TResult>(null);
        }

        public async Task SaveResult<TResult>(IModule module, TResult result) where TResult : class
        {
            Results.TryAdd(module.StagePath, result);
            await Task.CompletedTask;
        }

        public async Task SetStatus(IModule module, StageStatuses status)
        {
            await Task.CompletedTask;
        }
    }
}
