using System;
using PCG;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.Quests
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
            destinationGo.GetComponent<QuestDestinationComponent>().reachedByPlayer.AddListener(Finish);
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
