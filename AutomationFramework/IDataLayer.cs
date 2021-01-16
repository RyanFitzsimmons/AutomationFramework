using System.Threading;
using System.Threading.Tasks;

namespace AutomationFramework
{
    public interface IDataLayer
    {
        bool GetIsNewJob(IRunInfo runInfo);
        Task<IRunInfo> CreateJob(IKernel kernel, IRunInfo runInfo, CancellationToken cancellationToken);
        /// <summary>
        /// Perform checks on runinfo and version to confirm if the job should be run.
        /// Throw an exception if the job is invalid
        /// </summary>
        Task ValidateExistingJob(IRunInfo runInfo, string version, CancellationToken cancellationToken);

        Task<IRunInfo> CreateRequest(IRunInfo runInfo, IMetaData metaData, CancellationToken cancellationToken);

        Task CreateStage(IModule module, CancellationToken cancellationToken);
        Task SetStatus(IModule module, StageStatuses status, CancellationToken cancellationToken);

        Task SaveResult<TResult>(IModule module, TResult result, CancellationToken cancellationToken) where TResult : class;
        Task<TResult> GetCurrentResult<TResult>(IModule module, CancellationToken cancellationToken) where TResult : class;
        Task<TResult> GetPreviousResult<TResult>(IModule module, CancellationToken cancellationToken) where TResult : class;
    }
}
