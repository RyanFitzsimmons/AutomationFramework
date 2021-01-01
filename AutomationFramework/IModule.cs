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
        int MaxParallelChildren { get; init; }
        string Name { get; init; }
        bool IsEnabled { get; init; }
        IRunInfo RunInfo { get; }
        StagePath Path { get; }

        event Action<IModule, LogLevels, object> OnLog;
    }
}
