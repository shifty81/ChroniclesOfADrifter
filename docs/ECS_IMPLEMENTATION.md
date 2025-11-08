# Entity Component System (ECS) Implementation

## Overview

Chronicles of a Drifter uses an Entity Component System (ECS) architecture for managing all game objects and their behaviors. This document describes the implementation and usage.

## Core Concepts

### Entity

An entity is simply a unique integer ID that represents a game object. Entities have no data or behavior themselves.

```csharp
Entity player = world.CreateEntity();
```

### Components

Components are pure data containers with no logic. They define what an entity "is" or "has".

**Built-in Components:**
- `PositionComponent` - 2D position (X, Y)
- `VelocityComponent` - 2D velocity (VX, VY)
- `SpriteComponent` - Visual representation
- `HealthComponent` - Health/damage tracking
- `PlayerComponent` - Player-specific data (speed)
- `ScriptComponent` - Lua script attachment

**Example:**
```csharp
world.AddComponent(player, new PositionComponent(100, 200));
world.AddComponent(player, new HealthComponent(100));
```

### Systems

Systems contain logic that operates on entities with specific components. They define what entities "do".

**Built-in Systems:**
- `PlayerInputSystem` - Handles keyboard input for player
- `MovementSystem` - Updates position based on velocity
- `RenderingSystem` - Draws sprites to screen
- `ScriptSystem` - Executes Lua scripts

**Example:**
```csharp
world.AddSystem(new MovementSystem());
world.AddSystem(new RenderingSystem());
```

## Usage Examples

### Creating a Player Entity

```csharp
var player = world.CreateEntity();
world.AddComponent(player, new PlayerComponent { Speed = 100.0f });
world.AddComponent(player, new PositionComponent(960, 540));
world.AddComponent(player, new VelocityComponent());
world.AddComponent(player, new SpriteComponent(0, 32, 32));
world.AddComponent(player, new HealthComponent(100));
```

### Creating an Enemy with Lua AI

```csharp
var goblin = world.CreateEntity();
world.AddComponent(goblin, new PositionComponent(800, 400));
world.AddComponent(goblin, new SpriteComponent(1, 24, 24));
world.AddComponent(goblin, new HealthComponent(50));
world.AddComponent(goblin, new ScriptComponent("scripts/lua/enemies/goblin_example.lua"));
```

### Custom Components

Create custom components by implementing `IComponent`:

```csharp
public class ExperienceComponent : IComponent
{
    public int Level { get; set; } = 1;
    public int CurrentXP { get; set; } = 0;
    public int XPToNextLevel { get; set; } = 100;
}
```

### Custom Systems

Create custom systems by implementing `ISystem`:

```csharp
public class ExperienceSystem : ISystem
{
    public void Initialize(World world) { }
    
    public void Update(World world, float deltaTime)
    {
        foreach (var entity in world.GetEntitiesWithComponent<ExperienceComponent>())
        {
            var exp = world.GetComponent<ExperienceComponent>(entity);
            if (exp != null && exp.CurrentXP >= exp.XPToNextLevel)
            {
                exp.Level++;
                exp.CurrentXP -= exp.XPToNextLevel;
                exp.XPToNextLevel = (int)(exp.XPToNextLevel * 1.5f);
            }
        }
    }
}
```

## World Management

The `World` class manages all entities, components, and systems:

```csharp
var world = new World();

// Add systems
world.AddSystem(new PlayerInputSystem());
world.AddSystem(new MovementSystem());

// Create entities and add components
var player = world.CreateEntity();
world.AddComponent(player, new PositionComponent(0, 0));

// Update all systems every frame
world.Update(deltaTime);

// Clean up
world.DestroyEntity(player);
```

## Performance Considerations

- Components are stored in dictionaries by type for fast lookup
- Systems iterate only over entities with required components
- Entity creation/destruction is O(1)
- Component add/remove is O(1)
- Component queries are O(n) where n = entities with that component

## Integration with Lua

Entities can have Lua scripts attached via `ScriptComponent`:

```csharp
world.AddComponent(enemy, new ScriptComponent("scripts/lua/enemies/goblin.lua"));
```

The Lua script can define callbacks:
- `OnSpawn(entityId, position)` - Called when entity is created
- `OnUpdate(entityId, deltaTime, position)` - Called every frame
- `OnDeath(entityId)` - Called when entity is destroyed

Example Lua script:
```lua
local goblin = {
    speed = 3.0
}

function goblin.OnSpawn(entityId, position)
    print("Goblin " .. entityId .. " spawned!")
end

function goblin.OnUpdate(entityId, deltaTime, position)
    -- AI logic here
end

return goblin
```

## Best Practices

1. **Keep components data-only** - No logic in components
2. **Keep systems focused** - One system per concern
3. **Use composition over inheritance** - Combine components for behavior
4. **Cache component queries** - Don't query every frame if not needed
5. **Clean up entities** - Always destroy unused entities

## Future Enhancements

- Component pooling for better performance
- Event system for entity communication
- Multi-threading support for systems
- Entity prefab system
- Save/load entity state
