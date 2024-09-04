using System.Collections.Generic;
using Managers.Inventory.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Managers.Inventory
{
    public class CraftingUIManager : MonoBehaviour
    {
        // Data
        private GameObject selectedOptionUI;

        // UI
        [SerializeField] private GameObject optionsUITable;
        [SerializeField] private Button craftButton;

        // Components
        [SerializeField] private InventoryManager inventoryManager;
        [SerializeField] private GameObject optionUIPrefab;

        private void Awake()
        {
            GetCraftingOptions();
            craftButton.interactable = false;
        }

        private void GetCraftingOptions()
        {
            List<string> craftingOptionNames = inventoryManager.GetCraftingOptions();
            foreach (string optionName in craftingOptionNames) {
                GameObject optionUI = Instantiate(optionUIPrefab, optionsUITable.transform);
                optionUI.GetComponent<CraftingOptionUI>().Init(optionName);
                optionUI.GetComponent<ButtonComponent>().buttonClickedEvent.AddListener(SelectCraftingOption);
            }
        }

        private void SelectCraftingOption(GameObject option)
        {
            if (selectedOptionUI) {
                selectedOptionUI.GetComponent<ButtonComponent>().Unselect();
            }
            selectedOptionUI = option;
            craftButton.interactable = true;
        }

        public void CraftItem()
        {
            inventoryManager.CraftItem(selectedOptionUI.GetComponent<CraftingOptionUI>().optionName);
        }
    }
}
