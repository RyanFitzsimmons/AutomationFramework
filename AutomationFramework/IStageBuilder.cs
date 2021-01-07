using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
