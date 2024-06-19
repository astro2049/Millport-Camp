using UnityEngine;

namespace NPC
{
    public abstract class PerceptionComponent : MonoBehaviour
    {
        public abstract void OnPerceptionTriggerEnter(Collider other);

        public abstract void OnPerceptionTriggerExit(Collider other);
    }
}
