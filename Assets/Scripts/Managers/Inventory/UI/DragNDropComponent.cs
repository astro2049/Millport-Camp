using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Managers.Inventory.UI
{
    public class DragNDropComponent : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [HideInInspector] public Transform parentSlotTransform;
        private Image backgroundImage;
        [HideInInspector] public Transform dragTransform;

        private void Awake()
        {
            UpdateParentSlot();
            backgroundImage = GetComponent<Image>();
        }

        public void UpdateParentSlot()
        {
            parentSlotTransform = transform.parent;
        }

        public void Init(Transform aDragTransform)
        {
            dragTransform = aDragTransform;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            backgroundImage.raycastTarget = false;

            // Make sure the item appears on top of other slots
            transform.SetParent(dragTransform);
        }

        public void OnDrag(PointerEventData eventData)
        {
            transform.Translate(eventData.delta);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            backgroundImage.raycastTarget = true;

            // Set parent slot
            // At this point, it's either the original slot, or the new slot
            transform.SetParent(parentSlotTransform);
        }
    }
}
