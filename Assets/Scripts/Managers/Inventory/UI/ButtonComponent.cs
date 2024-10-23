using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Managers.Inventory.UI
{
    public class ButtonComponent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private Button button;
        [HideInInspector] public UnityEvent<GameObject> clickedEvent = new UnityEvent<GameObject>();
        [HideInInspector] public UnityEvent<GameObject> startHoveringEvent = new UnityEvent<GameObject>();
        [HideInInspector] public UnityEvent<GameObject> endHoveringEvent = new UnityEvent<GameObject>();

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(Select);
        }

        private void Select()
        {
            button.interactable = false;
            clickedEvent.Invoke(gameObject);
        }

        public void Unselect()
        {
            button.interactable = true;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            startHoveringEvent.Invoke(gameObject);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            endHoveringEvent.Invoke(gameObject);
        }
    }
}
