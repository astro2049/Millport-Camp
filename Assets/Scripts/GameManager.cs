using Cinemachine;
using TMPro;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.InputSystem;
using Vehicle;

public class GameManager : MonoBehaviour
{
    public Texture2D cursorTexture;
    public TextMeshProUGUI interactText;
    public GameObject player;
    public CinemachineVirtualCamera playerCamera;
    public CinemachineVirtualCamera vehicleCamera;

    private NavMeshSurface m_navMeshSurface;

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

    public void ShowInteraction(InteractableComponent interactableComponent)
    {
        interactText.enabled = true;
        player.GetComponent<PlayerStateComponent>().currentInteractable = interactableComponent;
    }

    public void CloseInteraction()
    {
        interactText.enabled = false;
        player.GetComponent<PlayerStateComponent>().currentInteractable = null;
    }

    public void EnterVehicle(GameObject vehicle)
    {
        // Switch Inputs
        player.GetComponent<PlayerInput>().enabled = false;
        player.GetComponent<PlayerInputComponent>().enabled = false;
        vehicle.GetComponent<PlayerInput>().enabled = true;
        vehicle.GetComponent<VehicleInputComponent>().enabled = true;

        // Switch camera
        vehicleCamera.Follow = vehicle.transform;
        playerCamera.Priority = 10;
        vehicleCamera.Priority = 20;

        // Turn off player's rendering and collision
        player.GetComponent<MeshRenderer>().enabled = false;
        player.GetComponent<CapsuleCollider>().enabled = false;

        // Attach player to vehicle
        player.transform.parent = vehicle.transform;
    }

    public void ExitVehicle(GameObject vehicle)
    {
        // Switch Inputs
        player.GetComponent<PlayerInput>().enabled = true;
        player.GetComponent<PlayerInputComponent>().enabled = true;
        vehicle.GetComponent<PlayerInput>().enabled = false;
        vehicle.GetComponent<VehicleInputComponent>().enabled = false;

        // Switch camera
        playerCamera.Priority = 20;
        vehicleCamera.Priority = 10;

        // Turn on player's rendering and collision
        player.GetComponent<MeshRenderer>().enabled = true;
        player.GetComponent<CapsuleCollider>().enabled = true;

        // Detach player from vehicle
        player.transform.parent = null;
    }
}
