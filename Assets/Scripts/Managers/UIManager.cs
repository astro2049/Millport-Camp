using Gun;
using Observer;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using EventType = Observer.EventType;

namespace Managers
{
    public class UIManager : MonoBehaviour, IObserver
    {
        [SerializeField] private Canvas combatModeCanvas, buildModeCanvas;
        [SerializeField] private GameObject interactPrompt;
        [SerializeField] private GameObject reloadPrompt;
        [SerializeField] private TextMeshProUGUI equippedGunNameText;
        [SerializeField] private TextMeshProUGUI magAmmoText;

        // Start is called before the first frame update
        private void Start()
        {

        }

        // Update is called once per frame
        private void Update()
        {

        }

        // Handle events
        // Player events are subscribed in GameManager.Awake(), and
        // gun events are subscribed in GameManager.OnNotify()
        public bool OnNotify(MCEvent mcEvent)
        {
            switch (mcEvent.type) {
                // Player
                case EventType.WeaponChanged:
                    equippedGunNameText.text = (mcEvent as MCEventWEntity)!.entity.GetComponent<GunStateComponent>().gunData.gunName;
                    break;
                case EventType.IsReloading:
                    reloadPrompt.SetActive(false);
                    break;
                case EventType.InteractionStarted:
                    interactPrompt.SetActive(true);
                    break;
                case EventType.InteractionEnded:
                    interactPrompt.SetActive(false);
                    break;
                case EventType.EnteredBuildMode:
                    combatModeCanvas.enabled = false;
                    buildModeCanvas.enabled = true;
                    break;
                case EventType.ExitedBuildMode:
                    combatModeCanvas.enabled = true;
                    buildModeCanvas.enabled = false;
                    break;
                // Gun
                case EventType.AmmoChanged:
                    magAmmoText.text = (mcEvent as MCEventWInt)!.value.ToString();
                    break;
                case EventType.MagEmpty:
                    reloadPrompt.SetActive(true);
                    break;
            }
            return true;
        }
    }
}
