using System;
using System.Collections;
using System.Collections.Generic;
using Entities.Ocean;
using Managers.UI.Map;
using PCG.Generators.POIs;
using PCG.Generators.Roads;
using PCG.Generators.Terrain;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.Events;
using Vector3 = UnityEngine.Vector3;

namespace PCG.Generators
{
    [Serializable]
    public class Biome
    {
        public BiomeType biomeType;
        public readonly List<Chunk> chunks = new List<Chunk>();
        public BiomeData.BiomeData biomeData;
    }

    public class LevelGenerator : MonoBehaviour
    {
        [Header("World Data")]
        [SerializeField] private WorldData worldData;

        [Header("Biomes")]
        public Biome[] biomes;

        [Header("Components")]
        private TerrainGenerator terrainGenerator;
        private ActorsPlacer actorsPlacer;
        private TownGenerator townGenerator;
        private RoadNetGenerator roadNetGenerator;
        private TownRoadsPlacer townRoadsPlacer;

        [SerializeField] private GridComponent grid;
        [SerializeField] private NavMeshSurface navMeshSurface;
        private MinimapGenerator minimapGenerator;
        [SerializeField] private OceanComponent ocean;

        [Header("Gameplay")]
        [SerializeField] private GameObject aimPlane;

        [HideInInspector] public UnityEvent levelGenerated = new UnityEvent();

        // TODO: hacky order...
        private void Initialize()
        {
            // Get components
            terrainGenerator = GetComponent<TerrainGenerator>();
            actorsPlacer = GetComponent<ActorsPlacer>();
            townGenerator = GetComponent<TownGenerator>();
            roadNetGenerator = GetComponent<RoadNetGenerator>();
            townRoadsPlacer = GetComponent<TownRoadsPlacer>();
            minimapGenerator = GetComponent<MinimapGenerator>();

            // Initialize components
            worldData.Initialize();
            grid.Initialize();
            terrainGenerator.Initialize();
            actorsPlacer.Initialize();

            // Position grid start to bottom-left of continent
            grid.transform.position = new Vector3(-worldData.worldSize / 2f, 0, -worldData.worldSize / 2f);
            // Generate aim plane. Same size as the terrain.
            aimPlane.transform.localScale = Vector3.one * worldData.worldSize / 10f;
        }

        /* Generate the game world */
        public void Generate()
        {
            Initialize();

            // Resize the ocean
            ocean.Resize(worldData.worldSize);

            // Generate the world map
            terrainGenerator.GenerateTerrainAndBiomes();
            actorsPlacer.PlaceBases();
            townGenerator.GenerateTowns(actorsPlacer.basePositions);
            roadNetGenerator.PlaceRoadNet(actorsPlacer.basePositions);
            townRoadsPlacer.PlaceRoadSegments();
            actorsPlacer.PlacePlants();

            // Wait for the current frame to finish. This is because there are Destroy() calls in Generate()
            StartCoroutine(LevelGenerationPart2());
        }

        private IEnumerator LevelGenerationPart2()
        {
            yield return new WaitForEndOfFrame();

            // Build nav mesh
            navMeshSurface.BuildNavMesh();

            // Take a bird's eye view shot of the map (for in-game minimap and outputting png)
            minimapGenerator.StreamWorldToMinimap(worldData.worldSize);

            // Place the actors
            actorsPlacer.PlaceActors();

            // Tell game manager to start the quests
            levelGenerated.Invoke();
        }
    }
}
