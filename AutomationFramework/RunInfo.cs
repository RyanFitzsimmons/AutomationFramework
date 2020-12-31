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
        public RunInfo(RunInfo<TId> runInfo)
            : this(runInfo.Type, runInfo.JobId, runInfo.RequestId, runInfo.Path) { }

        public RunInfo(RunType type, TId jobId, TId requestId, StagePath path)
        {
            Type = type;
            JobId = jobId;
            RequestId = requestId;
            Path = new StagePath(path);
        }

        public RunType Type { get; }
        public StagePath Path { get; }
        public TId JobId { get; }
        public TId RequestId { get; }

        public static RunInfo<TId> Empty { get { return new RunInfo<TId>(RunType.Standard, default, default, StagePath.Empty); } }

        public bool GetIsValid(out string exceptionMsg)
        {
            if (GetIsUnknownRunType())
            {
                exceptionMsg = "Unknown RunType: " + Type;
                return false;
            }

            if (GetIsBuildWithJobId(out exceptionMsg)) return false;
            if (GetIsBuildWithStagePath(out exceptionMsg)) return false;
            if (GetIsStandardWithJobId(out exceptionMsg)) return false;
            if (GetIsStandardWithStagePath(out exceptionMsg)) return false;
            if (GetIsFromWithNoJobId(out exceptionMsg)) return false;
            if (GetIsFromWithNoStagePath(out exceptionMsg)) return false;
            if (GetIsSingleWithNoJobId(out exceptionMsg)) return false;
            if (GetIsSingleWithNoStagePath(out exceptionMsg)) return false;

            exceptionMsg = null;
            return true;
        }

        private bool GetIsBuildWithJobId(out string exceptionMsg)
        {
            if (Type == RunType.Build && !EqualityComparer<TId>.Default.Equals(JobId, default))
            {
                exceptionMsg = $"The job ID must be empty for run type '{nameof(RunType.Build)}'";
                return true;
            }

            exceptionMsg = null;
            return false;
        }

        private bool GetIsBuildWithStagePath(out string exceptionMsg)
        {
            if (Type == RunType.Standard && Path.Length > 0)
            {
                exceptionMsg = $"The stage path must be empty for run type '{nameof(RunType.Build)}'";
                return true;
            }

            exceptionMsg = null;
            return false;
        }

        public bool GetIsStandardWithJobId(out string exceptionMsg)
        {
            if (Type == RunType.Standard && !EqualityComparer<TId>.Default.Equals(JobId, default))
            {
                exceptionMsg = $"The job ID must be empty for run type '{nameof(RunType.Standard)}'";
                return true;
            }

            exceptionMsg = null;
            return false;
        }

        public bool GetIsStandardWithStagePath(out string exceptionMsg)
        {
            if (Type == RunType.Standard && Path.Length > 0)
            {
                exceptionMsg = $"The stage path must be empty for run type '{nameof(RunType.Standard)}'";
                return true;
            }

            exceptionMsg = null;
            return false;
        }

        public bool GetIsFromWithNoJobId(out string exceptionMsg)
        {
            if (Type == RunType.From && EqualityComparer<TId>.Default.Equals(JobId, default))
            {
                exceptionMsg = $"A job ID must be provided for run type '{nameof(RunType.From)}'";
                return true;
            }

            exceptionMsg = null;
            return false;
        }

        public bool GetIsFromWithNoStagePath(out string exceptionMsg)
        {
            if (Type == RunType.From && Path.Length == 0)
            {
                exceptionMsg = $"A stage path must be provided for run type '{nameof(RunType.From)}'";
                return true;
            }

            exceptionMsg = null;
            return false;
        }

        public bool GetIsSingleWithNoJobId(out string exceptionMsg)
        {
            if (Type == RunType.Single && EqualityComparer<TId>.Default.Equals(JobId, default))
            {
                exceptionMsg = $"A job ID must be provided for run type '{nameof(RunType.Single)}'";
                return true;
            }

            exceptionMsg = null;
            return false;
        }

        public bool GetIsSingleWithNoStagePath(out string exceptionMsg)
        {
            if (Type == RunType.Single && Path.Length == 0)
            {
                exceptionMsg = $"A stage path must be provided for run type '{nameof(RunType.Single)}'";
                return true;
            }

            exceptionMsg = null;
            return false;
        }

        public bool GetIsUnknownRunType() =>
            Type switch
            {
                RunType.Standard or RunType.From or RunType.Single or RunType.Build => false,
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

        public override int GetHashCode() =>
            HashCode.Combine(JobId, RequestId, Path, Type.GetHashCode());

        public IRunInfo Clone() =>
            new RunInfo<TId>(this);

        public static bool operator ==(RunInfo<TId> a, RunInfo<TId> b) =>
            a.Equals(b);

        public static bool operator !=(RunInfo<TId> a, RunInfo<TId> b) =>
            !a.Equals(b);

        public override string ToString() =>
            $"[{Type}]-[{JobId}]-[{RequestId}]-[{Path}]";
    }
}
