using System;
using System.Collections.Generic;
using System.Text;

namespace AutomationFramework
{
    public interface IKernel<TId>
    {
        string Version { get; }
        string Name { get; }
        void Run(RunInfo<TId> runInfo, Func<object> getMetaData = null);
    }
}
