using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// System that manages camera zones and applies zone-specific camera settings
/// </summary>
public class CameraZoneSystem : ISystem
{
    private CameraZoneComponent? _currentZone = null;
    private float _targetZoom = 1.0f;
    private float _targetFollowSpeed = 5.0f;
    
    public void Initialize(World world)
    {
        // No initialization needed
    }
    
    public void Update(World world, float deltaTime)
    {
        // Get active camera
        var activeCamera = CameraSystem.GetActiveCamera(world);
        if (activeCamera == null)
            return;
        
        // Find which zone the camera is in
        var newZone = FindActiveZone(world, activeCamera.X, activeCamera.Y);
        
        // Check if we entered a new zone
        if (newZone != _currentZone)
        {
            OnZoneChanged(world, _currentZone, newZone);
            _currentZone = newZone;
        }
        
        // Apply zone settings (with smooth transitions)
        if (_currentZone != null)
        {
            ApplyZoneSettings(world, activeCamera, _currentZone, deltaTime);
        }
    }
    
    /// <summary>
    /// Find the active camera zone at a given position
    /// </summary>
    private CameraZoneComponent? FindActiveZone(World world, float x, float y)
    {
        CameraZoneComponent? highestPriorityZone = null;
        int highestPriority = int.MinValue;
        
        foreach (var entity in world.GetEntitiesWithComponent<CameraZoneComponent>())
        {
            var zone = world.GetComponent<CameraZoneComponent>(entity);
            
            if (zone != null && zone.IsActive && zone.ContainsPoint(x, y))
            {
                if (zone.Priority > highestPriority)
                {
                    highestPriority = zone.Priority;
                    highestPriorityZone = zone;
                }
            }
        }
        
        return highestPriorityZone;
    }
    
    /// <summary>
    /// Called when camera enters a new zone
    /// </summary>
    private void OnZoneChanged(World world, CameraZoneComponent? oldZone, CameraZoneComponent? newZone)
    {
        if (newZone != null)
        {
            Console.WriteLine($"[CameraZone] Entered zone: {newZone.Name}");
            
            // Set target values for smooth transition
            _targetZoom = newZone.Zoom;
            _targetFollowSpeed = newZone.FollowSpeed;
        }
        else if (oldZone != null)
        {
            Console.WriteLine($"[CameraZone] Exited zone: {oldZone.Name}");
            
            // Reset to default values
            _targetZoom = 1.0f;
            _targetFollowSpeed = 5.0f;
        }
    }
    
    /// <summary>
    /// Apply zone settings to camera with smooth transitions
    /// </summary>
    private void ApplyZoneSettings(World world, CameraComponent camera, CameraZoneComponent zone, float deltaTime)
    {
        // Smooth zoom transition
        if (zone.TransitionSpeed > 0)
        {
            float zoomLerpT = 1.0f - MathF.Exp(-zone.TransitionSpeed * deltaTime);
            camera.Zoom = Lerp(camera.Zoom, _targetZoom, zoomLerpT);
            
            // Smooth follow speed transition
            float followSpeedLerpT = 1.0f - MathF.Exp(-zone.TransitionSpeed * deltaTime);
            camera.FollowSpeed = Lerp(camera.FollowSpeed, _targetFollowSpeed, followSpeedLerpT);
        }
        else
        {
            // Instant transition
            camera.Zoom = _targetZoom;
            camera.FollowSpeed = _targetFollowSpeed;
        }
        
        // Update look-ahead settings
        UpdateLookAheadForZone(world, zone);
    }
    
    /// <summary>
    /// Update look-ahead settings based on zone
    /// </summary>
    private void UpdateLookAheadForZone(World world, CameraZoneComponent zone)
    {
        // Find camera entity with look-ahead component
        foreach (var entity in world.GetEntitiesWithComponent<CameraLookAheadComponent>())
        {
            var lookAhead = world.GetComponent<CameraLookAheadComponent>(entity);
            
            if (lookAhead != null)
            {
                lookAhead.IsEnabled = zone.EnableLookAhead;
                lookAhead.LookAheadDistance = zone.LookAheadDistance;
            }
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
    /// Create a camera zone entity
    /// </summary>
    public static Entity CreateCameraZone(World world, string name, float minX, float maxX, float minY, float maxY,
        float zoom = 1.0f, float followSpeed = 5.0f, bool enableLookAhead = true, 
        float lookAheadDistance = 100.0f, int priority = 0, float transitionSpeed = 2.0f)
    {
        var entity = world.CreateEntity();
        
        var zone = new CameraZoneComponent(name, minX, maxX, minY, maxY)
        {
            Zoom = zoom,
            FollowSpeed = followSpeed,
            EnableLookAhead = enableLookAhead,
            LookAheadDistance = lookAheadDistance,
            Priority = priority,
            TransitionSpeed = transitionSpeed
        };
        
        world.AddComponent(entity, zone);
        
        return entity;
    }
}
