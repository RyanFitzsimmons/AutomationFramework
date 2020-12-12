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

        private readonly object CancellationLock = new object();
        private readonly CancellationTokenSource CancellationSource = new CancellationTokenSource();

        protected ILogger Logger { get; }
        public abstract string Version { get; }
        public abstract string Name { get; }
        private bool HasRunBeenCalled { get; set; }
        private Func<object> GetMetaDataFunc { get; set; }
        private ConcurrentDictionary<StagePath, IModule> Stages { get; } = new ConcurrentDictionary<StagePath, IModule>();

        protected TDataLayer DataLayer { get; }

        protected TMetaData GetMetaData<TMetaData>() where TMetaData : class 
        {
            try
            {
                return GetMetaDataFunc?.Invoke() as TMetaData;
            }
            catch (Exception ex)
            {
                var message = "Failed to retrieve meta data";
                Logger?.Error(message);
                throw new Exception(message, ex);
            }
        }

        public void Run(RunInfo runInfo, Func<object> getMetaData = null)
        {
            try
            {
                Logger?.Information($"{Name} Started");
                GetMetaDataFunc = getMetaData;
                Stages.TryAdd(StagePath.Root, CreateStages());
                runInfo = Initialize(runInfo);
                Stages.TryGetValue(StagePath.Root, out IModule rootStage);
                RunStage(runInfo, StagePath.Root, rootStage);
                RunChildren(runInfo, StagePath.Root, rootStage);
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

        public void Cancel()
        {
            Cancel(StagePath.Root);
        }

        public void Cancel(StagePath path)
        {
            lock (CancellationLock)
            {
                CancellationSource.Cancel();
                foreach(var pathToCancel in GetPathAndDescendantsOf(path))
                {
                    Stages.TryGetValue(pathToCancel, out IModule stage);
                    stage.Cancel();
                }
            }
        }

        private StagePath[] GetPathAndDescendantsOf(StagePath path)
        {
            return Stages.Keys.Where(x => x == path || x.IsDescendantOf(path)).ToArray();
        }

        private void RunStage(RunInfo runInfo, StagePath path, IModule stage)
        {
            try
            {
                stage.Run(runInfo, path, GetMetaDataFunc, Logger);
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

        private Task RunStageInParallel(RunInfo runInfo, StagePath path, IModule stage)
        {
            return Task.Factory.StartNew(() => stage.Run(runInfo, path, GetMetaDataFunc, Logger), stage.GetCancellationToken())
                .ContinueWith((t) =>
                {
                    switch (t.Status)
                    {
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

        private StagePath[] CreateChildren(StagePath path, IModule stage)
        {
            int childindex = 0;
            List<StagePath> childPaths = new List<StagePath>();

            lock (CancellationLock)
            {
                if (CancellationSource.Token.IsCancellationRequested)
                    CancellationSource.Token.ThrowIfCancellationRequested();
                foreach (var child in stage.InvokeCreateChildren())
                {
                    var childPath = path.CreateChild(++childindex);
                    Stages.TryAdd(childPath, child);
                    childPaths.Add(childPath);
                }
            }

            return childPaths.ToArray();
        }

        private void RunChildren(RunInfo runInfo, StagePath path, IModule stage)
        {
            List<Task> tasks = new List<Task>();
            foreach (var childPath in CreateChildren(path, stage))
            {
                Stages.TryGetValue(childPath, out IModule child);
                if (stage.MaxParallelChildren == 1)
                {
                    RunStage(runInfo, childPath, child);
                    // Recursively runs the children of the stage that just ran
                    RunChildren(runInfo, childPath, child);
                }
                else
                {
                    tasks.Add(RunStageInParallel(runInfo, childPath, child).ContinueWith(t =>
                    {
                        // Recursively runs the children of the stage that just ran
                        if (t.Status == TaskStatus.RanToCompletion)
                            RunChildren(runInfo, childPath, child);
                    }));
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
        private RunInfo Initialize(RunInfo runInfo)
        {
            try
            {
                if (HasRunBeenCalled) throw new Exception("A job instance can only be run once");
                HasRunBeenCalled = true;

                ValidateRunInfo(runInfo);
                runInfo.JobId = GetJobId(runInfo);
                if (DataLayer.GetIsEmptyId(runInfo.JobId)) throw new Exception("No job Id exists");
                runInfo.RequestId = DataLayer.CreateRequest(runInfo, GetMetaDataFunc?.Invoke());
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
        /// <returns>The root stage module</returns>
        protected abstract IModule CreateStages();

        private object GetJobId(RunInfo runInfo)
        {
            if (DataLayer.GetIsEmptyId(runInfo.JobId))
            {
                return DataLayer.CreateJob(this);
            }
            else
            {
                DataLayer.CheckExistingJob(runInfo.JobId, Version);
                return runInfo.JobId;
            }
        }

        /// <summary>
        /// Performs validation on the RunInfo
        /// if validation fails a RunInfoValidationException is thrown.
        /// This method should only be called from the job thread.
        /// </summary>
        protected virtual void ValidateRunInfo(RunInfo runInfo)
        {
            if (!runInfo.GetIsValid(out string exMsg))
                throw new Exception(exMsg);
        }
    }
}
