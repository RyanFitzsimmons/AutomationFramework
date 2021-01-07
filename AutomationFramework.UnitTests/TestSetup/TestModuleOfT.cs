using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationFramework.UnitTests.TestSetup
{
    public class TestModuleWithResult : Module<TestModuleResult>
    {
        public TestModuleWithResult(IStageBuilder builder) : base(builder)
        {
        }

        public override string Name { get; init; } = "Test Stage With Result";

        public List<string> Actions { get; } = new List<string>();

        protected override TestModuleResult DoWork()
        {
            Actions.Add("Doing Work");
            return base.DoWork();
        }
    }
}
