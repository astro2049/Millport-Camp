using UnityEngine;
using UnityEngine.InputSystem;

namespace Managers.GameManager
{
    public class GameManagerInputComponent : MonoBehaviour
    {
        private GameManager gameManager;

        private void Awake()
        {
            gameManager = GetComponent<GameManager>();
        }

        // UI inputs
        public void OnEscape(InputAction.CallbackContext context)
        {
            if (context.phase != InputActionPhase.Performed) {
                return;
            }

            if (gameManager.playerMode == PlayerMode.Combat) {
                gameManager.OpenPauseMenu();
            } else if (gameManager.playerMode == PlayerMode.PauseMenu) {
                gameManager.ClosePauseMenu();
            } else if (gameManager.playerMode == PlayerMode.Inventory) {
                gameManager.CloseInventory();
            } else if (gameManager.playerMode == PlayerMode.Map) {
                gameManager.CloseMap();
            }
        }

        public void OnTab(InputAction.CallbackContext context)
        {
            if (context.phase != InputActionPhase.Performed) {
                return;
            }

            if (gameManager.playerMode == PlayerMode.Combat) {
                gameManager.OpenInventory();
            } else if (gameManager.playerMode == PlayerMode.Inventory) {
                gameManager.CloseInventory();
            }
        }

        public void OnM(InputAction.CallbackContext context)
        {
            if (context.phase != InputActionPhase.Performed) {
                return;
            }

            if (gameManager.playerMode == PlayerMode.Combat) {
                gameManager.OpenMap();
            } else if (gameManager.playerMode == PlayerMode.Map) {
                gameManager.CloseMap();
            }
        }
    }
}
