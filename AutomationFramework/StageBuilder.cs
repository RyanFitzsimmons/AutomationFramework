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
        public StageBuilder(IDataLayer dataLayer, IRunInfo runInfo, StagePath path)
        {
            DataLayer = dataLayer;
            RunInfo = runInfo;
            Path = path;
        }

        private IDataLayer DataLayer { get; }
        private IRunInfo RunInfo { get; }
        private StagePath Path { get; }
        private TModule Module { get; set; }
        private int ChildIndex { get; set; }
        private List<IStageBuilder> Builders { get; set; } = new List<IStageBuilder>();

        public StageBuilder<TModule> Configure(Func<IDataLayer, IRunInfo, StagePath, TModule> configureDelegate)
        {            
            Module = configureDelegate.Invoke(DataLayer, RunInfo, Path);
            return this;
        }

        public StageBuilder<TModule> ForEach<TType>(IEnumerable<TType> collection, Action<StageBuilder<TModule>, TType> foreachDelegate)
        {
            foreach (var item in collection)
                foreachDelegate.Invoke(this, item);
            return this;
        }

        public StageBuilder<TModule> Add<TChildModule>(Action<StageBuilder<TChildModule>> builderDelegate) where TChildModule : IModule
        {
            var builder = new StageBuilder<TChildModule>(DataLayer, RunInfo, Path.CreateChild(++ChildIndex));
            builderDelegate.Invoke(builder);
            Builders.Add(builder);
            return this;
        }

        public ConcurrentDictionary<StagePath, IModule> Build()
        {
            var stages = new ConcurrentDictionary<StagePath, IModule>();
            if (Module != null)
            {
                stages.TryAdd(Path, Module);
                foreach (var builder in Builders)
                    foreach (var pair in builder.Build())
                        stages.TryAdd(pair.Key, pair.Value);
            }
            return stages;
        }

        public IModule[] BuildToArray()
        {
            List<IModule> modules = new ();
            if (Module != null)
            {
                modules.Add(Module);
                foreach (var builder in Builders)
                    modules.AddRange(builder.BuildToArray());
            }
            return modules.ToArray();
        }
    }
}
