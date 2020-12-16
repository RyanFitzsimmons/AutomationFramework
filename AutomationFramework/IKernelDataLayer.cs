using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationFramework
{
    public interface IKernelDataLayer
    {
        IRunInfo GetJobId(IKernel kernel, IRunInfo runInfo);
        IRunInfo CreateRequest(IRunInfo runInfo, object metaData);
        IRunInfo CreateJob(IKernel kernel, IRunInfo runInfo);
        void CheckExistingJob(IRunInfo runInfo, string version);

    }
}
