namespace AutomationFramework
{
    public interface IDataLayer
    {
        bool GetIsNewJob(IRunInfo runInfo);
        IRunInfo CreateJob(IKernel kernel, IRunInfo runInfo);
        /// <summary>
        /// Perform checks on runinfo and version to confirm if the job should be run.
        /// Throw an exception if the job is invalid
        /// </summary>
        void ValidateExistingJob(IRunInfo runInfo, string version);

        IRunInfo CreateRequest(IRunInfo runInfo, IMetaData metaData);

        void CreateStage(IModule module);
        void SetStatus(IModule module, StageStatuses status);

        void SaveResult<TResult>(IModule module, TResult result) where TResult : class;
        TResult GetCurrentResult<TResult>(IModule module) where TResult : class;
        TResult GetPreviousResult<TResult>(IModule module) where TResult : class;
    }
}
