using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationFramework
{
    public interface IKernelDataLayer
    {
        object CreateRequest(RunInfo runInfo, object metaData);
        object CreateJob(IKernel kernel);
        void CheckExistingJob(object id, string version);
        bool GetIsEmptyId(object id);
    }
}
