using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationFramework.UnitTests.TestSetup
{
    public class TestParallelModule : Module<TestModuleResult>
    {
        public TestParallelModule(IStageBuilder builder) : base(builder) { }

        public override string Name { get; init; } = "Test Parallel Stage With Result";
        public int RandomInteger { get; init; }
        public string[] Array { get; init; }

        protected override TestModuleResult DoWork()
        {
            var task = Task.Delay(RandomInteger);
            while (!task.IsCompleted)
                foreach (var item in Array) _ = item;
            return new TestModuleResult()
            {
                Name = Name,
                StringArray = new string[]
                {
                    "T1",
                    "T2",
                    "T3",
                },
            };
        }
    }
}
