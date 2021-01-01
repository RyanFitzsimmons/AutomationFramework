﻿using System;
using System.Collections.Generic;
using System.Text;

namespace AutomationFramework.UnitTests.TestSetup
{
    public class TestKernel : KernelBase<TestDataLayer>
    {
        public TestKernel(int maxParallelChildren, ILogger logger = null) : base(logger)
        {
            MaxParallelChildren = maxParallelChildren;
        }

        public override string Name => "Test Job";

        public override string Version => "";

        private int MaxParallelChildren { get; }

        public List<IModule> TestModules { get; private set; } = new List<IModule>();

        protected override IStageBuilder Configure(IRunInfo runInfo)
        {
            var builder = GetStageBuilder<TestModuleWithResult>(runInfo);
            builder.Configure((dl, ri, sp) => new(dl, ri, sp)
            {
                    Name = Name + " " + 0,
                    IsEnabled = true,
                    MaxParallelChildren = MaxParallelChildren,
                });

            for (int i = 0; i < 3; i++)
                builder.Add<TestModuleWithResult>((b) => CreateChildModule1(b, i));

            TestModules.AddRange(builder.BuildToArray());
            return builder;
        }

        private IStageBuilder CreateChildModule1(StageBuilder<TestModuleWithResult> builder, int index)
        {
            builder.Configure((dl, ri, sp) => new(dl, ri, sp)
            {
                Name = Name + " " + index,
                IsEnabled = index != 0,
                MaxParallelChildren = MaxParallelChildren
            });

            for (int i = 0; i < 3; i++)
                builder.Add<TestModuleWithResult>((b) => CreateChildModule2(b, i));

            return builder;
        }

        private IStageBuilder CreateChildModule2(StageBuilder<TestModuleWithResult> builder, int index)
        {
            return builder.Configure((dl, ri, sp) => new(dl, ri, sp)
            {
                Name = Name + " " + index,
                IsEnabled = true,
                MaxParallelChildren = MaxParallelChildren
            });
        }

        protected override TestDataLayer CreateDataLayer() =>
            new TestDataLayer();
    }
}
