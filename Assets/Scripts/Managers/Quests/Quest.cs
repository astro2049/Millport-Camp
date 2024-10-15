using System;
using Entities.Abilities.QuestLocation;
using PCG.Generators;
using PCG.Generators.Terrain;
using UnityEngine;
using UnityEngine.Events;

namespace Managers.Quests
{
    [Serializable]
    // 'Go to destination' quest
    public class Quest
    {
        public string name;
        public string description;

        public BiomeType baseBiome;
        public GameObject basePrefab;

        public GameObject destinationGo;

        [HideInInspector] public UnityEvent questFinished;

        public void AssignDestinationGo(GameObject aDestinationGo)
        {
            destinationGo = aDestinationGo;

            GameObject destinationColliderGo = new GameObject("Destination Collider") {
                transform = {
                    parent = destinationGo.transform,
                    localPosition = Vector3.zero
                }
            };

            // Configure trigger collider (sphere collider) for detecting player
            SphereCollider sphereCollider = destinationColliderGo.AddComponent<SphereCollider>();
            Vector3 sphereColliderCenter = sphereCollider.center;
            sphereCollider.center = new Vector3(sphereColliderCenter.x, 0, sphereColliderCenter.z);
            sphereCollider.radius = 15f;
            sphereCollider.isTrigger = true;
            sphereCollider.excludeLayers = ~LayerMask.GetMask("Player");

            // Quest destination collider component
            QuestDestinationColliderComponent questDestinationColliderComponent = destinationColliderGo.AddComponent<QuestDestinationColliderComponent>();
            questDestinationColliderComponent.reachedByPlayer.AddListener(Finish);
        }

        public void Begin()
        {

        }

        private void Finish()
        {
            questFinished.Invoke();
        }
    }
}
