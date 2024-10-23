using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Managers.Inventory.UI
{
    /*
     * Inventory Slot, referenced:
     * Coco Code (2022) 'Unity INVENTORY: A Definitive Tutorial', YouTube, 29 Sept
     * Available at: https://youtu.be/oJAE6CbsQQA?si=GVPbgh6AO7kSig4U (Accessed 29 June 2024).
     */
    public class InventoryUISlot : MonoBehaviour, IDropHandler
    {
        public int slotNum;
        public InventoryUIItem uiItem;

        [HideInInspector] public UnityEvent<int, int> slotChanged = new UnityEvent<int, int>();

        public void Init(int aSlotNum)
        {
            slotNum = aSlotNum;
        }

        public void AssignItem(InventoryUIItem aUIItem)
        {
            uiItem = aUIItem;
            uiItem.transform.SetParent(transform, false);
            uiItem.GetComponent<DragNDropComponent>().UpdateParentSlot();
        }

        public void OnDrop(PointerEventData eventData)
        {
            GameObject dropItem = eventData.pointerDrag;
            slotChanged.Invoke(dropItem.GetComponent<InventoryUIItem>().slotNum, slotNum);
        }
    }
}
