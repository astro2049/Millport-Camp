using Entities.Abilities.InventoryItem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Managers.Inventory.UI
{
    public class InventoryUIItemDetailsCard : MonoBehaviour
    {
        // UI
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;

        public void DisplayInfo(InventoryItem data)
        {
            iconImage.enabled = true;
            nameText.enabled = true;
            descriptionText.enabled = true;

            iconImage.sprite = data.icon;
            nameText.text = data.name;
            descriptionText.text = data.description;
        }

        public void DisplayNothing()
        {
            iconImage.enabled = false;
            nameText.enabled = false;
            descriptionText.enabled = false;
        }
    }
}
