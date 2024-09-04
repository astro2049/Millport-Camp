using System;
using System.Collections.Generic;
using Entities.Gun;
using Entities.Player;
using Managers.GameManager;
using UnityEngine;
using UnityEngine.Events;

namespace Managers.Inventory
{
    public class Item
    {
        public Item(string aName, GameObject aGo)
        {
            name = aName;
            go = aGo;
        }

        public readonly string name;
        public readonly GameObject go;
    }

    public class InventoryManager : MonoBehaviour
    {
        [Header("Inventory")]
        public int inventorySize = 10;
        // TODO: public or private?
        public readonly Dictionary<int, Item> items = new Dictionary<int, Item>();
        public int equippedItemSlotNum = -1;

        [Header("Crafting")]
        [SerializeField] private GunStats[] gunStatsList;
        [SerializeField] private GameObject gunPrefab;

        [Header("Gameplay")]
        [SerializeField] private GameplayData gameplayData;

        // Events. To Presenter, inventory UI manager
        [HideInInspector] public UnityEvent itemsUpdated;
        [HideInInspector] public UnityEvent<int> itemEquipped;
        [HideInInspector] public UnityEvent<int> itemUnequipped;

        /*
         * - Inventory -
         */
        private bool AddItem(string itemName, GameObject go)
        {
            if (items.Count == inventorySize) {
                return false;
            }
            for (int i = 0; i < inventorySize; i++) {
                if (items.ContainsKey(i)) {
                    continue;
                }
                Item item = new Item(itemName, go);
                items.Add(i, item);
                TeleportUnderground(go);
                break;
            }

            itemsUpdated.Invoke();
            return true;
        }

        public bool ChangeItemSlot(int aSlotNum, int aNewSlotNum)
        {
            Item item = items[aSlotNum];
            items.Remove(aSlotNum);
            // Possible swap
            if (items.ContainsKey(aNewSlotNum)) {
                Item itemToSwap = items[aNewSlotNum];
                items.Remove(aNewSlotNum);
                // Move the destination item to current location
                items.Add(aSlotNum, itemToSwap);
            }
            items.Add(aNewSlotNum, item);
            // Caution
            if (equippedItemSlotNum == aSlotNum) {
                equippedItemSlotNum = aNewSlotNum;
            }

            itemsUpdated.Invoke();
            return true;
        }

        public bool EquipItem(int slotNum)
        {
            if (slotNum == equippedItemSlotNum) {
                return false;
            }
            if (equippedItemSlotNum != -1) {
                UnEquipItem();
            }
            equippedItemSlotNum = slotNum;
            itemEquipped.Invoke(slotNum);
            gameplayData.player.GetComponent<PlayerStateComponent>().EquipGun(items[slotNum].go);

            return true;
        }

        private bool UnEquipItem()
        {
            if (equippedItemSlotNum == -1) {
                return false;
            }
            TeleportUnderground(items[equippedItemSlotNum].go);
            itemUnequipped.Invoke(equippedItemSlotNum);
            equippedItemSlotNum = -1;

            return true;
        }

        /*
         * - Crafting -
         */
        public List<string> GetCraftingOptions()
        {
            List<string> optionNames = new List<string>();
            foreach (GunStats gunStats in gunStatsList) {
                optionNames.Add(gunStats.name);
            }
            return optionNames;
        }

        public bool CraftItem(string itemName)
        {
            if (items.Count == inventorySize) {
                return false;
            }
            foreach (GunStats gunStats in gunStatsList) {
                if (gunStats.name != itemName) {
                    continue;
                }
                GameObject gun = Instantiate(gunPrefab);
                gun.GetComponent<GunStateComponent>().Init(gunStats);
                AddItem(itemName, gun);
                return true;
            }
            throw new Exception("Crafting: didn't find a match in fabrication list");
        }

        private void TeleportUnderground(GameObject entity)
        {
            entity.transform.position = new Vector3(0, -1, 0);
        }

        public void PrintInventoryInfo()
        {
            string info = "Inventory:\n- " + items.Count + " items -\n";
            foreach (KeyValuePair<int, Item> kv in items) {
                info += "slot " + kv.Key + ": " + kv.Value.name + "\n";
            }
            Debug.Log(info);
        }
    }
}
