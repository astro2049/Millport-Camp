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
        public Texture2D cursorTexture;
        public GameObject player;
        public CinemachineVirtualCamera playerCamera;
        public CinemachineVirtualCamera vehicleCamera;

        public UIManager uiManager;

        private NavMeshSurface m_navMeshSurface;

        private void Awake()
        {
            // Subscribe to player events:
            // - WeaponChanged, EnteredVehicle, ExitedVehicle
            // Subscribe UI manager to player events:
            // - WeaponChanged, IsReloading, InteractionStarted, InteractionEnded, PlacingStructure
            SubjectComponent playerSubject = player.GetComponent<SubjectComponent>();
            playerSubject.AddObserver(this,
                EventType.WeaponChanged,
                EventType.EnteredVehicle,
                EventType.ExitedVehicle);
            playerSubject.AddObserver(uiManager,
                EventType.WeaponChanged,
                EventType.IsReloading,
                EventType.InteractionStarted,
                EventType.InteractionEnded,
                EventType.EnteredBuildMode,
                EventType.ExitedBuildMode,
                EventType.PlacingStructure
            );
        }

        // Start is called before the first frame update
        private void Start()
        {
            // https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Cursor.SetCursor.html
            Cursor.SetCursor(cursorTexture, new Vector2(cursorTexture.width / 2f, cursorTexture.height / 2f), CursorMode.Auto);

            m_navMeshSurface = GetComponent<NavMeshSurface>();

            // Build nav mesh
            // m_navMeshSurface.BuildNavMesh();
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
                // Vehicle
                case EventType.EnteredVehicle:
                    EnterVehicle((mcEvent as MCEventWEntity)!.entity);
                    break;
                case EventType.ExitedVehicle:
                    ExitVehicle((mcEvent as MCEventWEntity)!.entity);
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
