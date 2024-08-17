using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Managers
{
    /*
     * One Wheel Studio (2021) 'Changing Action Maps with Unity's "New" Input System', YouTube, 12 July.
     * Available at: https://www.youtube.com/watch?v=T8fG0D2_V5M (Accessed: 17 August 2024)
     * Game Dev Beginner (2023) 'Singletons in Unity (done right)', YouTube, 22 May.
     * Available at: https://www.youtube.com/watch?v=yhlyoQ2F-NM (Accessed: 17 August 2024).
     */
    public class InputManager : MonoBehaviour
    {
        public InputActionAsset gameplayActionMaps;
        [HideInInspector] public UnityEvent<InputActionMap> actionMapEnabled;
        [HideInInspector] public UnityEvent<InputActionMap> actionMapDisabled;

        public void EnableActionMap(string mapName)
        {
            gameplayActionMaps.FindActionMap(mapName).Enable();
        }

        public void DisableActionMap(string mapName)
        {
            gameplayActionMaps.FindActionMap(mapName).Disable();
        }

        // Singleton
        public static InputManager instance { get; private set; }

        private void Awake()
        {
            if (instance && instance != this) {
                Destroy(this);
            } else {
                instance = this;
            }
        }
    }
}
