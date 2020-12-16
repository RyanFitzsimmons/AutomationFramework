using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationFramework
{
    public class StageBuilder<TModule> : IStageBuilder where TModule : IModule
    {
        private TModule Module { get; set; } = Activator.CreateInstance<TModule>();

        private List<IStageBuilder> Builders { get; set; } = new List<IStageBuilder>();

        public StageBuilder<TModule> Configure(Action<TModule> configureDelegate)
        {
            configureDelegate.Invoke(Module);
            return this;
        }

        public StageBuilder<TModule> Add<TChildModule>(Action<StageBuilder<TChildModule>> builderDelegate) where TChildModule : IModule
        {
            var builder = new StageBuilder<TChildModule>();
            builderDelegate.Invoke(builder);
            Builders.Add(builder);
            return this;
        }

        public ConcurrentDictionary<StagePath, IModule> Build(StagePath path)
        {
            var stages = new ConcurrentDictionary<StagePath, IModule>();
            stages.TryAdd(path, Module);
            int childIndex = 0;
            foreach (var builder in Builders)
                foreach (var pair in builder.Build(path.CreateChild(++childIndex)))
                    stages.TryAdd(pair.Key, pair.Value);
            return stages;
        }

        public IModule[] BuildToArray(StagePath path)
        {
            List<IModule> modules = new (){ Module };
            int childIndex = 0;
            foreach (var builder in Builders)
                modules.AddRange(builder.BuildToArray(path.CreateChild(++childIndex)));
            return modules.ToArray();
        }
    }
}
