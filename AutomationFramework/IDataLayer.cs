using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationFramework
{
    public interface IDataLayer
    {
        /// <summary>
        /// Should return a new instance every call
        /// </summary>
        /// <param name="runInfo">The run info</param>
        /// <returns>A unique copy of the meta data</returns>
        IMetaData GetMetaData(IRunInfo runInfo);
        IRunInfo GetJobId(IKernel kernel, IRunInfo runInfo);
        IRunInfo CreateRequest(IRunInfo runInfo, IMetaData metaData);
        IRunInfo CreateJob(IKernel kernel, IRunInfo runInfo);
        void CheckExistingJob(IRunInfo runInfo, string version);


        void CreateStage(IModule module);
        void SetStatus(IModule module, StageStatuses status);

        void SaveResult<TResult>(IModule module, TResult result) where TResult : class;
        TResult GetCurrentResult<TResult>(IModule module) where TResult : class;
        TResult GetPreviousResult<TResult>(IModule module) where TResult : class;
    }
}
