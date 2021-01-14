using System;

namespace AutomationFramework
{
    public interface IStageBuilder
    {
        IDataLayer DataLayer { get; }
        IRunInfo RunInfo { get; }
        StagePath StagePath { get; }
        IStageBuilder Add<TChildModule>(Action<StageBuilder<TChildModule>> builderDelegate) where TChildModule : IModule;
        IModule[] Build();
    }
}
