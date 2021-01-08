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
        /// Should create a job and return the Id if none exists or
        /// return the existing job Id if one does.
        /// </summary>
        /// <param name="kernel">The kernel</param>
        /// <param name="runInfo">The runInfo</param>
        /// <param name="metaData">The metaData</param>
        /// <returns>The runInfo with the potentially updated job Id</returns>
        IRunInfo GetJobId(IKernel kernel, IRunInfo runInfo, IMetaData metaData);
        IRunInfo CreateRequest(IRunInfo runInfo);

        void CreateStage(IModule module);
        void SetStatus(IModule module, StageStatuses status);

        void SaveResult<TResult>(IModule module, TResult result) where TResult : class;
        TResult GetCurrentResult<TResult>(IModule module) where TResult : class;
        TResult GetPreviousResult<TResult>(IModule module) where TResult : class;
    }
}
