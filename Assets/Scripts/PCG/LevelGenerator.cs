using UnityEngine;

namespace PCG
{
    public class Tile
    {
        public BiomeType biome;
    }

    public class LevelGenerator : MonoBehaviour
    {
        [SerializeField] private BiomeData woodlandPrefabs;
        [SerializeField] private BiomeData snowlandPrefabs;

        [SerializeField] private int worldSize = 30;

        private Grid grid;

        private Transform environmentParent;
        private Transform floorsParent;

        [SerializeField] private GameObject floorPrefab;
        [SerializeField] private Color[] floorColors;

        [SerializeField] private WhittakerBiomes whittakerBiomes = new WhittakerBiomes();

        private void Awake()
        {
            grid = GetComponent<Grid>();

            floorPrefab.transform.localScale = grid.cellSize / 10f;

            whittakerBiomes.initializeParameters(worldSize);

            // Offset self to align with world center
            transform.position = new Vector3(-worldSize * grid.cellSize.x / 2, 0, -worldSize * grid.cellSize.z / 2);

            // Generate the world
            Generate();
        }

        public void Generate()
        {
            if (environmentParent) {
                Destroy(environmentParent.gameObject);
            }
            environmentParent = new GameObject("Environment").transform;
            environmentParent.parent = transform;
            floorsParent = new GameObject("Floors").transform;
            floorsParent.parent = environmentParent;

            whittakerBiomes.perlinNoiseOffset = Random.Range(0, 10);

            GenerateFloors();
            // GenerateTrees();
        }

        public void GenerateFloors()
        {
            for (int x = 0; x < worldSize; x++) {
                for (int y = 0; y < worldSize; y++) {
                    GameObject floor = Instantiate(floorPrefab, grid.GetCellCenterWorld(new Vector3Int(x, 0, y)), Quaternion.identity, floorsParent);
                    BiomeType biome = whittakerBiomes.DetermineBiome(x, y);
                    floor.GetComponent<MeshRenderer>().material.color = floorColors[biome.GetHashCode()];
                }
            }
        }

        public void GenerateTrees()
        {
            for (int x = 0; x < worldSize; x++) {
                for (int y = 0; y < worldSize; y++) {
                    if (Random.Range(0, 100) < 20) {
                        GenerateRandomGameObjectFromList(whittakerBiomes.DetermineBiome(x, y) == BiomeType.Woodland ? woodlandPrefabs.trees : snowlandPrefabs.trees, x, y, environmentParent);
                    }
                }
            }
        }

        private void GenerateRandomGameObjectFromList(GameObject[] options, int x, int y, Transform parent)
        {
            Instantiate(options[Random.Range(0, options.Length)], grid.GetCellCenterWorld(new Vector3Int(x, 0, y)), Quaternion.identity, parent);
        }

        public void RenderTiles()
        {

        }
    }
}
