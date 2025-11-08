using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.Terrain;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// System that handles swimming mechanics and water interaction
/// </summary>
public class SwimmingSystem : ISystem
{
    private ChunkManager? chunkManager;
    
    public void Initialize(World world)
    {
        Console.WriteLine("[Swimming] Swimming system initialized");
    }
    
    /// <summary>
    /// Set the chunk manager for water detection
    /// </summary>
    public void SetChunkManager(ChunkManager manager)
    {
        chunkManager = manager;
    }
    
    public void Update(World world, float deltaTime)
    {
        if (chunkManager == null) return;
        
        // Update all entities with swimming capability
        foreach (var entity in world.GetEntitiesWithComponent<SwimmingComponent>())
        {
            var swimming = world.GetComponent<SwimmingComponent>(entity);
            var position = world.GetComponent<PositionComponent>(entity);
            var velocity = world.GetComponent<VelocityComponent>(entity);
            var health = world.GetComponent<HealthComponent>(entity);
            
            if (swimming == null || position == null) continue;
            
            // Check if entity is in water
            bool inWater = IsInWater(position);
            bool wasInWater = swimming.IsInWater;
            swimming.IsInWater = inWater;
            
            if (inWater)
            {
                // Apply swimming mechanics
                if (velocity != null)
                {
                    // Reduce movement speed in water
                    velocity.VX *= swimming.SwimSpeed;
                    velocity.VY *= swimming.SwimSpeed;
                    
                    // Apply water flow if present
                    ApplyWaterFlow(entity, world, position, velocity);
                }
                
                // Handle breathing underwater
                if (!swimming.CanBreatheUnderwater)
                {
                    swimming.CurrentBreath -= deltaTime;
                    
                    if (swimming.CurrentBreath <= 0)
                    {
                        // Drowning damage
                        if (health != null)
                        {
                            health.CurrentHealth -= swimming.DrowningDamage * deltaTime;
                            
                            // Log drowning
                            if (swimming.CurrentBreath < -1.0f) // Only log occasionally
                            {
                                var isPlayer = world.HasComponent<PlayerComponent>(entity);
                                if (isPlayer)
                                {
                                    Console.WriteLine($"[Swimming] Player is drowning! Health: {health.CurrentHealth:F1}");
                                }
                                swimming.CurrentBreath = -0.5f; // Reset timer for next message
                            }
                        }
                    }
                    else if (swimming.CurrentBreath < swimming.MaxBreathTime * 0.3f)
                    {
                        // Low breath warning (only once when entering low breath)
                        if (wasInWater && swimming.CurrentBreath > swimming.MaxBreathTime * 0.29f)
                        {
                            var isPlayer = world.HasComponent<PlayerComponent>(entity);
                            if (isPlayer)
                            {
                                Console.WriteLine($"[Swimming] Low breath! {swimming.CurrentBreath:F1}s remaining");
                            }
                        }
                    }
                }
            }
            else
            {
                // Restore breath when not in water
                if (swimming.CurrentBreath < swimming.MaxBreathTime)
                {
                    swimming.CurrentBreath += deltaTime * 2.0f; // Restore breath faster than depleting
                    swimming.CurrentBreath = Math.Min(swimming.CurrentBreath, swimming.MaxBreathTime);
                }
                
                // Log when exiting water
                if (wasInWater)
                {
                    var isPlayer = world.HasComponent<PlayerComponent>(entity);
                    if (isPlayer)
                    {
                        Console.WriteLine("[Swimming] Exited water, breath restored");
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Check if a position is in water
    /// </summary>
    private bool IsInWater(PositionComponent position)
    {
        if (chunkManager == null) return false;
        
        int blockX = (int)Math.Floor(position.X / GameConstants.BlockSize);
        int blockY = (int)Math.Floor(position.Y / GameConstants.BlockSize);
        
        var tile = chunkManager.GetTile(blockX, blockY);
        return tile == TileType.Water;
    }
    
    /// <summary>
    /// Apply water flow forces to entities
    /// </summary>
    private void ApplyWaterFlow(Entity entity, World world, PositionComponent position, VelocityComponent velocity)
    {
        if (chunkManager == null) return;
        
        int blockX = (int)Math.Floor(position.X / GameConstants.BlockSize);
        int blockY = (int)Math.Floor(position.Y / GameConstants.BlockSize);
        
        // Get flow from water at this position
        var flow = GetWaterFlowAt(world, blockX, blockY);
        
        if (flow != null && flow.FlowStrength > 0)
        {
            // Apply flow force
            float flowForce = flow.FlowStrength * 50.0f; // Base flow force
            velocity.VX += flow.FlowX * flowForce * 0.016f; // Approximate deltaTime
            velocity.VY += flow.FlowY * flowForce * 0.016f;
            
            // Cap maximum velocity from flow
            float maxFlowVelocity = 200.0f;
            velocity.VX = Math.Clamp(velocity.VX, -maxFlowVelocity, maxFlowVelocity);
            velocity.VY = Math.Clamp(velocity.VY, -maxFlowVelocity, maxFlowVelocity);
        }
    }
    
    /// <summary>
    /// Get water flow at a specific block position
    /// For now, returns simulated flow based on water body type
    /// </summary>
    private WaterFlowComponent? GetWaterFlowAt(World world, int blockX, int blockY)
    {
        if (chunkManager == null) return null;
        
        var tile = chunkManager.GetTile(blockX, blockY);
        if (tile != TileType.Water) return null;
        
        // Simulate flow based on surrounding water
        // Rivers flow horizontally, oceans have tidal flow
        // For now, create a simple flow pattern
        
        // Check if this is near the surface (Y >= 0)
        bool isSurface = blockY >= 0;
        
        if (isSurface)
        {
            // Surface water - create gentle horizontal flow
            // Use noise-based flow direction
            float flowNoise = (float)Math.Sin(blockX * 0.1) * 0.3f;
            return new WaterFlowComponent(flowNoise, 0, 0.3f, WaterBodyType.River);
        }
        else
        {
            // Underground water - mostly still
            return new WaterFlowComponent(0, 0, 0.1f, WaterBodyType.UndergroundWater);
        }
    }
    
    /// <summary>
    /// Check if an entity is currently swimming
    /// </summary>
    public static bool IsSwimming(World world, Entity entity)
    {
        var swimming = world.GetComponent<SwimmingComponent>(entity);
        return swimming != null && swimming.IsInWater;
    }
}
