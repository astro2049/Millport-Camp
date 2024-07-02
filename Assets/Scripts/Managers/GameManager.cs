using Abilities.Observer;
using Cinemachine;
using Entities.Player;
using Entities.Vehicle;
using Unity.AI.Navigation;
using UnityEngine;
using EventType = Abilities.Observer.EventType;

namespace Managers
{
    public class GameManager : MonoBehaviour, IObserver
    {
        [SerializeField] private Texture2D combatCursorTexture, UICursorTecture;
        public GameObject player;
        public CinemachineVirtualCamera playerCamera;
        public CinemachineVirtualCamera vehicleCamera;

        public UIManager uiManager;

        private NavMeshSurface m_navMeshSurface;

        private void Awake()
        {
            // Subscribe to player events:
            // - WeaponChanged, EnteredVehicle, ExitedVehicle
            // - OpenedInventory, ClosedInventory
            // Subscribe UI manager to player events:
            // - WeaponChanged, IsReloading, InteractionStarted, InteractionEnded,
            // - EnteredBuildMode, PlacingStructure, ExitedBuildMode,
            // - OpenedInventory, ClosedInventory
            SubjectComponent playerSubject = player.GetComponent<SubjectComponent>();
            playerSubject.AddObserver(this,
                EventType.WeaponChanged,
                EventType.EnteredVehicle,
                EventType.ExitedVehicle,
                EventType.OpenedInventory,
                EventType.ClosedInventory
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
                EventType.ClosedInventory
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
            }
            return true;
        }

        private void EnterVehicle(GameObject vehicle)
        {
            // Switch Inputs
            player.GetComponent<PlayerInputComponent>().enabled = false;
            vehicle.GetComponent<VehicleInputComponent>().enabled = true;

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
            player.GetComponent<PlayerInputComponent>().enabled = true;
            vehicle.GetComponent<VehicleInputComponent>().enabled = false;

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
    }
}
