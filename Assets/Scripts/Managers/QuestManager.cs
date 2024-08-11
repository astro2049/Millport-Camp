using Managers.Quests;
using UnityEngine;

namespace Managers
{
    public class QuestManager : MonoBehaviour
    {
        [SerializeField] private UIManager uiManager;

        public Quest[] quests;
        private int currentQuestNumber = 0;

        public void Start()
        {
            if (quests.Length > 0) {
                StartCurrentQuest();
            }
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
