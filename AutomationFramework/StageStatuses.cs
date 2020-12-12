using System;
using System.Collections.Generic;
using System.Text;

namespace AutomationFramework
{
    [Serializable]
    public enum StageStatuses
    {
        None = 0,
        Bypassed = 1,
        Running = 2,
        Completed = 3,
        Cancelled = 4,
        Errored = 5,
        Disabled = 6,
    }
}
