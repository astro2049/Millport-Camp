using Entities.Abilities.InventoryItem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Managers.Inventory.UI
{
    public class InventoryUIItem : MonoBehaviour
    {
        // Data
        public int slotNum;
        [HideInInspector] public InventoryItem data;

        // UI
        private Button button;
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI equippedNumber;

        private void Awake()
        {
            button = GetComponent<Button>();
        }

        public void Init(int aSlotNum, InventoryItem data)
        {
            slotNum = aSlotNum;
            this.data = data;
            iconImage.sprite = data.icon;
            nameText.text = this.data.name;
        }

        public void Equip()
        {
            button.interactable = false;
            equippedNumber.enabled = true;
        }

        public void UnEquip()
        {
            button.interactable = true;
            equippedNumber.enabled = false;
        }
    }
}
