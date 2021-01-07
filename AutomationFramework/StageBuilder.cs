using System;
using System.Collections.Generic;

namespace AutomationFramework
{
    public class StageBuilder<TModule> : IStageBuilder where TModule : IModule
    {
        public StageBuilder(IDataLayer dataLayer, IRunInfo runInfo, StagePath stagePath)
        {
            DataLayer = dataLayer;
            RunInfo = runInfo;
            StagePath = stagePath;
        }

        public IDataLayer DataLayer { get; }
        public IRunInfo RunInfo { get; }
        public StagePath StagePath { get; }
        private int ChildIndex { get; set; }
        private List<IStageBuilder> Builders { get; set; } = new List<IStageBuilder>();
        private Func<IStageBuilder, TModule> ConfigureDelegate { get; set; }
        private bool IsBuilt { get; set; }

        public StageBuilder<TModule> Configure(Func<IStageBuilder, TModule> configureDelegate)
        {
            ConfigureDelegate = configureDelegate;
            return this;
        }

        public StageBuilder<TModule> ForEach<TType>(IEnumerable<TType> collection, Action<StageBuilder<TModule>, TType> foreachDelegate)
        {
            foreach (var item in collection)
                foreachDelegate.Invoke(this, item);
            return this;
        }

        public IStageBuilder Add<TChildModule>(Action<StageBuilder<TChildModule>> builderDelegate) where TChildModule : IModule
        {
            var builder = new StageBuilder<TChildModule>(DataLayer, RunInfo, StagePath.CreateChild(++ChildIndex));
            builderDelegate.Invoke(builder);
            Builders.Add(builder);
            return this;
        }

        public IModule[] Build()
        {
            List<IModule> modules = new ();
            if (ConfigureDelegate != null)
            {
                if (!IsBuilt)
                {
                    modules.Add(ConfigureDelegate.Invoke(this));
                    IsBuilt = true;
                }

                foreach (var builder in Builders)
                    modules.AddRange(builder.Build());
            }
            return modules.ToArray();
        }
    }
}
