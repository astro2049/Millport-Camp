using System.Collections.Generic;
using PCG.Generators.Roads;
using UnityEngine;

namespace PCG.Generators.POIs
{
    public class TownRoadsPlacer : MonoBehaviour
    {
        [SerializeField] private GameObject roadPrefab, crossroadPrefab;
        [SerializeField] private Transform roadsParent;

        [SerializeField] private RoadGridComponent roadGridComponent;

        public void PlaceRoadSegments()
        {
            foreach (Vector3Int roadSegmentCoordinate in roadGridComponent.roadCells) {
                List<Vector3Int> neighbors = roadGridComponent.GetNeighborRoadCells(roadSegmentCoordinate);

                GameObject roadSegment;
                Vector3 position = roadGridComponent.roadGrid.grid.GetCellCenterWorld(roadSegmentCoordinate);
                position.y = 0.01f;
                Quaternion rotation = Quaternion.identity;
                if (neighbors.Count == 1) {
                    roadSegment = roadPrefab;
                    rotation = Quaternion.LookRotation(Vector3.Normalize(roadSegmentCoordinate - neighbors[0]));
                } else if (neighbors.Count == 2) {
                    if (roadSegmentCoordinate - neighbors[0] == neighbors[1] - roadSegmentCoordinate) {
                        roadSegment = roadPrefab;
                        rotation = Quaternion.LookRotation(Vector3.Normalize(roadSegmentCoordinate - neighbors[0]));
                    } else {
                        roadSegment = crossroadPrefab;
                    }
                } else {
                    roadSegment = crossroadPrefab;
                }
                Instantiate(roadSegment, position, rotation, roadsParent);
            }
        }
    }
}
