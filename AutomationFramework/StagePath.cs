using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Collections;

namespace AutomationFramework
{
    public class StagePath : IEquatable<StagePath>
    {
        public StagePath(params int[] indices)
        {
            _Indices = indices.ToArray();
        }

        private int[] _Indices;

        /// <summary>
        /// Represents the path
        /// </summary>
        public int[] Indices { get { return _Indices ??= Array.Empty<int>(); } }

        /// <summary>
        /// Returns the last index of the Indices. If Indices is empty return 0.
        /// </summary>
        public int Index
        {
            get
            {
                if (Indices.Length == 0) return 0;
                return Indices.Last();
            }
        }

        public static StagePath Empty { get { return new StagePath(); } }

        public static StagePath Root { get { return new StagePath(1); } }

        public int this[int index]
        {
            get
            {
                return Indices[index];
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

        public int Length { get { return Indices.Length; } }

        public StagePath CreateChild(int index)
        {
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
            if (Length != other.Length) return false;
            for (int i = 0; i < Length; i++)
                if (this[i] != other[i]) return false;
            return true;
        }

        public override bool Equals(object other)
        {
            return other is StagePath ? this.Equals((StagePath)other) : false;
        }

        public override int GetHashCode()
        {
            return ((IStructuralEquatable)Indices).GetHashCode(EqualityComparer<int>.Default);
        }

        public StagePath Clone()
        {
            return new StagePath(Indices);
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
