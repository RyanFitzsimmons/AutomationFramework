using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationFramework
{
    public interface IModuleDataLayer
    {
        void CreateStage(IModule module);
        void SetStatus(IModule module, StageStatuses status);

        void SaveResult<TResult>(IModule module, TResult result) where TResult : class;
        TResult GetCurrentResult<TResult>(IModule module) where TResult : class;
        TResult GetPreviousResult<TResult>(IModule module) where TResult : class;

        IMetaData GetMetaData(IModule module);
    }
}
