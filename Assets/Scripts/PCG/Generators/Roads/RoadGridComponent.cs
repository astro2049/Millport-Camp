using System.Collections.Generic;
using UnityEngine;

namespace PCG.Generators.Roads
{
    public class RoadGridComponent : MonoBehaviour
    {
        public GridComponent roadGrid;
        public readonly HashSet<Vector3Int> roadCells = new HashSet<Vector3Int>();

        private readonly Vector3Int[] directions = {
            new Vector3Int(1, 0, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(0, 0, 1),
            new Vector3Int(0, 0, -1)
        };

        public List<Vector3Int> GetNeighborCells(Vector3Int cell)
        {
            List<Vector3Int> neighbors = new List<Vector3Int>();

            // Walkability checks
            foreach (Vector3Int direction in directions) {
                Vector3Int neighborPos = cell + direction;
                if (IsWalkable(neighborPos)) {
                    neighbors.Add(neighborPos);
                }
            }

            return neighbors;
        }

        public bool IsWalkable(Vector3Int cell)
        {
            // A cell is walkable if it's land and below a certain height

            // 0. Land
            // Get the center of the cell in world coordinates
            Vector3 cellWorldPosition = roadGrid.grid.GetCellCenterWorld(cell);

            // Start 1 unit above the center of the cell; Direction of the raycast is downwards
            Ray ray = new Ray(cellWorldPosition + Vector3.up * 1f, Vector3.down);
            // Draw the debug ray
            // Debug.DrawRay(rayOrigin, rayDirection * rayLength, Color.red, 10f);
            if (!Physics.Raycast(ray, out RaycastHit hitInfo, 2f, LayerMask.GetMask("Floor"))) {
                return false;
            }

            return true;

            // 1. Not part of a mountain
            // float height = mapFeatures.GetPointHeight(cellWorldPosition);
        }

        public List<Vector3Int> GetNeighborRoadCells(Vector3Int cell)
        {
            List<Vector3Int> neighbors = new List<Vector3Int>();

            foreach (Vector3Int direction in directions) {
                Vector3Int neighborPos = cell + direction;
                if (roadCells.Contains(neighborPos)) {
                    neighbors.Add(neighborPos);
                }
            }

            return neighbors;
        }
    }
}
