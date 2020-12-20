using System;
using System.Collections.Generic;
using System.Text;

namespace AutomationFramework
{
    public interface ILogger
    {
        void Information(string message);
        void Information(object message);
        void Information(StagePath path, string message);
        void Information(StagePath path, object message);
        void Warning(string message);
        void Warning(object message);
        void Warning(StagePath path, string message);
        void Warning(StagePath path, object message);
        void Error(string message);
        void Error(object message);
        void Error(StagePath path, string message);
        void Error(StagePath path, object message);
        void Fatal(string message);
        void Fatal(object message);
        void Fatal(StagePath path, string message);
        void Fatal(StagePath path, object message);
    }
}
