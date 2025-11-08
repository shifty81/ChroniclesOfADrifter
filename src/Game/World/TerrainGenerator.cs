namespace ChroniclesOfADrifter.Terrain;

/// <summary>
/// Biome types that affect terrain generation
/// </summary>
public enum BiomeType
{
    Plains,     // Grassy surface, gentle hills, standard underground
    Desert,     // Sandy surface, minimal vegetation, sandstone underground
    Forest,     // Dense trees, grass surface, dirt/stone underground
    Snow,       // Snow-covered surface, pine trees, frozen underground
    Swamp,      // Water-logged surface, swamp trees, peat underground
    Rocky,      // Rocky surface, minimal topsoil, exposed stone
    Jungle,     // Dense vegetation, vines, clay underground
    Beach,      // Sandy transition between land and water
}

/// <summary>
/// Generates terrain using Perlin noise for realistic landscapes
/// </summary>
public class TerrainGenerator
{
    private int seed;
    private Random random;
    private VegetationGenerator vegetationGenerator;
    
    // Noise parameters
    private const float SURFACE_FREQUENCY = 0.03f;  // Controls surface terrain smoothness
    private const float BIOME_FREQUENCY = 0.005f;   // Controls biome transitions
    private const float CAVE_FREQUENCY = 0.08f;     // Controls cave generation
    
    public TerrainGenerator(int? seed = null)
    {
        this.seed = seed ?? Environment.TickCount;
        this.random = new Random(this.seed);
        this.vegetationGenerator = new VegetationGenerator(this.seed);
        SimplexNoise.Noise.Seed = this.seed;
    }
    
    /// <summary>
    /// Generates terrain for a chunk
    /// </summary>
    public void GenerateChunk(Chunk chunk)
    {
        int startX = chunk.GetWorldStartX();
        
        // Store biome info for vegetation generation
        BiomeType[] biomeMap = new BiomeType[Chunk.CHUNK_WIDTH];
        
        for (int localX = 0; localX < Chunk.CHUNK_WIDTH; localX++)
        {
            int worldX = startX + localX;
            
            // Determine biome for this X coordinate
            BiomeType biome = GetBiomeAt(worldX);
            biomeMap[localX] = biome;
            
            // Generate surface height using noise (0-9 range for surface)
            float surfaceNoise = SimplexNoise.Noise.CalcPixel1D(worldX, SURFACE_FREQUENCY);
            int surfaceHeight = 4 + (int)((surfaceNoise / 255.0f) * 6); // Range: 4-9
            
            // Generate column of tiles
            for (int y = 0; y < Chunk.CHUNK_HEIGHT; y++)
            {
                ECS.Components.TileType tileType;
                
                if (y < surfaceHeight)
                {
                    // Above surface - air
                    tileType = ECS.Components.TileType.Air;
                }
                else if (y == surfaceHeight)
                {
                    // Surface block
                    tileType = GetSurfaceBlock(biome);
                }
                else if (y < surfaceHeight + 4)
                {
                    // Topsoil (layers 0-3 below surface)
                    tileType = GetTopsoilBlock(biome);
                }
                else if (y < Chunk.CHUNK_HEIGHT - 1)
                {
                    // Underground layers (4-19 below surface)
                    tileType = GetUndergroundBlock(worldX, y, biome);
                }
                else
                {
                    // Bottom layer - bedrock
                    tileType = ECS.Components.TileType.Bedrock;
                }
                
                chunk.SetTile(localX, y, tileType);
            }
        }
        
        // Generate vegetation after terrain is complete
        vegetationGenerator.GenerateVegetation(chunk, biomeMap);
        
        chunk.SetGenerated();
    }
    
    /// <summary>
    /// Determines the biome at a given world X coordinate using temperature and moisture
    /// </summary>
    private BiomeType GetBiomeAt(int worldX)
    {
        // Use two noise functions for temperature and moisture
        float temperatureNoise = SimplexNoise.Noise.CalcPixel1D(worldX, BIOME_FREQUENCY);
        float moistureNoise = SimplexNoise.Noise.CalcPixel1D(worldX + 10000, BIOME_FREQUENCY * 1.2f); // Offset seed
        
        float temperature = temperatureNoise / 255.0f; // 0-1 range
        float moisture = moistureNoise / 255.0f;       // 0-1 range
        
        // Biome selection based on temperature and moisture
        // Cold climates (temperature < 0.25)
        if (temperature < 0.25f)
        {
            return BiomeType.Snow;
        }
        // Hot and dry (temperature > 0.75, moisture < 0.3)
        else if (temperature > 0.75f && moisture < 0.3f)
        {
            return BiomeType.Desert;
        }
        // Hot and wet (temperature > 0.7, moisture > 0.6)
        else if (temperature > 0.7f && moisture > 0.6f)
        {
            return BiomeType.Jungle;
        }
        // Moderate and wet (moisture > 0.7)
        else if (moisture > 0.7f)
        {
            return BiomeType.Swamp;
        }
        // Moderate and forested (temperature 0.4-0.7, moisture 0.4-0.7)
        else if (temperature >= 0.4f && temperature <= 0.7f && moisture >= 0.4f && moisture <= 0.7f)
        {
            return BiomeType.Forest;
        }
        // Rocky terrain (low moisture, moderate temp)
        else if (moisture < 0.3f && temperature >= 0.3f && temperature <= 0.6f)
        {
            return BiomeType.Rocky;
        }
        // Beach/coastal (specific moisture/temp combo)
        else if (moisture >= 0.35f && moisture <= 0.45f)
        {
            return BiomeType.Beach;
        }
        // Default to plains
        else
        {
            return BiomeType.Plains;
        }
    }
    
    /// <summary>
    /// Gets the surface block type for a biome
    /// </summary>
    private ECS.Components.TileType GetSurfaceBlock(BiomeType biome)
    {
        return biome switch
        {
            BiomeType.Plains => ECS.Components.TileType.Grass,
            BiomeType.Desert => ECS.Components.TileType.Sand,
            BiomeType.Forest => ECS.Components.TileType.Grass,
            BiomeType.Snow => ECS.Components.TileType.Snow,
            BiomeType.Swamp => ECS.Components.TileType.Grass,  // Muddy grass
            BiomeType.Rocky => ECS.Components.TileType.Stone,  // Exposed rock
            BiomeType.Jungle => ECS.Components.TileType.Grass, // Jungle grass
            BiomeType.Beach => ECS.Components.TileType.Sand,   // Beach sand
            _ => ECS.Components.TileType.Grass
        };
    }
    
    /// <summary>
    /// Gets the topsoil block type for a biome
    /// </summary>
    private ECS.Components.TileType GetTopsoilBlock(BiomeType biome)
    {
        return biome switch
        {
            BiomeType.Plains => ECS.Components.TileType.Dirt,
            BiomeType.Desert => ECS.Components.TileType.Sand,
            BiomeType.Forest => ECS.Components.TileType.Dirt,
            BiomeType.Snow => ECS.Components.TileType.Snow,    // Snow layers
            BiomeType.Swamp => ECS.Components.TileType.Dirt,   // Peat/mud (using dirt)
            BiomeType.Rocky => ECS.Components.TileType.Stone,  // Thin topsoil, mostly stone
            BiomeType.Jungle => ECS.Components.TileType.Dirt,  // Rich soil (clay would be better)
            BiomeType.Beach => ECS.Components.TileType.Sand,   // Deep sand
            _ => ECS.Components.TileType.Dirt
        };
    }
    
    /// <summary>
    /// Gets the underground block type with depth-based variation and ore generation
    /// </summary>
    private ECS.Components.TileType GetUndergroundBlock(int worldX, int y, BiomeType biome)
    {
        // Check for cave pocket
        float caveNoise = SimplexNoise.Noise.CalcPixel2D(worldX, y, CAVE_FREQUENCY);
        if (caveNoise > 200) // Threshold for cave generation
        {
            return ECS.Components.TileType.Air;
        }
        
        // Determine base stone type by depth
        ECS.Components.TileType baseType;
        if (y < 15)
        {
            baseType = ECS.Components.TileType.Stone;
        }
        else
        {
            baseType = ECS.Components.TileType.DeepStone;
        }
        
        // Generate ores based on depth
        float oreNoise = SimplexNoise.Noise.CalcPixel2D(worldX * 2, y * 2, 0.1f);
        
        // Copper ore (common, shallow)
        if (y >= 10 && y < 18 && oreNoise > 230)
        {
            return ECS.Components.TileType.CopperOre;
        }
        
        // Iron ore (uncommon, medium depth)
        if (y >= 14 && y < 24 && oreNoise > 240)
        {
            return ECS.Components.TileType.IronOre;
        }
        
        // Gold ore (rare, deep)
        if (y >= 20 && y < 28 && oreNoise > 245)
        {
            return ECS.Components.TileType.GoldOre;
        }
        
        return baseType;
    }
}
