# Swimming and Water Mechanics Documentation

## Overview

The swimming and water mechanics system allows entities to interact with water bodies, including swimming, breath management, drowning, and water flow effects. It provides realistic water physics and survival mechanics.

## Components

### SwimmingComponent

Represents an entity's ability to swim and interact with water.

**Properties:**
- `IsInWater` - Whether the entity is currently in water
- `SwimSpeed` - Movement speed multiplier in water (default: 0.7 = 70% speed)
- `MaxBreathTime` - Maximum time underwater before drowning (seconds)
- `CurrentBreath` - Current remaining breath time
- `DrowningDamage` - Damage per second when out of breath
- `CanBreatheUnderwater` - Whether this entity can breathe underwater

**Example:**
```csharp
// Create a swimmer with 10 seconds of breath
var swimming = new SwimmingComponent(swimSpeed: 0.7f, maxBreathTime: 10.0f);

// Create an aquatic creature that can breathe underwater
var aquatic = new SwimmingComponent();
aquatic.CanBreatheUnderwater = true;
```

### WaterFlowComponent

Represents water flow characteristics for water bodies.

**Properties:**
- `FlowX` - Horizontal flow direction (-1 to 1)
- `FlowY` - Vertical flow direction (-1 to 1)
- `FlowStrength` - Flow speed/strength (0.0 to 1.0)
- `BodyType` - Type of water body (Lake, River, Ocean, UndergroundWater)
- `Pressure` - Pressure level that pushes entities

**Water Body Types:**
- **Lake** - Still water with minimal flow
- **River** - Flowing water with directional current
- **Ocean** - Large body with tidal flow and waves
- **UndergroundWater** - Subterranean water pockets

## SwimmingSystem

The `SwimmingSystem` manages all swimming mechanics and water interactions.

### Features

#### 1. Water Detection
Automatically detects when entities enter/exit water:
```csharp
bool inWater = IsInWater(position);
swimming.IsInWater = inWater;
```

#### 2. Movement Speed Reduction
Swimming reduces movement speed based on `SwimSpeed`:
```csharp
if (swimming.IsInWater)
{
    velocity.VX *= swimming.SwimSpeed;  // 70% speed by default
    velocity.VY *= swimming.SwimSpeed;
}
```

#### 3. Breath Management
Tracks breath underwater and restores on surface:
- Breath depletes at 1 second per second underwater
- Restores at 2 seconds per second on land
- Warning at 30% breath remaining
- Drowning damage when out of breath

#### 4. Drowning Mechanics
Applies damage when breath runs out:
```csharp
if (swimming.CurrentBreath <= 0 && swimming.IsInWater)
{
    health.CurrentHealth -= swimming.DrowningDamage * deltaTime;
}
```

#### 5. Water Flow Effects
Applies flow forces to entities in water:
- Rivers push entities horizontally
- Ocean currents create tidal effects
- Underground water is mostly still
- Flow strength affects push force

## Usage Example

```csharp
// Create the swimming system
var swimmingSystem = new SwimmingSystem();
swimmingSystem.Initialize(world);

// Set the chunk manager for water detection
swimmingSystem.SetChunkManager(chunkManager);

// Create a player with swimming ability
var player = world.CreateEntity();
world.AddComponent(player, new PositionComponent { X = 100, Y = 50 });
world.AddComponent(player, new VelocityComponent());
world.AddComponent(player, new HealthComponent(100));

// Add swimming component (10 seconds breath, 70% speed)
var swimming = new SwimmingComponent(swimSpeed: 0.7f, maxBreathTime: 10.0f);
world.AddComponent(player, swimming);

// Update system each frame
swimmingSystem.Update(world, deltaTime);

// Check if player is swimming
bool isSwimming = SwimmingSystem.IsSwimming(world, player);
```

## Water Flow Simulation

The system simulates water flow based on depth and body type:

### Surface Water (Y >= 0)
- Creates gentle horizontal flow
- Uses noise-based patterns for natural movement
- Typical flow strength: 0.3

### Underground Water (Y < 0)
- Mostly still water
- Minimal flow
- Typical flow strength: 0.1

### Custom Flow
You can create custom flow patterns:
```csharp
var flow = new WaterFlowComponent(
    flowX: 0.5f,      // Flow right
    flowY: -0.2f,     // Flow slightly down
    flowStrength: 0.8f,
    bodyType: WaterBodyType.River
);
```

## Integration with Other Systems

### ChunkManager Integration
The swimming system requires a `ChunkManager` to detect water tiles:
```csharp
swimmingSystem.SetChunkManager(chunkManager);
```

### Health System Integration
Drowning automatically damages entities with `HealthComponent`:
```csharp
if (health != null && swimming.CurrentBreath <= 0)
{
    health.CurrentHealth -= swimming.DrowningDamage * deltaTime;
}
```

### Movement System Integration
Swimming affects entity velocity through the `VelocityComponent`:
```csharp
// Swim speed reduction
velocity.VX *= swimming.SwimSpeed;

// Water flow force
velocity.VX += flow.FlowX * flowForce;
```

## Console Messages

The system provides helpful console feedback:
- "Player is drowning! Health: X" - When taking drowning damage
- "Low breath! X.Xs remaining" - At 30% breath
- "Exited water, breath restored" - When leaving water

## Testing

Run swimming system tests:
```bash
dotnet run -- swimming-test
```

The test suite includes:
- Component initialization tests
- Breath management tests
- Swim speed reduction tests
- Drowning mechanics tests
- Water flow component tests

## Performance Considerations

- Water detection is performed each frame only for entities with `SwimmingComponent`
- Flow calculations use simple noise-based patterns (fast)
- Breath updates are lightweight float operations
- System scales well with many swimming entities

## Future Enhancements

- Swimming animations
- Swimming fatigue system
- Diving mechanics (deeper underwater)
- Water temperature effects
- Currents affecting projectiles
- Water surface effects (splashes, ripples)
- Swimming skill progression
- Boat/swimming equipment
