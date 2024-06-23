using Gun;
using Observer;
using Player;
using TMPro;
using UnityEngine;
using EventType = Observer.EventType;

namespace Managers
{
    public class UIManager : MonoBehaviour, IObserver
    {
        [SerializeField] private TextMeshProUGUI interactText;
        [SerializeField] private TextMeshProUGUI equippedGunNameText;
        [SerializeField] private TextMeshProUGUI magAmmoText;
        [SerializeField] private TextMeshProUGUI reloadText;

        // Start is called before the first frame update
        private void Start()
        {

        }

        // Update is called once per frame
        private void Update()
        {

        }

        // Handle events
        public bool OnNotify(MCEvent mcEvent)
        {
            switch (mcEvent.type) {
                // Player
                case EventType.WeaponChanged:
                    equippedGunNameText.text = (mcEvent as MCEventWEntity)!.entity.GetComponent<GunStateComponent>().gunData.gunName;
                    break;
                case EventType.IsReloading:
                    reloadText.enabled = false;
                    break;
                case EventType.InteractionStarted:
                    interactText.enabled = true;
                    break;
                case EventType.InteractionEnded:
                    interactText.enabled = false;
                    break;
                // Gun
                case EventType.AmmoChanged:
                    magAmmoText.text = (mcEvent as MCEventWInt)!.value.ToString();
                    break;
                case EventType.MagEmpty:
                    reloadText.enabled = true;
                    break;
            }
            return true;
        }
    }
}
