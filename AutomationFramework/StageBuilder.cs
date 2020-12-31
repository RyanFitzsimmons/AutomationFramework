﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationFramework
{
    public class StageBuilder<TModule> : IStageBuilder where TModule : IModule
    {
        public StageBuilder(IRunInfo runInfo, StagePath path, Func<IMetaData> getMetaData)
        {
            RunInfo = runInfo;
            Path = path;
            GetMetaData = getMetaData;
        }

        private IRunInfo RunInfo { get; }

        private StagePath Path { get; }

        private Func<IMetaData> GetMetaData { get; }

        private TModule Module { get; set; }

        private int ChildIndex { get; set; }

        private List<IStageBuilder> Builders { get; set; } = new List<IStageBuilder>();

        public StageBuilder<TModule> Configure(Func<IRunInfo, StagePath, IMetaData, TModule> configureDelegate)
        {            
            Module = configureDelegate.Invoke(RunInfo, Path, GetMetaData.Invoke());
            return this;
        }

        public StageBuilder<TModule> Add<TChildModule>(Action<StageBuilder<TChildModule>> builderDelegate) where TChildModule : IModule
        {
            var builder = new StageBuilder<TChildModule>(RunInfo, Path.CreateChild(++ChildIndex), GetMetaData);
            builderDelegate.Invoke(builder);
            Builders.Add(builder);
            return this;
        }

        public ConcurrentDictionary<StagePath, IModule> Build()
        {
            var stages = new ConcurrentDictionary<StagePath, IModule>();
            stages.TryAdd(Path, Module);
            foreach (var builder in Builders)
                foreach (var pair in builder.Build())
                    stages.TryAdd(pair.Key, pair.Value);
            return stages;
        }

        public IModule[] BuildToArray()
        {
            List<IModule> modules = new (){ Module };
            foreach (var builder in Builders)
                modules.AddRange(builder.BuildToArray());
            return modules.ToArray();
        }
    }
}
