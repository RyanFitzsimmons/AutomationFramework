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
        private readonly CancellationTokenSource _cancellationSource = new CancellationTokenSource();

        public KernelBase(TDataLayer dataLayer, ILogger logger = null)
        {
            DataLayer = dataLayer;
            Logger = logger;
        }

        public abstract string Version { get; }
        public abstract string Name { get; }
        protected ILogger Logger { get; }
        protected TDataLayer DataLayer { get; }
        private IMetaData MetaData { get; set; }
        private IRunInfo RunInfo { get; set; }
        private bool HasRunBeenCalled { get; set; }
        private ConcurrentDictionary<StagePath, IModule> Stages { get; set; } 
            = new ConcurrentDictionary<StagePath, IModule>();               

        protected TMetaData GetMetaData<TMetaData>() where TMetaData : class, IMetaData =>
            MetaData as TMetaData;

        protected RunInfo<TId> GetRunInfo<TId>() => RunInfo as RunInfo<TId>;

        protected StageBuilder<TModule> GetStageBuilder<TModule>() where TModule : IModule => 
            new StageBuilder<TModule>(DataLayer, RunInfo, StagePath.Root);

        public async Task Run(IRunInfo runInfo, IMetaData metaData)
        {
            try
            {
                Logger?.Write(LogLevels.Information, $"{Name} Started");
                MetaData = metaData;
                RunInfo = await Initialize(runInfo);
                await BuildInitialStages();
                if (runInfo.Type != RunType.Build)
                    await RunStage(StagePath.Root, GetStage(StagePath.Root));
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
            finally
            {
                await RunFinally();
            }
        }

        protected virtual async Task RunFinally() => await Task.CompletedTask;

        private async Task BuildInitialStages()
        {
            Logger?.Write(LogLevels.Information, "Building initial stages");
            var builder = Configure();
            foreach (var stage in builder.Build())
            {
                if (!Stages.TryAdd(stage.StagePath, stage))
                    throw new Exception($"Failed to add stage to the concurrent dictionary {stage.StagePath}");
                await BuildStage(stage);
            }
        }

        private async Task BuildStage(IModule stage)
        {
            try
            {
                await PreStageBuild(stage);
                stage.OnLog += Stage_OnLog;
                await stage.Build();
                await PostStageBuild(stage);
            }
            catch (Exception ex)
            {
                Logger?.Write(LogLevels.Error, stage.StagePath, $"Stage {stage} faulted during build: {ex}");
            }
        }

        private void Stage_OnLog(IModule stage, LogLevels level, object message) => 
            Logger?.Write(level, stage.StagePath, message);

        protected virtual async Task PreStageBuild(IModule stage) => await Task.CompletedTask;

        protected virtual async Task PostStageBuild(IModule stage) => await Task.CompletedTask;

        private CancellationToken GetCancellationToken() => _cancellationSource.Token;

        public void Cancel() => Cancel(StagePath.Root);

        public void Cancel(StagePath path)
        {
            try
            {
                if (path == StagePath.Root)
                    _cancellationSource.Cancel();

                foreach (var pathToCancel in GetPathAndDescendantsOf(path))
                {
                    if (Stages.TryGetValue(pathToCancel, out IModule stage))
                        stage.Cancel();
                    else throw new Exception($"Unable to find stage to cancel {pathToCancel}");
                }
            }
            catch (Exception ex)
            {
                Logger?.Write(LogLevels.Error, "Error during cancellation");
                Logger?.Write(LogLevels.Error, ex);
            }
        }

        private StagePath[] GetPathAndDescendantsOf(StagePath path) => 
            Stages.Where(x => x.Key == path || x.Key.IsDescendantOf(path)).Select(x => x.Key).OrderBy(x => x).ToArray();

        private async Task InvokeCreateChildren(IModule stage)
        {
            try
            {
                foreach (var child in await stage.InvokeCreateChildren())
                {
                    if (!Stages.TryAdd(child.StagePath, child))
                        throw new Exception($"Failed to add stage to the concurrent dictionary {child.StagePath}");
                    await BuildStage(child);
                }
            }
            catch (OperationCanceledException)
            {
                Logger?.Write(LogLevels.Warning, stage.StagePath, $"Stage {stage} was cancelled, unable to create children");
            }
            catch (Exception ex)
            {
                Logger?.Write(LogLevels.Error, stage.StagePath, $"Stage {stage} faulted during invoke create children: {ex}");
            }
        }

        private async Task RunStage(StagePath path, IModule stage)
        {
            try
            {
                await stage.Run();
                await InvokeCreateChildren(stage);
                await RunChildren(path, stage);
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

        private IModule GetStage(StagePath path)
        {
            if (!Stages.TryGetValue(path, out IModule stage))
                throw new Exception($"Unable to find stage {path}");
            return stage;
        }

        private StagePath[] GetChildPaths(StagePath path) => 
            Stages.Where(x => path.IsParentOf(x.Key)).Select(x => x.Key).OrderBy(x => x).ToArray();

        private async Task RunChildren(StagePath path, IModule stage)
        {
            List<Task> tasks = new List<Task>();
            if (!stage.IsEnabled) return;
            foreach (var childPath in GetChildPaths(path))
            {
                var child = GetStage(childPath);
                tasks.Add(RunStage(childPath, child));
                if (stage.MaxParallelChildren > 0 && tasks.Count == stage.MaxParallelChildren)
                    tasks.Remove(await Task.WhenAny(tasks.ToArray()));
            }

            await Task.WhenAll(tasks.ToArray());
        }

        /// <summary>
        /// Takes the run info, validates it and returns it with a job and request id if none existed.
        /// </summary>
        /// <param name="runInfo"></param>
        /// <returns>The updated run info</returns>
        private async Task<IRunInfo> Initialize(IRunInfo runInfo)
        {
            if (HasRunBeenCalled) throw new Exception("A job instance can only be run once");
            HasRunBeenCalled = true;
            ValidateRunInfo(runInfo);
            if (DataLayer.GetIsNewJob(runInfo))
                runInfo = await DataLayer.CreateJob(this, runInfo, GetCancellationToken());
            else await DataLayer.ValidateExistingJob(runInfo, Version, GetCancellationToken());
            runInfo = await DataLayer.CreateRequest(runInfo, MetaData, GetCancellationToken());
            return runInfo;
        }

        /// <summary>
        /// Creates the stages - Called at the start of the run method
        /// </summary>
        /// <returns>The root stage builder</returns>
        protected abstract IStageBuilder Configure();

        /// <summary>
        /// Performs validation on the RunInfo
        /// if validation fails an Exception is thrown.
        /// </summary>
        protected virtual void ValidateRunInfo(IRunInfo runInfo)
        {
            if (!runInfo.GetIsValid(out string exMsg))
                throw new Exception(exMsg);
        }
    }
}
