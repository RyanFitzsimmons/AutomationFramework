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

        public IRunInfo CreateJob(IKernel kernel, IRunInfo runInfo)
        {
            return new RunInfo<string>(runInfo.Type, "Test Job ID", (runInfo as RunInfo<string>).RequestId, runInfo.Path);
        }

        public void ValidateExistingJob(IRunInfo runInfo, string version)
        {

        }

        public IRunInfo CreateRequest(IRunInfo runInfo, IMetaData metaData)
        {
            return new RunInfo<string>(runInfo.Type, (runInfo as RunInfo<string>).JobId, "Test Request ID", runInfo.Path);
        }

        public IRunInfo CreateRequest(IRunInfo runInfo)
        {
            return new RunInfo<string>(runInfo.Type, (runInfo as RunInfo<string>).JobId, "Test Request ID", runInfo.Path);
        }

        public void CreateStage(IModule module)
        {

        }

        public TResult GetCurrentResult<TResult>(IModule module) where TResult : class
        {
            Results.TryGetValue(module.StagePath, out object value);
            return value as TResult;
        }

        public TResult GetPreviousResult<TResult>(IModule module) where TResult : class
        {
            return null;
        }

        public void SaveResult<TResult>(IModule module, TResult result) where TResult : class
        {
            Results.TryAdd(module.StagePath, result);
        }

        public void SetStatus(IModule module, StageStatuses status)
        {

        }
    }
}
