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

        [SerializeField] private GameManager gameManager;

        // Start is called before the first frame update
        private void Start()
        {

        }

        // Update is called once per frame
        private void Update()
        {

        }

        // Handle events
        public bool OnNotify(EventType mcEvent)
        {
            switch (mcEvent) {
                case EventType.WeaponChanged:
                    equippedGunNameText.text = gameManager.player.GetComponent<PlayerStateComponent>().equippedGun.gunData.gunName;
                    break;
                case EventType.AmmoChanged:
                    magAmmoText.text = gameManager.player.GetComponent<PlayerStateComponent>().equippedGun.GetComponent<GunStateComponent>().currentMagAmmo.ToString();
                    break;
                case EventType.InteractionStarted:
                    interactText.enabled = true;
                    break;
                case EventType.InteractionEnded:
                    interactText.enabled = false;
                    break;
            }
            return true;
        }
    }
}
