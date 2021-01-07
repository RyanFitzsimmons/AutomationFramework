using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationFramework.UnitTests.TestSetup
{
    public class TestDataLayer : IDataLayer
    {
        public List<IModule> TestModules { get; private set; } = new List<IModule>();

        public void CheckExistingJob(IRunInfo runInfo, string version)
        {
            //throw new NotSupportedException("Data layer is for Run type only");
        }

        public IRunInfo CreateJob(IKernel kernel, IRunInfo runInfo, IMetaData metaData)
        {
            return new RunInfo<string>(runInfo.Type, "Test Job ID", (runInfo as RunInfo<string>).RequestId, runInfo.Path);
        }

        public IRunInfo CreateRequest(IRunInfo runInfo)
        {
            return new RunInfo<string>(runInfo.Type, (runInfo as RunInfo<string>).JobId, "Test Request ID", runInfo.Path);
        }

        public IRunInfo GetJobId(IKernel kernel, IRunInfo runInfo, IMetaData metaData)
        {
            if (string.IsNullOrWhiteSpace((runInfo as RunInfo<string>).JobId))
            {
                return CreateJob(kernel, runInfo, metaData);
            }
            else
            {
                CheckExistingJob(runInfo, kernel.Version);
                return runInfo;
            }
        }

        public IMetaData GetMetaData(IRunInfo runInfo) 
        {
            return new TestMetaData();
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
