using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.Crafting
{
    public class CraftingOption : MonoBehaviour
    {
        private Button button;
        [HideInInspector] public new string name;
        public UnityEvent<CraftingOption> buttonClickedEvent = new UnityEvent<CraftingOption>();

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(Select);

            name = transform.Find("Text - Name").GetComponent<TextMeshProUGUI>().text;
        }

        private void Select()
        {
            button.interactable = false;
            buttonClickedEvent.Invoke(this);
        }

        public void Unselect()
        {
            button.interactable = true;
        }
    }
}
