using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace AutomationFramework.UnitTests.TestSetup
{
    public class TestDataLayer : IDataLayer
    {
        public ConcurrentBag<IModule> TestModules { get; private set; } = new ConcurrentBag<IModule>();

        public bool GetIsNewJob(IRunInfo runInfo)
        {
            return string.IsNullOrWhiteSpace((runInfo as RunInfo<string>).JobId);
        }

        public async Task<IRunInfo> CreateJob(IKernel kernel, IRunInfo runInfo, CancellationToken cancellationToken)
        {
            return await Task.FromResult(new RunInfo<string>(runInfo.Type, "Test Job ID", (runInfo as RunInfo<string>).RequestId, runInfo.Path));
        }

        public async Task ValidateExistingJob(IRunInfo runInfo, string version, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }

        public async Task<IRunInfo> CreateRequest(IRunInfo runInfo, IMetaData metaData, CancellationToken cancellationToken)
        {
            return await Task.FromResult(new RunInfo<string>(runInfo.Type, (runInfo as RunInfo<string>).JobId, "Test Request ID", runInfo.Path));
        }

        public async Task CreateStage(IModule module, CancellationToken cancellationToken)
        {
            TestModules.Add(module);
            string action = "Create Stage";
            (module as TestModuleWithResult).Actions.Add(action);
            await Task.CompletedTask;
        }

        public async Task<TResult> GetCurrentResult<TResult>(IModule module, CancellationToken cancellationToken) where TResult : class
        {
            string action = "Get Current Result";
            (module as TestModuleWithResult).Actions.Add(action);
            return await Task.FromResult(Activator.CreateInstance<TResult>());
        }

        public async Task<TResult> GetPreviousResult<TResult>(IModule module, CancellationToken cancellationToken) where TResult : class
        {
            string action = "Get Existing Result";
            (module as TestModuleWithResult).Actions.Add(action);
            return await Task.FromResult(Activator.CreateInstance<TResult>());
        }

        public async Task SaveResult<TResult>(IModule module, TResult result, CancellationToken cancellationToken) where TResult : class
        {
            string action = "Save Result";
            (module as TestModuleWithResult).Actions.Add(action);
            await Task.CompletedTask;
        }

        public async Task SetStatus(IModule module, StageStatuses status, CancellationToken cancellationToken)
        {
            string action = "Set Status " + status;
            (module as TestModuleWithResult).Actions.Add(action);
            await Task.CompletedTask;
        }
    }
}
