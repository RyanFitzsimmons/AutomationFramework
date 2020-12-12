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
        RunInfo RunInfo { get; }
        StagePath StagePath { get; }
        Func<object> GetMetaDataFunc { get; }

        CancellationToken GetCancellationToken();
        void Run(RunInfo runInfo, StagePath path, Func<object> getMetaData, ILogger logger);
        void Cancel();
        IModule[] InvokeCreateChildren();
    }
}
