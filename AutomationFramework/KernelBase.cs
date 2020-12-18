using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AutomationFramework
{
    public abstract class KernelBase<TDataLayer> : IKernel where TDataLayer : IKernelDataLayer
    {
        public KernelBase(ILogger logger = null)
        {
            Logger = logger;
            DataLayer = Activator.CreateInstance<TDataLayer>();
        }

        private readonly CancellationTokenSource CancellationSource = new CancellationTokenSource();

        protected ILogger Logger { get; }
        public abstract string Version { get; }
        public abstract string Name { get; }
        private bool HasRunBeenCalled { get; set; }
        private ConcurrentDictionary<StagePath, IModule> Stages { get; set; } 

        private TDataLayer DataLayer { get; }

        /// <summary>
        /// Stores the meta data for GetMetaData method
        /// </summary>
        private IMetaData MetaData { get; set; }

        /// <summary>
        /// This method should only be used when configuring the stage builder
        /// </summary>
        /// <typeparam name="TMetaData"></typeparam>
        /// <returns>Meta data</returns>
        protected TMetaData GetMetaData<TMetaData>() where TMetaData : class, IMetaData
        {
            return MetaData as TMetaData;
        }

        protected StageBuilder<TModule> GetStageBuilder<TModule>() where TModule : IModule
        {
            return new StageBuilder<TModule>();
        }

        public void Run(IRunInfo runInfo, IMetaData metaData)
        {
            try
            {
                Logger?.Information($"{Name} Started");
                runInfo = Initialize(runInfo, metaData);
                BuildStages();
                RunStage(runInfo, StagePath.Root, metaData, GetStage(StagePath.Root));
                Logger?.Information($"{Name} Finished");
            }
            catch (OperationCanceledException)
            {
                Logger?.Warning($"{Name} Canceled");
            }
            catch (Exception)
            {
                Logger?.Error($"{Name} threw an exception");
            }
        }

        private void BuildStages()
        {
            var builder = Configure();
            Stages = builder.Build(StagePath.Root);
        }

        public void Cancel()
        {
            Cancel(StagePath.Root);
        }

        public void Cancel(StagePath path)
        {
            CancellationSource.Cancel();
            foreach(var pathToCancel in GetPathAndDescendantsOf(path))
            {
                Stages.TryGetValue(pathToCancel, out IModule stage);
                stage.Cancel();
            }
        }

        private StagePath[] GetPathAndDescendantsOf(StagePath path)
        {
            return Stages.Where(x => x.Key == path || x.Key.IsDescendantOf(path)).Select(x => x.Key).ToArray();
        }

        private void RunStage(IRunInfo runInfo, StagePath path, IMetaData metaData, IModule stage)
        {
            try
            {
                stage.Run(runInfo, path, metaData, Logger);
                RunChildren(runInfo, path, metaData, stage);
            }
            catch (OperationCanceledException)
            {
                Logger?.Warning(path, $"Stage {stage} was cancelled");
            }
            catch (Exception ex)
            {
                Logger?.Error(path, $"Stage {stage} faulted: {ex}");
            }
        }

        private Task RunStageInParallel(IRunInfo runInfo, StagePath path, IMetaData metaData, IModule stage)
        {
            return Task.Factory.StartNew(() =>
                {
                    stage.Run(runInfo, path, metaData, Logger);
                }, stage.GetCancellationToken())
                .ContinueWith((t) =>
                {
                    switch (t.Status)
                    {
                        case TaskStatus.RanToCompletion:
                            RunChildren(runInfo, path, metaData, stage);
                            break;
                        case TaskStatus.Canceled:
                            Logger?.Warning(path, $"Stage {stage} was cancelled");
                            break;
                        case TaskStatus.Faulted:
                            Logger?.Error(path, $"Stage {stage} faulted: {t.Exception}");
                            break;
                        default:
                            Logger?.Fatal(path, $"Something unexpected happened with stage {stage}");
                            break;
                    }
                });
        }

        private IModule GetStage(StagePath path)
        {
            Stages.TryGetValue(path, out IModule stage);
            return stage;
        }

        private StagePath[] GetChildPaths(StagePath path)
        {
            return Stages.Where(x => path.IsParentOf(x.Key)).Select(x => x.Key).OrderBy(x => x).ToArray();
        }

        private void RunChildren(IRunInfo runInfo, StagePath path, IMetaData metaData, IModule stage)
        {
            List<Task> tasks = new List<Task>();
            foreach (var childPath in GetChildPaths(path))
            {
                var child = GetStage(childPath);
                stage.InvokeConfigureChild(child);

                if (stage.MaxParallelChildren == 1)
                {
                    RunStage(runInfo, childPath, metaData, child);
                }
                else
                {
                    tasks.Add(RunStageInParallel(runInfo.Clone(), childPath.Clone(), DataLayer.GetMetaData(runInfo), child));
                    if (stage.MaxParallelChildren > 0 && tasks.Count == stage.MaxParallelChildren)
                        tasks.RemoveAt(Task.WaitAny(tasks.ToArray()));
                }
            }

            Task.WaitAll(tasks.ToArray());
        }

        /// <summary>
        /// Takes the run info, validates it and returns it with a job id if none existed.
        /// </summary>
        /// <param name="runInfo"></param>
        /// <returns>The updated run info</returns>
        private IRunInfo Initialize(IRunInfo runInfo, IMetaData metaData)
        {
            try
            {
                if (HasRunBeenCalled) throw new Exception("A job instance can only be run once");
                HasRunBeenCalled = true;

                ValidateRunInfo(runInfo);
                MetaData = metaData;
                runInfo = DataLayer.GetJobId(this, runInfo);
                runInfo = DataLayer.CreateRequest(runInfo, metaData);
                return runInfo;
            }
            catch (Exception ex)
            {
                Logger?.Error($"Failed to initialize job: {ex}");
                throw;
            }
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
