using System.Collections.Generic;
using UnityEngine;

namespace PCG.Generators.POIs
{
    public class TownGenerator : MonoBehaviour
    {
        public LSystem lSystem;
        public GameObject roadPrefab;
        public GameObject buildingPrefab;
        public int length = 4;

        private void Start()
        {
            string sentence = lSystem.GenerateSentence();
            Debug.Log("sentence: " + sentence);
            GenerateTown(sentence);
        }

        private void GenerateTown(string sentence)
        {
            Stack<TransformInfo> transformStack = new Stack<TransformInfo>();
            Vector3 position = new Vector3(0, 0.05f, 0);
            Quaternion rotation = Quaternion.identity;

            foreach (char c in sentence) {
                switch (c) {
                    case 'F':
                        for (int i = 0; i < length; i++) {
                            GameObject roadLeft = Instantiate(roadPrefab, position, rotation);
                            roadLeft.transform.Translate(new Vector3(5, 0, 5));
                            GameObject roadRight = Instantiate(roadPrefab, position, rotation);
                            roadRight.transform.Translate(new Vector3(-5, 0, 0));
                            roadRight.transform.Rotate(new Vector3(0, 180, 0));
                            position += rotation * Vector3.forward * 5;
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
            public Vector3 position;
            public Quaternion rotation;
            public int length;
        }
    }
}
