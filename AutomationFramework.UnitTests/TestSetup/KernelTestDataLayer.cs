using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationFramework.UnitTests.TestSetup
{
    public class KernelTestDataLayer : IKernelDataLayer
    {
        public void CheckExistingJob(object id, string version)
        {
            throw new NotSupportedException("Data layer is for Run type only");
        }

        public object CreateJob(IKernel kernel)
        {
            return "Test Job ID";
        }

        public object CreateRequest(RunInfo runInfo, object metaData)
        {
            return "Test Request ID";
        }

        public bool GetIsEmptyId(object id)
        {
            return string.IsNullOrWhiteSpace((string)id);
        }
    }
}
