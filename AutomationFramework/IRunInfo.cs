using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationFramework
{
    public interface IRunInfo
    {
        RunType Type { get; }
        StagePath Path { get; }
        bool GetIsValid(out string exceptionMsg);
        IRunInfo Clone();
    }
}
