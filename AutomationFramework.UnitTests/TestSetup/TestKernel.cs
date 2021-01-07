using System;
using System.Collections.Generic;
using System.Text;

namespace AutomationFramework.UnitTests.TestSetup
{
    public class TestKernel : KernelBase<TestDataLayer>
    {
        public TestKernel(int maxParallelChildren, TestDataLayer dataLayer, ILogger logger = null) : base(dataLayer, logger)
        {
            MaxParallelChildren = maxParallelChildren;
        }

        public override string Name => "Test Job";
        public override string Version => "";
        private int MaxParallelChildren { get; }

        protected override IStageBuilder Configure()
        {
            var builder = GetStageBuilder<TestModuleWithResult>();
            builder.Configure((b) => new(b)
            {
                    Name = Name + " " + 0,
                    IsEnabled = true,
                    MaxParallelChildren = MaxParallelChildren,
                });

            for (int i = 0; i < 3; i++)
                builder.Add<TestModuleWithResult>((b) => CreateChildModule1(b, i));

            return builder;
        }
        
        private IStageBuilder CreateChildModule1(StageBuilder<TestModuleWithResult> builder, int index)
        {
            builder.Configure((b) => new(b)
            {
                Name = Name + " " + index,
                IsEnabled = index != 0,
                MaxParallelChildren = MaxParallelChildren,
                CreateChildren = (b, r) => b.Add<TestModuleWithResult>((b) => CreateChildModule2(b, 99)),
            });

            for (int i = 0; i < 3; i++)
                builder.Add<TestModuleWithResult>((b) => CreateChildModule2(b, i));

            return builder;
        }

        private IStageBuilder CreateChildModule2(StageBuilder<TestModuleWithResult> builder, int index)
        {
            return builder.Configure((b) => new(b)
            {
                Name = Name + " " + index,
                IsEnabled = true,
                MaxParallelChildren = MaxParallelChildren
            });
        }
    }
}
