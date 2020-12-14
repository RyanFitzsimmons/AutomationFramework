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

        protected override IModule CreateStages()
        {
            var root = new TestModuleWithResult()
            {
                Name = Name + " " + 0,
                IsEnabled = true,
                MaxParallelChildren = MaxParallelChildren
            };
            TestModules.Add(root);
            root.CreateChildren = (m, r) =>
            {
                var children = new List<IModule>();
                for (int i = 0; i < 3; i++)
                    children.Add(CreateChildModule1(i));
                return children;
            };
            return root;
        }

        private IModule CreateChildModule1(int index)
        {
            var child = new TestModuleWithResult()
            {
                Name = Name + " " + index,
                IsEnabled = index != 0,
                MaxParallelChildren = MaxParallelChildren
            };
            TestModules.Add(child);
            child.CreateChildren = (m, r) =>
            {
                var children = new List<IModule>();
                for (int i = 0; i < 3; i++)
                    children.Add(CreateChildModule2(i));
                return children;
            };
            return child;
        }

        private IModule CreateChildModule2(int index)
        {
            var child = new TestModuleWithResult()
            {
                Name = Name + " " + index,
                IsEnabled = true,
                MaxParallelChildren = MaxParallelChildren
            };
            TestModules.Add(child);
            return child;
        }
    }
}
