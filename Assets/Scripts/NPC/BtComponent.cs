using NPBehave;
using UnityEngine;

namespace NPC
{
    public abstract class BtComponent : MonoBehaviour
    {
        protected Root bt;

        // Start is called before the first frame update
        private void Start()
        {

        }

        // Update is called once per frame
        private void Update()
        {

        }

        protected abstract void InitializeBt();

        public void DeactivateBt()
        {
            if (bt.CurrentState == Node.State.ACTIVE) {
                bt.Stop();
            }
        }
    }
}
