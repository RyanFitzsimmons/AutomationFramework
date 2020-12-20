using System;
using System.Collections.Generic;
using System.Text;

namespace AutomationFramework
{
    [Serializable]
    public enum RunType
    {
        Run = 0,
        RunFrom = 1,
        RunSingle = 2,
        BuildOnly = 3,
    }
}
