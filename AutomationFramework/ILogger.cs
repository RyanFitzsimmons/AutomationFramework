using System;
using System.Collections.Generic;
using System.Text;

namespace AutomationFramework
{
    public interface ILogger
    {
        void Write(LogLevels level, object message);
        void Write(LogLevels level, StagePath path, object message);        
    }
}
