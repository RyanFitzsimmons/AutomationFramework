using System.Collections.Generic;
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

        protected override async Task<TestModuleResult> DoWork()
        {
            Actions.Add("Doing Work");
            return await base.DoWork();
        }
    }
}
