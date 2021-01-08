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

        public IRunInfo CreateRequest(IRunInfo runInfo)
        {
            return new RunInfo<string>(runInfo.Type, (runInfo as RunInfo<string>).JobId, "Test Request ID", runInfo.Path);
        }

        public static IRunInfo CreateJob(IRunInfo runInfo)
        {
            return new RunInfo<string>(runInfo.Type, "Test Job ID", (runInfo as RunInfo<string>).RequestId, runInfo.Path);
        }

        public void CreateStage(IModule module)
        {

        }

        public TResult GetCurrentResult<TResult>(IModule module) where TResult : class
        {
            Results.TryGetValue(module.StagePath, out object value);
            return value as TResult;
        }

        public IRunInfo GetJobId(IKernel kernel, IRunInfo runInfo, IMetaData metaData)
        {
            if (string.IsNullOrWhiteSpace((runInfo as RunInfo<string>).JobId))
            {
                return CreateJob(runInfo);
            }
            else
            {
                return runInfo;
            }
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
