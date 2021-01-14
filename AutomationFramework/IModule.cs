using System;

namespace AutomationFramework
{
    public interface IModule
    {
        int MaxParallelChildren { get; init; }
        string Name { get; init; }
        bool IsEnabled { get; init; }
        IRunInfo RunInfo { get; }
        StagePath StagePath { get; }

        event Action<IModule, LogLevels, object> OnLog;
    }
}
