using TMPro;
using UnityEngine;

namespace Managers.Inventory.UI
{
    public class CraftingOptionUI : MonoBehaviour
    {
        // Data
        public string optionName;

        // UI
        [SerializeField] private TextMeshProUGUI optionNameText;

        public void Init(string aOptionName)
        {
            optionName = aOptionName;
            optionNameText.text = aOptionName;
        }
    }
}
