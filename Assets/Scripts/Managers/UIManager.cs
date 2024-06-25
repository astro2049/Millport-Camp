using Gun;
using Observer;
using TMPro;
using UnityEngine;
using EventType = Observer.EventType;

namespace Managers
{
    public class UIManager : MonoBehaviour, IObserver
    {
        [SerializeField] private Canvas combatModeCanvas, buildModeCanvas;
        [SerializeField] private GameObject interactPrompt;
        [SerializeField] private GameObject reloadPrompt;
        [SerializeField] private TextMeshProUGUI magAmmoText;
        [SerializeField] private TextMeshProUGUI equippedGunNameText;
        [SerializeField] private GameObject overlappingObjectsText;

        // Handle events
        // Player events are subscribed in GameManager.Awake(), and
        // gun events are subscribed in GameManager.OnNotify()
        public bool OnNotify(MCEvent mcEvent)
        {
            switch (mcEvent.type) {
                // Player
                case EventType.WeaponChanged:
                    GunStateComponent gun = (mcEvent as MCEventWEntity)!.entity.GetComponent<GunStateComponent>();
                    equippedGunNameText.text = gun.gunData.gunName;
                    magAmmoText.text = gun.currentMagAmmo.ToString();
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
                case EventType.PlacingStructure:
                    (mcEvent as MCEventWEntity)!.entity.GetComponent<SubjectComponent>().AddObserver(this,
                        EventType.CanPlace,
                        EventType.CannotPlace
                    );
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
                // Structures
                case EventType.CanPlace:
                    overlappingObjectsText.SetActive(false);
                    break;
                case EventType.CannotPlace:
                    overlappingObjectsText.SetActive(true);
                    break;
            }
            return true;
        }
    }
}
