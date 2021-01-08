using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationFramework.UnitTests.TestSetup
{
    public class TestParallelKernel : KernelBase<TestParallelDataLayer>
    {
        public TestParallelKernel(TestParallelDataLayer dataLayer, ILogger logger = null) : base(dataLayer, logger)
        {
        }

        public override string Name => "Test Parallel Job";
        public override string Version => "";

        private readonly Random Random = new Random();

        protected override IStageBuilder Configure() =>
            GetStageBuilder<TestParallelModule>().Configure((b) => new(b)
            {
                Name = GetMetaData<TestMetaData>().Test,
                RandomInteger = Random.Next(100, 1000),
                Array = Array.Empty<string>(),
                MaxParallelChildren = 0,
                CreateChildren = (b, r) =>
                {
                    for (int i = 0; i < 2; i++)
                        b.Add<TestParallelModule>((c) => CreateChild(c, r.StringArray));
                },
            });

        private IStageBuilder CreateChild(StageBuilder<TestParallelModule> builder, string[] array) =>
            builder.Configure((b) => new(b)
            {
                Name = GetMetaData<TestMetaData>().Test,
                RandomInteger = Random.Next(100, 1000),
                Array = array.ToArray(),
                MaxParallelChildren = 0,
                CreateChildren = (b, r) => 
                {
                    for (int i = 0; i < Random.Next(1, 16); i++)
                    {
                        b.Add<TestParallelModule>((c) => CreateEndChild(c, r.StringArray));
                    }
                },
            });

        private IStageBuilder CreateEndChild(StageBuilder<TestParallelModule> builder, string[] array) =>
            builder.Configure((b) => new(b)
            {
                Name = GetMetaData<TestMetaData>().Test,
                RandomInteger = Random.Next(100, 1000),
                Array = array.ToArray(),
            });
    }
}
