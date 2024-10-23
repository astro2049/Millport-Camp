using System.Collections.Generic;
using Managers.Inventory.UI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Managers.Inventory
{
    // TODO: Only supports guns in inventory (un-stackable)
    public class InventoryUIManager : MonoBehaviour
    {
        // Data
        private readonly List<InventoryUISlot> slots = new List<InventoryUISlot>();
        private GameObject toEquipItem;

        [Header("UI")]
        [SerializeField] private InventoryUIItemDetailsCard equippedItemDetailsCard;
        [SerializeField] private InventoryUIItemDetailsCard toEquipItemDetailsCard;
        [SerializeField] private GameObject slotsTable;
        [SerializeField] private Button equipButton;
        [SerializeField] private Transform dragTransform;

        [Header("Components and Prefabs")]
        [SerializeField] private InventoryManager inventoryManager;
        [SerializeField] private GameObject slotUIPrefab;
        [SerializeField] private GameObject itemUIPrefab;

        public void Initialize()
        {
            equippedItemDetailsCard.DisplayNothing();
            toEquipItemDetailsCard.DisplayNothing();

            InitializeSlots();
            equipButton.interactable = false;
            inventoryManager.itemsUpdated.AddListener(RefreshUIItems);
            inventoryManager.itemEquipped.AddListener(EquipUIItem);
            inventoryManager.itemUnequipped.AddListener(UnequipUIItem);
        }

        /*
         * Inventory
         */
        private void InitializeSlots()
        {
            for (int i = 0; i < inventoryManager.inventorySize; i++) {
                GameObject slot = Instantiate(slotUIPrefab, slotsTable.transform);
                slot.GetComponent<InventoryUISlot>().Init(i);
                slot.GetComponent<InventoryUISlot>().slotChanged.AddListener(ChangeItemSlot);
                slots.Add(slot.GetComponent<InventoryUISlot>());
            }
        }

        private void ChangeItemSlot(int slotNum, int newSlotNum)
        {
            inventoryManager.ChangeItemSlot(slotNum, newSlotNum);
        }

        private void RefreshUIItems()
        {
            // inventoryManager.PrintInventoryInfo();

            // Clear slots
            foreach (InventoryUISlot slot in slots) {
                if (slot.uiItem) {
                    Destroy(slot.uiItem.gameObject);
                }
            }

            // Put in items
            foreach (KeyValuePair<int, Item> kv in inventoryManager.items) {
                InventoryUISlot uiSlot = slots[kv.Key];
                GameObject item = Instantiate(itemUIPrefab);
                uiSlot.AssignItem(item.GetComponent<InventoryUIItem>());
                item.GetComponent<InventoryUIItem>().Init(kv.Key, kv.Value.data);

                // Subscribe to events
                ButtonComponent buttonComponent = item.GetComponent<ButtonComponent>();
                buttonComponent.clickedEvent.AddListener(SelectNewUIItemToEquip);
                buttonComponent.startHoveringEvent.AddListener(HoverOverUIItem);
                buttonComponent.endHoveringEvent.AddListener(EndHoverOverUIItem);

                item.GetComponent<DragNDropComponent>().Init(dragTransform);
            }

            // Equip item ...?
            if (inventoryManager.equippedItemSlotNum != -1) {
                GameObject item = slots[inventoryManager.equippedItemSlotNum].uiItem.gameObject;

                // Disable item button's color fading temporarily
                Button button = item.GetComponent<Button>();
                ColorBlock colors = button.colors;
                colors.fadeDuration = 0f;
                button.colors = colors;

                EquipUIItem(inventoryManager.equippedItemSlotNum);

                // Reenable color fading
                colors.fadeDuration = 0.1f;
                button.colors = colors;
            }
        }

        private void EquipUIItem(int slotNum)
        {
            slots[slotNum].uiItem.Equip();
            equippedItemDetailsCard.DisplayInfo(slots[slotNum].uiItem.data);
            toEquipItem = null;
            toEquipItemDetailsCard.DisplayNothing();
            equipButton.interactable = false;
        }

        private void UnequipUIItem(int slotNum)
        {
            slots[slotNum].uiItem.UnEquip();
        }

        public void EquipItem()
        {
            inventoryManager.EquipItem(toEquipItem.GetComponent<InventoryUIItem>().slotNum);
        }

        private void SelectNewUIItemToEquip(GameObject item)
        {
            // This actually shouldn't happen, but just in case
            if (inventoryManager.equippedItemSlotNum == item.GetComponent<InventoryUIItem>().slotNum) {
                return;
            }

            if (toEquipItem) {
                toEquipItem.GetComponent<ButtonComponent>().Unselect();
            }
            toEquipItem = item;
            equipButton.interactable = true;
        }

        private void HoverOverUIItem(GameObject item)
        {
            InventoryUIItem uiItem = item.GetComponent<InventoryUIItem>();
            if (inventoryManager.equippedItemSlotNum == uiItem.slotNum) {
                return;
            }
            toEquipItemDetailsCard.DisplayInfo(uiItem.data);
        }

        private void EndHoverOverUIItem(GameObject item)
        {
            if (toEquipItem) {
                toEquipItemDetailsCard.DisplayInfo(toEquipItem.GetComponent<InventoryUIItem>().data);
            } else {
                toEquipItemDetailsCard.DisplayNothing();
            }
        }
    }
}
