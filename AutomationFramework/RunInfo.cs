using System;
using System.Collections.Generic;
using System.Text;

namespace AutomationFramework
{
    /// <summary>
    /// Contains information needed to process and retrieve data during a run
    /// </summary>
    /// <typeparam name="TId">The ID type of the job and request. Should only be used to store immutable types.</typeparam>
    public class RunInfo<TId> : IRunInfo
    {
        public RunInfo(RunType type, TId jobId, TId requestId, StagePath path)
        {
            Type = type;
            JobId = jobId;
            RequestId = requestId;
            Path = path;
        }

        public RunType Type { get; }
        public StagePath Path { get; }
        public TId JobId { get; }
        public TId RequestId { get; }

        public static RunInfo<TId> Empty { get { return new RunInfo<TId>(RunType.Run, default, default, StagePath.Empty); } }

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
            if (Type == RunType.Run && !EqualityComparer<TId>.Default.Equals(JobId, default))
            {
                exceptionMsg = "The job ID must be empty for run type 'Run'";
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
            if (Type == RunType.RunFrom && EqualityComparer<TId>.Default.Equals(JobId, default))
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
            if (Type == RunType.RunSingle && EqualityComparer<TId>.Default.Equals(JobId, default))
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
            if (!(obj is RunInfo<TId>)) return false;
            var runInfo = (RunInfo<TId>)obj;
            return EqualityComparer<TId>.Default.Equals(JobId, runInfo.JobId) &&
                EqualityComparer<TId>.Default.Equals(RequestId, runInfo.RequestId) &&
                Path == runInfo.Path &&
                Type == runInfo.Type;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(JobId, RequestId, Path, Type.GetHashCode());
        }

        public IRunInfo Clone()
        {
            return new RunInfo<TId>(Type, JobId, RequestId, Path.Clone());
        }

        public static bool operator ==(RunInfo<TId> a, RunInfo<TId> b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(RunInfo<TId> a, RunInfo<TId> b)
        {
            return !a.Equals(b);
        }

        public override string ToString()
        {
            return $"[{Type}]-[{JobId}]-[{RequestId}]-[{Path}]";
        }
    }
}
