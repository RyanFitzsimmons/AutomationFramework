using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutomationFramework
{
    public interface IModule
    {
        int MaxParallelChildren { get; set; }
        string Name { get; set; }
        bool IsEnabled { get; set; }
        IRunInfo RunInfo { get; }
        StagePath StagePath { get; }

        CancellationToken GetCancellationToken();
        void Run(IRunInfo runInfo, StagePath path, object metaData, ILogger logger);
        void Cancel();
        void InvokeConfigureChild(IModule child);
    }
}
