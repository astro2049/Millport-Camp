using System.Collections.Generic;
using PCG.Generators.Roads;
using UnityEngine;

namespace PCG.Generators.POIs
{
    public class TownGenerator : MonoBehaviour
    {
        [SerializeField] private LSystem lSystem;
        [SerializeField] private GameObject buildingPrefab;
        [SerializeField] private int initialLength = 4;

        [SerializeField] private RoadGridComponent roadGridComponent;

        public void GenerateTowns(List<Vector3> basePositions)
        {
            foreach (Vector3 basePosition in basePositions) {
                GenerateTown(basePosition);
            }
        }

        private void GenerateTown(Vector3 center)
        {
            string sentence = lSystem.GenerateSentence();
            // Debug.Log("sentence: " + sentence);
            ParseSentence(sentence, center);
        }

        private void ParseSentence(string sentence, Vector3 center)
        {
            Stack<TransformInfo> transformStack = new Stack<TransformInfo>();
            Vector3Int position = roadGridComponent.roadGrid.grid.WorldToCell(center);
            Quaternion rotation = Quaternion.identity;
            rotation *= Quaternion.Euler(Vector3.up * 90 * Random.Range(0, 4));
            int length = initialLength;

            foreach (char c in sentence) {
                switch (c) {
                    case 'F':
                        for (int i = 0; i < length; i++) {
                            if (roadGridComponent.IsWalkable(position)) {
                                roadGridComponent.roadCells.Add(position);
                            }
                            position += Vector3Int.RoundToInt(rotation * Vector3.forward);
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
                            position = position,
                            rotation = rotation,
                            length = length
                        });
                        break;
                    case ']':
                        // Load
                        if (transformStack.Count > 0) {
                            TransformInfo ti = transformStack.Pop();
                            position = ti.position;
                            rotation = ti.rotation;
                            length = ti.length;
                        }
                        break;
                    case 'B':
                        GameObject building = Instantiate(buildingPrefab);
                        building.transform.position = position;
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
    }
}
