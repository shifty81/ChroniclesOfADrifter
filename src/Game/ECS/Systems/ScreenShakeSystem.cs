using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// System that processes screen shake effects for cameras
/// </summary>
public class ScreenShakeSystem : ISystem
{
    private Random _random = new Random();
    
    public void Initialize(World world)
    {
        // No initialization needed
    }
    
    public void Update(World world, float deltaTime)
    {
        // Process all cameras with screen shake
        foreach (var entity in world.GetEntitiesWithComponent<ScreenShakeComponent>())
        {
            var shake = world.GetComponent<ScreenShakeComponent>(entity);
            var camera = world.GetComponent<CameraComponent>(entity);
            
            if (shake == null || camera == null)
                continue;
            
            if (shake.IsActive)
            {
                // Update elapsed time
                shake.ElapsedTime += deltaTime;
                
                // Calculate decay factor (shake intensity decreases over time)
                float progress = shake.ElapsedTime / shake.Duration;
                float currentIntensity = shake.Intensity * (1.0f - progress);
                
                // Generate shake offset using noise-like pattern
                float angle = shake.ElapsedTime * shake.Frequency;
                shake.OffsetX = MathF.Sin(angle) * currentIntensity;
                shake.OffsetY = MathF.Cos(angle * 1.3f) * currentIntensity;
                
                // Add some randomness for more natural shake
                shake.OffsetX += (_random.NextSingle() * 2 - 1) * currentIntensity * 0.3f;
                shake.OffsetY += (_random.NextSingle() * 2 - 1) * currentIntensity * 0.3f;
            }
            else
            {
                // Reset shake offsets when not active
                shake.OffsetX = 0f;
                shake.OffsetY = 0f;
            }
        }
    }
    
    /// <summary>
    /// Trigger a screen shake effect on a camera
    /// </summary>
    public static void TriggerShake(World world, Entity cameraEntity, float intensity, float duration, float frequency = 25f)
    {
        var shake = world.GetComponent<ScreenShakeComponent>(cameraEntity);
        
        if (shake != null)
        {
            shake.Trigger(intensity, duration, frequency);
        }
    }
    
    /// <summary>
    /// Trigger a light shake (e.g., player taking damage)
    /// </summary>
    public static void TriggerLightShake(World world, Entity cameraEntity)
    {
        TriggerShake(world, cameraEntity, intensity: 5f, duration: 0.2f);
    }
    
    /// <summary>
    /// Trigger a medium shake (e.g., enemy hit, explosion)
    /// </summary>
    public static void TriggerMediumShake(World world, Entity cameraEntity)
    {
        TriggerShake(world, cameraEntity, intensity: 10f, duration: 0.3f);
    }
    
    /// <summary>
    /// Trigger a heavy shake (e.g., boss attack, large explosion)
    /// </summary>
    public static void TriggerHeavyShake(World world, Entity cameraEntity)
    {
        TriggerShake(world, cameraEntity, intensity: 20f, duration: 0.5f, frequency: 30f);
    }
    
    /// <summary>
    /// Get the current shake offset for a camera
    /// </summary>
    public static (float x, float y) GetShakeOffset(World world, Entity cameraEntity)
    {
        var shake = world.GetComponent<ScreenShakeComponent>(cameraEntity);
        
        if (shake != null && shake.IsActive)
        {
            return (shake.OffsetX, shake.OffsetY);
        }
        
        return (0f, 0f);
    }
}
