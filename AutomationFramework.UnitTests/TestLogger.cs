using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomationFramework.UnitTests
{
    public class TestLogger : ILogger
    {
        public void Error(string message)
        {
            Error(StagePath.Empty, message);
        }

        public void Error(StagePath path, string message)
        {
            Log.Error(GetText(path, message));
        }

        public void Fatal(string message)
        {
            Fatal(StagePath.Empty, message);
        }

        public void Fatal(StagePath path, string message)
        {
            Log.Fatal(GetText(path, message));
        }

        public void Information(string message)
        {
            Information(StagePath.Empty, message);
        }

        public void Information(StagePath path, string message)
        {
            Log.Information(GetText(path, message));
        }

        public void Warning(string message)
        {
            Warning(StagePath.Empty, message);
        }

        public void Warning(StagePath path, string message)
        {
            Log.Warning(GetText(path, message));
        }

        private static string GetText(StagePath path, string message)
        {
            if (path == StagePath.Empty)
                return message;
            return $"{path} - {message}";
        }
    }
}
