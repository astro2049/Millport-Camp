using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Managers.Inventory.UI
{
    public class InventoryItemUI : MonoBehaviour
    {
        // Data
        public int slotNum;
        public string itemName;

        // UI
        private Button button;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI equippedEText;

        private void Awake()
        {
            button = GetComponent<Button>();
        }

        public void Init(int aSlotNum, string aItemName)
        {
            slotNum = aSlotNum;
            itemName = aItemName;
            nameText.text = aItemName;
        }

        public void Equip()
        {
            button.interactable = false;
            equippedEText.enabled = true;
        }

        public void UnEquip()
        {
            button.interactable = true;
            equippedEText.enabled = false;
        }
    }
}
