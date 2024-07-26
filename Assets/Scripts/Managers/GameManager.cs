using Abilities.Input;
using Abilities.Observer;
using Cinemachine;
using Entities.Player;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using EventType = Abilities.Observer.EventType;

namespace Managers
{
    public class GameManager : MonoBehaviour, IObserver
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject playerPrefab;

        [Header("Cursor")]
        [SerializeField] private Texture2D combatCursorTexture, UICursorTecture;

        [Header("Gameplay")]
        [SerializeField] private GameObject player;
        [SerializeField] private GameObject currentControllingActor;
        [SerializeField] private CinemachineVirtualCamera playerCamera;
        [SerializeField] private CinemachineVirtualCamera vehicleCamera;

        [Header("Managers")]
        [SerializeField] private UIManager uiManager;
        [SerializeField] private InventoryManager inventoryManager;

        private NavMeshSurface m_navMeshSurface;

        private bool isPaused = false;

        private void SwitchControlActor(GameObject actor)
        {
            if (currentControllingActor) {
                currentControllingActor.GetComponent<InputComponent>().enabled = false;
            }
            currentControllingActor = actor;
            currentControllingActor.GetComponent<InputComponent>().enabled = true;
        }

        private void Awake()
        {
            // Set world time flowing speed to normal
            Time.timeScale = 1f;

            // Spawn player and focus camera
            player = Instantiate(playerPrefab);
            playerCamera.Follow = player.transform;
            SwitchControlActor(player);
            // TODO: hacky
            inventoryManager.playerInventoryComponent = player.GetComponent<PlayerInventoryComponent>();

            // Subscribe self and UI manager to player events
            SubjectComponent playerSubject = player.GetComponent<SubjectComponent>();
            playerSubject.AddObserver(this,
                EventType.WeaponChanged,
                EventType.EnteredVehicle,
                EventType.ExitedVehicle,
                EventType.OpenedInventory,
                EventType.ClosedInventory,
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
                EventType.OpenedInventory,
                EventType.ClosedInventory,
                EventType.PawnDead
            );
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
            } else if (texture == UICursorTecture) {
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
                case EventType.OpenedInventory:
                    SetCursor(UICursorTecture);
                    break;
                case EventType.ClosedInventory:
                    SetCursor(combatCursorTexture);
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
            vehicleCamera.Follow = vehicle.transform;
            playerCamera.Priority = 10;
            vehicleCamera.Priority = 20;

            // Set player's rigidbody to kinematic, and attach the player to vehicle
            player.GetComponent<Rigidbody>().isKinematic = true;
            player.transform.parent = vehicle.transform;

            // Turn off player's rendering and collision
            player.GetComponent<MeshRenderer>().enabled = false;
            player.GetComponent<CapsuleCollider>().enabled = false;
        }

        private void ExitVehicle(GameObject vehicle)
        {
            // Switch Inputs
            SwitchControlActor(player);

            // Switch camera
            playerCamera.Priority = 20;
            vehicleCamera.Priority = 10;

            // Detach player from vehicle, and set its rigidbody back to dynamic
            player.transform.parent = null;
            player.GetComponent<Rigidbody>().isKinematic = false;

            // Turn on player's rendering and collision
            player.GetComponent<MeshRenderer>().enabled = true;
            player.GetComponent<CapsuleCollider>().enabled = true;
            player.GetComponent<Rigidbody>().isKinematic = false;
        }

        public void OnPause()
        {
            if (!isPaused) {
                Pause(true);
            } else {
                UnPause();
            }
            isPaused = !isPaused;
        }

        private void Pause(bool freezeTime)
        {
            if (freezeTime) {
                // Pause the game
                Time.timeScale = 0f;
            }

            // Open pause menu
            uiManager.OpenPauseMenu();
            SetCursor(UICursorTecture);

            // Disable current actor's inputs
            currentControllingActor.GetComponent<InputComponent>().enabled = false;
        }

        private void UnPause()
        {
            // Unpause the game
            Time.timeScale = 1f;

            // Close pause menu
            uiManager.ClosePauseMenu();
            SetCursor(combatCursorTexture);

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
