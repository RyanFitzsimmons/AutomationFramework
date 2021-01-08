using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace AutomationFramework.UnitTests
{
    public class StagePathTests
    {
        public StagePathTests(ITestOutputHelper output)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Xunit(output)
                .CreateLogger();
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

        [Fact]
        public void OrderTest()
        {
            List<StagePath> paths = new()
            {
                new StagePath(1),
                new StagePath(1, 1),
                new StagePath(1, 2),
                new StagePath(1, 10),
                new StagePath(1, 21),
                new StagePath(1, 100),
                new StagePath(1, 100, 1),
                new StagePath(1, 100, 2),
                new StagePath(1, 100, 2, 1),
                new StagePath(2),
                new StagePath(10),
                new StagePath(21),
                new StagePath(100),
            };

            int count = 0;
            List<Tuple<int, StagePath>> ordered = new();
            foreach (var path in paths)
                ordered.Add(new Tuple<int, StagePath>(++count, path));
            var shuffled = ordered.ToList().Shuffle();

            var result = shuffled.OrderBy(x => x.Item2).Select(x => x.Item1).ToArray();

            for (int i = 1; i <= result.Length; i++)
                Assert.Equal(i, result[i - 1]);
        }
    }
}
