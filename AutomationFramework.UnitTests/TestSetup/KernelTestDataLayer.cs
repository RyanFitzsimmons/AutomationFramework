using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationFramework.UnitTests.TestSetup
{
    public class KernelTestDataLayer : IKernelDataLayer<string>
    {
        public void CheckExistingJob(string id, string version)
        {
            throw new NotSupportedException("Data layer is for Run type only");
        }

        public string CreateJob(IKernel<string> kernel)
        {
            return "Test Job ID";
        }

        public string CreateRequest(RunInfo<string> runInfo, object metaData)
        {
            return "Test Request ID";
        }

        public bool GetIsEmptyId(string id)
        {
            return string.IsNullOrWhiteSpace(id);
        }
    }
}
