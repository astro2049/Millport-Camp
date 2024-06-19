using Cinemachine;
using Observer;
using Player;
using Unity.AI.Navigation;
using UnityEngine;
using Vehicle;
using EventType = Observer.EventType;

namespace Managers
{
    public class GameManager : MonoBehaviour, IObserver
    {
        public Texture2D cursorTexture;
        public GameObject player;
        public GameObject vehicle;
        public CinemachineVirtualCamera playerCamera;
        public CinemachineVirtualCamera vehicleCamera;

        public UIManager uiManager;

        private NavMeshSurface m_navMeshSurface;

        private void Awake()
        {
            // TODO: This is very hacky
            // Subscribe to subjects
            SubscribeToEntitySubject(player);
            SubscribeToEntitySubject(vehicle);
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

        private void SubscribeToEntitySubject(GameObject entity)
        {
            SubjectComponent entitySubject = entity.GetComponent<SubjectComponent>();
            entitySubject.AddObserver(this);
            entitySubject.AddObserver(uiManager);
        }

        private void UnSubscribeToEntitySubject(GameObject entity)
        {
            SubjectComponent entitySubject = entity.GetComponent<SubjectComponent>();
            entitySubject.RemoveObserver(this);
            entitySubject.RemoveObserver(uiManager);
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
                    // TODO: And we also need to unsubscribe from previous weapon, which means an argument will need to be passed through
                    SubscribeToEntitySubject(player.GetComponent<PlayerStateComponent>().equippedGun.gameObject);
                    break;
                case EventType.EnteredVehicle:
                    EnterVehicle();
                    break;
                case EventType.ExitedVehicle:
                    ExitVehicle();
                    break;
            }
            return true;
        }

        private void EnterVehicle()
        {
            // Switch Inputs
            player.GetComponent<PlayerInputComponent>().enabled = false;
            vehicle.GetComponent<VehicleInputComponent>().enabled = true;

            // Switch camera
            vehicleCamera.Follow = vehicle.transform;
            playerCamera.Priority = 10;
            vehicleCamera.Priority = 20;

            // Attach player to vehicle
            player.transform.parent = vehicle.transform;
            // Turn off player's rendering and collision
            player.GetComponent<MeshRenderer>().enabled = false;
            player.GetComponent<CapsuleCollider>().enabled = false;
        }

        private void ExitVehicle()
        {
            // Switch Inputs
            player.GetComponent<PlayerInputComponent>().enabled = true;
            vehicle.GetComponent<VehicleInputComponent>().enabled = false;

            // Switch camera
            playerCamera.Priority = 20;
            vehicleCamera.Priority = 10;

            // Detach player from vehicle
            player.transform.parent = null;
            // Turn on player's rendering and collision
            player.GetComponent<MeshRenderer>().enabled = true;
            player.GetComponent<CapsuleCollider>().enabled = true;
        }
    }
}
