using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationFramework.UnitTests.TestSetup
{
    public class ModuleTestDataLayer : IModuleDataLayer<string>
    {
        public void CreateStage(IModule<string> module)
        {
            string action = "Create Stage";
            (module as TestModuleWithResult).Actions.Add(action);
        }

        public TResult GetCurrentResult<TResult>(IModule<string> module) where TResult : class
        {
            string action = "Get Current Result";
            (module as TestModuleWithResult).Actions.Add(action);
            return Activator.CreateInstance<TResult>();
        }

        public TResult GetPreviousResult<TResult>(IModule<string> module) where TResult : class
        {
            string action = "Get Existing Result";
            (module as TestModuleWithResult).Actions.Add(action);
            return Activator.CreateInstance<TResult>();
        }

        public void SaveResult<TResult>(IModule<string> module, TResult result) where TResult : class
        {
            string action = "Save Result";
            (module as TestModuleWithResult).Actions.Add(action);
        }

        public void SetStatus(IModule<string> module, StageStatuses status)
        {
            string action = "Set Status " + status;
            (module as TestModuleWithResult).Actions.Add(action);
        }
    }
}
