using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AutomationFramework
{
    public abstract class KernelBase<TDataLayer> : IKernel where TDataLayer : IDataLayer
    {
        private readonly CancellationTokenSource CancellationSource = new CancellationTokenSource();

        public KernelBase(TDataLayer dataLayer, ILogger logger = null)
        {
            DataLayer = dataLayer;
            Logger = logger;
        }

        public abstract string Version { get; }
        public abstract string Name { get; }
        protected ILogger Logger { get; }
        protected TDataLayer DataLayer { get; }

        /// <summary>
        /// Stores the meta data for GetMetaData method
        /// </summary>
        private IMetaData MetaData { get; set; }
        private IRunInfo RunInfo { get; set; }
        private bool HasRunBeenCalled { get; set; }
        private ConcurrentDictionary<StagePath, IModule> Stages { get; set; } 
            = new ConcurrentDictionary<StagePath, IModule>();               

        /// <summary>
        /// This method should only be used when configuring the stage builder
        /// </summary>
        /// <typeparam name="TMetaData"></typeparam>
        /// <returns>Meta data</returns>
        protected TMetaData GetMetaData<TMetaData>() where TMetaData : class, IMetaData =>
            MetaData as TMetaData;

        protected RunInfo<TId> GetRunInfo<TId>() => RunInfo as RunInfo<TId>;

        protected StageBuilder<TModule> GetStageBuilder<TModule>() where TModule : IModule => 
            new StageBuilder<TModule>(DataLayer, RunInfo, StagePath.Root);

        public void Run(IRunInfo runInfo, IMetaData metaData)
        {
            try
            {
                Logger?.Write(LogLevels.Information, $"{Name} Started");
                MetaData = metaData;
                RunInfo = Initialize(runInfo);
                BuildInitialStages();
                if (runInfo.Type != RunType.Build)
                    RunStage(StagePath.Root, GetStage(StagePath.Root));
                Logger?.Write(LogLevels.Information, $"{Name} Finished");
            }
            catch (OperationCanceledException)
            {
                Logger?.Write(LogLevels.Warning, $"{Name} Cancelled");
            }
            catch (Exception ex)
            {
                Logger?.Write(LogLevels.Fatal, $"{Name} threw an exception");
                Logger?.Write(LogLevels.Fatal, ex);
            }
        }

        private void BuildInitialStages()
        {
            Logger?.Write(LogLevels.Information, "Building initial stages");
            var builder = Configure();
            foreach (var stage in builder.Build())
            {
                Stages.TryAdd(stage.StagePath, stage);
                BuildStage(stage);
            }
        }

        private void BuildStage(IModule stage)
        {
            try
            {
                PreStageBuild(stage);
                stage.OnLog += Stage_OnLog;
                (stage as ModuleBase).Build();
                PostStageBuild(stage);
            }
            catch (Exception ex)
            {
                Logger?.Write(LogLevels.Error, stage.StagePath, $"Stage {stage} faulted during build: {ex}");
            }
        }

        private void Stage_OnLog(IModule stage, LogLevels level, object message) => 
            Logger?.Write(level, stage.StagePath, message);

        protected virtual void PreStageBuild(IModule stage) { }

        protected virtual void PostStageBuild(IModule stage) { }

        public CancellationToken GetCancellationToken() => CancellationSource.Token;

        public void Cancel() => Cancel(StagePath.Root);

        public void Cancel(StagePath path)
        {
            try
            {
                CancellationSource.Cancel();
                foreach (var pathToCancel in GetPathAndDescendantsOf(path))
                {
                    Stages.TryGetValue(pathToCancel, out IModule stage);
                    (stage as ModuleBase).Cancel();
                }
            }
            catch (Exception ex)
            {
                Logger?.Write(LogLevels.Fatal, "Error during cancellation");
                Logger?.Write(LogLevels.Fatal, ex);
            }
        }

        private StagePath[] GetPathAndDescendantsOf(StagePath path) => 
            Stages.Where(x => x.Key == path || x.Key.IsDescendantOf(path)).Select(x => x.Key).ToArray();

        private void InvokeCreateChildren(IModule stage)
        {
            try
            {
                foreach (var child in (stage as ModuleBase).InvokeCreateChildren())
                {
                    Stages.TryAdd(child.StagePath, child);
                    BuildStage(child);
                }
            }
            catch (Exception ex)
            {
                Logger?.Write(LogLevels.Error, stage.StagePath, $"Stage {stage} faulted during invoke create children: {ex}");
            }
        }

        private void RunStage(StagePath path, IModule stage)
        {
            try
            {
                (stage as ModuleBase).Run();
                InvokeCreateChildren(stage);
                RunChildren(path, stage);
            }
            catch (OperationCanceledException)
            {
                Logger?.Write(LogLevels.Warning, path, $"Stage {stage} was cancelled");
            }
            catch (Exception ex)
            {
                Logger?.Write(LogLevels.Error, path, $"Stage {stage} faulted: {ex}");
            }
        }

        private Task RunStageInParallel(StagePath path, IModule stage)
        {
            return Task.Factory.StartNew(() =>
                {
                    (stage as ModuleBase).Run();
                }, (stage as ModuleBase).GetCancellationToken())
                .ContinueWith((t) =>
                {
                    switch (t.Status)
                    {
                        case TaskStatus.RanToCompletion:
                            InvokeCreateChildren(stage);
                            RunChildren(path, stage);
                            break;
                        case TaskStatus.Canceled:
                            Logger?.Write(LogLevels.Warning, path, $"Stage {stage} was cancelled");
                            break;
                        case TaskStatus.Faulted:
                            Logger?.Write(LogLevels.Error, path, $"Stage {stage} faulted: {t.Exception}");
                            break;
                        default:
                            Logger?.Write(LogLevels.Error, path, $"Something unexpected happened with stage {stage}: {t.Exception}");
                            break;
                    }
                });
        }

        private IModule GetStage(StagePath path)
        {
            Stages.TryGetValue(path, out IModule stage);
            return stage;
        }

        private StagePath[] GetChildPaths(StagePath path) => 
            Stages.Where(x => path.IsParentOf(x.Key)).Select(x => x.Key).OrderBy(x => x).ToArray();

        private void RunChildren(StagePath path, IModule stage)
        {
            List<Task> tasks = new List<Task>();
            if (!stage.IsEnabled) return;

            foreach (var childPath in GetChildPaths(path))
            {
                var child = GetStage(childPath);

                if (stage.MaxParallelChildren == 1)
                {
                    RunStage(childPath, child);
                }
                else
                {
                    tasks.Add(RunStageInParallel(childPath, child));
                    if (stage.MaxParallelChildren > 0 && tasks.Count == stage.MaxParallelChildren)
                        tasks.RemoveAt(Task.WaitAny(tasks.ToArray()));
                }
            }

            Task.WaitAll(tasks.ToArray());
        }

        /// <summary>
        /// Takes the run info, validates it and returns it with a job and request id if none existed.
        /// </summary>
        /// <param name="runInfo"></param>
        /// <returns>The updated run info</returns>
        private IRunInfo Initialize(IRunInfo runInfo)
        {
            if (HasRunBeenCalled) throw new Exception("A job instance can only be run once");
            HasRunBeenCalled = true;

            ValidateRunInfo(runInfo);
            runInfo = DataLayer.GetJobId(this, runInfo, MetaData);
            runInfo = DataLayer.CreateRequest(runInfo);
            return runInfo;
        }

        /// <summary>
        /// Creates the stages - Called at the start of the run method
        /// </summary>
        /// <returns>The root stage builder</returns>
        protected abstract IStageBuilder Configure();

        /// <summary>
        /// Performs validation on the RunInfo
        /// if validation fails a RunInfoValidationException is thrown.
        /// This method should only be called from the job thread.
        /// </summary>
        protected virtual void ValidateRunInfo(IRunInfo runInfo)
        {
            if (!runInfo.GetIsValid(out string exMsg))
                throw new Exception(exMsg);
        }
    }
}
