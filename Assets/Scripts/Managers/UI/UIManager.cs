using Entities.Abilities.Observer;
using Entities.Gun;
using Managers.Quests;
using Managers.UI.Map;
using PCG;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using EventType = Entities.Abilities.Observer.EventType;

namespace Managers.UI
{
    public class UIManager : MonoBehaviour, IObserver
    {
        [Header("Canvases")]
        [SerializeField] private Canvas combatModeCanvas;
        [SerializeField] private Canvas buildModeCanvas;
        [SerializeField] private Canvas inventoryCanvas;
        [SerializeField] private Canvas pauseMenuCanvas;
        [SerializeField] private Canvas mapCanvas;

        [Header("Combat Mode")]
        [SerializeField] private GameObject interactPrompt;
        [SerializeField] private GameObject reloadPrompt;
        [SerializeField] private TextMeshProUGUI magAmmoText;
        [SerializeField] private Image equippedGunIconImage;
        [SerializeField] private TextMeshProUGUI equippedGunNameText;

        [Header("Build Mode")]
        [SerializeField] private GameObject overlappingObjectsText;

        [Header("Pause Menu")]
        [SerializeField] private Image pauseMenuBackground;
        [SerializeField] private TextMeshProUGUI pauseMenuTitle;
        [SerializeField] private Button resumeButton;

        [Header("Debug")]
        [SerializeField] private TextMeshProUGUI FPSText;
        [SerializeField] private TextMeshProUGUI versionText;

        [Header("World Data")]
        [SerializeField] private WorldData worldData;

        private void Awake()
        {
            versionText.text = Application.version;
        }

        // Handle events
        // Player events are subscribed in GameManager.Awake(), and
        // gun events are subscribed in GameManager.OnNotify()
        public bool OnNotify(MCEvent mcEvent)
        {
            switch (mcEvent.type) {
                // Player
                case EventType.WeaponChanged:
                    GunStateComponent gun = (mcEvent as MCEventWEntity)!.entity.GetComponent<GunStateComponent>();
                    equippedGunIconImage.sprite = gun.stats.icon;
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
                case EventType.Dead:
                    pauseMenuBackground.enabled = false;
                    pauseMenuTitle.text = "You're Dead";
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

        public void OpenInventory()
        {
            combatModeCanvas.enabled = false;
            inventoryCanvas.enabled = true;
        }

        public void CloseInventory()
        {
            combatModeCanvas.enabled = true;
            inventoryCanvas.enabled = false;
        }

        private void Update()
        {
            UpdateFPSText();
        }

        // FPS counter
        private const int c_FPSSampleFrames = 100;
        private readonly float[] deltaTimeSamples = new float[c_FPSSampleFrames];
        private float deltaTimeSamplesSum = 0;
        private int frameCount = 0;

        private void UpdateFPSText()
        {
            int sampleIndex = frameCount % c_FPSSampleFrames;
            // Subtract old sample from samples sum
            deltaTimeSamplesSum -= deltaTimeSamples[sampleIndex];
            // Put in the fresh sample, and add it to the samples sum
            deltaTimeSamples[sampleIndex] = Time.unscaledDeltaTime;
            deltaTimeSamplesSum += deltaTimeSamples[sampleIndex];
            frameCount++;

            // Update FPS value
            FPSText.text = "FPS: " + (int)(c_FPSSampleFrames / deltaTimeSamplesSum);
        }

        // Quests
        [Header("Quest")]
        [SerializeField] private TextMeshProUGUI questNameText;
        [SerializeField] private TextMeshProUGUI questDescriptionText;
        [SerializeField] private GameObject endingScreen;

        public void UpdateQuest(Quest quest)
        {
            questNameText.text = quest.name;
            questDescriptionText.text = quest.description;
        }

        public void GameCompleted()
        {
            endingScreen.SetActive(true);
        }

        // Maps
        [Header("Maps")]
        [SerializeField] private MapUIComponent mapUIComponent;

        public void RenderMapUIs(Texture2D mapTexture2D)
        {
            mapUIComponent.Initialize(mapTexture2D, worldData.worldSize);
        }

        public void OpenMap(Transform playerTransform)
        {
            mapUIComponent.UpdatePlayerLocation(playerTransform);

            // Switch canvas
            combatModeCanvas.enabled = false;
            mapCanvas.enabled = true;
        }

        public void CloseMap()
        {
            // Switch canvas
            combatModeCanvas.enabled = true;
            mapCanvas.enabled = false;
        }
    }
}
