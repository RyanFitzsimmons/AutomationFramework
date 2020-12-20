﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationFramework
{
    public interface IKernelDataLayer
    {
        IMetaData GetMetaData(IRunInfo runInfo);
        IRunInfo GetJobId(IKernel kernel, IRunInfo runInfo);
        IRunInfo CreateRequest(IRunInfo runInfo, IMetaData metaData);
        IRunInfo CreateJob(IKernel kernel, IRunInfo runInfo);
        void CheckExistingJob(IRunInfo runInfo, string version);
    }
}
