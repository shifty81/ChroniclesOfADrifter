using ChroniclesOfADrifter.ECS;

namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// Component that defines a 2D camera for rendering
/// </summary>
public class CameraComponent : IComponent
{
    /// <summary>
    /// Camera position in world space
    /// </summary>
    public float X { get; set; }
    public float Y { get; set; }
    
    /// <summary>
    /// Zoom level (1.0 = normal, > 1.0 = zoomed in, < 1.0 = zoomed out)
    /// </summary>
    public float Zoom { get; set; } = 1.0f;
    
    /// <summary>
    /// Viewport width in pixels
    /// </summary>
    public int ViewportWidth { get; set; }
    
    /// <summary>
    /// Viewport height in pixels
    /// </summary>
    public int ViewportHeight { get; set; }
    
    /// <summary>
    /// Target entity to follow (default = no follow)
    /// </summary>
    public Entity FollowTarget { get; set; } = default;
    
    /// <summary>
    /// Smooth follow speed (0 = instant, higher = slower)
    /// </summary>
    public float FollowSpeed { get; set; } = 5.0f;
    
    /// <summary>
    /// Camera bounds - minimum X coordinate (null = no limit)
    /// </summary>
    public float? MinX { get; set; } = null;
    
    /// <summary>
    /// Camera bounds - maximum X coordinate (null = no limit)
    /// </summary>
    public float? MaxX { get; set; } = null;
    
    /// <summary>
    /// Camera bounds - minimum Y coordinate (null = no limit)
    /// </summary>
    public float? MinY { get; set; } = null;
    
    /// <summary>
    /// Camera bounds - maximum Y coordinate (null = no limit)
    /// </summary>
    public float? MaxY { get; set; } = null;
    
    /// <summary>
    /// Whether this camera is active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    public CameraComponent(int viewportWidth, int viewportHeight)
    {
        ViewportWidth = viewportWidth;
        ViewportHeight = viewportHeight;
        X = viewportWidth / 2.0f;
        Y = viewportHeight / 2.0f;
    }
    
    /// <summary>
    /// Convert world coordinates to screen coordinates
    /// </summary>
    public (float screenX, float screenY) WorldToScreen(float worldX, float worldY)
    {
        float screenX = (worldX - X) * Zoom + ViewportWidth / 2.0f;
        float screenY = (worldY - Y) * Zoom + ViewportHeight / 2.0f;
        return (screenX, screenY);
    }
    
    /// <summary>
    /// Convert screen coordinates to world coordinates
    /// </summary>
    public (float worldX, float worldY) ScreenToWorld(float screenX, float screenY)
    {
        float worldX = (screenX - ViewportWidth / 2.0f) / Zoom + X;
        float worldY = (screenY - ViewportHeight / 2.0f) / Zoom + Y;
        return (worldX, worldY);
    }
}
