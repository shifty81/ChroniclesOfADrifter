using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// System that manages camera behavior including following targets and applying bounds
/// </summary>
public class CameraSystem : ISystem
{
    public void Initialize(World world)
    {
        // No initialization needed
    }
    
    public void Update(World world, float deltaTime)
    {
        foreach (var entity in world.GetEntitiesWithComponent<CameraComponent>())
        {
            var camera = world.GetComponent<CameraComponent>(entity);
            if (camera == null || !camera.IsActive)
                continue;
            
            // Follow target if set
            if (camera.FollowTarget.Id != 0)
            {
                var targetPos = world.GetComponent<PositionComponent>(camera.FollowTarget);
                if (targetPos != null)
                {
                    FollowTarget(camera, targetPos, deltaTime);
                }
            }
            
            // Apply bounds
            ApplyBounds(camera);
        }
    }
    
    /// <summary>
    /// Smoothly follow the target entity
    /// </summary>
    private void FollowTarget(CameraComponent camera, PositionComponent target, float deltaTime)
    {
        if (camera.FollowSpeed <= 0)
        {
            // Instant follow
            camera.X = target.X;
            camera.Y = target.Y;
        }
        else
        {
            // Smooth follow using lerp
            float t = 1.0f - MathF.Exp(-camera.FollowSpeed * deltaTime);
            camera.X = Lerp(camera.X, target.X, t);
            camera.Y = Lerp(camera.Y, target.Y, t);
        }
    }
    
    /// <summary>
    /// Apply camera bounds to keep camera within defined limits
    /// </summary>
    private void ApplyBounds(CameraComponent camera)
    {
        // Calculate half viewport size in world units
        float halfViewportWidth = (camera.ViewportWidth / 2.0f) / camera.Zoom;
        float halfViewportHeight = (camera.ViewportHeight / 2.0f) / camera.Zoom;
        
        // Apply X bounds
        if (camera.MinX.HasValue)
        {
            float minCameraX = camera.MinX.Value + halfViewportWidth;
            camera.X = MathF.Max(camera.X, minCameraX);
        }
        if (camera.MaxX.HasValue)
        {
            float maxCameraX = camera.MaxX.Value - halfViewportWidth;
            camera.X = MathF.Min(camera.X, maxCameraX);
        }
        
        // Apply Y bounds
        if (camera.MinY.HasValue)
        {
            float minCameraY = camera.MinY.Value + halfViewportHeight;
            camera.Y = MathF.Max(camera.Y, minCameraY);
        }
        if (camera.MaxY.HasValue)
        {
            float maxCameraY = camera.MaxY.Value - halfViewportHeight;
            camera.Y = MathF.Min(camera.Y, maxCameraY);
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
    /// Get the active camera from the world, or null if none exists
    /// </summary>
    public static CameraComponent? GetActiveCamera(World world)
    {
        foreach (var entity in world.GetEntitiesWithComponent<CameraComponent>())
        {
            var camera = world.GetComponent<CameraComponent>(entity);
            if (camera != null && camera.IsActive)
            {
                return camera;
            }
        }
        return null;
    }
    
    /// <summary>
    /// Set camera to follow an entity
    /// </summary>
    public static void SetFollowTarget(World world, Entity cameraEntity, Entity targetEntity, float followSpeed = 5.0f)
    {
        var camera = world.GetComponent<CameraComponent>(cameraEntity);
        if (camera != null)
        {
            camera.FollowTarget = targetEntity;
            camera.FollowSpeed = followSpeed;
        }
    }
    
    /// <summary>
    /// Set camera bounds
    /// </summary>
    public static void SetBounds(World world, Entity cameraEntity, float? minX, float? maxX, float? minY, float? maxY)
    {
        var camera = world.GetComponent<CameraComponent>(cameraEntity);
        if (camera != null)
        {
            camera.MinX = minX;
            camera.MaxX = maxX;
            camera.MinY = minY;
            camera.MaxY = maxY;
        }
    }
    
    /// <summary>
    /// Set camera zoom
    /// </summary>
    public static void SetZoom(World world, Entity cameraEntity, float zoom)
    {
        var camera = world.GetComponent<CameraComponent>(cameraEntity);
        if (camera != null)
        {
            camera.Zoom = MathF.Max(0.1f, zoom); // Prevent negative or zero zoom
        }
    }
}
