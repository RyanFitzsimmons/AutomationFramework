using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationFramework
{
    public interface IKernelDataLayer<TId>
    {
        TId CreateRequest(RunInfo<TId> runInfo, object metaData);
        TId CreateJob(IKernel<TId> kernel);
        void CheckExistingJob(TId id, string version);
        bool GetIsEmptyId(TId id);
    }
}
