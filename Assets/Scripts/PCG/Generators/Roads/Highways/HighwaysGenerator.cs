using System.Collections.Generic;
using UnityEngine;

namespace PCG.Generators.Roads.Highways
{
    public class HighwaysGenerator : MonoBehaviour
    {
        [SerializeField] private RoadGridComponent roadGridComponent;

        public void CalculateHighwayNet(List<Vector3> basePositions)
        {
            for (int i = 1; i < basePositions.Count; i++) {
                Vector3Int startCell = roadGridComponent.grid.WorldToCell(basePositions[i - 1]);
                Vector3Int endCell = roadGridComponent.grid.WorldToCell(basePositions[i]);
                // Debug.Log(startCell + " " + endCell);
                List<Vector3Int> roadCells = AStarPath(startCell, endCell);
                foreach (Vector3Int roadCell in roadCells) {
                    if (roadGridComponent.roadCells.Contains(roadCell)) {
                        // Existing road segment, might also belong to a highway(s) so it's a tryadd
                        roadGridComponent.highwayCells.TryAdd(roadCell, false); // Just for debugging
                    } else {
                        // A new road segment
                        roadGridComponent.roadCells.Add(roadCell);
                        roadGridComponent.highwayCells.Add(roadCell, true); // Just for debugging
                    }
                }
            }
        }

        private List<Vector3Int> AStarPath(Vector3Int startCell, Vector3Int endCell)
        {
            // Priority queue for the to-explore list
            PriorityQueue<RoadNode> cellsToTraverse = new PriorityQueue<RoadNode>();
            // Closed set to keep track of evaluated nodes
            HashSet<Vector3Int> visited = new HashSet<Vector3Int>();

            RoadNode startRoadNode = new RoadNode(startCell, roadGridComponent.roadCells.Contains(startCell)) {
                a = 0,
                h = GetDistance(startCell, endCell)
            };
            cellsToTraverse.Enqueue(startRoadNode);

            while (cellsToTraverse.count > 0) {
                RoadNode currentRoadNode = cellsToTraverse.Dequeue();
                visited.Add(currentRoadNode.position);

                // Check if we have reached the goal
                if (currentRoadNode.position == endCell) {
                    return ReconstructPath(currentRoadNode);
                }

                // Get neighbors of the current node
                foreach (Vector3Int neighborPosition in roadGridComponent.GetNeighborCells(currentRoadNode.position)) {
                    if (visited.Contains(neighborPosition)) {
                        continue;
                    }

                    int a = currentRoadNode.a + 1;

                    RoadNode neighborRoadNode = new RoadNode(neighborPosition, roadGridComponent.roadCells.Contains(neighborPosition)) {
                        a = a,
                        h = GetDistance(neighborPosition, endCell),
                        parent = currentRoadNode
                    };

                    // Check if the neighbor is already in to-explore list with a lower a score
                    if (cellsToTraverse.Contains(neighborRoadNode)) {
                        RoadNode existingRoadNode = cellsToTraverse.GetNode(neighborRoadNode);
                        if (a < existingRoadNode.a) {
                            existingRoadNode.a = a;
                            existingRoadNode.parent = currentRoadNode;
                            // Since the priority (f score) has changed, we need to update its position in the queue
                            cellsToTraverse.UpdatePriority(existingRoadNode);
                        }
                    } else {
                        cellsToTraverse.Enqueue(neighborRoadNode);
                    }
                }
            }

            // No path found
            return new List<Vector3Int>();
        }

        private int GetDistance(Vector3Int a, Vector3Int b)
        {
            // Using Manhattan distance as the heuristic
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.z - b.z);
        }

        private List<Vector3Int> ReconstructPath(RoadNode endRoadNode)
        {
            List<Vector3Int> path = new List<Vector3Int>();
            RoadNode currentRoadNode = endRoadNode;

            while (currentRoadNode != null) {
                path.Add(currentRoadNode.position);
                currentRoadNode = currentRoadNode.parent;
            }

            path.Reverse();
            return path;
        }
    }
}
