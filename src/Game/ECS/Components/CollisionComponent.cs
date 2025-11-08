namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// Collision component - defines an axis-aligned bounding box (AABB) for collision detection
/// </summary>
public class CollisionComponent : IComponent
{
    /// <summary>
    /// Width of the collision box
    /// </summary>
    public float Width { get; set; }
    
    /// <summary>
    /// Height of the collision box
    /// </summary>
    public float Height { get; set; }
    
    /// <summary>
    /// Offset from entity position to collision box center (X)
    /// Allows the collision box to be offset from the entity's position
    /// </summary>
    public float OffsetX { get; set; }
    
    /// <summary>
    /// Offset from entity position to collision box center (Y)
    /// Allows the collision box to be offset from the entity's position
    /// </summary>
    public float OffsetY { get; set; }
    
    /// <summary>
    /// Whether this entity blocks other entities (static colliders like walls)
    /// </summary>
    public bool IsStatic { get; set; }
    
    /// <summary>
    /// Whether this entity should check for collisions with terrain
    /// </summary>
    public bool CheckTerrain { get; set; }
    
    /// <summary>
    /// Whether this entity should check for collisions with other entities
    /// </summary>
    public bool CheckEntities { get; set; }
    
    /// <summary>
    /// Collision layer mask for filtering collisions
    /// </summary>
    public CollisionLayer Layer { get; set; }
    
    /// <summary>
    /// Which layers this entity can collide with
    /// </summary>
    public CollisionLayer CollidesWith { get; set; }
    
    public CollisionComponent(float width, float height, 
        float offsetX = 0, float offsetY = 0,
        bool isStatic = false,
        bool checkTerrain = true,
        bool checkEntities = true,
        CollisionLayer layer = CollisionLayer.Default,
        CollisionLayer collidesWith = CollisionLayer.All)
    {
        Width = width;
        Height = height;
        OffsetX = offsetX;
        OffsetY = offsetY;
        IsStatic = isStatic;
        CheckTerrain = checkTerrain;
        CheckEntities = checkEntities;
        Layer = layer;
        CollidesWith = collidesWith;
    }
    
    /// <summary>
    /// Gets the actual bounds of the collision box in world space
    /// </summary>
    public (float left, float top, float right, float bottom) GetBounds(float entityX, float entityY)
    {
        float centerX = entityX + OffsetX;
        float centerY = entityY + OffsetY;
        float halfWidth = Width / 2;
        float halfHeight = Height / 2;
        
        return (
            left: centerX - halfWidth,
            top: centerY - halfHeight,
            right: centerX + halfWidth,
            bottom: centerY + halfHeight
        );
    }
}

/// <summary>
/// Collision layers for filtering which entities can collide with each other
/// </summary>
[Flags]
public enum CollisionLayer
{
    None = 0,
    Default = 1 << 0,      // Default layer
    Player = 1 << 1,        // Player entities
    Enemy = 1 << 2,         // Enemy entities
    Projectile = 1 << 3,    // Projectiles (arrows, bullets)
    Item = 1 << 4,          // Collectible items
    Terrain = 1 << 5,       // Terrain/world geometry
    Trigger = 1 << 6,       // Trigger zones (don't block movement)
    All = ~0                // Everything
}
