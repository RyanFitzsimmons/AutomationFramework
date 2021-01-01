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
        public KernelBase(ILogger logger = null)
        {
            Logger = logger;
            DataLayer = CreateDataLayer();
        }

        private readonly CancellationTokenSource CancellationSource = new CancellationTokenSource();

        protected ILogger Logger { get; }
        public abstract string Version { get; }
        public abstract string Name { get; }
        private bool HasRunBeenCalled { get; set; }
        private ConcurrentDictionary<StagePath, IModule> Stages { get; set; } 

        protected TDataLayer DataLayer { get; }

        /// <summary>
        /// Stores the meta data for GetMetaData method
        /// </summary>
        private IMetaData MetaData { get; set; }

        protected abstract TDataLayer CreateDataLayer();

        /// <summary>
        /// This method should only be used when configuring the stage builder
        /// </summary>
        /// <typeparam name="TMetaData"></typeparam>
        /// <returns>Meta data</returns>
        protected TMetaData GetMetaData<TMetaData>() where TMetaData : class, IMetaData => 
            MetaData as TMetaData;

        protected StageBuilder<TModule> GetStageBuilder<TModule>(IRunInfo runInfo) where TModule : IModule => 
            new StageBuilder<TModule>(DataLayer, runInfo, StagePath.Root);

        public void Run(IRunInfo runInfo, IMetaData metaData)
        {
            try
            {
                Logger?.Write(LogLevels.Information, $"{Name} Started");
                runInfo = Initialize(runInfo, metaData);
                BuildStages(runInfo);
                if (runInfo.Type != RunType.Build)
                    RunStage(StagePath.Root, GetStage(StagePath.Root));
                Logger?.Write(LogLevels.Information, $"{Name} Finished");
            }
            catch (OperationCanceledException)
            {
                Logger?.Write(LogLevels.Warning, $"{Name} Canceled");
            }
            catch (Exception ex)
            {
                Logger?.Write(LogLevels.Fatal, $"{Name} threw an exception");
                Logger?.Write(LogLevels.Fatal, ex);
            }
        }

        private void BuildStages(IRunInfo runInfo)
        {
            var builder = Configure(runInfo);
            Stages = builder.Build();
            foreach (var stage in Stages.OrderBy(x => x.Key).Select(x => x.Value))
            {
                var metaData = DataLayer.GetMetaData(runInfo);
                PreStageBuild(stage, metaData);
                stage.OnLog += Stage_OnLog;
                (stage as ModuleBase).Build();
                PostStageBuild(stage, metaData);
            }
        }

        private void Stage_OnLog(IModule stage, LogLevels level, object message) => 
            Logger?.Write(level, stage.Path, message);

        protected virtual void PreStageBuild(IModule stage, IMetaData metaData) { }

        protected virtual void PostStageBuild(IModule stage, IMetaData metaData) { }

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

        private void RunStage(StagePath path, IModule stage)
        {
            try
            {
                (stage as ModuleBase).Run();
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
                (stage as ModuleBase).InvokeConfigureChild(child);

                if (stage.MaxParallelChildren == 1)
                {
                    RunStage(childPath, child);
                }
                else
                {
                    tasks.Add(RunStageInParallel(childPath.Clone(), child));
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
        private IRunInfo Initialize(IRunInfo runInfo, IMetaData metaData)
        {
            if (HasRunBeenCalled) throw new Exception("A job instance can only be run once");
            HasRunBeenCalled = true;

            ValidateRunInfo(runInfo);
            MetaData = metaData;
            runInfo = DataLayer.GetJobId(this, runInfo);
            runInfo = DataLayer.CreateRequest(runInfo, metaData);
            return runInfo;
        }

        /// <summary>
        /// Creates the stages - Called at the start of the run method
        /// </summary>
        /// <returns>The root stage builder</returns>
        protected abstract IStageBuilder Configure(IRunInfo runInfo);

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
