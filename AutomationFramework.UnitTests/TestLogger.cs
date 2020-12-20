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

        public void Information(object message)
        {
            Information(message.ToString());
        }

        public void Information(StagePath path, object message)
        {
            Information(path, message.ToString());
        }

        public void Warning(object message)
        {
            Warning(message.ToString());
        }

        public void Warning(StagePath path, object message)
        {
            Warning(path, message.ToString());
        }

        public void Error(object message)
        {
            Error(message.ToString());
        }

        public void Error(StagePath path, object message)
        {
            Error(path, message.ToString());
        }

        public void Fatal(object message)
        {
            Fatal(message.ToString());
        }

        public void Fatal(StagePath path, object message)
        {
            Fatal(path, message.ToString());
        }
    }
}
