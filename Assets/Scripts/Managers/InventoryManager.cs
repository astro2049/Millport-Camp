﻿using System;
using Entities.Gun;
using Entities.Player;
using UI.Crafting;
using UI.Inventory;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Managers
{
    /*
     * Inventory Manager, referenced:
     * Coco Code (2022) 'Unity INVENTORY: A Definitive Tutorial', YouTube, 29 Sept
     * Available at: https://youtu.be/oJAE6CbsQQA?si=GVPbgh6AO7kSig4U (Accessed 29 June 2024).
     */
    // TODO: Only supports guns in inventory (un-stackable)
    public class InventoryManager : MonoBehaviour
    {
        /*
         * Inventory
         */
        [HideInInspector] public PlayerInventoryComponent playerInventoryComponent;

        [SerializeField] private GameObject inventorySlotsTable;
        private InventorySlot[] inventorySlots;
        [SerializeField] private GameObject inventoryItemPrefab;
        [SerializeField] private GunData[] gunDatas;
        [SerializeField] private GameObject gunPrefab;
        public Transform dragTransform;

        [SerializeField] private InventoryItem selectingItem;
        [SerializeField] private GameObject equipPrompt;
        [SerializeField] private InventoryItem equippedItem;

        [SerializeField] private InputActionAsset inputActionsAsset;

        private void Awake()
        {
            // Initialize inventory slots
            inventorySlots = inventorySlotsTable.GetComponentsInChildren<InventorySlot>();

            // Initialize crafting options
            foreach (CraftingOption craftingOption in craftingOptionsTable.GetComponentsInChildren<CraftingOption>()) {
                craftingOption.buttonClickedEvent.AddListener(SelectCraftingOption);
            }
            craftButton.interactable = false;

            RegisterActionCallbacks();
        }

        private void Start()
        {
            // Add a default weapon for the player
            CraftItem("AK-42");
            EquipItem(inventorySlots[0].GetComponentInChildren<InventoryItem>());
        }

        private void RegisterActionCallbacks()
        {
            InputActionMap inventoryInputActions = inputActionsAsset.FindActionMap("Inventory");
            inventoryInputActions.FindAction("Equip").performed += OnEquip;
        }

        private bool AddItem(string name, GameObject entity)
        {
            // Check for free slot,
            // and if yes, spawn a new inventory item in the slot
            foreach (InventorySlot slot in inventorySlots) {
                if (!slot.GetComponentInChildren<InventoryItem>()) {
                    SpawnNewItem(slot, name, entity);
                    return true;
                }
            }
            return false;
        }

        private void SpawnNewItem(InventorySlot slot, string name, GameObject entity)
        {
            playerInventoryComponent.AddItem(entity);
            GameObject newItemGo = Instantiate(inventoryItemPrefab, slot.transform);

            InventoryItem item = newItemGo.GetComponent<InventoryItem>();
            // Inject drag transform
            item.Init(name, entity, dragTransform);
            // Subscribe to hover/un-hover events
            item.mouseEnterEvent.AddListener(SetHoveringItem);
            item.mouseExitEvent.AddListener(ClearHoveringItem);
        }

        private void TeleportUnderground(GameObject entity)
        {
            entity.transform.position = new Vector3(0, -1, 0);
        }

        private void SetHoveringItem(InventoryItem item)
        {
            selectingItem = item;
            equipPrompt.SetActive(true);
        }

        private void ClearHoveringItem()
        {
            selectingItem = null;
            equipPrompt.SetActive(false);
        }

        private void OnEquip(InputAction.CallbackContext context)
        {
            if (selectingItem) {
                EquipItem();
            }
        }

        private void EquipItem(InventoryItem item = null)
        {
            if (equippedItem) {
                TeleportUnderground(equippedItem.entity);
                equippedItem.UnEquip();
            }
            if (!item) {
                item = selectingItem;
            }
            equippedItem = item;
            playerInventoryComponent.EquipItem(equippedItem.entity);
            equippedItem.Equip();
        }

        /*
         * Crafting
         */
        [SerializeField] private GameObject craftingOptionsTable;
        private CraftingOption selectedCraftingOption;
        [SerializeField] private Button craftButton;

        private void SelectCraftingOption(CraftingOption craftingOption)
        {
            if (selectedCraftingOption) {
                selectedCraftingOption.Unselect();
            }
            selectedCraftingOption = craftingOption;
            craftButton.interactable = true;
        }

        public void CraftItem(string optionName)
        {
            if (optionName == "") {
                optionName = selectedCraftingOption.name;
            }
            GameObject gun = FabricateGun(optionName);
            AddItem(optionName, gun);
        }

        private GameObject FabricateGun(string optionName)
        {
            foreach (GunData stats in gunDatas) {
                if (stats.name == optionName) {
                    GameObject gun = Instantiate(gunPrefab);
                    gun.GetComponent<GunStateComponent>().stats = stats;
                    gun.GetComponent<GunStateComponent>().Init();
                    TeleportUnderground(gun);
                    return gun;
                }
            }
            throw new Exception("Crafting: didn't find a match in fabrication list");
        }
    }
}
