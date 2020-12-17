using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationFramework.UnitTests.TestSetup
{
    public class KernelTestDataLayer : IKernelDataLayer
    {
        public void CheckExistingJob(IRunInfo runInfo, string version)
        {
            throw new NotSupportedException("Data layer is for Run type only");
        }

        public IRunInfo CreateJob(IKernel kernel, IRunInfo runInfo)
        {
            return new RunInfo<string>(runInfo.Type, "Test Job ID", (runInfo as RunInfo<string>).RequestId, runInfo.Path.Clone());
        }

        public IRunInfo CreateRequest(IRunInfo runInfo, IMetaData metaData)
        {
            var md = metaData as TestMetaData;
            return new RunInfo<string>(runInfo.Type, (runInfo as RunInfo<string>).JobId, "Test Request ID", runInfo.Path.Clone());
        }

        public IRunInfo GetJobId(IKernel kernel, IRunInfo runInfo)
        {
            if (string.IsNullOrWhiteSpace((runInfo as RunInfo<string>).JobId))
            {
                return CreateJob(kernel, runInfo);
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
    }
}
