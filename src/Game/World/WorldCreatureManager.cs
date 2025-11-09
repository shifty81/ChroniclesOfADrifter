using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.ECS.Systems;
using ChroniclesOfADrifter.Terrain;

namespace ChroniclesOfADrifter.WorldManagement;

/// <summary>
/// Manages creature spawning across the world with chunk-based spawn tracking.
/// Ensures creatures spawn naturally when chunks are loaded and manages spawn density.
/// </summary>
public class WorldCreatureManager
{
    private readonly CreatureSpawnSystem spawnSystem;
    private readonly Random random;
    private readonly Dictionary<int, HashSet<Entity>> chunkCreatures;
    private readonly Dictionary<int, float> chunkSpawnCooldowns;
    private readonly int worldSeed;
    
    // Spawn configuration
    private const float SPAWN_CHECK_INTERVAL = 5f;  // Check every 5 seconds
    private const float MIN_SPAWN_DISTANCE = 300f;  // Min distance from player
    private const float MAX_SPAWN_DISTANCE = 800f;  // Max spawn distance
    private const int MAX_CREATURES_PER_CHUNK = 8;  // Maximum creatures per chunk
    
    /// <summary>
    /// Spawn density multipliers by biome (0.0 to 1.0)
    /// </summary>
    private readonly Dictionary<BiomeType, float> biomeSpawnDensity = new()
    {
        { BiomeType.Plains, 0.7f },
        { BiomeType.Desert, 0.3f },
        { BiomeType.Forest, 0.9f },
        { BiomeType.Snow, 0.5f },
        { BiomeType.Swamp, 0.8f },
        { BiomeType.Rocky, 0.4f },
        { BiomeType.Jungle, 1.0f },
        { BiomeType.Beach, 0.4f }
    };
    
    /// <summary>
    /// Spawn density multipliers by depth (0.0 to 1.0)
    /// </summary>
    private float GetDepthSpawnDensity(int depth)
    {
        if (depth <= 3) return 0.6f;      // Surface - moderate spawns
        if (depth <= 8) return 0.8f;      // Shallow underground - more spawns
        if (depth <= 14) return 1.0f;     // Deep underground - frequent spawns
        return 1.2f;                      // Very deep - high spawn rate
    }
    
    public WorldCreatureManager(int seed)
    {
        worldSeed = seed;
        random = new Random(seed);
        spawnSystem = new CreatureSpawnSystem(seed);
        chunkCreatures = new Dictionary<int, HashSet<Entity>>();
        chunkSpawnCooldowns = new Dictionary<int, float>();
    }
    
    /// <summary>
    /// Updates creature spawning based on loaded chunks and player position
    /// </summary>
    public void Update(World world, ChunkManager chunkManager, float playerX, float playerY, float deltaTime)
    {
        int playerChunkX = Chunk.WorldToChunkCoord((int)playerX);
        int playerDepth = GetDepthAtPosition(playerY);
        
        // Get loaded chunks
        var loadedChunks = chunkManager.GetLoadedChunks().ToList();
        
        foreach (var chunk in loadedChunks)
        {
            int chunkX = chunk.ChunkX;
            
            // Initialize tracking for new chunks
            if (!chunkCreatures.ContainsKey(chunkX))
            {
                chunkCreatures[chunkX] = new HashSet<Entity>();
                chunkSpawnCooldowns[chunkX] = 0f;
            }
            
            // Update cooldown
            chunkSpawnCooldowns[chunkX] -= deltaTime;
            
            // Skip if on cooldown or chunk is too close to player
            if (chunkSpawnCooldowns[chunkX] > 0f)
            {
                continue;
            }
            
            // Calculate chunk center world position
            float chunkCenterX = chunk.GetWorldStartX() + (Chunk.CHUNK_WIDTH * GameConstants.BlockSize / 2f);
            float chunkCenterY = playerY; // Use player Y for now
            
            // Check distance from player
            float distanceFromPlayer = MathF.Abs(chunkCenterX - playerX);
            
            // Skip if too close to player
            if (distanceFromPlayer < MIN_SPAWN_DISTANCE)
            {
                continue;
            }
            
            // Skip if too far (beyond render distance)
            if (distanceFromPlayer > MAX_SPAWN_DISTANCE)
            {
                continue;
            }
            
            // Clean up dead creatures in this chunk
            CleanupDeadCreatures(world, chunkX);
            
            // Check if chunk needs more creatures
            int currentCreatureCount = chunkCreatures[chunkX].Count;
            if (currentCreatureCount >= MAX_CREATURES_PER_CHUNK)
            {
                continue;
            }
            
            // Determine biome and depth for this chunk location
            BiomeType biome = GetBiomeForChunk(chunk, chunkManager);
            
            // Calculate spawn chance based on biome, depth, and density
            float biomeDensity = biomeSpawnDensity.GetValueOrDefault(biome, 0.5f);
            float depthDensity = GetDepthSpawnDensity(playerDepth);
            float spawnChance = biomeDensity * depthDensity * 0.3f; // 30% base chance
            
            // Try to spawn creatures
            if (random.NextDouble() < spawnChance)
            {
                int spawnCount = random.Next(1, 4); // Spawn 1-3 creatures
                spawnCount = Math.Min(spawnCount, MAX_CREATURES_PER_CHUNK - currentCreatureCount);
                
                // Random position within chunk
                float spawnX = chunk.GetWorldStartX() + random.Next(Chunk.CHUNK_WIDTH) * GameConstants.BlockSize;
                float spawnY = chunkCenterY + (float)(random.NextDouble() * 200 - 100);
                
                // Spawn creatures appropriate for biome and depth
                var spawnedCreatures = spawnSystem.SpawnByBiomeAndDepth(world, biome, playerDepth, spawnX, spawnY, spawnCount);
                
                // Track creatures in this chunk
                foreach (var creature in spawnedCreatures)
                {
                    chunkCreatures[chunkX].Add(creature);
                }
            }
            
            // Reset cooldown
            chunkSpawnCooldowns[chunkX] = SPAWN_CHECK_INTERVAL;
        }
        
        // Clean up tracking for unloaded chunks
        CleanupUnloadedChunks(loadedChunks);
    }
    
    /// <summary>
    /// Initial spawn for a newly generated chunk
    /// </summary>
    public void InitialChunkSpawn(World world, Chunk chunk, BiomeType biome)
    {
        int chunkX = chunk.ChunkX;
        
        // Initialize tracking
        if (!chunkCreatures.ContainsKey(chunkX))
        {
            chunkCreatures[chunkX] = new HashSet<Entity>();
        }
        
        // Determine number of initial creatures based on biome
        float biomeDensity = biomeSpawnDensity.GetValueOrDefault(biome, 0.5f);
        int initialCreatureCount = (int)(MAX_CREATURES_PER_CHUNK * biomeDensity * random.NextDouble());
        
        if (initialCreatureCount == 0) return;
        
        // Spawn creatures at various positions within the chunk
        for (int i = 0; i < initialCreatureCount; i++)
        {
            float spawnX = chunk.GetWorldStartX() + random.Next(Chunk.CHUNK_WIDTH) * GameConstants.BlockSize;
            float spawnY = random.Next(Chunk.CHUNK_HEIGHT) * GameConstants.BlockSize;
            int depth = (int)(spawnY / GameConstants.BlockSize);
            
            var spawnedCreatures = spawnSystem.SpawnByBiomeAndDepth(world, biome, depth, spawnX, spawnY, 1);
            foreach (var creature in spawnedCreatures)
            {
                chunkCreatures[chunkX].Add(creature);
            }
        }
    }
    
    /// <summary>
    /// Gets the biome type for a chunk
    /// </summary>
    private BiomeType GetBiomeForChunk(Chunk chunk, ChunkManager chunkManager)
    {
        // Sample a tile from the middle of the chunk to determine biome
        int middleX = chunk.GetWorldStartX() + Chunk.CHUNK_WIDTH / 2;
        var surfaceTile = chunkManager.GetTile(middleX, Chunk.SURFACE_HEIGHT);
        
        // Determine biome from surface tile type
        return surfaceTile switch
        {
            TileType.Grass => BiomeType.Plains,
            TileType.Sand => BiomeType.Desert,
            TileType.Snow => BiomeType.Snow,
            TileType.Dirt => BiomeType.Forest,
            TileType.Stone => BiomeType.Rocky,
            _ => BiomeType.Plains
        };
    }
    
    /// <summary>
    /// Gets the depth level from Y position
    /// </summary>
    private int GetDepthAtPosition(float y)
    {
        return (int)(y / GameConstants.BlockSize);
    }
    
    /// <summary>
    /// Removes dead creatures from tracking
    /// </summary>
    private void CleanupDeadCreatures(World world, int chunkX)
    {
        if (!chunkCreatures.ContainsKey(chunkX)) return;
        
        var creatures = chunkCreatures[chunkX].ToList();
        foreach (var creature in creatures)
        {
            // Check if entity still exists and has health
            var health = world.GetComponent<HealthComponent>(creature);
            if (health == null || !health.IsAlive)
            {
                chunkCreatures[chunkX].Remove(creature);
            }
        }
    }
    
    /// <summary>
    /// Removes tracking for chunks that are no longer loaded
    /// </summary>
    private void CleanupUnloadedChunks(List<Chunk> loadedChunks)
    {
        var loadedChunkIds = new HashSet<int>(loadedChunks.Select(c => c.ChunkX));
        var trackedChunks = chunkCreatures.Keys.ToList();
        
        foreach (var chunkX in trackedChunks)
        {
            if (!loadedChunkIds.Contains(chunkX))
            {
                chunkCreatures.Remove(chunkX);
                chunkSpawnCooldowns.Remove(chunkX);
            }
        }
    }
    
    /// <summary>
    /// Gets statistics about creatures in the world
    /// </summary>
    public (int totalCreatures, int activeChunks, Dictionary<CreatureType, int> creatureTypeCounts) GetStatistics(World world)
    {
        int totalCreatures = 0;
        var creatureTypeCounts = new Dictionary<CreatureType, int>();
        
        foreach (var creatures in chunkCreatures.Values)
        {
            foreach (var creature in creatures)
            {
                var creatureComp = world.GetComponent<CreatureComponent>(creature);
                if (creatureComp != null)
                {
                    totalCreatures++;
                    
                    if (!creatureTypeCounts.ContainsKey(creatureComp.Type))
                    {
                        creatureTypeCounts[creatureComp.Type] = 0;
                    }
                    creatureTypeCounts[creatureComp.Type]++;
                }
            }
        }
        
        return (totalCreatures, chunkCreatures.Count, creatureTypeCounts);
    }
    
    /// <summary>
    /// Clears all creature tracking (useful for world reset)
    /// </summary>
    public void Clear()
    {
        chunkCreatures.Clear();
        chunkSpawnCooldowns.Clear();
    }
}
