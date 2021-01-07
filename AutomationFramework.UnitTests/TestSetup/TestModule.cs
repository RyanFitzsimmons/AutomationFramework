using System;
using System.Collections.Generic;
using System.Text;

namespace AutomationFramework.UnitTests.TestSetup
{
    public class TestModule : Module
    {
        public TestModule(IStageBuilder builder) : base(builder)
        {
        }

        public override string Name { get; init; } = "Test Stage";

        public List<string> Actions { get; } = new List<string>();
    }
}
