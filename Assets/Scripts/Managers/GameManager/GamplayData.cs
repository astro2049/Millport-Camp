using Managers.Quests.UI;
using UnityEngine;

namespace Managers.GameManager
{
    [CreateAssetMenu(fileName = "Gameplay Data", menuName = "Scriptable Objects/Gameplay Data", order = 3)]
    public class GameplayData : ScriptableObject
    {
        public GameObject player;
        public QuestLocationIndicatorComponent questLocationIndicator;
    }
}
