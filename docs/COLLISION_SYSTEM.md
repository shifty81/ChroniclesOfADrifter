# Collision Detection System

Chronicles of a Drifter implements a comprehensive collision detection system that handles both entity-to-terrain and entity-to-entity collisions using Axis-Aligned Bounding Boxes (AABB).

## Overview

The collision system provides:
- **AABB Collision Detection**: Fast and accurate bounding box collision detection
- **Entity-to-Terrain Collision**: Prevents entities from moving through solid blocks
- **Entity-to-Entity Collision**: Detects and resolves collisions between entities
- **Sliding Response**: Smooth wall sliding when moving diagonally into obstacles
- **Collision Layers**: Filtering system to control which entities can collide with each other
- **Static vs. Dynamic Entities**: Support for both moving and stationary collidable objects

## Architecture

### Components

#### CollisionComponent

Defines the collision properties of an entity:

```csharp
public class CollisionComponent : IComponent
{
    public float Width { get; set; }           // Width of collision box
    public float Height { get; set; }          // Height of collision box
    public float OffsetX { get; set; }         // X offset from entity position
    public float OffsetY { get; set; }         // Y offset from entity position
    public bool IsStatic { get; set; }         // If true, entity doesn't move
    public bool CheckTerrain { get; set; }     // Check collision with terrain
    public bool CheckEntities { get; set; }    // Check collision with entities
    public CollisionLayer Layer { get; set; }  // This entity's collision layer
    public CollisionLayer CollidesWith { get; set; } // Which layers to collide with
}
```

#### Collision Layers

Collision layers allow selective collision detection:

```csharp
[Flags]
public enum CollisionLayer
{
    None = 0,
    Default = 1 << 0,      // Default layer
    Player = 1 << 1,       // Player entities
    Enemy = 1 << 2,        // Enemy entities
    Projectile = 1 << 3,   // Projectiles (arrows, bullets)
    Item = 1 << 4,         // Collectible items
    Terrain = 1 << 5,      // Terrain/world geometry
    Trigger = 1 << 6,      // Trigger zones (don't block movement)
    All = ~0               // Everything
}
```

### Systems

#### CollisionSystem

The collision system processes all entities with `CollisionComponent` and resolves collisions:

```csharp
public class CollisionSystem : ISystem
{
    public void SetChunkManager(ChunkManager chunkManager);
    public void Update(World world, float deltaTime);
    public bool IsPointInSolidTerrain(float x, float y);
    public TileType GetTileAtPosition(float x, float y);
}
```

**Key Features:**
- Integrates with `ChunkManager` for terrain collision
- Performs AABB collision detection
- Implements sliding collision response
- Respects collision layer filtering

## Usage

### Adding Collision to an Entity

```csharp
// Create entity with collision
var player = world.CreateEntity();
world.AddComponent(player, new PositionComponent(100, 100));
world.AddComponent(player, new VelocityComponent());

// Add collision component
world.AddComponent(player, new CollisionComponent(
    width: 32,              // Collision box width
    height: 32,             // Collision box height
    offsetX: 0,             // Center aligned
    offsetY: 0,
    isStatic: false,        // Can move
    checkTerrain: true,     // Collide with terrain
    checkEntities: true,    // Collide with other entities
    layer: CollisionLayer.Player,
    collidesWith: CollisionLayer.Enemy | CollisionLayer.Terrain
));
```

### Setting Up the Collision System

```csharp
// Create and add collision system
var collisionSystem = new CollisionSystem();
world.AddSystem(collisionSystem);

// Set chunk manager for terrain collision
var chunkManager = new ChunkManager();
collisionSystem.SetChunkManager(chunkManager);
```

### System Order

The collision system should be updated **after** movement but **before** rendering:

```csharp
world.AddSystem(new PlayerInputSystem());
world.AddSystem(new MovementSystem());
world.AddSystem(new CollisionSystem());  // After movement!
world.AddSystem(new CameraSystem());
world.AddSystem(new RenderingSystem());
```

## Collision Detection Algorithms

### AABB Collision Detection

The system uses Axis-Aligned Bounding Boxes (AABB) for efficient collision detection:

```csharp
bool CheckAABBCollision(bounds1, bounds2)
{
    return bounds1.left < bounds2.right &&
           bounds1.right > bounds2.left &&
           bounds1.top < bounds2.bottom &&
           bounds1.bottom > bounds2.top;
}
```

### Terrain Collision

Terrain collision converts world coordinates to block coordinates and checks if the entity's bounding box overlaps with any solid blocks:

```csharp
// Convert world position to block coordinates
int blockX = (int)Math.Floor(worldX / BLOCK_SIZE);
int blockY = (int)Math.Floor(worldY / BLOCK_SIZE);

// Check if block is solid
var tileType = chunkManager.GetTile(blockX, blockY);
if (tileType.IsSolid())
{
    // Collision detected!
}
```

### Sliding Collision Response

When a collision is detected, the system attempts to slide the entity along the obstacle:

1. **Try moving horizontally only**: If the entity can move in X but not diagonally, slide along Y
2. **Try moving vertically only**: If the entity can move in Y but not diagonally, slide along X
3. **Stop completely**: If movement is blocked in both directions

This creates smooth wall-sliding behavior when moving diagonally into obstacles.

## Collision Layers

### Layer Filtering

Entities only collide if their layer masks match:

```csharp
// Player collides with enemies and terrain, but not with projectiles
player.Layer = CollisionLayer.Player;
player.CollidesWith = CollisionLayer.Enemy | CollisionLayer.Terrain;

// Enemy collides with player and other enemies
enemy.Layer = CollisionLayer.Enemy;
enemy.CollidesWith = CollisionLayer.Player | CollisionLayer.Enemy;

// Projectile only collides with enemies (not player)
projectile.Layer = CollisionLayer.Projectile;
projectile.CollidesWith = CollisionLayer.Enemy;
```

### Common Layer Configurations

**Player Entity:**
```csharp
layer: CollisionLayer.Player
collidesWith: CollisionLayer.Enemy | CollisionLayer.Terrain | CollisionLayer.Default
```

**Enemy Entity:**
```csharp
layer: CollisionLayer.Enemy
collidesWith: CollisionLayer.Player | CollisionLayer.Enemy | CollisionLayer.Terrain
```

**Projectile:**
```csharp
layer: CollisionLayer.Projectile
collidesWith: CollisionLayer.Enemy | CollisionLayer.Terrain
```

**Collectible Item:**
```csharp
layer: CollisionLayer.Item
collidesWith: CollisionLayer.None  // Trigger-only, doesn't block movement
```

## Static vs. Dynamic Entities

### Static Entities

Static entities don't move and are optimized for performance:

```csharp
// Static obstacle (wall, rock, etc.)
world.AddComponent(entity, new CollisionComponent(
    width: 64,
    height: 64,
    isStatic: true,        // Won't move
    checkTerrain: false,   // Don't check terrain (already placed correctly)
    checkEntities: false   // Don't need to check (other entities check against it)
));
```

### Dynamic Entities

Dynamic entities can move and check for collisions:

```csharp
// Moving entity (player, enemy, etc.)
world.AddComponent(entity, new CollisionComponent(
    width: 32,
    height: 32,
    isStatic: false,       // Can move
    checkTerrain: true,    // Check terrain collision
    checkEntities: true    // Check entity collision
));
```

## Performance Considerations

### Optimization Strategies

1. **Static Entities**: Mark non-moving entities as static to skip collision checks
2. **Layer Filtering**: Use collision layers to reduce unnecessary collision checks
3. **Spatial Partitioning**: The system only checks nearby entities (within loaded chunks)
4. **Early Exit**: Collision detection exits early when collision is found

### Complexity

- **Entity-to-Entity**: O(n²) in worst case, but typically O(n*k) where k is nearby entities
- **Entity-to-Terrain**: O(1) with chunked terrain system
- **AABB Check**: O(1) constant time per check

## Testing

### Running Collision Tests

```bash
dotnet run collision-test
```

This runs comprehensive tests:
1. AABB collision detection
2. Entity-to-entity collision
3. Terrain collision
4. Collision layer filtering
5. Sliding collision (wall sliding)

### Running Collision Demo

```bash
dotnet run collision
```

Interactive demo featuring:
- Player with WASD/Arrow key movement
- Terrain collision with generated chunks
- Static obstacle entities
- Enemy entities with collision
- Smooth wall sliding behavior

## Integration with Existing Systems

### Movement System

The collision system works seamlessly with the `MovementSystem`:

```csharp
// MovementSystem updates velocity
world.AddSystem(new MovementSystem());

// CollisionSystem applies collision response after movement
world.AddSystem(new CollisionSystem());
```

### Terrain System

Collision system integrates with `ChunkManager` for terrain collision:

```csharp
var chunkManager = new ChunkManager();
var terrainGenerator = new TerrainGenerator();
chunkManager.SetTerrainGenerator(terrainGenerator);

// Link to collision system
collisionSystem.SetChunkManager(chunkManager);
```

### Player Input

Player input works naturally with collision:

```csharp
// Player input sets velocity
world.AddSystem(new PlayerInputSystem());

// Movement applies velocity to position
world.AddSystem(new MovementSystem());

// Collision prevents invalid positions
world.AddSystem(new CollisionSystem());
```

## Examples

### Example 1: Player with Terrain Collision

```csharp
var player = world.CreateEntity();
world.AddComponent(player, new PlayerComponent { Speed = 200.0f });
world.AddComponent(player, new PositionComponent(960, 540));
world.AddComponent(player, new VelocityComponent());
world.AddComponent(player, new CollisionComponent(
    width: 28,
    height: 28,
    checkTerrain: true,
    layer: CollisionLayer.Player,
    collidesWith: CollisionLayer.Terrain | CollisionLayer.Enemy
));
```

### Example 2: Static Obstacle

```csharp
var wall = world.CreateEntity();
world.AddComponent(wall, new PositionComponent(500, 500));
world.AddComponent(wall, new VelocityComponent(0, 0));
world.AddComponent(wall, new CollisionComponent(
    width: 64,
    height: 128,
    isStatic: true,
    checkTerrain: false,
    checkEntities: false,
    layer: CollisionLayer.Default
));
```

### Example 3: Enemy with AI

```csharp
var enemy = world.CreateEntity();
world.AddComponent(enemy, new PositionComponent(700, 400));
world.AddComponent(enemy, new VelocityComponent());
world.AddComponent(enemy, new ScriptComponent("enemies/patrol.lua"));
world.AddComponent(enemy, new CollisionComponent(
    width: 24,
    height: 24,
    checkTerrain: true,
    checkEntities: true,
    layer: CollisionLayer.Enemy,
    collidesWith: CollisionLayer.Player | CollisionLayer.Enemy | CollisionLayer.Terrain
));
```

## Future Enhancements

Potential improvements to the collision system:

- [ ] **Continuous Collision Detection**: Prevent tunneling at high speeds
- [ ] **Circle Colliders**: Alternative to AABB for better fit on round objects
- [ ] **Trigger Volumes**: Non-blocking collision zones that trigger events
- [ ] **Physics Materials**: Friction and bounciness properties
- [ ] **Collision Events**: Callbacks when collisions occur
- [ ] **Spatial Hash Grid**: More efficient broad-phase collision detection
- [ ] **Collision Groups**: More granular control than layers
- [ ] **One-Way Platforms**: Platforms you can jump through from below

## References

- [AABB Collision Detection](https://developer.mozilla.org/en-US/docs/Games/Techniques/2D_collision_detection)
- [Sliding Collision Response](https://gamedev.stackexchange.com/questions/45578/moving-a-2d-character-and-detecting-collisions)
- [Collision Layers](https://docs.unity3d.com/Manual/LayerBasedCollision.html)
