using System.Collections;
using Cinemachine;
using Entities.Abilities.ClearingDistance;
using Entities.Abilities.Input;
using Entities.Abilities.Observer;
using Entities.Player;
using Gameplay;
using Gameplay.Quests;
using PCG;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
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

    public class GameManager : MonoBehaviour, IObserver
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject playerPrefab;

        [Header("Cursor")]
        [SerializeField] private Texture2D combatCursorTexture;
        [FormerlySerializedAs("UICursorTecture")]
        [Header("Cursor")]
        [SerializeField] private Texture2D UICursorTexture;

        [Header("Gameplay")]
        [SerializeField] private GameObject player;
        [SerializeField] private GameObject currentControllingActor;
        [SerializeField] private CinemachineVirtualCamera playerCamera;
        [SerializeField] private CinemachineVirtualCamera vehicleCamera;

        [Header("Managers")]
        [SerializeField] private UIManager uiManager;
        [SerializeField] private InventoryManager inventoryManager;
        [SerializeField] private LevelGenerator levelGenerator;
        private MinimapGenerator minimapGenerator;
        private QuestManager questManager;

        private NavMeshSurface m_navMeshSurface;

        [HideInInspector] public PlayerMode playerMode = PlayerMode.Combat;

        // Quest Destination Indicator
        [FormerlySerializedAs("destinationIndicatorComponent")]
        [Header("Destination Indicator")]
        [SerializeField] private QuestLocationIndicatorComponent questLocationIndicatorComponent;

        private void SwitchControlActor(GameObject actor)
        {
            // Assign current actor to currentControllingActor and switch inputs
            if (currentControllingActor) {
                currentControllingActor.GetComponent<InputComponent>().enabled = false;
            }
            currentControllingActor = actor;
            currentControllingActor.GetComponent<InputComponent>().enabled = true;

            // Set actor for destinationIndicatorComponent
            questLocationIndicatorComponent.SetActor(currentControllingActor);
        }

        private void Awake()
        {
            // Get components
            minimapGenerator = GetComponent<MinimapGenerator>();
            questManager = GetComponent<QuestManager>();

            // Set world time flowing speed to normal (for reloading level)
            Time.timeScale = 1f;

            // Subscribe to minimap generator event: minimapGenerated
            minimapGenerator.minimapGenerated.AddListener(mapTexture2D => uiManager.RenderMapUIs(mapTexture2D));

            // Generate the world
            levelGenerator.Generate();

            // Wait for the current frame to finish. This is because there are Destroy() calls in levelGenerator.Generate()
            StartCoroutine(OnLevelGenerationComplete());
        }

        private IEnumerator OnLevelGenerationComplete()
        {
            yield return new WaitForEndOfFrame();

            // Take a bird's eye view shot of the map (for in-game minimap and outputting png)
            minimapGenerator.StreamWorldToMinimap(WorldConfigurations.s_worldGridSize * WorldConfigurations.c_chunkSize);

            // Spawn the player
            SpawnPlayer();

            // Start the first quest
            if (questManager.quests.Length > 0) {
                questManager.StartStory();
            }
            questLocationIndicatorComponent.enabled = true;
        }

        private void SpawnPlayer()
        {
            // Spawn player and focus camera
            player = Instantiate(playerPrefab);
            playerCamera.Follow = player.transform;
            SwitchControlActor(player);

            // Subscribe self and UI manager to player events
            SubjectComponent playerSubject = player.GetComponent<SubjectComponent>();
            playerSubject.AddObserver(this,
                EventType.WeaponChanged,
                EventType.EnteredVehicle,
                EventType.ExitedVehicle,
                EventType.PawnDead
            );
            playerSubject.AddObserver(uiManager,
                EventType.WeaponChanged,
                EventType.IsReloading,
                EventType.InteractionStarted,
                EventType.InteractionEnded,
                EventType.EnteredBuildMode,
                EventType.PlacingStructure,
                EventType.ExitedBuildMode,
                EventType.PawnDead
            );

            // TODO: hacky
            inventoryManager.SetPlayerInventoryComponent(player.GetComponent<PlayerInventoryComponent>());
        }

        // Start is called before the first frame update
        private void Start()
        {
            SetCursor(combatCursorTexture);

            m_navMeshSurface = GetComponent<NavMeshSurface>();

            // Build nav mesh
            // m_navMeshSurface.BuildNavMesh();
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

        // Update is called once per frame
        private void Update()
        {

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
                case EventType.PawnDead:
                    // Free camera
                    playerCamera.Follow = null;
                    // Open Pause Menu
                    Pause(false);
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
            TeleportActor(player, new Vector3(0, 100, 0));
            // Let player float
            player.GetComponent<Rigidbody>().useGravity = false;

            // Configure clearing distance collider according to vehicle camera
            Camera.main.GetComponent<ClearingDistanceColliderComponent>().ConfigureCollider(vehicleCamera.m_Lens.OrthographicSize);
        }

        private void TeleportActor(GameObject actor, Vector3 position)
        {
            actor.transform.position = position;
        }

        private void ExitVehicle(GameObject vehicle)
        {
            // Switch Inputs
            SwitchControlActor(player);

            // Switch camera
            playerCamera.Follow = player.transform;
            playerCamera.Priority = 20;
            vehicleCamera.Priority = 10;

            // Teleport player next to door
            TeleportActor(player, vehicle.transform.Find("Drop Off Point").transform.position);
            // Capture player with gravity again
            player.GetComponent<Rigidbody>().useGravity = true;

            // Configure clearing distance collider according to player camera
            Camera.main.GetComponent<ClearingDistanceColliderComponent>().ConfigureCollider(playerCamera.m_Lens.OrthographicSize);
        }

        public void OpenPauseMenu()
        {
            // Assign status markers
            Pause(true);
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
            Pause(true);
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
            Pause(true);
            playerMode = PlayerMode.Map;

            SetCursor(UICursorTexture);
            uiManager.OpenMap(currentControllingActor.transform);
        }

        public void CloseMap()
        {
            // Assign status markers
            UnPause();
            playerMode = PlayerMode.Combat;

            SetCursor(combatCursorTexture);
            uiManager.CloseMap();
        }

        private void Pause(bool freezeTime)
        {
            if (freezeTime) {
                // Pause the game
                Time.timeScale = 0f;
            }

            // Disable current actor's inputs
            currentControllingActor.GetComponent<InputComponent>().enabled = false;
        }

        private void UnPause()
        {
            // Unpause the game
            Time.timeScale = 1f;

            // Enable current actor's inputs
            currentControllingActor.GetComponent<InputComponent>().enabled = true;
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
