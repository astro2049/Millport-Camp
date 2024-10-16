using System.Collections.Generic;
using UnityEngine;

namespace PCG.Generators.Roads
{
    public class RoadNetGenerator : MonoBehaviour
    {
        [SerializeField] private GridComponent roadGrid;
        [SerializeField] private GameObject roadPrefab;
        [SerializeField] private Transform roadsParent;

        [SerializeField] private RoadGridComponent roadGridComponent;
        [SerializeField] private MapFeatures mapFeatures;

        public void PlaceRoadNet(List<Vector3> basePositions)
        {
            for (int i = 0; i < basePositions.Count - 1; i++) {
                GenerateRoadBetween2Towns(basePositions[i], basePositions[i + 1]);
            }
        }

        private void GenerateRoadBetween2Towns(Vector3 startPoint, Vector3 endPoint)
        {
            Vector3Int startCell = roadGrid.grid.WorldToCell(startPoint);
            Vector3Int endCell = roadGrid.grid.WorldToCell(endPoint);
            // Debug.Log(startCell + " " + endCell);
            List<Vector3Int> roadCells = AStarPath(startCell, endCell);

            for (int i = 1; i < roadCells.Count - 1; i++) {
                Vector3Int currentCell = roadCells[i];
                if (roadGridComponent.roadCells.Contains(currentCell)) {
                    continue;
                }
                Vector3Int previousCell = roadCells[i - 1];

                Vector3 currentPosition = roadGrid.grid.GetCellCenterWorld(currentCell);
                Vector3 previousPosition = roadGrid.grid.GetCellCenterWorld(previousCell);

                Vector3 direction = (currentPosition - previousPosition).normalized;
                Quaternion rotation = Quaternion.LookRotation(direction);

                // "Snap" the road to the ground
                currentPosition.y = 0.01f;
                Instantiate(roadPrefab, currentPosition, rotation, roadsParent);
            }
        }

        private List<Vector3Int> AStarPath(Vector3Int startCell, Vector3Int endCell)
        {
            // Priority queue for the open list
            PriorityQueue<RoadNode> cellsToTraverse = new PriorityQueue<RoadNode>();
            // Closed set to keep track of evaluated nodes
            HashSet<Vector3Int> visited = new HashSet<Vector3Int>();

            RoadNode startRoadNode = new RoadNode(startCell) {
                a = 0,
                h = GetDistance(startCell, endCell)
            };
            cellsToTraverse.Enqueue(startRoadNode, startRoadNode.f);

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

                    RoadNode neighborRoadNode = new RoadNode(neighborPosition) {
                        a = a,
                        h = GetDistance(neighborPosition, endCell),
                        parent = currentRoadNode
                    };

                    // Check if the neighbor is already in the open list with a lower a score
                    if (cellsToTraverse.Contains(neighborRoadNode)) {
                        RoadNode existingRoadNode = cellsToTraverse.GetNode(neighborRoadNode);
                        if (a < existingRoadNode.a) {
                            existingRoadNode.a = a;
                            existingRoadNode.parent = currentRoadNode;
                            // Since the priority (f score) has changed, we need to update its position in the queue
                            cellsToTraverse.UpdatePriority(existingRoadNode, existingRoadNode.f);
                        }
                    } else {
                        cellsToTraverse.Enqueue(neighborRoadNode, neighborRoadNode.f);
                    }
                }
            }

            // No path found
            return new List<Vector3Int>();
        }

        private int GetDistance(Vector3Int a, Vector3Int b)
        {
            // Using Manhattan distance as the heuristic
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
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
