using Entities.Gun;
using Managers.Inventory.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Managers.Inventory
{
    public class CraftingUIManager : MonoBehaviour
    {
        // Data
        private GameObject selectedUIOption;

        // UI
        [SerializeField] private InventoryUIItemDetailsCard selectedUIOptionDetailsCard;
        [SerializeField] private GameObject optionsUITable;
        [SerializeField] private Button craftButton;

        // Components
        [SerializeField] private InventoryManager inventoryManager;
        [SerializeField] private GameObject optionUIPrefab;

        private void Awake()
        {
            GetCraftingOptions();
            craftButton.interactable = false;
            selectedUIOptionDetailsCard.DisplayNothing();
        }

        private void GetCraftingOptions()
        {
            GunStats[] craftingOptionNames = inventoryManager.GetCraftingOptions();
            foreach (GunStats option in craftingOptionNames) {
                GameObject optionUI = Instantiate(optionUIPrefab, optionsUITable.transform);
                optionUI.GetComponent<InventoryUIItem>().Init(-1, option);
                ButtonComponent buttonComponent = optionUI.GetComponent<ButtonComponent>();
                buttonComponent.clickedEvent.AddListener(SelectCraftingOption);
                buttonComponent.startHoveringEvent.AddListener(HoverOverCraftingOption);
                buttonComponent.endHoveringEvent.AddListener(EndHoverOverCraftingOption);
            }
        }

        private void HoverOverCraftingOption(GameObject option)
        {
            selectedUIOptionDetailsCard.DisplayInfo(option.GetComponent<InventoryUIItem>().data);
        }

        private void EndHoverOverCraftingOption(GameObject option)
        {
            if (selectedUIOption) {
                selectedUIOptionDetailsCard.DisplayInfo(selectedUIOption.GetComponent<InventoryUIItem>().data);
            } else {
                selectedUIOptionDetailsCard.DisplayNothing();
            }
        }

        private void SelectCraftingOption(GameObject option)
        {
            if (selectedUIOption) {
                selectedUIOption.GetComponent<ButtonComponent>().Unselect();
            }
            selectedUIOption = option;
            craftButton.interactable = true;
        }

        public void CraftItem()
        {
            inventoryManager.CraftItem(selectedUIOption.GetComponent<InventoryUIItem>().data.name);
        }
    }
}
