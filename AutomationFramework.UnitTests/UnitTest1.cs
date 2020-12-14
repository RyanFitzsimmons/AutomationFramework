using AutomationFramework.UnitTests.TestSetup;
using Serilog;
using System;
using System.Collections.Generic;
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
        public void Test1()
        {
            var job = new TestKernel(1, new TestLogger());
            job.Run(RunInfo.Empty);
            Test1Results(job);
        }

        [Fact]
        public void Test1Parallel()
        {
            var job = new TestKernel(3, new TestLogger());
            job.Run(RunInfo.Empty);
            Test1Results(job);
        }

        private static void Test1Results(TestKernel job)
        {
            Assert.Equal(13, job.TestModules.Count);

            foreach (TestModuleWithResult module in job.TestModules)
            {
                switch (module.StagePath.ToString())
                {
                    case "1":
                    case "12":
                    case "13":
                    case "121":
                    case "122":
                    case "123":
                    case "131":
                    case "132":
                    case "133":
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
                                    if (module.CreateChildren != null)
                                        Assert.Equal("Get Current Result", action);
                                    break;
                                default:
                                    throw new Exception("No test implemented");
                            }
                        }
                        break;
                    case "11":
                    case "111":
                    case "112":
                    case "113":
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
                                    if (module.CreateChildren != null)
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
            var validInfos = new List<RunInfo>
            {
                new RunInfo { Type = RunType.Run, JobId = null, RequestId = null, Path = StagePath.Empty },
                new RunInfo { Type = RunType.RunFrom, JobId = 1, RequestId = null, Path = new StagePath { Indices = 1 } },
                new RunInfo { Type = RunType.RunSingle, JobId = 1, RequestId = null, Path = new StagePath { Indices = 1 } }
            };

            // invalid run infos
            var invalidInfos = new List<RunInfo>
            {
                new RunInfo { Type = RunType.Run, JobId = 1, RequestId = null, Path = StagePath.Empty },
                new RunInfo { Type = RunType.Run, JobId = null, RequestId = null, Path = new StagePath { Indices = 1 } },
                new RunInfo { Type = RunType.Run, JobId = 1, RequestId = null, Path = new StagePath { Indices = 1 } },

                new RunInfo { Type = RunType.RunFrom, JobId = null, RequestId = null, Path = new StagePath { Indices = 1 } },
                new RunInfo { Type = RunType.RunFrom, JobId = 1, RequestId = null, Path = StagePath.Empty },
                new RunInfo { Type = RunType.RunFrom, JobId = null, RequestId = null, Path = StagePath.Empty },

                new RunInfo { Type = RunType.RunSingle, JobId = null, RequestId = null, Path = new StagePath { Indices = 1 } },
                new RunInfo { Type = RunType.RunSingle, JobId = 1, RequestId = null, Path = StagePath.Empty },
                new RunInfo { Type = RunType.RunSingle, JobId = null, RequestId = null, Path = StagePath.Empty }
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
                { RelationNames.Ancestor, new StagePath { Indices = 1 } },
                { RelationNames.Child, new StagePath { Indices = 1234 } },
                { RelationNames.Descendant, new StagePath { Indices = 12345 } },
                { RelationNames.Parent, new StagePath { Indices = 12 } },
                { RelationNames.Self, new StagePath { Indices = 123 } },
            },
            new Dictionary<RelationNames, StagePath>
            {
                { RelationNames.Ancestor, new StagePath { Indices = 2 } },
                { RelationNames.Child, new StagePath { Indices = 2234 } },
                { RelationNames.Descendant, new StagePath { Indices = 22345 } },
                { RelationNames.Parent, new StagePath { Indices = 22 } },
                { RelationNames.Self, new StagePath { Indices = 224 } },
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
            var self2 = new StagePath { Indices = 123 };
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
