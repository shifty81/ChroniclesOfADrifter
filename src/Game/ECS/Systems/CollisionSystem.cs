using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.Terrain;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// Collision detection and resolution system
/// Handles entity-to-terrain and entity-to-entity collisions
/// </summary>
public class CollisionSystem : ISystem
{
    private ChunkManager? _chunkManager;
    private const float BLOCK_SIZE = 32.0f; // Size of a block in world units
    
    public void Initialize(World world)
    {
        // Chunk manager will be set externally by scenes that need terrain collision
    }
    
    /// <summary>
    /// Sets the chunk manager for terrain collision detection
    /// </summary>
    public void SetChunkManager(ChunkManager chunkManager)
    {
        _chunkManager = chunkManager;
    }
    
    public void Update(World world, float deltaTime)
    {
        // Get all entities with collision components
        var collisionEntities = world.GetEntitiesWithComponent<CollisionComponent>().ToList();
        
        foreach (var entity in collisionEntities)
        {
            var collision = world.GetComponent<CollisionComponent>(entity);
            var position = world.GetComponent<PositionComponent>(entity);
            var velocity = world.GetComponent<VelocityComponent>(entity);
            
            if (collision == null || position == null || velocity == null)
                continue;
            
            // Skip static entities (they don't move)
            if (collision.IsStatic)
                continue;
            
            // Calculate desired new position based on velocity
            float desiredX = position.X + velocity.VX * deltaTime;
            float desiredY = position.Y + velocity.VY * deltaTime;
            
            // Check terrain collision if enabled
            if (collision.CheckTerrain && _chunkManager != null)
            {
                (desiredX, desiredY) = ResolveTerrainCollision(
                    collision, position.X, position.Y, desiredX, desiredY);
            }
            
            // Check entity-to-entity collision if enabled
            if (collision.CheckEntities)
            {
                (desiredX, desiredY) = ResolveEntityCollisions(
                    world, entity, collision, position.X, position.Y, desiredX, desiredY, collisionEntities);
            }
            
            // Update position with collision-resolved coordinates
            position.X = desiredX;
            position.Y = desiredY;
        }
    }
    
    /// <summary>
    /// Resolves collision with terrain blocks
    /// </summary>
    private (float x, float y) ResolveTerrainCollision(
        CollisionComponent collision, 
        float currentX, float currentY,
        float desiredX, float desiredY)
    {
        if (_chunkManager == null)
            return (desiredX, desiredY);
        
        var bounds = collision.GetBounds(desiredX, desiredY);
        
        // Convert world coordinates to block coordinates
        int minBlockX = (int)Math.Floor(bounds.left / BLOCK_SIZE);
        int maxBlockX = (int)Math.Floor(bounds.right / BLOCK_SIZE);
        int minBlockY = (int)Math.Floor(bounds.top / BLOCK_SIZE);
        int maxBlockY = (int)Math.Floor(bounds.bottom / BLOCK_SIZE);
        
        // Check for collision with solid blocks
        bool collisionDetected = false;
        
        for (int blockX = minBlockX; blockX <= maxBlockX; blockX++)
        {
            for (int blockY = minBlockY; blockY <= maxBlockY; blockY++)
            {
                var tileType = _chunkManager.GetTile(blockX, blockY);
                
                // Check if tile is solid
                if (tileType.IsSolid())
                {
                    collisionDetected = true;
                    break;
                }
            }
            if (collisionDetected)
                break;
        }
        
        // If collision detected, try sliding along walls
        if (collisionDetected)
        {
            // Try moving only horizontally
            var boundsX = collision.GetBounds(desiredX, currentY);
            bool canMoveX = true;
            
            minBlockX = (int)Math.Floor(boundsX.left / BLOCK_SIZE);
            maxBlockX = (int)Math.Floor(boundsX.right / BLOCK_SIZE);
            minBlockY = (int)Math.Floor(boundsX.top / BLOCK_SIZE);
            maxBlockY = (int)Math.Floor(boundsX.bottom / BLOCK_SIZE);
            
            for (int blockX = minBlockX; blockX <= maxBlockX && canMoveX; blockX++)
            {
                for (int blockY = minBlockY; blockY <= maxBlockY; blockY++)
                {
                    var tileType = _chunkManager.GetTile(blockX, blockY);
                    if (tileType.IsSolid())
                    {
                        canMoveX = false;
                        break;
                    }
                }
            }
            
            // Try moving only vertically
            var boundsY = collision.GetBounds(currentX, desiredY);
            bool canMoveY = true;
            
            minBlockX = (int)Math.Floor(boundsY.left / BLOCK_SIZE);
            maxBlockX = (int)Math.Floor(boundsY.right / BLOCK_SIZE);
            minBlockY = (int)Math.Floor(boundsY.top / BLOCK_SIZE);
            maxBlockY = (int)Math.Floor(boundsY.bottom / BLOCK_SIZE);
            
            for (int blockX = minBlockX; blockX <= maxBlockX && canMoveY; blockX++)
            {
                for (int blockY = minBlockY; blockY <= maxBlockY; blockY++)
                {
                    var tileType = _chunkManager.GetTile(blockX, blockY);
                    if (tileType.IsSolid())
                    {
                        canMoveY = false;
                        break;
                    }
                }
            }
            
            // Apply sliding movement
            if (canMoveX)
                desiredY = currentY;
            else if (canMoveY)
                desiredX = currentX;
            else
            {
                // Can't move in either direction, stay in place
                desiredX = currentX;
                desiredY = currentY;
            }
        }
        
        return (desiredX, desiredY);
    }
    
    /// <summary>
    /// Resolves collision with other entities
    /// </summary>
    private (float x, float y) ResolveEntityCollisions(
        World world,
        Entity entity,
        CollisionComponent collision,
        float currentX, float currentY,
        float desiredX, float desiredY,
        List<Entity> allCollisionEntities)
    {
        var bounds = collision.GetBounds(desiredX, desiredY);
        
        foreach (var otherEntity in allCollisionEntities)
        {
            // Don't collide with self
            if (otherEntity.Id == entity.Id)
                continue;
            
            var otherCollision = world.GetComponent<CollisionComponent>(otherEntity);
            var otherPosition = world.GetComponent<PositionComponent>(otherEntity);
            
            if (otherCollision == null || otherPosition == null)
                continue;
            
            // Check collision layer filtering
            if ((collision.CollidesWith & otherCollision.Layer) == 0)
                continue;
            
            var otherBounds = otherCollision.GetBounds(otherPosition.X, otherPosition.Y);
            
            // Check for AABB collision
            if (CheckAABBCollision(bounds, otherBounds))
            {
                // Simple collision response: push back to previous position
                // More sophisticated responses could be added here
                desiredX = currentX;
                desiredY = currentY;
                break;
            }
        }
        
        return (desiredX, desiredY);
    }
    
    /// <summary>
    /// Checks if two axis-aligned bounding boxes overlap
    /// </summary>
    private bool CheckAABBCollision(
        (float left, float top, float right, float bottom) a,
        (float left, float top, float right, float bottom) b)
    {
        return a.left < b.right &&
               a.right > b.left &&
               a.top < b.bottom &&
               a.bottom > b.top;
    }
    
    /// <summary>
    /// Checks if a point is inside a tile that blocks movement
    /// </summary>
    public bool IsPointInSolidTerrain(float x, float y)
    {
        if (_chunkManager == null)
            return false;
        
        int blockX = (int)Math.Floor(x / BLOCK_SIZE);
        int blockY = (int)Math.Floor(y / BLOCK_SIZE);
        
        var tileType = _chunkManager.GetTile(blockX, blockY);
        return tileType.IsSolid();
    }
    
    /// <summary>
    /// Gets the tile at a world position
    /// </summary>
    public TileType GetTileAtPosition(float x, float y)
    {
        if (_chunkManager == null)
            return TileType.Air;
        
        int blockX = (int)Math.Floor(x / BLOCK_SIZE);
        int blockY = (int)Math.Floor(y / BLOCK_SIZE);
        
        return _chunkManager.GetTile(blockX, blockY);
    }
}
