using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutomationFramework
{
    public interface IModule<TId>
    {
        int MaxParallelChildren { get; set; }
        string Name { get; set; }
        bool IsEnabled { get; set; }
        RunInfo<TId> RunInfo { get; }
        StagePath StagePath { get; }
        Func<object> GetMetaDataFunc { get; }

        CancellationToken GetCancellationToken();
        void Run(RunInfo<TId> runInfo, StagePath path, Func<object> getMetaData, ILogger logger);
        void Cancel();
        IModule<TId>[] InvokeCreateChildren();
    }
}
