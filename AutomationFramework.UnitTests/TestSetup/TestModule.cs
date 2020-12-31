using System;
using System.Collections.Generic;
using System.Text;

namespace AutomationFramework.UnitTests.TestSetup
{
    public class TestModule : Module<ModuleTestDataLayer>
    {
        public TestModule(IRunInfo runInfo, StagePath stagePath, IMetaData metaData) : base(runInfo, stagePath, metaData)
        {
        }

        public override string Name { get; init; } = "Test Stage";

        public List<string> Actions { get; } = new List<string>();
    }
}
