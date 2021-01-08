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
    public class StageBuilderTest
    {
        public StageBuilderTest(ITestOutputHelper output)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Xunit(output)
                .CreateLogger();
        }

        [Fact]
        public void TestBuilder()
        {
            StageBuilder<TestModule> builder = new StageBuilder<TestModule>(new TestDataLayer(), RunInfo<int>.Empty, StagePath.Root);
            var modules = builder.Configure((b) => new(b) { Name = "Root" })
                .Add<TestModuleWithResult>((builder) => builder.Configure((b) => new(b) { Name = "Test 1" })
                    .Add<TestModule>((builder) => builder.Configure((b) => new(b) { Name = "Test 1-1" }))
                    .Add<TestModule>((builder) => builder.Configure((b) => new(b) { Name = "Test 1-2" })
                        .ForEach<int>(new List<int> { 1, 2, 3 }, (b, i) => 
                            b.Add<TestModule>((builder) => builder.Configure((b) => new TestModule(b) { Name = $"Test 1-2-{i}" })))))
                .Add<TestModule>((builder) => builder.Configure((b) => new(b) { Name = "Test 2" })
#pragma warning disable CS0162 // Unreachable code detected
                    .Add<TestModule>((builder) => { if (false) builder.Configure((b) => new(b) { Name = "Test 2-1" }); })
#pragma warning restore CS0162 // Unreachable code detected
                    .Add<TestModule>((builder) => builder.Configure((b) => new(b) { Name = "Test 2-2" })))
                .Build();

            Assert.Equal(9, modules.Length);
        }
    }
}
