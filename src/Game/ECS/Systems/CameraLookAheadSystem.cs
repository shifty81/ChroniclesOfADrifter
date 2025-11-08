using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// System that implements camera look-ahead based on target velocity
/// Shifts camera focus in the direction of player movement
/// </summary>
public class CameraLookAheadSystem : ISystem
{
    public void Initialize(World world)
    {
        // No initialization needed
    }
    
    public void Update(World world, float deltaTime)
    {
        // Find all cameras with look-ahead enabled
        foreach (var cameraEntity in world.GetEntitiesWithComponent<CameraComponent>())
        {
            var camera = world.GetComponent<CameraComponent>(cameraEntity);
            var lookAhead = world.GetComponent<CameraLookAheadComponent>(cameraEntity);
            
            if (camera == null || lookAhead == null || !lookAhead.IsEnabled || !camera.IsActive)
                continue;
            
            // Get the target entity the camera is following
            if (camera.FollowTarget.Id == 0)
                continue;
            
            var targetPos = world.GetComponent<PositionComponent>(camera.FollowTarget);
            var targetVel = world.GetComponent<VelocityComponent>(camera.FollowTarget);
            
            if (targetPos == null || targetVel == null)
                continue;
            
            // Calculate current speed
            float speed = MathF.Sqrt(targetVel.VX * targetVel.VX + targetVel.VY * targetVel.VY);
            
            float targetOffsetX = 0.0f;
            float targetOffsetY = 0.0f;
            
            if (speed > lookAhead.MinVelocityThreshold)
            {
                // Calculate normalized direction
                float directionX = targetVel.VX / speed;
                float directionY = targetVel.VY / speed;
                
                // Calculate desired look-ahead offset
                targetOffsetX = directionX * lookAhead.LookAheadDistance;
                targetOffsetY = directionY * lookAhead.LookAheadDistance;
            }
            // else: target is stopped, return to center (targetOffset = 0)
            
            // Smoothly interpolate current look-ahead to target
            float t = 1.0f - MathF.Exp(-lookAhead.LookAheadSpeed * deltaTime);
            lookAhead.CurrentOffsetX = Lerp(lookAhead.CurrentOffsetX, targetOffsetX, t);
            lookAhead.CurrentOffsetY = Lerp(lookAhead.CurrentOffsetY, targetOffsetY, t);
            
            // Apply look-ahead offset to camera position
            // This creates the "looking ahead" effect
            // We apply this after the camera follows the target in CameraSystem
            camera.X += lookAhead.CurrentOffsetX * lookAhead.OffsetScale;
            camera.Y += lookAhead.CurrentOffsetY * lookAhead.OffsetScale;
        }
    }
    
    /// <summary>
    /// Linear interpolation helper
    /// </summary>
    private float Lerp(float a, float b, float t)
    {
        return a + (b - a) * MathF.Max(0, MathF.Min(1, t));
    }
    
    /// <summary>
    /// Enable look-ahead for a camera
    /// </summary>
    public static void EnableLookAhead(World world, Entity cameraEntity, 
        float lookAheadDistance = 100.0f, float lookAheadSpeed = 3.0f, float offsetScale = 0.1f)
    {
        var existing = world.GetComponent<CameraLookAheadComponent>(cameraEntity);
        if (existing != null)
        {
            existing.IsEnabled = true;
            existing.LookAheadDistance = lookAheadDistance;
            existing.LookAheadSpeed = lookAheadSpeed;
            existing.OffsetScale = offsetScale;
        }
        else
        {
            world.AddComponent(cameraEntity, new CameraLookAheadComponent(lookAheadDistance, lookAheadSpeed)
            {
                OffsetScale = offsetScale
            });
        }
    }
    
    /// <summary>
    /// Disable look-ahead for a camera
    /// </summary>
    public static void DisableLookAhead(World world, Entity cameraEntity)
    {
        var lookAhead = world.GetComponent<CameraLookAheadComponent>(cameraEntity);
        if (lookAhead != null)
        {
            lookAhead.IsEnabled = false;
        }
    }
}
