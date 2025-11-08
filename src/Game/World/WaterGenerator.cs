namespace ChroniclesOfADrifter.Terrain;

/// <summary>
/// Generates water bodies including rivers, lakes, and oceans
/// Uses noise-based patterns for natural-looking water placement
/// </summary>
public class WaterGenerator
{
    private int seed;
    private Random random;
    
    // Noise parameters for water generation
    private const float RIVER_FREQUENCY = 0.005f;    // Very low frequency for smooth river paths
    private const float LAKE_FREQUENCY = 0.008f;     // Low frequency for lake placement
    private const float OCEAN_FREQUENCY = 0.002f;    // Ultra low frequency for ocean zones
    
    // Water generation thresholds (normalized 0-1 range)
    private const float RIVER_THRESHOLD = 0.60f;     // Medium threshold = occasional rivers
    private const float LAKE_THRESHOLD = 0.65f;      // High threshold = occasional lakes  
    private const float OCEAN_THRESHOLD = 0.70f;     // Very high threshold = rare ocean zones
    
    // Water depth constants
    private const int RIVER_DEPTH = 2;               // Rivers are 2 blocks deep
    private const int LAKE_DEPTH = 3;                // Lakes are 3 blocks deep
    private const int OCEAN_DEPTH = 5;               // Oceans are 5 blocks deep
    
    public WaterGenerator(int? seed = null)
    {
        this.seed = seed ?? Environment.TickCount;
        this.random = new Random(this.seed);
    }
    
    /// <summary>
    /// Generates water bodies for a chunk after terrain has been generated
    /// This should be called after terrain generation but before vegetation
    /// </summary>
    public void GenerateWater(Chunk chunk, BiomeType[] biomeMap)
    {
        int startX = chunk.GetWorldStartX();
        
        for (int localX = 0; localX < Chunk.CHUNK_WIDTH; localX++)
        {
            int worldX = startX + localX;
            BiomeType biome = biomeMap[localX];
            
            // Check for ocean zones (mainly in beach biomes)
            if (biome == BiomeType.Beach && ShouldGenerateOcean(worldX))
            {
                GenerateOceanColumn(chunk, localX);
                continue;
            }
            
            // Check for rivers (can appear in any biome except desert and snow)
            if (biome != BiomeType.Desert && biome != BiomeType.Snow && ShouldGenerateRiver(worldX))
            {
                GenerateRiverColumn(chunk, localX);
                continue;
            }
            
            // Check for lakes (common in swamp and forest biomes)
            if ((biome == BiomeType.Swamp || biome == BiomeType.Forest || biome == BiomeType.Plains) 
                && ShouldGenerateLake(worldX))
            {
                GenerateLakeColumn(chunk, localX);
            }
        }
    }
    
    /// <summary>
    /// Determines if an ocean should be generated at this X coordinate
    /// </summary>
    private bool ShouldGenerateOcean(int worldX)
    {
        float oceanNoise = SimplexNoise.Noise.CalcPixel1D(worldX, OCEAN_FREQUENCY);
        float normalizedValue = oceanNoise / 255.0f;
        return normalizedValue > OCEAN_THRESHOLD;
    }
    
    /// <summary>
    /// Determines if a river should be generated at this X coordinate
    /// Uses a combination of noise to create meandering river patterns
    /// </summary>
    private bool ShouldGenerateRiver(int worldX)
    {
        // Use two noise layers for more interesting river patterns
        float riverNoise1 = SimplexNoise.Noise.CalcPixel1D(worldX, RIVER_FREQUENCY);
        float riverNoise2 = SimplexNoise.Noise.CalcPixel1D(worldX + 5000, RIVER_FREQUENCY * 1.5f);
        
        float combinedNoise = (riverNoise1 + riverNoise2 * 0.5f) / (255.0f * 1.5f);
        return combinedNoise > RIVER_THRESHOLD;
    }
    
    /// <summary>
    /// Determines if a lake should be generated at this X coordinate
    /// </summary>
    private bool ShouldGenerateLake(int worldX)
    {
        float lakeNoise = SimplexNoise.Noise.CalcPixel1D(worldX, LAKE_FREQUENCY);
        float normalizedValue = lakeNoise / 255.0f;
        return normalizedValue > LAKE_THRESHOLD;
    }
    
    /// <summary>
    /// Generates an ocean column (deep water at the surface)
    /// </summary>
    private void GenerateOceanColumn(Chunk chunk, int localX)
    {
        // Find the surface level
        int surfaceY = FindSurfaceLevel(chunk, localX);
        
        if (surfaceY < 0) return; // No surface found
        
        // Replace surface and some blocks below with water
        for (int y = surfaceY; y < surfaceY + OCEAN_DEPTH && y < Chunk.CHUNK_HEIGHT; y++)
        {
            // Don't replace bedrock or deep stone with water
            var currentType = chunk.GetTile(localX, y);
            if (currentType != ECS.Components.TileType.Bedrock && 
                currentType != ECS.Components.TileType.DeepStone)
            {
                chunk.SetTile(localX, y, ECS.Components.TileType.Water);
            }
        }
    }
    
    /// <summary>
    /// Generates a river column (shallow water at the surface)
    /// </summary>
    private void GenerateRiverColumn(Chunk chunk, int localX)
    {
        int surfaceY = FindSurfaceLevel(chunk, localX);
        
        if (surfaceY < 0) return;
        
        // Rivers carve shallow channels
        // Replace surface block and 1-2 blocks below with water
        for (int y = surfaceY; y < surfaceY + RIVER_DEPTH && y < Chunk.CHUNK_HEIGHT; y++)
        {
            var currentType = chunk.GetTile(localX, y);
            if (currentType != ECS.Components.TileType.Bedrock)
            {
                chunk.SetTile(localX, y, ECS.Components.TileType.Water);
            }
        }
    }
    
    /// <summary>
    /// Generates a lake column (medium depth water in natural depressions)
    /// </summary>
    private void GenerateLakeColumn(Chunk chunk, int localX)
    {
        int surfaceY = FindSurfaceLevel(chunk, localX);
        
        if (surfaceY < 0) return;
        
        // Lakes are medium depth
        for (int y = surfaceY; y < surfaceY + LAKE_DEPTH && y < Chunk.CHUNK_HEIGHT; y++)
        {
            var currentType = chunk.GetTile(localX, y);
            if (currentType != ECS.Components.TileType.Bedrock && 
                currentType != ECS.Components.TileType.DeepStone)
            {
                chunk.SetTile(localX, y, ECS.Components.TileType.Water);
            }
        }
    }
    
    /// <summary>
    /// Finds the surface level (first non-air block from top) for a given column
    /// </summary>
    private int FindSurfaceLevel(Chunk chunk, int localX)
    {
        for (int y = 0; y < Chunk.CHUNK_HEIGHT; y++)
        {
            var tileType = chunk.GetTile(localX, y);
            if (tileType != ECS.Components.TileType.Air)
            {
                return y;
            }
        }
        return -1; // No surface found (shouldn't happen)
    }
}
