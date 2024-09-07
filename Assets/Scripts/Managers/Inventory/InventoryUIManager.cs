using System.Collections.Generic;
using Managers.Inventory.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Managers.Inventory
{
    // TODO: Only supports guns in inventory (un-stackable)
    public class InventoryUIManager : MonoBehaviour
    {
        // Data
        private readonly List<InventorySlotUI> slots = new List<InventorySlotUI>();
        private GameObject selectedItem;

        [Header("UI")]
        [SerializeField] private GameObject slotsTable;
        [SerializeField] private Button equipButton;
        [SerializeField] private Transform dragTransform;

        [Header("Components and Prefabs")]
        [SerializeField] private InventoryManager inventoryManager;
        [SerializeField] private GameObject slotUIPrefab;
        [SerializeField] private GameObject itemUIPrefab;

        public void Initialize()
        {
            InitializeSlots();
            equipButton.interactable = false;
            inventoryManager.itemsUpdated.AddListener(RefreshItems);
            inventoryManager.itemEquipped.AddListener(EquipItem);
            inventoryManager.itemUnequipped.AddListener(UnequipItem);
        }

        /*
         * Inventory
         */
        private void InitializeSlots()
        {
            for (int i = 0; i < inventoryManager.inventorySize; i++) {
                GameObject slot = Instantiate(slotUIPrefab, slotsTable.transform);
                slot.GetComponent<InventorySlotUI>().Init(i);
                slot.GetComponent<InventorySlotUI>().slotChanged.AddListener(ChangeItemSlot);
                slots.Add(slot.GetComponent<InventorySlotUI>());
            }
        }

        private void ChangeItemSlot(int slotNum, int newSlotNum)
        {
            inventoryManager.ChangeItemSlot(slotNum, newSlotNum);
        }

        private void RefreshItems()
        {
            // inventoryManager.PrintInventoryInfo();

            // Clear slots
            foreach (InventorySlotUI slot in slots) {
                if (slot.item) {
                    Destroy(slot.item.gameObject);
                }
            }

            // Put in items
            foreach (KeyValuePair<int, Item> kv in inventoryManager.items) {
                InventorySlotUI slot = slots[kv.Key];
                GameObject item = Instantiate(itemUIPrefab);
                slot.AssignItem(item.GetComponent<InventoryItemUI>());
                item.GetComponent<InventoryItemUI>().Init(kv.Key, kv.Value.name);
                item.GetComponent<ButtonComponent>().buttonClickedEvent.AddListener(SelectItem);
                item.GetComponent<DragNDropComponent>().Init(dragTransform);
            }

            // Equip item (...?)
            if (inventoryManager.equippedItemSlotNum != -1) {
                GameObject item = slots[inventoryManager.equippedItemSlotNum].item.gameObject;

                // Disable item button's color fading temporarily
                Button button = item.GetComponent<Button>();
                ColorBlock colors = button.colors;
                colors.fadeDuration = 0f;
                button.colors = colors;

                EquipItem(inventoryManager.equippedItemSlotNum);

                // Reenable color fading
                colors.fadeDuration = 0.1f;
                button.colors = colors;
            }
        }

        private void SelectItem(GameObject item)
        {
            if (selectedItem) {
                selectedItem.GetComponent<ButtonComponent>().Unselect();
            }
            selectedItem = item;
            equipButton.interactable = selectedItem.GetComponent<InventoryItemUI>().slotNum != inventoryManager.equippedItemSlotNum;
        }

        public void RequestToEquipItem()
        {
            inventoryManager.EquipItem(selectedItem.GetComponent<InventoryItemUI>().slotNum);
        }

        private void EquipItem(int slotNum)
        {
            slots[slotNum].item.Equip();
        }

        private void UnequipItem(int slotNum)
        {
            slots[slotNum].item.UnEquip();
        }
    }
}
