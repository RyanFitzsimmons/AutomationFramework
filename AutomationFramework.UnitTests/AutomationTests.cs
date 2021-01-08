using AutomationFramework.UnitTests.TestSetup;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace AutomationFramework.UnitTests
{
    public class AutomationTests
    {
        public AutomationTests(ITestOutputHelper output)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Xunit(output)
                .CreateLogger();
        }

        [Fact]
        public void Run()
        {
            var dataLayer = new TestDataLayer();
            var job = new TestKernel(1, dataLayer, new TestLogger());
            job.Run(RunInfo<string>.Empty, new TestMetaData());
            RunResults(dataLayer);
        }

        [Fact]
        public void RunParallel()
        {
            var dataLayer = new TestDataLayer();
            var job = new TestKernel(3, dataLayer, new TestLogger());
            job.Run(RunInfo<string>.Empty, new TestMetaData());
            RunResults(dataLayer);
        }

        [Fact]
        public void RunSingle()
        {
            var dataLayer = new TestDataLayer();
            var job = new TestKernel(1, dataLayer, new TestLogger());
            job.Run(new RunInfo<string>(RunType.Single, "Test", "Test", new StagePath(1, 2)), new TestMetaData());
            RunSingleResults(dataLayer);
        }

        [Fact]
        public void RunSingleParallel()
        {
            var dataLayer = new TestDataLayer();
            var job = new TestKernel(3, dataLayer, new TestLogger());
            job.Run(new RunInfo<string>(RunType.Single, "Test", "Test", new StagePath(1, 2)), new TestMetaData());
            RunSingleResults(dataLayer);
        }

        [Fact]
        public void RunFrom()
        {
            var dataLayer = new TestDataLayer();
            var job = new TestKernel(1, dataLayer, new TestLogger());
            job.Run(new RunInfo<string>(RunType.From, "Test", "Test", new StagePath(1, 2)), new TestMetaData());
            RunFromResults(dataLayer);
        }

        [Fact]
        public void RunFromParallel()
        {
            var dataLayer = new TestDataLayer();
            var job = new TestKernel(3, dataLayer, new TestLogger());
            job.Run(new RunInfo<string>(RunType.From, "Test", "Test", new StagePath(1, 2)), new TestMetaData());
            RunFromResults(dataLayer);
        }

        private static void RunSingleResults(TestDataLayer dataLayer)
        {
            Assert.Equal(16, dataLayer.TestModules.Count);

            foreach (TestModuleWithResult module in dataLayer.TestModules)
            {
                switch (module.StagePath.ToString())
                {
                    case "1-2":
                        for (int j = 0; j < module.Actions.Count; j++)
                        {
                            var action = module.Actions[j];
                            switch (j)
                            {
                                case 0:
                                    Assert.Equal("Create Stage", action);
                                    break;
                                case 1:
                                    Assert.Equal("Set Status Running", action);
                                    break;
                                case 2:
                                    Assert.Equal("Doing Work", action);
                                    break;
                                case 3:
                                    Assert.Equal("Save Result", action);
                                    break;
                                case 4:
                                    Assert.Equal("Set Status Completed", action);
                                    break;
                                case 5:
                                case 6:
                                case 7:
                                    if (dataLayer.TestModules.Any(x => x.StagePath.IsChildOf(module.StagePath)))
                                        Assert.Equal("Get Current Result", action);
                                    break;
                                default:
                                    throw new Exception("No test implemented");
                            }
                        }
                        break;
                    case "1-2-1":
                    case "1-2-2":
                    case "1-2-3":
                    case "1-2-4":
                    case "1-3-1":
                    case "1-3-2":
                    case "1-3-3":
                    case "1-3-4":
                        for (int j = 0; j < module.Actions.Count; j++)
                        {
                            var action = module.Actions[j];
                            switch (j)
                            {
                                case 0:
                                    Assert.Equal("Create Stage", action);
                                    break;
                                case 1:
                                    Assert.Equal("Set Status Bypassed", action);
                                    break;
                                case 2:
                                case 3:
                                case 4:
                                    if (dataLayer.TestModules.Any(x => x.StagePath.IsChildOf(module.StagePath)))
                                        Assert.Equal("Get Current Result", action);
                                    break;
                                default:
                                    throw new Exception("No test implemented");
                            }
                        }
                        break;
                    case "1":
                    case "1-3":
                        for (int j = 0; j < module.Actions.Count; j++)
                        {
                            var action = module.Actions[j];
                            switch (j)
                            {
                                case 0:
                                    Assert.Equal("Create Stage", action);
                                    break;
                                case 1:
                                    Assert.Equal("Set Status Bypassed", action);
                                    break;
                                case 2:
                                case 3:
                                case 4:
                                    if (dataLayer.TestModules.Any(x => x.StagePath.IsChildOf(module.StagePath)))
                                        Assert.Equal("Get Existing Result", action);
                                    break;
                                default:
                                    throw new Exception("No test implemented");
                            }
                        }
                        break;
                    case "1-1-1":
                    case "1-1-2":
                    case "1-1-3":
                    case "1-1-4":
                        for (int j = 0; j < module.Actions.Count; j++)
                        {
                            var action = module.Actions[j];
                            switch (j)
                            {
                                case 0:
                                    Assert.Equal("Create Stage", action);
                                    break;
                                case 1:
                                    Assert.Equal("Set Status Disabled", action);
                                    break;
                                case 2:
                                case 3:
                                case 4:
                                    if (dataLayer.TestModules.Any(x => x.StagePath.IsChildOf(module.StagePath)))
                                        Assert.Equal("Get Current Result", action);
                                    break;
                                default:
                                    throw new Exception("No test implemented");
                            }
                        }
                        break;
                    case "1-1":
                        for (int j = 0; j < module.Actions.Count; j++)
                        {
                            var action = module.Actions[j];
                            switch (j)
                            {
                                case 0:
                                    Assert.Equal("Create Stage", action);
                                    break;
                                case 1:
                                    Assert.Equal("Set Status Disabled", action);
                                    break;
                                case 2:
                                case 3:
                                case 4:
                                    if (dataLayer.TestModules.Any(x => x.StagePath.IsChildOf(module.StagePath)))
                                        Assert.Equal("Get Existing Result", action);
                                    break;
                                default:
                                    throw new Exception("No test implemented");
                            }
                        }
                        break;
                    default:
                        throw new Exception("Unknown stage");
                }
            }
        }

        private static void RunFromResults(TestDataLayer dataLayer)
        {
            Assert.Equal(16, dataLayer.TestModules.Count);

            foreach (TestModuleWithResult module in dataLayer.TestModules)
            {
                switch (module.StagePath.ToString())
                {
                    case "1-2":
                    case "1-2-1":
                    case "1-2-2":
                    case "1-2-3":
                    case "1-2-4":
                        for (int j = 0; j < module.Actions.Count; j++)
                        { 
                            var action = module.Actions[j];
                            switch (j)
                            {
                                case 0:
                                    Assert.Equal("Create Stage", action);
                                    break;
                                case 1:
                                    Assert.Equal("Set Status Running", action);
                                    break;
                                case 2:
                                    Assert.Equal("Doing Work", action);
                                    break;
                                case 3:
                                    Assert.Equal("Save Result", action);
                                    break;
                                case 4:
                                    Assert.Equal("Set Status Completed", action);
                                    break;
                                case 5:
                                case 6:
                                case 7:
                                    if (dataLayer.TestModules.Any(x => x.StagePath.IsChildOf(module.StagePath)))
                                        Assert.Equal("Get Current Result", action);
                                    break;
                                default:
                                    throw new Exception("No test implemented");
                            }
                        }
                        break;
                    case "1":
                    case "1-3-1":
                    case "1-3-2":
                    case "1-3-3":
                    case "1-3-4":
                        for (int j = 0; j < module.Actions.Count; j++)
                        {
                            var action = module.Actions[j];
                            switch (j)
                            {
                                case 0:
                                    Assert.Equal("Create Stage", action);
                                    break;
                                case 1:
                                    Assert.Equal("Set Status Bypassed", action);
                                    break;
                                case 2:
                                case 3:
                                case 4:
                                    if (dataLayer.TestModules.Any(x => x.StagePath.IsChildOf(module.StagePath)))
                                        Assert.Equal("Get Current Result", action);
                                    break;
                                default:
                                    throw new Exception("No test implemented");
                            }
                        }
                        break;
                    case "1-3":
                        for (int j = 0; j < module.Actions.Count; j++)
                        {
                            var action = module.Actions[j];
                            switch (j)
                            {
                                case 0:
                                    Assert.Equal("Create Stage", action);
                                    break;
                                case 1:
                                    Assert.Equal("Set Status Bypassed", action);
                                    break;
                                case 2:
                                case 3:
                                case 4:
                                    if (dataLayer.TestModules.Any(x => x.StagePath.IsChildOf(module.StagePath)))
                                        Assert.Equal("Get Existing Result", action);
                                    break;
                                default:
                                    throw new Exception("No test implemented");
                            }
                        }
                        break;
                    case "1-1-1":
                    case "1-1-2":
                    case "1-1-3":
                    case "1-1-4":
                        for (int j = 0; j < module.Actions.Count; j++)
                        {
                            var action = module.Actions[j];
                            switch (j)
                            {
                                case 0:
                                    Assert.Equal("Create Stage", action);
                                    break;
                                case 1:
                                    Assert.Equal("Set Status Disabled", action);
                                    break;
                                case 2:
                                case 3:
                                case 4:
                                    if (dataLayer.TestModules.Any(x => x.StagePath.IsChildOf(module.StagePath)))
                                        Assert.Equal("Get Current Result", action);
                                    break;
                                default:
                                    throw new Exception("No test implemented");
                            }
                        }
                        break;
                    case "1-1":
                        for (int j = 0; j < module.Actions.Count; j++)
                        {
                            var action = module.Actions[j];
                            switch (j)
                            {
                                case 0:
                                    Assert.Equal("Create Stage", action);
                                    break;
                                case 1:
                                    Assert.Equal("Set Status Disabled", action);
                                    break;
                                case 2:
                                case 3:
                                case 4:
                                    if (dataLayer.TestModules.Any(x => x.StagePath.IsChildOf(module.StagePath)))
                                        Assert.Equal("Get Existing Result", action);
                                    break;
                                default:
                                    throw new Exception("No test implemented");
                            }
                        }
                        break;
                    default:
                        throw new Exception("Unknown stage");
                }
            }
        }

        private static void RunResults(TestDataLayer dataLayer)
        {
            Assert.Equal(16, dataLayer.TestModules.Count);

            foreach (TestModuleWithResult module in dataLayer.TestModules)
            {
                switch (module.StagePath.ToString())
                {
                    case "1":
                    case "1-2":
                    case "1-3":
                    case "1-2-1":
                    case "1-2-2":
                    case "1-2-3":
                    case "1-2-4":
                    case "1-3-1":
                    case "1-3-2":
                    case "1-3-3":
                    case "1-3-4":
                        for (int j = 0; j < module.Actions.Count; j++)
                        {
                            var action = module.Actions[j];
                            switch (j)
                            {
                                case 0:
                                    Assert.Equal("Create Stage", action);
                                    break;
                                case 1:
                                    Assert.Equal("Set Status Running", action);
                                    break;
                                case 2:
                                    Assert.Equal("Doing Work", action);
                                    break;
                                case 3:
                                    Assert.Equal("Save Result", action);
                                    break;
                                case 4:
                                    Assert.Equal("Set Status Completed", action);
                                    break;
                                case 5:
                                case 6:
                                case 7:
                                    if (dataLayer.TestModules.Any(x => x.StagePath.IsChildOf(module.StagePath)))
                                        Assert.Equal("Get Current Result", action);
                                    break;
                                default:
                                    throw new Exception("No test implemented");
                            }
                        }
                        break;
                    case "1-1":
                    case "1-1-1":
                    case "1-1-2":
                    case "1-1-3":
                    case "1-1-4":
                        for (int j = 0; j < module.Actions.Count; j++)
                        {
                            var action = module.Actions[j];
                            switch (j)
                            {
                                case 0:
                                    Assert.Equal("Create Stage", action);
                                    break;
                                case 1:
                                    Assert.Equal("Set Status Disabled", action);
                                    break;
                                case 2:
                                case 3:
                                case 4:
                                    if (dataLayer.TestModules.Any(x => x.StagePath.IsChildOf(module.StagePath)))
                                        Assert.Equal("Get Current Result", action);
                                    break;
                                default:
                                    throw new Exception("No test implemented");
                            }
                        }
                        break;
                    default:
                        throw new Exception("Unknown stage");
                }
            }
        }
    }
}
