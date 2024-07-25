﻿using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Inventory
{
    /*
     * Inventory Item, referenced:
     * Coco Code (2022) 'Unity INVENTORY: A Definitive Tutorial', YouTube, 29 Sept
     * Available at: https://youtu.be/oJAE6CbsQQA?si=GVPbgh6AO7kSig4U (Accessed 29 June 2024).
     */
    public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        // Fields
        public GameObject entity;

        // UI
        [HideInInspector] public Button button;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI equippedEText;
        [HideInInspector] public Transform parentSlotTransform;
        private Image backgroundImage;
        [HideInInspector] public Transform dragTransform;

        // Mouse hover/un-hover events
        public UnityEvent<InventoryItem> mouseEnterEvent = new UnityEvent<InventoryItem>();
        public UnityEvent mouseExitEvent = new UnityEvent();

        private void Awake()
        {
            button = GetComponent<Button>();

            parentSlotTransform = transform.parent;

            backgroundImage = GetComponent<Image>();
            UnEquip();
        }

        public void Init(string name, GameObject entity, Transform dragTransform)
        {
            nameText.text = name;
            this.entity = entity;
            this.dragTransform = dragTransform;
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

        public void OnPointerEnter(PointerEventData eventData)
        {
            mouseEnterEvent.Invoke(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            mouseExitEvent.Invoke();
        }

        public void Equip()
        {
            button.interactable = false;
            equippedEText.enabled = true;
        }

        public void UnEquip()
        {
            button.interactable = true;
            equippedEText.enabled = false;
        }
    }
}