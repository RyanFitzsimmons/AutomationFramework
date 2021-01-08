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
    public class ParallelTests
    {
        public ParallelTests(ITestOutputHelper output)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Xunit(output)
                .CreateLogger();
        }

        [Fact]
        public void Parallel()
        {
            var dataLayer = new TestParallelDataLayer();
            var job = new TestParallelKernel(dataLayer, new TestLogger());
            job.Run(RunInfo<string>.Empty, new TestMetaData());
        }
    }
}
