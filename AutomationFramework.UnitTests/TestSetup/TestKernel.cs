using System;
using System.Collections.Generic;
using System.Text;

namespace AutomationFramework.UnitTests.TestSetup
{
    public class TestKernel : KernelBase<KernelTestDataLayer>
    {
        public TestKernel(int maxParallelChildren, ILogger logger = null) : base(logger)
        {
            MaxParallelChildren = maxParallelChildren;
        }

        public override string Name => "Test Job";

        public override string Version => "";

        private int MaxParallelChildren { get; }

        public List<IModule> TestModules { get; private set; } = new List<IModule>();

        protected override KernelTestDataLayer CreateDataLayer()
        {
            return new KernelTestDataLayer();
        }

        protected override IStageBuilder Configure()
        {
            var builder = GetStageBuilder<TestModuleWithResult>();
            builder.Configure((m) =>
            {
                m.Name = Name + " " + 0;
                m.IsEnabled = true;
                m.MaxParallelChildren = MaxParallelChildren;
            });

            for (int i = 0; i < 3; i++)
                builder.Add<TestModuleWithResult>((b) => CreateChildModule1(b, i));

            TestModules.AddRange(builder.BuildToArray(StagePath.Root));
            return builder;
        }

        private IStageBuilder CreateChildModule1(StageBuilder<TestModuleWithResult> builder, int index)
        {
            builder.Configure((m) =>
            {
                m.Name = Name + " " + index;
                m.IsEnabled = index != 0;
                m.MaxParallelChildren = MaxParallelChildren;
            });

            for (int i = 0; i < 3; i++)
                builder.Add<TestModuleWithResult>((b) => CreateChildModule2(b, i));

            return builder;
        }

        private IStageBuilder CreateChildModule2(StageBuilder<TestModuleWithResult> builder, int index)
        {
            return builder.Configure((m) =>
            {
                m.Name = Name + " " + index;
                m.IsEnabled = true;
                m.MaxParallelChildren = MaxParallelChildren;
            });
        }
    }
}
