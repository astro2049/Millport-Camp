using Cinemachine;
using Entities.Abilities.ClearingDistance;
using Entities.Abilities.Input;
using Entities.Abilities.Observer;
using Entities.Ocean;
using Managers.Inventory;
using Managers.Quests;
using Managers.Quests.UI;
using Managers.UI;
using PCG.Generators;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using EventType = Entities.Abilities.Observer.EventType;

namespace Managers.GameManager
{
    public enum PlayerMode
    {
        Combat = 0,
        Inventory = 1,
        PauseMenu = 2,
        Map = 3
    }

    public enum ActorType
    {
        Player = 0,
        Vehicle = 1
    }

    public class GameManager : MonoBehaviour, IObserver
    {
        [Header("Cursor")]
        [SerializeField] private Texture2D combatCursorTexture;
        [SerializeField] private Texture2D UICursorTexture;

        [Header("Gameplay Data")]
        [SerializeField] private GameplayData gameplayData;
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private QuestLocationIndicatorComponent questLocationIndicator;

        [Header("Gameplay")]
        [SerializeField] private GameObject currentActor;
        [SerializeField] private CinemachineVirtualCamera playerCamera;
        [SerializeField] private CinemachineVirtualCamera vehicleCamera;
        [SerializeField] private AudioClip closeDoorClip;
        [SerializeField] private GameObject actorActivationCollider;

        [Header("Managers")]
        [SerializeField] private UIManager uiManager;
        [SerializeField] private InventoryUIManager inventoryUIManager;
        [SerializeField] private InventoryManager inventoryManager;
        [SerializeField] private LevelGenerator levelGenerator;
        private QuestsManager questsManager;

        [HideInInspector] public PlayerMode playerMode = PlayerMode.Combat;

        [Header("PCG")]
        [SerializeField] private bool usePCGLevel = true;

        [Header("Scene")]
        [SerializeField] private GameObject testingGround;
        [SerializeField] private OceanComponent ocean;

        private void Awake()
        {
            questsManager = GetComponent<QuestsManager>();

            // Set world time flowing speed to normal (for reloading level)
            Time.timeScale = 1f;

            InitializeGameplayData();

            // Generate the world
            if (usePCGLevel) {
                testingGround.SetActive(false);
                testingGround.transform.position = new Vector3(0, -100, 0);
                levelGenerator.levelGenerated.AddListener(StartCampaign);
                levelGenerator.Generate();
            } else {
                ocean.Resize(ocean.transform.localScale.x * 10f);
            }

            SetCursor(combatCursorTexture);
        }

        private void InitializeGameplayData()
        {
            gameplayData.questLocationIndicator = questLocationIndicator;
            SetPlayer(Instantiate(playerPrefab));
        }

        private void SetPlayer(GameObject aPlayer)
        {
            // 0. Store player reference to gameplay data scriptable object
            gameplayData.player = aPlayer;

            // 1. Focus camera, switch input, enable quest location indicator
            playerCamera.Follow = aPlayer.transform;
            SwitchControlActor(aPlayer);

            // 2. Subscribe self and UI manager to player events
            SubjectComponent playerSubject = aPlayer.GetComponent<SubjectComponent>();
            playerSubject.AddObserver(this,
                EventType.WeaponChanged,
                EventType.EnteredVehicle,
                EventType.ExitedVehicle
            );
            playerSubject.AddObserver(uiManager,
                EventType.WeaponChanged,
                EventType.IsReloading,
                EventType.InteractionStarted,
                EventType.InteractionEnded,
                EventType.EnteredBuildMode,
                EventType.PlacingStructure,
                EventType.ExitedBuildMode
            );

            // 3. Add a default weapon for the player
            inventoryUIManager.Initialize();
            inventoryManager.CraftItem("AE-45");
            inventoryManager.EquipItem(0);
        }

        private void StartCampaign()
        {
            // Start the first quest
            if (questsManager.quests.Length > 0) {
                questsManager.StartStory();
            }
        }

        // https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Cursor.SetCursor.html
        private void SetCursor(Texture2D texture)
        {
            if (texture == combatCursorTexture) {
                // Crosshair center
                Cursor.SetCursor(texture, new Vector2(texture.width / 2f, texture.height / 2f), CursorMode.Auto);
            } else if (texture == UICursorTexture) {
                // Mouse tip on top left
                Cursor.SetCursor(texture, Vector2.zero, CursorMode.Auto);
            }
        }

        private void SwitchControlActor(GameObject actor)
        {
            SubjectComponent subject;

            if (currentActor) {
                currentActor.tag = "Untagged";
                currentActor.GetComponent<InputComponent>().enabled = false;
                subject = currentActor.GetComponent<SubjectComponent>();
                subject.RemoveObserver(this, EventType.Dead);
                subject.RemoveObserver(uiManager, EventType.Dead);
                subject.NotifyObservers(new MCEventWEntity(EventType.NotControlledByPlayer, currentActor));

                // Remove AudioListener component
                Destroy(currentActor.GetComponent<AudioListener>());
            }

            // Assign current actor to currentControllingActor
            currentActor = actor;
            currentActor.tag = "Player";
            // Switch inputs, and subscribe to Dead event
            currentActor.GetComponent<InputComponent>().enabled = true;
            subject = currentActor.GetComponent<SubjectComponent>();
            subject.AddObserver(this, EventType.Dead);
            subject.AddObserver(uiManager, EventType.Dead);

            // Add AudioListener component
            currentActor.AddComponent<AudioListener>();

            // Attach actor activation collider
            actorActivationCollider.transform.parent = currentActor.transform;
            actorActivationCollider.transform.localPosition = Vector3.zero;

            // Set actor for destinationIndicatorComponent
            gameplayData.questLocationIndicator.SetActor(currentActor);
        }

        // Handle events
        // Player events, subscribed in Awake()
        public bool OnNotify(MCEvent mcEvent)
        {
            switch (mcEvent.type) {
                // Player
                case EventType.WeaponChanged:
                    // TODO: And we also need to unsubscribe from previous weapon
                    // Do the favor of helping UI Manager to subscribe to (equipped) gun events:
                    // - AmmoChanged, MagEmpty
                    (mcEvent as MCEventWEntity)!.entity.GetComponent<SubjectComponent>().AddObserver(uiManager,
                        EventType.AmmoChanged,
                        EventType.MagEmpty
                    );
                    break;
                case EventType.EnteredVehicle:
                    EnterVehicle((mcEvent as MCEventWEntity)!.entity);
                    break;
                case EventType.ExitedVehicle:
                    ExitVehicle((mcEvent as MCEventWEntity)!.entity);
                    break;
                case EventType.Dead:
                    // Disable inputs
                    currentActor.GetComponent<InputComponent>().enabled = false;
                    // Free camera
                    playerCamera.Follow = null;
                    // Open Pause Menu
                    OpenPauseMenu(false);
                    // Disallow closing the pause menu
                    GetComponent<PlayerInput>().actions.FindActionMap("Menu").Disable();
                    break;
            }
            return true;
        }

        private void EnterVehicle(GameObject vehicle)
        {
            // Switch Inputs
            SwitchControlActor(vehicle);

            // Switch camera
            playerCamera.Follow = null;
            vehicleCamera.Follow = vehicle.transform;
            playerCamera.Priority = 10;
            vehicleCamera.Priority = 20;

            // Teleport player to sky
            TeleportActor(gameplayData.player, new Vector3(0, 100, 0));
            // Let player float
            gameplayData.player.GetComponent<Rigidbody>().useGravity = false;

            // Configure clearing distance collider according to vehicle camera
            Camera.main.GetComponent<ClearingDistanceColliderComponent>().ConfigureCollider(vehicleCamera.m_Lens.OrthographicSize);

            // Play close door SFX
            AudioManager.GetAudioSource().PlayOneShot(closeDoorClip);
        }

        private void TeleportActor(GameObject actor, Vector3 position)
        {
            actor.transform.position = position;
        }

        private void ExitVehicle(GameObject vehicle)
        {
            // Switch Inputs
            SwitchControlActor(gameplayData.player);

            // Switch camera
            playerCamera.Follow = gameplayData.player.transform;
            playerCamera.Priority = 20;
            vehicleCamera.Priority = 10;

            // Teleport player next to door
            TeleportActor(gameplayData.player, vehicle.transform.Find("Drop Off Point").transform.position);
            // Capture player with gravity again
            gameplayData.player.GetComponent<Rigidbody>().useGravity = true;

            // Configure clearing distance collider according to player camera
            Camera.main.GetComponent<ClearingDistanceColliderComponent>().ConfigureCollider(playerCamera.m_Lens.OrthographicSize);

            // Play close door SFX
            AudioManager.GetAudioSource().PlayOneShot(closeDoorClip);
        }

        public void OpenPauseMenu(bool pause)
        {
            // Assign status markers
            if (pause) {
                Pause();
            }
            playerMode = PlayerMode.PauseMenu;

            // Open pause menu
            uiManager.OpenPauseMenu();
            SetCursor(UICursorTexture);
        }

        public void ClosePauseMenu()
        {
            // Assign status markers
            UnPause();
            playerMode = PlayerMode.Combat;

            // Close pause menu
            uiManager.ClosePauseMenu();
            SetCursor(combatCursorTexture);
        }

        public void OpenInventory()
        {
            // Assign status markers
            Pause();
            playerMode = PlayerMode.Inventory;

            SetCursor(UICursorTexture);
            uiManager.OpenInventory();
        }

        public void CloseInventory()
        {
            // Assign status markers
            UnPause();
            playerMode = PlayerMode.Combat;

            SetCursor(combatCursorTexture);
            uiManager.CloseInventory();
        }

        public void OpenMap()
        {
            // Assign status markers
            Pause();
            playerMode = PlayerMode.Map;

            SetCursor(UICursorTexture);
            uiManager.OpenMap(currentActor.transform);
        }

        public void CloseMap()
        {
            // Assign status markers
            UnPause();
            playerMode = PlayerMode.Combat;

            SetCursor(combatCursorTexture);
            uiManager.CloseMap();
        }

        private void Pause()
        {
            // Pause the game
            Time.timeScale = 0f;

            // Disable current actor's inputs
            currentActor.GetComponent<InputComponent>().enabled = false;
        }

        private void UnPause()
        {
            // Unpause the game
            Time.timeScale = 1f;

            // Enable current actor's inputs
            currentActor.GetComponent<InputComponent>().enabled = true;
        }

        public void ReloadCurrentScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}
