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

        private readonly object metaDataLock = new object();
        private readonly CancellationTokenSource CancellationSource = new CancellationTokenSource();

        protected ILogger Logger { get; }
        public abstract string Version { get; }
        public abstract string Name { get; }
        private bool HasRunBeenCalled { get; set; }
        private Func<object> GetMetaDataFunc { get; set; }
        private ConcurrentDictionary<StagePath, IModule> Stages { get; set; } 

        protected TDataLayer DataLayer { get; }

        protected TMetaData GetMetaData<TMetaData>() where TMetaData : class 
        {
            try
            {
                lock(metaDataLock)
                    return GetMetaDataFunc?.Invoke() as TMetaData;
            }
            catch (Exception ex)
            {
                var message = "Failed to retrieve meta data";
                Logger?.Error(message);
                throw new Exception(message, ex);
            }
        }

        public StageBuilder<TModule> GetStageBuilder<TModule>() where TModule : IModule
        {
            return new StageBuilder<TModule>();
        }

        public void Run(IRunInfo runInfo, Func<object> getMetaData = null)
        {
            try
            {
                Logger?.Information($"{Name} Started");
                GetMetaDataFunc = getMetaData;
                runInfo = Initialize(runInfo);
                BuildStages();
                RunStage(runInfo.Clone(), StagePath.Root, GetStage(StagePath.Root));
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

        private void RunStage(IRunInfo runInfo, StagePath path, IModule stage)
        {
            try
            {
                stage.Run(runInfo, path, GetMetaData<object>(), Logger);
                RunChildren(runInfo, path, stage);
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

        private Task RunStageInParallel(IRunInfo runInfo, StagePath path, object metaData, IModule stage)
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
                            RunChildren(runInfo, path, stage);
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

        private void RunChildren(IRunInfo runInfo, StagePath path, IModule stage)
        {
            List<Task> tasks = new List<Task>();
            foreach (var childPath in GetChildPaths(path))
            {
                var child = GetStage(childPath);
                stage.InvokeConfigureChild(child);

                if (stage.MaxParallelChildren == 1)
                {
                    RunStage(runInfo, childPath, child);
                }
                else
                {
                    tasks.Add(RunStageInParallel(runInfo.Clone(), childPath.Clone(), GetMetaData<object>(), child));
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
        private IRunInfo Initialize(IRunInfo runInfo)
        {
            try
            {
                if (HasRunBeenCalled) throw new Exception("A job instance can only be run once");
                HasRunBeenCalled = true;

                ValidateRunInfo(runInfo);
                runInfo = DataLayer.GetJobId(this, runInfo);
                //if (DataLayer.GetIsEmptyId()) throw new Exception("No job Id exists");
                runInfo = DataLayer.CreateRequest(runInfo, GetMetaDataFunc?.Invoke());
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
