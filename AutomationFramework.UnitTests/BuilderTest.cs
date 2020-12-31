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
            StageBuilder<TestModule> builder = new StageBuilder<TestModule>(RunInfo<int>.Empty, StagePath.Root, () => null);
            var modules = builder.Configure((ri, sp, md) => new(ri, sp, md) { Name = "Root" })
                .Add<TestModuleWithResult>((builder) => builder.Configure((ri, sp, md) => new(ri, sp, md) { Name = "Test 1" })
                    .Add<TestModule>((builder) => builder.Configure((ri, sp, md) => new(ri, sp, md) { Name = "Test 1-1" }))
                    .Add<TestModule>((builder) => builder.Configure((ri, sp, md) => new(ri, sp, md) { Name = "Test 1-2" })))
                .Add<TestModule>((builder) => builder.Configure((ri, sp, md) => new(ri, sp, md) { Name = "Test 2" })
                    .Add<TestModule>((builder) => builder.Configure((ri, sp, md) => new(ri, sp, md) { Name = "Test 2-1" }))
                    .Add<TestModule>((builder) => builder.Configure((ri, sp, md) => new(ri, sp, md) { Name = "Test 2-2" })))
                .Build();

            Assert.Equal(7, modules.Count);
        }
    }
}
