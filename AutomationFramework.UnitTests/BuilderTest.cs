using AutomationFramework.UnitTests.TestSetup;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace AutomationFramework.UnitTests
{
    public class BuilderTest
    {
        public BuilderTest(ITestOutputHelper output)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Xunit(output)
                .CreateLogger();
        }

        [Fact]
        public void TestBuilder()
        {
            StageBuilder<TestModule> builder = new StageBuilder<TestModule>();
            var modules = builder.Configure((module) => module.Name = "Root")
                .Add<TestModuleWithResult>((builder) => builder.Configure((module) => module.Name = "Test 1")
                    .Add<TestModule>((builder) => builder.Configure((module) => module.Name = "Test 1-1"))
                    .Add<TestModule>((builder) => builder.Configure((module) => module.Name = "Test 1-2")))
                .Add<TestModule>((builder) => builder.Configure((module) => module.Name = "Test 2")
                    .Add<TestModule>((builder) => builder.Configure((module) => module.Name = "Test 2-1"))
                    .Add<TestModule>((builder) => builder.Configure((module) => module.Name = "Test 2-2")))
                .Build(StagePath.Root);

            Assert.Equal(7, modules.Count);
        }
    }
}
