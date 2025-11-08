using ChroniclesOfADrifter.ECS.Components;

namespace ChroniclesOfADrifter.Terrain;

/// <summary>
/// Generates vegetation (trees, grass, bushes) on terrain
/// </summary>
public class VegetationGenerator
{
    private Random random;
    private const float VEGETATION_FREQUENCY = 0.15f;
    
    public VegetationGenerator(int seed)
    {
        this.random = new Random(seed + 12345); // Offset seed for vegetation
    }
    
    /// <summary>
    /// Generates vegetation for a chunk based on biome and terrain
    /// </summary>
    public void GenerateVegetation(Chunk chunk, BiomeType[] biomeMap)
    {
        int startX = chunk.GetWorldStartX();
        
        for (int localX = 0; localX < Chunk.CHUNK_WIDTH; localX++)
        {
            int worldX = startX + localX;
            BiomeType biome = biomeMap[localX];
            
            // Find the surface height for this column
            int surfaceY = FindSurfaceHeight(chunk, localX);
            
            // Skip if no valid surface or if surface is too deep
            if (surfaceY == -1 || surfaceY >= Chunk.CHUNK_HEIGHT)
            {
                continue;
            }
            
            // Get the surface tile type
            var surfaceTile = chunk.GetTile(localX, surfaceY);
            
            // Only place vegetation on appropriate surface blocks
            if (!IsValidSurface(surfaceTile))
            {
                continue;
            }
            
            // Use noise-based probability for vegetation placement
            float vegetationNoise = SimplexNoise.Noise.CalcPixel1D(worldX, VEGETATION_FREQUENCY);
            float probability = vegetationNoise / 255.0f;
            
            // Determine vegetation type and density based on biome
            var vegetation = DetermineVegetation(biome, probability);
            
            if (vegetation.HasValue)
            {
                chunk.SetVegetation(localX, vegetation.Value);
            }
        }
    }
    
    /// <summary>
    /// Finds the Y coordinate of the surface in a chunk column
    /// </summary>
    private int FindSurfaceHeight(Chunk chunk, int localX)
    {
        for (int y = 0; y < Chunk.CHUNK_HEIGHT; y++)
        {
            var tile = chunk.GetTile(localX, y);
            
            // First solid tile we hit is the surface
            if (tile.IsSolid())
            {
                return y;
            }
        }
        
        return -1; // No surface found
    }
    
    /// <summary>
    /// Checks if a tile is a valid surface for vegetation
    /// </summary>
    private bool IsValidSurface(ECS.Components.TileType tileType)
    {
        return tileType switch
        {
            ECS.Components.TileType.Grass => true,
            ECS.Components.TileType.Sand => true,
            ECS.Components.TileType.Dirt => true,
            ECS.Components.TileType.Snow => true,
            _ => false
        };
    }
    
    /// <summary>
    /// Determines what vegetation (if any) to place based on biome and probability
    /// </summary>
    private ECS.Components.TileType? DetermineVegetation(BiomeType biome, float probability)
    {
        return biome switch
        {
            BiomeType.Forest => DetermineForestVegetation(probability),
            BiomeType.Plains => DeterminePlainsVegetation(probability),
            BiomeType.Desert => DetermineDesertVegetation(probability),
            _ => null
        };
    }
    
    /// <summary>
    /// Determines vegetation for Forest biome (dense trees and bushes)
    /// </summary>
    private ECS.Components.TileType? DetermineForestVegetation(float probability)
    {
        // Forest has 60% vegetation coverage
        if (probability < 0.40f)
        {
            return null; // No vegetation
        }
        else if (probability < 0.70f)
        {
            // 30% chance for trees
            return random.Next(100) < 70 
                ? ECS.Components.TileType.TreeOak 
                : ECS.Components.TileType.TreePine;
        }
        else if (probability < 0.85f)
        {
            // 15% chance for bushes
            return ECS.Components.TileType.Bush;
        }
        else if (probability < 0.95f)
        {
            // 10% chance for tall grass
            return ECS.Components.TileType.TallGrass;
        }
        else
        {
            // 5% chance for flowers
            return ECS.Components.TileType.Flower;
        }
    }
    
    /// <summary>
    /// Determines vegetation for Plains biome (scattered trees and grass)
    /// </summary>
    private ECS.Components.TileType? DeterminePlainsVegetation(float probability)
    {
        // Plains has 30% vegetation coverage
        if (probability < 0.70f)
        {
            return null; // No vegetation
        }
        else if (probability < 0.80f)
        {
            // 10% chance for trees
            return ECS.Components.TileType.TreeOak;
        }
        else if (probability < 0.90f)
        {
            // 10% chance for tall grass
            return ECS.Components.TileType.TallGrass;
        }
        else if (probability < 0.95f)
        {
            // 5% chance for bushes
            return ECS.Components.TileType.Bush;
        }
        else
        {
            // 5% chance for flowers
            return ECS.Components.TileType.Flower;
        }
    }
    
    /// <summary>
    /// Determines vegetation for Desert biome (minimal vegetation, cacti)
    /// </summary>
    private ECS.Components.TileType? DetermineDesertVegetation(float probability)
    {
        // Desert has only 5% vegetation coverage
        if (probability < 0.95f)
        {
            return null; // No vegetation
        }
        else if (probability < 0.98f)
        {
            // 3% chance for cacti
            return ECS.Components.TileType.Cactus;
        }
        else
        {
            // 2% chance for palm trees (oasis-like)
            return ECS.Components.TileType.TreePalm;
        }
    }
}
