using System;

namespace AutomationFramework
{
    [Serializable]
    public enum StageStatuses
    {
        Errored = -3,
        Cancelled = -2,
        Disabled = -1,
        None = 0,
        Bypassed = 1,
        Running = 2,
        Completed = 3,
    }
}
