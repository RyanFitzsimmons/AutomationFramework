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
        ConcurrentDictionary<StagePath, IModule> Build(StagePath path);
        IModule[] BuildToArray(StagePath path);
    }
}
