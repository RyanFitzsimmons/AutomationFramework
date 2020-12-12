using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace AutomationFramework
{
    [Serializable]
    public struct StagePath : IEquatable<StagePath>
    {
        private long _Indices;

        /// <summary>
        /// Represents the path
        /// </summary>
        public long Indices
        {
            get { return _Indices; }
            set
            {
                if (value < 0) throw new Exception("Value cannot be negative");
                _Indices = value;
            }
        }

        /// <summary>
        /// Returns the last index of the Indices. If Indices is empty return 0.
        /// </summary>
        public int Index
        {
            get
            {
                var array = ToArray();
                return array.Length == 0 ? 0 : array.Last();
            }
        }

        public static StagePath Empty { get => new StagePath(); }

        public static StagePath Root { get => new StagePath { Indices = 1 }; }

        public int this[int index] { get => ToArray()[index]; }

        public int[] ToArray()
        {
            List<int> integers = new List<int>();
            foreach (var c in Indices.ToString())
                integers.Add(int.Parse(c.ToString()));
            return integers.ToArray();
        }

        public static StagePath Parse(string s)
        {
            if (string.IsNullOrEmpty(s)) return Empty;
            var split = SplitString(s);
            List<int> ints = new List<int>();
            if (split == null)
                foreach (var index in split)
                    ints.Add(int.Parse(index));
            else
                foreach (var c in s)
                    ints.Add(c);
            return new StagePath { Indices = ToLong(ints) };
        }

        private static long ToLong(IEnumerable<int> indices)
        {
            string s = null;
            foreach (var i in indices) s += i;
            return long.Parse(s);
        }

        private static string[] SplitString(string path)
        {
            var chars = new char[] { ',', '-', '|', '_', '.' };
            foreach (var c in chars)
                if (path.Contains(c)) return path.Split(c);
            return null;
        }

        public static bool TryParse(string s, out StagePath path)
        {
            try
            {
                path = Parse(s);
                return true;
            }
            catch
            {
                path = Empty;
                return false;
            }
        }

        public int Length { get => Indices == 0 ? 0 : ToArray().Length; }

        public StagePath CreateChild(int index)
        {
            var indices = ToArray().ToList();
            indices.Add(index);
            return new StagePath { Indices = ToLong(indices) };
        }

        /// <summary>
        /// This is an ancestor of the path argument
        /// </summary>
        public bool IsAncestorOf(StagePath path)
        {
            if (path.Length <= this.Length) return false;
            for (int i = 0; i < this.Length; i++)
            {
                if (i >= path.Length) break;
                if (this[i] != path[i]) return false;
            }
            return true;
        }

        /// <summary>
        /// This is a child of the path argument
        /// </summary>
        public bool IsChildOf(StagePath path)
        {
            if (path.Length != this.Length - 1) return false;
            for (int i = 0; i < path.Length; i++)
            {
                if (this[i] != path[i]) return false;
            }
            return true;
        }

        /// <summary>
        /// This is the parent of the path argument
        /// </summary>
        public bool IsParentOf(StagePath path)
        {
            if (path.Length != this.Length + 1) return false;
            for (int i = 0; i < this.Length; i++)
            {
                if (this[i] != path[i]) return false;
            }
            return true;
        }

        /// <summary>
        /// This is a descendant of the path argument
        /// </summary>
        public bool IsDescendantOf(StagePath path)
        {
            if (this.Length < path.Length) return false;
            for (int i = 0; i < path.Length; i++)
            {
                if (i >= this.Length) break;
                if (this[i] != path[i]) return false;
            }
            return true;
        }

        public override string ToString()
        {
            return Indices.ToString();
        }

        public bool Equals(StagePath other)
        {
            return Indices == other.Indices;
        }

        public override bool Equals(object obj) => obj is StagePath path && Equals(path);

        public override int GetHashCode()
        {
            return Indices.GetHashCode();
        }

        public static bool operator ==(StagePath a, StagePath b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(StagePath a, StagePath b)
        {
            return !a.Equals(b);
        }

        public static bool operator <(StagePath a, StagePath b)
        {
            return a.IsAncestorOf(b);
        }

        public static bool operator >(StagePath a, StagePath b)
        {
            return a.IsDescendantOf(b);
        }
    }
}
