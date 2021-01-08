using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationFramework.UnitTests.TestSetup
{
    public class TestDataLayer : IDataLayer
    {
        public ConcurrentBag<IModule> TestModules { get; private set; } = new ConcurrentBag<IModule>();

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

        public void CreateStage(IModule module)
        {
            TestModules.Add(module);
            string action = "Create Stage";
            (module as TestModuleWithResult).Actions.Add(action);
        }

        public TResult GetCurrentResult<TResult>(IModule module) where TResult : class
        {
            string action = "Get Current Result";
            (module as TestModuleWithResult).Actions.Add(action);
            return Activator.CreateInstance<TResult>();
        }

        public TResult GetPreviousResult<TResult>(IModule module) where TResult : class
        {
            string action = "Get Existing Result";
            (module as TestModuleWithResult).Actions.Add(action);
            return Activator.CreateInstance<TResult>();
        }

        public void SaveResult<TResult>(IModule module, TResult result) where TResult : class
        {
            string action = "Save Result";
            (module as TestModuleWithResult).Actions.Add(action);
        }

        public void SetStatus(IModule module, StageStatuses status)
        {
            string action = "Set Status " + status;
            (module as TestModuleWithResult).Actions.Add(action);
        }
    }
}
