using AutomationFramework.UnitTests.TestSetup;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace AutomationFramework.UnitTests
{
    public class UnitTest1
    {
        public UnitTest1(ITestOutputHelper output)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Xunit(output)
                .CreateLogger();
        }

        [Fact]
        public void Run()
        {
            var job = new TestKernel(1, new TestDataLayer(), new TestLogger());
            job.Run(RunInfo<string>.Empty, new TestMetaData());
            RunResults(job);
        }

        [Fact]
        public void RunParallel()
        {
            var job = new TestKernel(3, new TestDataLayer(), new TestLogger());
            job.Run(RunInfo<string>.Empty, new TestMetaData());
            RunResults(job);
        }

        [Fact]
        public void RunSingle()
        {
            var job = new TestKernel(1, new TestDataLayer(), new TestLogger());
            job.Run(new RunInfo<string>(RunType.Single, "Test", "Test", new StagePath(1, 2)), new TestMetaData());
            RunSingleResults(job);
        }

        [Fact]
        public void RunSingleParallel()
        {
            var job = new TestKernel(3, new TestDataLayer(), new TestLogger());
            job.Run(new RunInfo<string>(RunType.Single, "Test", "Test", new StagePath(1, 2)), new TestMetaData());
            RunSingleResults(job);
        }

        [Fact]
        public void RunFrom()
        {
            var job = new TestKernel(1, new TestDataLayer(), new TestLogger());
            job.Run(new RunInfo<string>(RunType.From, "Test", "Test", new StagePath(1, 2)), new TestMetaData());
            RunFromResults(job);
        }

        [Fact]
        public void RunFromParallel()
        {
            var job = new TestKernel(3, new TestDataLayer(), new TestLogger());
            job.Run(new RunInfo<string>(RunType.From, "Test", "Test", new StagePath(1, 2)), new TestMetaData());
            RunFromResults(job);
        }

        private static void RunSingleResults(TestKernel job)
        {
            Assert.Equal(13, job.TestModules.Count);

            foreach (TestModuleWithResult module in job.TestModules)
            {
                switch (module.Path.ToString())
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
                                    if (job.TestModules.Any(x => x.Path.IsChildOf(module.Path)))
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
                    case "1-3-1":
                    case "1-3-2":
                    case "1-3-3":
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
                                    if (job.TestModules.Any(x => x.Path.IsChildOf(module.Path)))
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
                                    if (job.TestModules.Any(x => x.Path.IsChildOf(module.Path)))
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
                                    if (job.TestModules.Any(x => x.Path.IsChildOf(module.Path)))
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
                                    if (job.TestModules.Any(x => x.Path.IsChildOf(module.Path)))
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

        private static void RunFromResults(TestKernel job)
        {
            Assert.Equal(13, job.TestModules.Count);

            foreach (TestModuleWithResult module in job.TestModules)
            {
                switch (module.Path.ToString())
                {
                    case "1-2":
                    case "1-2-1":
                    case "1-2-2":
                    case "1-2-3":
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
                                    if (job.TestModules.Any(x => x.Path.IsChildOf(module.Path)))
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
                                    if (job.TestModules.Any(x => x.Path.IsChildOf(module.Path)))
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
                                    if (job.TestModules.Any(x => x.Path.IsChildOf(module.Path)))
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
                                    if (job.TestModules.Any(x => x.Path.IsChildOf(module.Path)))
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
                                    if (job.TestModules.Any(x => x.Path.IsChildOf(module.Path)))
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

        private static void RunResults(TestKernel job)
        {
            Assert.Equal(13, job.TestModules.Count);

            foreach (TestModuleWithResult module in job.TestModules)
            {
                switch (module.Path.ToString())
                {
                    case "1":
                    case "1-2":
                    case "1-3":
                    case "1-2-1":
                    case "1-2-2":
                    case "1-2-3":
                    case "1-3-1":
                    case "1-3-2":
                    case "1-3-3":
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
                                    if (job.TestModules.Any(x => x.Path.IsChildOf(module.Path)))
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
                                    if (job.TestModules.Any(x => x.Path.IsChildOf(module.Path)))
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

        [Fact]
        public void GetIsValidTest()
        {
            // valid run infos
            var validInfos = new List<RunInfo<string>>
            {
                new RunInfo<string>(RunType.Standard, null, null, StagePath.Empty),
                new RunInfo<string>(RunType.From, "1", null, StagePath.Root),
                new RunInfo<string>(RunType.Single, "1", null, StagePath.Root)
            };

            // invalid run infos
            var invalidInfos = new List<RunInfo<string>>
            {
                new RunInfo<string>(RunType.Standard, "1", null, StagePath.Empty),
                new RunInfo<string>(RunType.Standard, null, null, StagePath.Root),
                new RunInfo<string>(RunType.Standard, "1", null, StagePath.Root),

                new RunInfo<string>(RunType.From, null, null, StagePath.Root),
                new RunInfo<string>(RunType.From, "1", null, StagePath.Empty),
                new RunInfo<string>(RunType.From, null, null, StagePath.Empty),

                new RunInfo<string>(RunType.Single, null, null, StagePath.Root),
                new RunInfo<string>(RunType.Single, "1", null, StagePath.Empty),
                new RunInfo<string>(RunType.Single, null, null, StagePath.Empty)
            };

            string exMsg;
            foreach (var info in validInfos)
                Assert.True(info.GetIsValid(out exMsg), exMsg);

            foreach (var info in invalidInfos)
                Assert.False(info.GetIsValid(out exMsg), exMsg);
        }

        public enum RelationNames
        {
            Self,
            Ancestor,
            Parent,
            Child,
            Descendant,
        }

        readonly List<Dictionary<RelationNames, StagePath>> Sets = new List<Dictionary<RelationNames, StagePath>>
        {
            new Dictionary<RelationNames, StagePath>
            {
                { RelationNames.Ancestor, StagePath.Root },
                { RelationNames.Child, new StagePath(1, 2, 3, 4) },
                { RelationNames.Descendant, new StagePath(1, 2, 3, 4, 5) },
                { RelationNames.Parent, new StagePath(1, 2) },
                { RelationNames.Self, new StagePath(1, 2, 3) },
            },
            new Dictionary<RelationNames, StagePath>
            {
                { RelationNames.Ancestor, new StagePath(2) },
                { RelationNames.Child, new StagePath(2, 2, 3, 4) },
                { RelationNames.Descendant, new StagePath(2, 2, 3, 4, 5) },
                { RelationNames.Parent, new StagePath(2, 2) },
                { RelationNames.Self, new StagePath(2, 2, 4) },
            },
        };

        [Fact]
        public void IsAncestorOfTest()
        {
            var self = Sets[0][RelationNames.Self];

            Assert.True(Sets[0][RelationNames.Ancestor].IsAncestorOf(self));
            Assert.True(Sets[0][RelationNames.Parent].IsAncestorOf(self));

            Assert.False(self.IsAncestorOf(self));
            Assert.False(Sets[0][RelationNames.Child].IsAncestorOf(self));
            Assert.False(Sets[0][RelationNames.Descendant].IsAncestorOf(self));

            Assert.False(Sets[1][RelationNames.Ancestor].IsAncestorOf(self));
            Assert.False(Sets[1][RelationNames.Parent].IsAncestorOf(self));
            Assert.False(Sets[1][RelationNames.Self].IsAncestorOf(self));
            Assert.False(Sets[1][RelationNames.Child].IsAncestorOf(self));
            Assert.False(Sets[1][RelationNames.Descendant].IsAncestorOf(self));
        }

        [Fact]
        public void IsDescendantOfTest()
        {
            var self = Sets[0][RelationNames.Self];

            Assert.True(Sets[0][RelationNames.Child].IsDescendantOf(self));
            Assert.True(Sets[0][RelationNames.Descendant].IsDescendantOf(self));

            Assert.False(Sets[0][RelationNames.Ancestor].IsDescendantOf(self));
            Assert.False(Sets[0][RelationNames.Parent].IsDescendantOf(self));
            Assert.False(self.IsAncestorOf(self));

            Assert.False(Sets[1][RelationNames.Ancestor].IsDescendantOf(self));
            Assert.False(Sets[1][RelationNames.Parent].IsDescendantOf(self));
            Assert.False(Sets[1][RelationNames.Self].IsDescendantOf(self));
            Assert.False(Sets[1][RelationNames.Child].IsDescendantOf(self));
            Assert.False(Sets[1][RelationNames.Descendant].IsDescendantOf(self));
        }

        [Fact]
        public void IsChildOfTest()
        {
            var self = Sets[0][RelationNames.Self];

            Assert.True(Sets[0][RelationNames.Child].IsChildOf(self));

            Assert.False(Sets[0][RelationNames.Descendant].IsChildOf(self));
            Assert.False(Sets[0][RelationNames.Ancestor].IsChildOf(self));
            Assert.False(Sets[0][RelationNames.Parent].IsChildOf(self));
            Assert.False(self.IsChildOf(self));

            Assert.False(Sets[1][RelationNames.Ancestor].IsChildOf(self));
            Assert.False(Sets[1][RelationNames.Parent].IsChildOf(self));
            Assert.False(Sets[1][RelationNames.Self].IsChildOf(self));
            Assert.False(Sets[1][RelationNames.Child].IsChildOf(self));
            Assert.False(Sets[1][RelationNames.Descendant].IsChildOf(self));
        }

        [Fact]
        public void IsParentOfTest()
        {
            var self = Sets[0][RelationNames.Self];

            Assert.True(Sets[0][RelationNames.Parent].IsParentOf(self));

            Assert.False(Sets[0][RelationNames.Child].IsParentOf(self));
            Assert.False(Sets[0][RelationNames.Descendant].IsParentOf(self));
            Assert.False(Sets[0][RelationNames.Ancestor].IsParentOf(self));
            Assert.False(self.IsParentOf(self));

            Assert.False(Sets[1][RelationNames.Ancestor].IsParentOf(self));
            Assert.False(Sets[1][RelationNames.Parent].IsParentOf(self));
            Assert.False(Sets[1][RelationNames.Self].IsParentOf(self));
            Assert.False(Sets[1][RelationNames.Child].IsParentOf(self));
            Assert.False(Sets[1][RelationNames.Descendant].IsParentOf(self));
        }

        [Fact]
        public void EqualsTest()
        {
            var self = Sets[0][RelationNames.Self];
            var self2 = new StagePath(1, 2, 3);
            var self3 = Sets[1][RelationNames.Self];

            Assert.True(self.Equals(self));
            Assert.True(self.Equals(self2));
            Assert.False(self.Equals(self3));

            Assert.True(self == self2);
            Assert.False(self != self2);
            Assert.False(self == self3);
            Assert.True(self != self3);

            Assert.Equal(self, self);
            Assert.Equal(self, self2);
            Assert.NotEqual(self, self3);

            Assert.Equal(self.GetHashCode(), self.GetHashCode());
            Assert.Equal(self.GetHashCode(), self2.GetHashCode());
            Assert.NotEqual(self.GetHashCode(), self3.GetHashCode());

            Assert.NotEqual(self, Sets[0][RelationNames.Ancestor]);
            Assert.NotEqual(self, Sets[0][RelationNames.Child]);
            Assert.NotEqual(self, Sets[0][RelationNames.Descendant]);
            Assert.NotEqual(self, Sets[0][RelationNames.Parent]);
        }
    }
}
