using NPBehave;
using UnityEngine;

namespace Entities.AI.Abilities.Bt
{
    public abstract class BtComponent : MonoBehaviour
    {
        protected Root bt;

        protected abstract void InitializeBt();

        private void OnEnable()
        {
            ActivateBt();
        }

        private void OnDisable()
        {
            DeactivateBt();
        }

        private void ActivateBt()
        {
            if (bt.CurrentState == Node.State.INACTIVE) {
                bt.Start();
            }
        }

        private void DeactivateBt()
        {
            if (bt.CurrentState == Node.State.ACTIVE) {
                bt.Stop();
            }
        }
    }
}
