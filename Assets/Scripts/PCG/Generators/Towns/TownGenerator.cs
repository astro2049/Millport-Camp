using System.Collections.Generic;
using Managers.Quests;
using PCG.Generators.Roads;
using UnityEngine;

namespace PCG.Generators.Towns
{
    public class TownGenerator : MonoBehaviour
    {
        [SerializeField] private LSystem lSystem;
        [SerializeField] private int initialLength = 4;

        [SerializeField] private RoadGridComponent roadGridComponent;

        [SerializeField] private Transform basesParent;
        [SerializeField] private QuestsManager questsManager;

        public void GenerateTownsRoads(List<Vector3> townCentersWorld)
        {
            foreach (Vector3 townCenterWorld in townCentersWorld) {
                Vector3Int centerCell = roadGridComponent.grid.WorldToCell(townCenterWorld);
                GenerateTownRoads(centerCell);
            }
        }

        private void GenerateTownRoads(Vector3Int centerCell)
        {
            string sentence = lSystem.GenerateSentence();
            // Debug.Log("sentence: " + sentence);
            ParseSentence(sentence, centerCell);
        }

        private void ParseSentence(string sentence, Vector3Int cell)
        {
            Stack<TransformInfo> transformStack = new Stack<TransformInfo>();
            Quaternion rotation = Quaternion.identity;
            rotation *= Quaternion.Euler(Vector3.up * 90 * Random.Range(0, 4));
            int length = initialLength;

            foreach (char c in sentence) {
                switch (c) {
                    case 'F':
                        for (int i = 0; i < length; i++) {
                            if (roadGridComponent.IsWalkable(cell)) {
                                roadGridComponent.roadCells.Add(cell);
                            }
                            cell += Vector3Int.RoundToInt(rotation * Vector3.forward);
                        }
                        length -= 1;
                        break;
                    case '+':
                        rotation *= Quaternion.Euler(0, lSystem.angle, 0);
                        break;
                    case '-':
                        rotation *= Quaternion.Euler(0, -lSystem.angle, 0);
                        break;
                    case '[':
                        // Save
                        transformStack.Push(new TransformInfo {
                            position = cell,
                            rotation = rotation,
                            length = length
                        });
                        break;

                    case ']':
                        // Load
                        if (transformStack.Count > 0) {
                            TransformInfo ti = transformStack.Pop();
                            cell = ti.position;
                            rotation = ti.rotation;
                            length = ti.length;
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private struct TransformInfo
        {
            public Vector3Int position;
            public Quaternion rotation;
            public int length;
        }

        public void PlaceTownsBuildings(List<Vector3> townCentersWorld)
        {
            for (int i = 0; i < townCentersWorld.Count; i++) {
                Vector3Int centerCell = roadGridComponent.grid.WorldToCell(townCentersWorld[i]);
                PlaceTownBuildings(i, centerCell);
            }
        }

        private void PlaceTownBuildings(int i, Vector3Int centerCell)
        {
            Vector3Int buildingCell = SelectCellForCentralBuilding(centerCell);
            if (buildingCell == new Vector3Int(0, -1, 0)) {
                return;
            }

            // Calculate position
            Vector3 position = roadGridComponent.grid.GetCellCenterWorld(buildingCell);
            position.y = 0;
            // Calculate Rotation
            List<Vector3Int> neighbors = roadGridComponent.GetNeighborCells(buildingCell);
            Quaternion rotation = Quaternion.identity;
            foreach (Vector3Int neighbor in neighbors) {
                if (roadGridComponent.roadCells.Contains(neighbor)) {
                    rotation = Quaternion.LookRotation(Vector3.Normalize(neighbor - buildingCell));
                }
            }

            // Place base in the center
            GameObject researchBase = Instantiate(questsManager.quests[i].basePrefab, position, rotation, basesParent);

            // Assign this research base to the corresponding quest
            questsManager.quests[i].AssignDestinationGo(researchBase);
        }

        private Vector3Int SelectCellForCentralBuilding(Vector3Int startCell)
        {
            Queue<Vector3Int> queue = new Queue<Vector3Int>();
            queue.Enqueue(startCell);
            HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
            visited.Add(startCell);

            while (queue.Count > 0) {
                Vector3Int currentCell = queue.Dequeue();
                if (!roadGridComponent.roadCells.Contains(currentCell)) {
                    return currentCell;
                }

                List<Vector3Int> neighbors = roadGridComponent.GetNeighborCells(currentCell);
                foreach (Vector3Int neighbor in neighbors) {
                    if (!visited.Contains(neighbor)) {
                        queue.Enqueue(neighbor);
                        visited.Add(neighbor);
                    }
                }
            }

            return new Vector3Int(0, -1, 0);
        }
    }
}
