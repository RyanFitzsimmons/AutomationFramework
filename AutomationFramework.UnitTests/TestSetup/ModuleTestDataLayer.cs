using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationFramework.UnitTests.TestSetup
{
    public class ModuleTestDataLayer : IModuleDataLayer
    {
        public void CreateStage(IModule module)
        {
            string action = "Create Stage";
            (module as TestModuleWithResult).Actions.Add(action);
        }

        public TResult GetCurrentResult<TResult>(IModule module) where TResult : class
        {
            string action = "Get Current Result";
            (module as TestModuleWithResult).Actions.Add(action);
            return Activator.CreateInstance<TResult>();
        }

        public IMetaData GetMetaData(IModule module)
        {
            string action = "Get Meta Data";
            (module as TestModuleWithResult).Actions.Add(action);
            return new TestMetaData();
        }

        public TResult GetPreviousResult<TResult>(IModule module) where TResult : class
        {
            string action = "Get Existing Result";
            (module as TestModuleWithResult).Actions.Add(action);
            return Activator.CreateInstance<TResult>();
        }

        public void SaveResult<TResult>(IModule module, TResult result) where TResult : class
        {
            string action = "Save Result";
            (module as TestModuleWithResult).Actions.Add(action);
        }

        public void SetStatus(IModule module, StageStatuses status)
        {
            string action = "Set Status " + status;
            (module as TestModuleWithResult).Actions.Add(action);
        }
    }
}
