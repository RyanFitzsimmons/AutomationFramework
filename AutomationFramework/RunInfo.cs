using System;
using System.Collections.Generic;
using System.Text;

namespace AutomationFramework
{
    [Serializable]
    public struct RunInfo
    {
        public RunType Type;
        public object JobId;
        public object RequestId;
        public StagePath Path;

        public static RunInfo Empty { get => new RunInfo { Type = RunType.Run, JobId = null, RequestId = null, Path = StagePath.Empty }; }

        public bool GetIsValid(out string exceptionMsg)
        {
            if (GetIsUnknownRunType())
            {
                exceptionMsg = "Unknown RunType: " + Type;
                return false;
            }

            if (GetIsRunWithJobId(out exceptionMsg)) return false;
            if (GetIsRunWithStagePath(out exceptionMsg)) return false;
            if (GetIsRunFromWithNoJobId(out exceptionMsg)) return false;
            if (GetIsRunFromWithNoStagePath(out exceptionMsg)) return false;
            if (GetIsRunOnlyWithNoJobId(out exceptionMsg)) return false;
            if (GetIsRunOnlyWithNoStagePath(out exceptionMsg)) return false;

            exceptionMsg = null;
            return true;
        }

        public bool GetIsRunWithJobId(out string exceptionMsg)
        {
            if (Type == RunType.Run && JobId != null)
            {
                exceptionMsg = "The job ID must be null for run type 'Run'";
                return true;
            }

            exceptionMsg = null;
            return false;
        }

        public bool GetIsRunWithStagePath(out string exceptionMsg)
        {
            if (Type == RunType.Run && Path.Length > 0)
            {
                exceptionMsg = "The stage path must be empty for run type 'Run'";
                return true;
            }

            exceptionMsg = null;
            return false;
        }

        public bool GetIsRunFromWithNoJobId(out string exceptionMsg)
        {
            if (Type == RunType.RunFrom && JobId == null)
            {
                exceptionMsg = "A job ID must be provided for run type 'RunFrom'";
                return true;
            }

            exceptionMsg = null;
            return false;
        }

        public bool GetIsRunFromWithNoStagePath(out string exceptionMsg)
        {
            if (Type == RunType.RunFrom && Path.Length == 0)
            {
                exceptionMsg = "A stage path must be provided for run type 'RunFrom'";
                return true;
            }

            exceptionMsg = null;
            return false;
        }

        public bool GetIsRunOnlyWithNoJobId(out string exceptionMsg)
        {
            if (Type == RunType.RunSingle && JobId == null)
            {
                exceptionMsg = "A job ID must be provided for run type 'RunOnly'";
                return true;
            }

            exceptionMsg = null;
            return false;
        }

        public bool GetIsRunOnlyWithNoStagePath(out string exceptionMsg)
        {
            if (Type == RunType.RunSingle && Path.Length == 0)
            {
                exceptionMsg = "A stage path must be provided for run type 'RunOnly'";
                return true;
            }

            exceptionMsg = null;
            return false;
        }

        public bool GetIsUnknownRunType() =>
            Type switch
            {
                RunType.Run or RunType.RunFrom or RunType.RunSingle => false,
                _ => true,
            };

        public override bool Equals(object obj)
        {
            if (!(obj is RunInfo)) return false;
            var runInfo = (RunInfo)obj;
            return this.JobId == runInfo.JobId &&
                this.RequestId == runInfo.RequestId &&
                this.Path == runInfo.Path &&
                this.Type == runInfo.Type;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(JobId, RequestId, Path, Type.GetHashCode());
        }

        public static bool operator ==(RunInfo a, RunInfo b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(RunInfo a, RunInfo b)
        {
            return !a.Equals(b);
        }
    }
}
