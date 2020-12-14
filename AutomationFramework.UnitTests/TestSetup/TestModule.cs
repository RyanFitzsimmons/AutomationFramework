using System;
using System.Collections.Generic;
using System.Text;

namespace AutomationFramework.UnitTests.TestSetup
{
    public class TestModule : Module<string, ModuleTestDataLayer>
    {
        public override string Name { get; set; } = "Test Stage";

        public List<string> Actions { get; } = new List<string>();
    }
}
