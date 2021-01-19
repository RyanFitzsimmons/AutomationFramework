using System;

namespace AutomationFramework
{
    public interface ILogger : IDisposable
    {
        void Write(LogLevels level, object message);
        void Write(LogLevels level, StagePath path, object message);        
    }
}
