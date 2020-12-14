using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationFramework.UnitTests.TestSetup
{
    public class TestModuleWithResult : Module<string, ModuleTestDataLayer, TestModuleResult>
    {
        public override string Name { get; set; } = "Test Stage With Result";

        public List<string> Actions { get; } = new List<string>();

        protected override TestModuleResult DoWork()
        {
            Actions.Add("Doing Work");
            return base.DoWork();
        }
    }
}
