using System;
using System.Collections.Generic;
using System.Text;

namespace AutomationFramework
{
    public interface ILogger
    {
        void Information(string message);
        void Information(StagePath path, string message);
        void Warning(string message);
        void Warning(StagePath path, string message);
        void Error(string message);
        void Error(StagePath path, string message);
        void Fatal(string message);
        void Fatal(StagePath path, string message);
    }
}
