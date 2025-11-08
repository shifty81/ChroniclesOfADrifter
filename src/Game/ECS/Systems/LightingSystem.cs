using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.Terrain;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// System that handles light propagation and fog of war
/// Calculates lighting levels based on light sources and updates visibility
/// </summary>
public class LightingSystem : ISystem
{
    // Surface layers (Y < 10) have full ambient light during day
    private const int SURFACE_Y_THRESHOLD = 10;
    
    // Shallow underground (Y 10-19) has dim ambient light
    private const int SHALLOW_UNDERGROUND_Y = 19;
    
    // Deep underground (Y >= 20) is pitch black without light sources
    
    // Light falloff constants
    private const float LIGHT_FALLOFF_RATE = 0.15f;
    
    // Minimum light level to consider a tile "lit"
    private const float MIN_VISIBLE_LIGHT = 0.1f;
    
    private ChunkManager? _chunkManager;
    
    public void Initialize(World world)
    {
        // Get chunk manager from shared resources
        _chunkManager = world.GetSharedResource<ChunkManager>("ChunkManager");
    }
    
    public void Update(World world, float deltaTime)
    {
        // Get chunk manager from shared resources
        if (_chunkManager == null)
        {
            _chunkManager = world.GetSharedResource<ChunkManager>("ChunkManager");
            if (_chunkManager == null)
            {
                return; // No terrain to light
            }
        }
        
        // Reset light levels for all tiles
        ResetLightLevels(world);
        
        // Calculate ambient light based on depth
        ApplyAmbientLight(world);
        
        // Apply light from all light sources
        var lightSources = world.GetEntitiesWithComponent<LightSourceComponent>();
        foreach (var entity in lightSources)
        {
            var lightSource = world.GetComponent<LightSourceComponent>(entity);
            var position = world.GetComponent<PositionComponent>(entity);
            
            if (lightSource != null && position != null && lightSource.IsActive)
            {
                PropagateLightFromSource(world, position, lightSource);
            }
        }
        
        // Update visibility and exploration based on light levels
        UpdateVisibilityAndExploration(world);
    }
    
    /// <summary>
    /// Reset all light levels to zero (before recalculating)
    /// </summary>
    private void ResetLightLevels(World world)
    {
        var entities = world.GetEntitiesWithComponent<LightingComponent>();
        foreach (var entity in entities)
        {
            var lighting = world.GetComponent<LightingComponent>(entity);
            if (lighting != null)
            {
                lighting.LightLevel = 0f;
                lighting.IsCurrentlyVisible = false;
            }
        }
    }
    
    /// <summary>
    /// Apply ambient light based on depth (surface is bright, underground is dark)
    /// </summary>
    private void ApplyAmbientLight(World world)
    {
        if (_chunkManager == null) return;
        
        var entities = world.GetEntitiesWithComponent<LightingComponent>();
        foreach (var entity in entities)
        {
            var position = world.GetComponent<PositionComponent>(entity);
            var lighting = world.GetComponent<LightingComponent>(entity);
            
            if (position != null && lighting != null)
            {
                float ambientLight = GetAmbientLightForDepth(position.Y);
                lighting.LightLevel = Math.Max(lighting.LightLevel, ambientLight);
            }
        }
    }
    
    /// <summary>
    /// Get the ambient light level for a given Y coordinate (depth)
    /// </summary>
    private float GetAmbientLightForDepth(float y)
    {
        if (y < SURFACE_Y_THRESHOLD)
        {
            // Surface: Full daylight
            return 1.0f;
        }
        else if (y < SHALLOW_UNDERGROUND_Y)
        {
            // Shallow underground: Dim light (0.3 to 0.0)
            float depth = y - SURFACE_Y_THRESHOLD;
            float maxDepth = SHALLOW_UNDERGROUND_Y - SURFACE_Y_THRESHOLD;
            return Math.Max(0.3f - (depth / maxDepth) * 0.3f, 0f);
        }
        else
        {
            // Deep underground: Pitch black
            return 0f;
        }
    }
    
    /// <summary>
    /// Propagate light from a single light source
    /// Uses a simple radial distance-based falloff
    /// </summary>
    private void PropagateLightFromSource(World world, PositionComponent sourcePos, LightSourceComponent lightSource)
    {
        if (_chunkManager == null) return;
        
        float radius = lightSource.Radius;
        float intensity = lightSource.Intensity;
        
        // Convert world coordinates to block coordinates
        int centerBlockX = (int)(sourcePos.X / 32); // Assuming 32 pixels per block
        int centerBlockY = (int)(sourcePos.Y / 32);
        
        // Check blocks in a square around the light source
        int radiusBlocks = (int)Math.Ceiling(radius);
        
        for (int dy = -radiusBlocks; dy <= radiusBlocks; dy++)
        {
            for (int dx = -radiusBlocks; dx <= radiusBlocks; dx++)
            {
                int blockX = centerBlockX + dx;
                int blockY = centerBlockY + dy;
                
                // Calculate distance from light source
                float distance = (float)Math.Sqrt(dx * dx + dy * dy);
                
                if (distance > radius)
                {
                    continue; // Outside light radius
                }
                
                // Calculate light intensity at this distance
                float lightAtDistance = intensity * (1.0f - (distance / radius));
                lightAtDistance = Math.Max(lightAtDistance, 0f);
                
                // Apply light to tile at this position
                ApplyLightToBlock(world, blockX, blockY, lightAtDistance);
            }
        }
    }
    
    /// <summary>
    /// Apply light to a specific block position
    /// </summary>
    private void ApplyLightToBlock(World world, int blockX, int blockY, float lightIntensity)
    {
        // Find entity at this block position
        // Note: In a real implementation, we'd have a spatial index
        // For now, we search through entities with position and lighting
        var entities = world.GetEntitiesWithComponent<LightingComponent>();
        
        foreach (var entity in entities)
        {
            var position = world.GetComponent<PositionComponent>(entity);
            var lighting = world.GetComponent<LightingComponent>(entity);
            
            if (position != null && lighting != null)
            {
                // Check if this entity is at the target block position
                int entityBlockX = (int)(position.X / 32);
                int entityBlockY = (int)(position.Y / 32);
                
                if (entityBlockX == blockX && entityBlockY == blockY)
                {
                    lighting.LightLevel = Math.Max(lighting.LightLevel, lightIntensity);
                    break;
                }
            }
        }
    }
    
    /// <summary>
    /// Update visibility and exploration status based on light levels
    /// </summary>
    private void UpdateVisibilityAndExploration(World world)
    {
        var entities = world.GetEntitiesWithComponent<LightingComponent>();
        
        foreach (var entity in entities)
        {
            var lighting = world.GetComponent<LightingComponent>(entity);
            
            if (lighting != null)
            {
                // Tile is visible if it has sufficient light
                lighting.IsCurrentlyVisible = lighting.LightLevel >= MIN_VISIBLE_LIGHT;
                
                // Once visible, mark as explored (permanent)
                if (lighting.IsCurrentlyVisible)
                {
                    lighting.IsExplored = true;
                }
            }
        }
    }
}
