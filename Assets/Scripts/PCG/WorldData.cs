using UnityEngine;

namespace PCG
{
    [CreateAssetMenu(fileName = "World Data", menuName = "Scriptable Objects/World Data", order = 4)]
    public class WorldData : ScriptableObject
    {
        public int worldGridSize = 32;
        public int chunkSize = 16; // m, Unity unit
        [HideInInspector] public int worldSize = 512;
        public int actorGridSize = 16;
        [HideInInspector] public int actorCellSize = 1; // m, Unity unit
        public int floorTileGridSize = 4;
        [HideInInspector] public int floorTileSize = 4; // m, Unity unit

        private void OnEnable()
        {
            worldSize = worldGridSize * chunkSize;
            actorCellSize = chunkSize / actorGridSize;
            floorTileSize = chunkSize / floorTileGridSize;
        }
    }
}
