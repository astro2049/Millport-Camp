using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.Inventory
{
    /*
     * Inventory Slot, referenced:
     * Coco Code (2022) 'Unity INVENTORY: A Definitive Tutorial', YouTube, 29 Sept
     * Available at: https://youtu.be/oJAE6CbsQQA?si=GVPbgh6AO7kSig4U (Accessed 29 June 2024).
     */
    public class InventorySlot : MonoBehaviour, IDropHandler
    {
        public void OnDrop(PointerEventData eventData)
        {
            if (transform.childCount == 0) {
                InventoryItem inventoryItem = eventData.pointerDrag.GetComponent<InventoryItem>();
                inventoryItem.parentSlotTransform = transform;
            }
        }
    }
}
