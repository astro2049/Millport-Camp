using UnityEngine;

namespace Gameplay
{
    public class WorldConfigurations : MonoBehaviour
    {
        public static int s_worldGridSize = 32;
        public const int c_chunkSize = 16; // m, Unity unit

        [SerializeField] private int worldGridSize;

        private void Awake()
        {
            s_worldGridSize = worldGridSize;
        }
    }
}
