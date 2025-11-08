using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.Engine;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// Handles keyboard input for camera controls (zoom)
/// </summary>
public class CameraInputSystem : ISystem
{
    // Key codes for zoom
    private const int KEY_EQUALS = 61;      // = key (zoom in, same as +)
    private const int KEY_MINUS = 45;       // - key (zoom out)
    private const int KEY_PLUS = 43;        // + key (zoom in)
    private const int KEY_UNDERSCORE = 95;  // _ key (zoom out, shift + -)
    
    private float _zoomSpeed = 1.0f; // Zoom change per second
    
    public void Initialize(World world)
    {
        // No initialization needed
    }
    
    public void Update(World world, float deltaTime)
    {
        // Find active camera
        foreach (var entity in world.GetEntitiesWithComponent<CameraComponent>())
        {
            var camera = world.GetComponent<CameraComponent>(entity);
            if (camera == null || !camera.IsActive)
                continue;
            
            // Zoom in (+ or = keys)
            if (EngineInterop.Input_IsKeyDown(KEY_EQUALS) || EngineInterop.Input_IsKeyDown(KEY_PLUS))
            {
                camera.Zoom += _zoomSpeed * deltaTime;
                camera.Zoom = MathF.Min(camera.Zoom, 4.0f); // Max zoom 4x
            }
            
            // Zoom out (- or _ keys)
            if (EngineInterop.Input_IsKeyDown(KEY_MINUS) || EngineInterop.Input_IsKeyDown(KEY_UNDERSCORE))
            {
                camera.Zoom -= _zoomSpeed * deltaTime;
                camera.Zoom = MathF.Max(camera.Zoom, 0.25f); // Min zoom 0.25x
            }
        }
    }
}
