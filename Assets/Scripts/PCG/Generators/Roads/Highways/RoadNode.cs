using UnityEngine;

namespace PCG.Generators.Roads.Highways
{
    public class RoadNode : System.IComparable<RoadNode>
    {
        public readonly Vector3Int position;
        public RoadNode parent;
        public int a; // Actual cost from start node
        public int h; // Heuristic cost to end node
        private readonly bool isRoadCell; // Indicates if this node is a road cell

        public int f
        {
            get { return a + h; } // Total estimated cost
        }

        public RoadNode(Vector3Int position, bool isRoadCell)
        {
            this.position = position;
            a = int.MaxValue; // Initialize with a high value
            h = 0;
            parent = null;
            this.isRoadCell = isRoadCell;
        }

        public int CompareTo(RoadNode other)
        {
            int compare = f.CompareTo(other.f);
            if (compare == 0) {
                // Always advancing towards destination
                compare = h.CompareTo(other.h);
                if (compare == 0) {
                    // Prefer nodes that are road cells
                    if (isRoadCell && !other.isRoadCell) {
                        return -1;
                    } else if (!isRoadCell && other.isRoadCell) {
                        return 1;
                    } else {
                        return 0;
                    }
                }
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
