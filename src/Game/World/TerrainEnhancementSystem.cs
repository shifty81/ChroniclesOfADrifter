using ChroniclesOfADrifter.ECS.Components;

namespace ChroniclesOfADrifter.Terrain;

/// <summary>
/// Enhances terrain generation with advanced features like biome blending,
/// ore clustering, and mini-biome features
/// </summary>
public class TerrainEnhancementSystem
{
    private int seed;
    private Random random;
    
    // Biome transition parameters
    private const float TRANSITION_BLEND_RANGE = 5.0f; // Blocks to blend between biomes
    private const float ORE_CLUSTER_CHANCE = 0.15f;    // 15% chance for ore veins
    private const int ORE_CLUSTER_SIZE = 8;            // Blocks per ore cluster
    
    public TerrainEnhancementSystem(int seed)
    {
        this.seed = seed;
        this.random = new Random(seed);
    }
    
    /// <summary>
    /// Blend between two biomes for smooth transitions
    /// </summary>
    public BiomeType BlendBiomes(BiomeType biome1, BiomeType biome2, float blendFactor)
    {
        // blendFactor: 0.0 = biome1, 1.0 = biome2, 0.5 = equal blend
        
        // For now, use threshold-based blending
        // In future, could create visual blending of tile colors
        if (blendFactor < 0.3f)
            return biome1;
        else if (blendFactor > 0.7f)
            return biome2;
        else
        {
            // In the transition zone, randomly pick based on blend factor
            return random.NextDouble() < blendFactor ? biome2 : biome1;
        }
    }
    
    /// <summary>
    /// Calculate biome blend factor at a position between two biomes
    /// </summary>
    public float CalculateBiomeBlendFactor(int worldX, BiomeType currentBiome, BiomeType neighborBiome)
    {
        // Use noise to create natural-looking transitions
        // SimplexNoise.Noise.CalcPixel1D returns values in range 0-255
        float biomeNoise = SimplexNoise.Noise.CalcPixel1D(worldX, 0.02f) / 255.0f;
        
        // Smooth step function for transitions
        float t = MathF.Max(0, MathF.Min(1, biomeNoise));
        return t * t * (3 - 2 * t); // Smoothstep
    }
    
    /// <summary>
    /// Generate clustered ore veins instead of random individual ores
    /// </summary>
    public List<(int x, int y, TileType oreType)> GenerateOreCluster(int centerX, int centerY, TileType oreType)
    {
        var orePositions = new List<(int x, int y, TileType oreType)>();
        
        // Create a roughly spherical cluster
        int clusterRadius = ORE_CLUSTER_SIZE / 2;
        
        for (int dx = -clusterRadius; dx <= clusterRadius; dx++)
        {
            for (int dy = -clusterRadius; dy <= clusterRadius; dy++)
            {
                float distance = MathF.Sqrt(dx * dx + dy * dy);
                
                // Probability decreases with distance from center
                float probability = 1.0f - (distance / clusterRadius);
                
                if (probability > 0 && random.NextDouble() < probability)
                {
                    orePositions.Add((centerX + dx, centerY + dy, oreType));
                }
            }
        }
        
        return orePositions;
    }
    
    /// <summary>
    /// Determine if an ore cluster should spawn at this location
    /// </summary>
    public bool ShouldSpawnOreCluster(int worldX, int y, TileType oreType)
    {
        // Use simple hash for better performance in world generation
        // Deterministic based on position, ore type, and seed
        int hash = (seed * 31 + worldX) * 31 + y + (int)oreType * 7919;
        var localRandom = new Random(hash);
        
        return localRandom.NextDouble() < ORE_CLUSTER_CHANCE;
    }
    
    /// <summary>
    /// Get appropriate ore type for a given depth
    /// </summary>
    public TileType? GetOreTypeForDepth(int depth, Random random)
    {
        // Depth-based ore distribution with weighted selection for overlapping ranges
        var possibleOres = new List<TileType>();
        
        if (depth >= 4 && depth <= 8) possibleOres.Add(TileType.CoalOre);
        if (depth >= 6 && depth <= 12) possibleOres.Add(TileType.CopperOre);
        if (depth >= 8 && depth <= 14) possibleOres.Add(TileType.IronOre);
        if (depth >= 12 && depth <= 18) possibleOres.Add(TileType.SilverOre);
        if (depth >= 14 && depth <= 19) possibleOres.Add(TileType.GoldOre);
        if (depth >= 16 && depth <= 19 && random.NextDouble() < 0.3) // Diamonds are rarer
            possibleOres.Add(TileType.DiamondOre);
        
        if (possibleOres.Count == 0)
            return null;
        
        return possibleOres[random.Next(possibleOres.Count)];
    }
    
    /// <summary>
    /// Generate mini-biome features (oasis in desert, clearing in forest, etc.)
    /// </summary>
    public MiniBiomeFeature? GenerateMiniBiomeFeature(int worldX, BiomeType biome)
    {
        // Use noise to determine mini-biome locations
        float featureNoise = SimplexNoise.Noise.CalcPixel1D(worldX, 0.005f) / 255.0f;
        
        // Only spawn mini-biomes occasionally
        if (featureNoise < 0.95f)
            return null;
        
        return biome switch
        {
            BiomeType.Desert => new MiniBiomeFeature
            {
                Type = MiniBiomeType.Oasis,
                CenterX = worldX,
                Radius = 8,
                TileType = TileType.Water,
                VegetationType = TileType.TreePalm
            },
            BiomeType.Forest => new MiniBiomeFeature
            {
                Type = MiniBiomeType.Clearing,
                CenterX = worldX,
                Radius = 6,
                TileType = TileType.TallGrass,
                VegetationType = TileType.Flower
            },
            BiomeType.Snow => new MiniBiomeFeature
            {
                Type = MiniBiomeType.IceLake,
                CenterX = worldX,
                Radius = 10,
                TileType = TileType.Water,
                VegetationType = TileType.Air
            },
            BiomeType.Rocky => new MiniBiomeFeature
            {
                Type = MiniBiomeType.Boulder,
                CenterX = worldX,
                Radius = 4,
                TileType = TileType.Stone,
                VegetationType = TileType.Air
            },
            _ => null
        };
    }
    
    /// <summary>
    /// Enhance cave generation with more variety
    /// </summary>
    public CaveStyle GetCaveStyleForBiome(BiomeType biome)
    {
        return biome switch
        {
            BiomeType.Desert => CaveStyle.LargeCaverns,
            BiomeType.Snow => CaveStyle.IceCaves,
            BiomeType.Swamp => CaveStyle.WaterFilled,
            BiomeType.Jungle => CaveStyle.MushroomCaves,
            BiomeType.Rocky => CaveStyle.TightTunnels,
            _ => CaveStyle.Standard
        };
    }
    
    /// <summary>
    /// Calculate enhanced surface height with more variation
    /// </summary>
    public int CalculateEnhancedSurfaceHeight(int worldX, BiomeType biome, bool amplified = false)
    {
        float baseNoise = SimplexNoise.Noise.CalcPixel1D(worldX, 0.03f) / 255.0f;
        
        // Add biome-specific height variation
        float biomeHeightMod = biome switch
        {
            BiomeType.Rocky => 0.8f,      // Mountains
            BiomeType.Plains => 0.3f,     // Flat
            BiomeType.Forest => 0.5f,     // Gentle hills
            BiomeType.Snow => 0.7f,       // Alpine
            BiomeType.Beach => 0.1f,      // Very flat
            BiomeType.Swamp => 0.2f,      // Low lying
            _ => 0.5f
        };
        
        // Amplified terrain option
        if (amplified)
        {
            biomeHeightMod *= 1.8f;
        }
        
        int baseHeight = 4;
        int variation = (int)(biomeHeightMod * 10);
        
        return baseHeight + (int)(baseNoise * variation);
    }
}

/// <summary>
/// Represents a mini-biome feature within a larger biome
/// </summary>
public class MiniBiomeFeature
{
    public MiniBiomeType Type { get; set; }
    public int CenterX { get; set; }
    public int Radius { get; set; }
    public TileType TileType { get; set; }
    public TileType VegetationType { get; set; }
}

/// <summary>
/// Types of mini-biome features
/// </summary>
public enum MiniBiomeType
{
    Oasis,          // Desert water source
    Clearing,       // Open area in forest
    IceLake,        // Frozen lake in snow biome
    Boulder,        // Rock formation in rocky biome
    MushroomPatch,  // Mushroom area
    FlowerField     // Dense flower area
}

/// <summary>
/// Cave generation styles
/// </summary>
public enum CaveStyle
{
    Standard,
    LargeCaverns,
    TightTunnels,
    IceCaves,
    WaterFilled,
    MushroomCaves
}
