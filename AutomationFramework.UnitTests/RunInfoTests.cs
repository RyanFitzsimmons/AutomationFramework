using Serilog;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace AutomationFramework.UnitTests
{
    public class RunInfoTests
    {
        public RunInfoTests(ITestOutputHelper output)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Xunit(output)
                .CreateLogger();
        }

        [Fact]
        public void GetIsValidTest()
        {
            // valid run infos
            var validInfos = new List<RunInfo<string>>
            {
                new RunInfo<string>(RunType.Standard, null, null, StagePath.Empty),
                new RunInfo<string>(RunType.From, "1", null, StagePath.Root),
                new RunInfo<string>(RunType.Single, "1", null, StagePath.Root)
            };

            // invalid run infos
            var invalidInfos = new List<RunInfo<string>>
            {
                new RunInfo<string>(RunType.Standard, "1", null, StagePath.Empty),
                new RunInfo<string>(RunType.Standard, null, null, StagePath.Root),
                new RunInfo<string>(RunType.Standard, "1", null, StagePath.Root),

                new RunInfo<string>(RunType.From, null, null, StagePath.Root),
                new RunInfo<string>(RunType.From, "1", null, StagePath.Empty),
                new RunInfo<string>(RunType.From, null, null, StagePath.Empty),

                new RunInfo<string>(RunType.Single, null, null, StagePath.Root),
                new RunInfo<string>(RunType.Single, "1", null, StagePath.Empty),
                new RunInfo<string>(RunType.Single, null, null, StagePath.Empty)
            };

            string exMsg;
            foreach (var info in validInfos)
                Assert.True(info.GetIsValid(out exMsg), exMsg);

            foreach (var info in invalidInfos)
                Assert.False(info.GetIsValid(out exMsg), exMsg);
        }

    }
}
