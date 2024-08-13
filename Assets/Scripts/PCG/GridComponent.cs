using UnityEngine;

namespace PCG
{
    public class GridComponent : MonoBehaviour
    {
        private Grid grid;

        private void Awake()
        {
            // Get Grid component
            grid = GetComponent<Grid>();
            // Change grid's cell size accordingly
            grid.cellSize = new Vector3(WorldConfigurations.c_chunkSize, 1, WorldConfigurations.c_chunkSize);
        }

        public Vector3 GetChunkCenterWorld(Chunk chunk)
        {
            Vector3Int chunkCellCoordinate = new Vector3Int(chunk.cellCoordinate.x, 0, chunk.cellCoordinate.y);
            Vector3 chunkCellCenterWorldCoordinate = grid.GetCellCenterWorld(chunkCellCoordinate);
            return new Vector3(chunkCellCenterWorldCoordinate.x, 0, chunkCellCenterWorldCoordinate.z);
        }

        public Vector3 GetChunkBottomLeftCornerWorld(Chunk chunk)
        {
            Vector3Int chunkCellCoordinate = new Vector3Int(chunk.cellCoordinate.x, 0, chunk.cellCoordinate.y);
            return grid.CellToWorld(chunkCellCoordinate);
        }
    }
}
