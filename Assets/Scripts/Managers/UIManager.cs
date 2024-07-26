using Abilities.Observer;
using Entities.Gun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using EventType = Abilities.Observer.EventType;

namespace Managers
{
    public class UIManager : MonoBehaviour, IObserver
    {
        [SerializeField] private Canvas combatModeCanvas, buildModeCanvas, inventoryCanvas, pauseMenuCanvas;
        
        [Header("Combat Mode")]
        [SerializeField] private GameObject interactPrompt;
        [SerializeField] private GameObject reloadPrompt;
        [SerializeField] private TextMeshProUGUI magAmmoText;
        [SerializeField] private TextMeshProUGUI equippedGunNameText;
        
        [Header("Build Mode")]
        [SerializeField] private GameObject overlappingObjectsText;

        [Header("Pause Menu")]
        [SerializeField] private Button resumeButton;

        // Handle events
        // Player events are subscribed in GameManager.Awake(), and
        // gun events are subscribed in GameManager.OnNotify()
        public bool OnNotify(MCEvent mcEvent)
        {
            switch (mcEvent.type) {
                // Player
                case EventType.WeaponChanged:
                    GunStateComponent gun = (mcEvent as MCEventWEntity)!.entity.GetComponent<GunStateComponent>();
                    equippedGunNameText.text = gun.stats.name;
                    magAmmoText.text = gun.magAmmo.ToString();
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
                case EventType.OpenedInventory:
                    combatModeCanvas.enabled = false;
                    inventoryCanvas.enabled = true;
                    break;
                case EventType.ClosedInventory:
                    combatModeCanvas.enabled = true;
                    inventoryCanvas.enabled = false;
                    break;
                case EventType.PawnDead:
                    resumeButton.interactable = false;
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

        public void OpenPauseMenu()
        {
            pauseMenuCanvas.enabled = true;
        }

        public void ClosePauseMenu()
        {
            pauseMenuCanvas.enabled = false;
        }
    }
}
