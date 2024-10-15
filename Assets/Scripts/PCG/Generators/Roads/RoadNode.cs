using UnityEngine;

namespace PCG.Generators.Roads
{
    public class RoadNode : System.IComparable<RoadNode>
    {
        public readonly Vector3Int position;
        public RoadNode parent;
        public int a; // Actual cost from start node
        public int h; // Heuristic cost to end node
        public int f
        {
            get { return a + h; } // Total estimated cost
        }

        public RoadNode(Vector3Int position)
        {
            this.position = position;
            a = int.MaxValue; // Initialize with a high value
            h = 0;
            parent = null;
        }

        public int CompareTo(RoadNode other)
        {
            int compare = f.CompareTo(other.f);
            if (compare == 0) {
                // Break ties by h value
                compare = h.CompareTo(other.h);
            }
            return compare;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is RoadNode))
                return false;
            return position.Equals(((RoadNode)obj).position);
        }

        public override int GetHashCode()
        {
            return position.GetHashCode();
        }
    }
}
