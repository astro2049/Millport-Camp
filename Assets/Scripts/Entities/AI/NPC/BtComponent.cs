using NPBehave;
using UnityEngine;

namespace Entities.AI.NPC
{
    public abstract class BtComponent : MonoBehaviour
    {
        protected Root bt;

        protected abstract void InitializeBt();

        private void OnEnable()
        {
            bt.Start();
        }

        private void OnDisable()
        {
            DeactivateBt();
        }

        private void DeactivateBt()
        {
            if (bt.CurrentState == Node.State.ACTIVE) {
                bt.Stop();
            }
        }
    }
}
