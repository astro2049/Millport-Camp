using Gameplay.Quests;
using UnityEngine;
using UnityEngine.Serialization;

namespace Managers
{
    public class QuestManager : MonoBehaviour
    {
        [SerializeField] private UIManager uiManager;

        public Quest[] quests;
        private int currentQuestNumber = 0;

        // Quest Destination Indicator
        [FormerlySerializedAs("destinationIndicatorComponent")]
        [Header("Destination Indicator")]
        [SerializeField] private QuestLocationIndicatorComponent questLocationIndicatorComponent;

        public void StartStory()
        {
            StartCurrentQuest();
        }

        private void StartCurrentQuest()
        {
            // Subscribe to the current quest's questFinished event
            Quest quest = quests[currentQuestNumber];
            quest.questFinished.AddListener(FinishCurrentQuest);

            // Begin the current quest
            quest.Begin();

            // Tell ui manager to update quest
            uiManager.UpdateQuest(quest);

            // Set destination for destinationIndicatorComponent
            questLocationIndicatorComponent.SetDestination(quest.destinationGo);
        }

        private void FinishCurrentQuest()
        {
            // Unsubscribe from the current quest's questFinished event
            Quest quest = quests[currentQuestNumber];
            quest.questFinished.RemoveListener(FinishCurrentQuest);

            // If this is already the last quest, game is completed
            if (currentQuestNumber == quests.Length - 1) {
                uiManager.GameCompleted();
                return;
            }

            // Start the next quest
            currentQuestNumber++;
            StartCurrentQuest();
        }
    }
}
