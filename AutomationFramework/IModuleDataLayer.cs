using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationFramework
{
    public interface IModuleDataLayer<TId>
    {
        void CreateStage(IModule<TId> module);
        void SetStatus(IModule<TId> module, StageStatuses status);

        void SaveResult<TResult>(IModule<TId> module, TResult result) where TResult : class;
        TResult GetCurrentResult<TResult>(IModule<TId> module) where TResult : class;
        TResult GetPreviousResult<TResult>(IModule<TId> module) where TResult : class;
    }
}
