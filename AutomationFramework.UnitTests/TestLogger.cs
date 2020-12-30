using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomationFramework.UnitTests
{
    public class TestLogger : ILogger
    {
        public void Write(LogLevels level, object message)
        {
            Write(level, StagePath.Empty, message);
        }

        public void Write(LogLevels level, StagePath path, object message)
        {
            switch (level)
            {
                case LogLevels.Error:
                    Log.Error(GetText(path, message));
                    break;
                case LogLevels.Fatal:
                    Log.Fatal(GetText(path, message));
                    break;
                case LogLevels.Information:
                    Log.Information(GetText(path, message));
                    break;
                case LogLevels.Warning:
                    Log.Warning(GetText(path, message));
                    break;
                default:
                    throw new Exception($"Unknown LogLevel {level}");
            }
        }

        private static string GetText(StagePath path, object message)
        {
            if (path == StagePath.Empty)
                return message.ToString();
            return $"{path} - {message}";
        }
    }
}
