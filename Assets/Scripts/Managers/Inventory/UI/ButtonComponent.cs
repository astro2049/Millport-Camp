using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Managers.Inventory.UI
{
    public class ButtonComponent : MonoBehaviour
    {
        private Button button;
        [HideInInspector] public UnityEvent<GameObject> buttonClickedEvent = new UnityEvent<GameObject>();

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(Select);
        }

        private void Select()
        {
            button.interactable = false;
            buttonClickedEvent.Invoke(gameObject);
        }

        public void Unselect()
        {
            button.interactable = true;
        }
    }
}
