using System;
using System.Threading;
using System.Threading.Tasks;

namespace AutomationFramework
{
    public interface IModule
    {
        event Action<IModule, LogLevels, object> OnLog;
        int MaxParallelChildren { get; init; }
        string Name { get; init; }
        bool IsEnabled { get; init; }
        IRunInfo RunInfo { get; }
        StagePath StagePath { get; }
        Task Build();
        Task Run();
        Task<IModule[]> InvokeCreateChildren();
        void Cancel();
    }
}
