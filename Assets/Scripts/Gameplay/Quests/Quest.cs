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

        [HideInInspector] public UnityEvent questFinished;

        public void AssignDestinationGo(GameObject destinationGo)
        {
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
