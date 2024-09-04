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
    public class InventorySlotUI : MonoBehaviour, IDropHandler
    {
        public int slotNum;
        public InventoryItemUI item;

        [HideInInspector] public UnityEvent<int, int> slotChanged = new UnityEvent<int, int>();

        public void Init(int aSlotNum)
        {
            slotNum = aSlotNum;
        }

        public void AssignItem(InventoryItemUI aItem)
        {
            item = aItem;
            item.transform.SetParent(transform, false);
            item.GetComponent<DragNDropComponent>().UpdateParentSlot();
        }

        public void OnDrop(PointerEventData eventData)
        {
            GameObject dropItem = eventData.pointerDrag;
            slotChanged.Invoke(dropItem.GetComponent<InventoryItemUI>().slotNum, slotNum);
        }
    }
}
