using UnityEngine;

namespace PCG.Generators.Chunks
{
    public class ChunkGridComponent : MonoBehaviour
    {
        [SerializeField] private Grid grid;

        [SerializeField] private WorldData worldData;

        // TODO: hacky order...?
        public void Initialize()
        {
            // Change grid's cell size accordingly
            grid.cellSize = new Vector3(worldData.chunkSize, 1, worldData.chunkSize);
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
