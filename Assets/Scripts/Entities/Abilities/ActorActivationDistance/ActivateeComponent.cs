using UnityEngine;

namespace Entities.Abilities.ActorActivationDistance
{
    public class ActivateeComponent : MonoBehaviour
    {
        [SerializeField] private GameObject pawn;

        private void Awake()
        {
            Deactivate();
        }

        public void Activate()
        {
            pawn.SetActive(true);
        }

        public void Deactivate()
        {
            pawn.SetActive(false);
        }
    }
}
