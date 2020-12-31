using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Collections;

namespace AutomationFramework
{
    public class StagePath : IEquatable<StagePath>, IComparable
    {
        public StagePath(StagePath path) 
            : this(path.Indices.ToArray()) { }

        public StagePath(params int[] indices)
        {
            _Indices = indices == null ? Array.Empty<int>() : indices.ToArray();
            ValidateIndices(_Indices);
        }

        private readonly int[] _Indices;

        /// <summary>
        /// Represents the path
        /// </summary>
        public IEnumerable<int> Indices { get { return _Indices; } }

        /// <summary>
        /// Returns the last index of the Indices. If Indices is empty return 0.
        /// </summary>
        public int Index
        {
            get
            {
                if (_Indices.Length == 0) return 0;
                return Indices.Last();
            }
        }

        public static StagePath Empty { get { return new StagePath(); } }

        public static StagePath Root { get { return new StagePath(1); } }

        public int this[int index]
        {
            get
            {
                return _Indices[index];
            }
        }

        public static StagePath Parse(string s)
        {
            if (string.IsNullOrEmpty(s)) return Empty;
            List<int> ints = new List<int>();
            foreach (var index in SplitString(s))
                ints.Add(int.Parse(index));
            return new StagePath(ints.ToArray());
        }

        private static string[] SplitString(string s)
        {
            var chars = new char[] { ',', '-', '.', '_', '|', ' ' };
            return s.Split(chars);
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

        public int Length { get { return _Indices.Length; } }

        private static void ValidateIndices(IEnumerable<int> indices)
        {
            foreach (var index in indices) ValidateIndex(index);
        }

        private static void ValidateIndex(int index)
        {
            if (index < 1) throw new Exception("A stage path index cannot be less than one");
        }

        public StagePath CreateChild(int index)
        {
            ValidateIndex(index);
            List<int> indices = Indices.ToList();
            indices.Add(index);

            return new StagePath(indices.ToArray());
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
            string path = "";
            foreach (var id in Indices)
                path += id + "-";
            return path.TrimEnd('-');
        }

        public bool Equals(StagePath other)
        {
            return other != null && this.ToString() == other.ToString();
        }

        public override bool Equals(object other)
        {
            return other is StagePath path && Equals(path);
        }

        public override int GetHashCode()
        {
            return ((IStructuralEquatable)Indices).GetHashCode(EqualityComparer<int>.Default);
        }

        public StagePath Clone()
        {
            return new StagePath(this);
        }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            StagePath otherStagePath = obj as StagePath;
            if (otherStagePath != null)
                return this.ToString().CompareTo(otherStagePath.ToString());
            else
                throw new ArgumentException("Object is not a StagePath");
        }

        public static bool operator ==(StagePath a, StagePath b)
        {
            if (a is null)
                return b is null;

            return a.Equals(b);
        }

        public static bool operator !=(StagePath a, StagePath b)
        {
            return !(a == b);
        }

        public static bool operator <(StagePath a, StagePath b)
        {
            return a.CompareTo(b) < 0;
        }

        public static bool operator >(StagePath a, StagePath b)
        {
            return a.CompareTo(b) > 0;
        }
    }
}
