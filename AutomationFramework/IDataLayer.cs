using System.Threading.Tasks;

namespace AutomationFramework
{
    public interface IDataLayer
    {
        bool GetIsNewJob(IRunInfo runInfo);
        Task<IRunInfo> CreateJob(IKernel kernel, IRunInfo runInfo);
        /// <summary>
        /// Perform checks on runinfo and version to confirm if the job should be run.
        /// Throw an exception if the job is invalid
        /// </summary>
        Task ValidateExistingJob(IRunInfo runInfo, string version);

        Task<IRunInfo> CreateRequest(IRunInfo runInfo, IMetaData metaData);

        Task CreateStage(IModule module);
        Task SetStatus(IModule module, StageStatuses status);

        Task SaveResult<TResult>(IModule module, TResult result) where TResult : class;
        Task<TResult> GetCurrentResult<TResult>(IModule module) where TResult : class;
        Task<TResult> GetPreviousResult<TResult>(IModule module) where TResult : class;
    }
}
