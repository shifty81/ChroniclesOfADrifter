# Player Death & Respawn System

## Overview
The Death & Respawn system provides a complete death penalty and respawn mechanic for Chronicles of a Drifter. When the player's health reaches zero, they die, lose a percentage of their gold, and respawn at a designated safe location after a short countdown.

## Architecture

### Components

#### RespawnComponent (`src/Game/ECS/Components/RespawnComponent.cs`)
Stores respawn state and configuration for entities:

```csharp
public class RespawnComponent : IComponent
{
    public float RespawnX { get; set; }              // X coordinate of respawn point
    public float RespawnY { get; set; }              // Y coordinate of respawn point
    public float DeathPenaltyPercent { get; set; }   // Death penalty (0-100%)
    public int DeathCount { get; set; }              // Total deaths for statistics
    public bool IsDead { get; set; }                 // Current death state
    public float RespawnTimer { get; set; }          // Time until respawn
    public float InvulnerabilityDuration { get; set; } // Duration of post-respawn invulnerability
    public float InvulnerabilityTimer { get; set; }  // Time remaining for invulnerability
    public bool IsInvulnerable { get; }              // Current invulnerability state
}
```

**Default Values:**
- Death Penalty: 10% of gold (configurable)
- Respawn Delay: 3 seconds
- Invulnerability Duration: 2 seconds

### Systems

#### DeathSystem (`src/Game/ECS/Systems/DeathSystem.cs`)
Handles death detection, penalties, and respawn for all entities:

**Player Death Flow:**
1. Detect when health reaches 0
2. Set `IsDead` flag
3. Apply death penalty (lose percentage of gold)
4. Start 3-second countdown timer
5. Display countdown messages
6. Respawn player at respawn point
7. Restore health to full
8. Grant 2 seconds of invulnerability
9. Increment death counter

**Enemy Death Flow:**
1. Detect when health reaches 0
2. Log death message
3. Remove entity from world
4. Hook for future loot drop system

**Integration Points:**
- Runs after CombatSystem in the update loop
- Checks all entities with HealthComponent
- Special handling for entities with PlayerComponent

#### Updated Systems

**CombatSystem (`src/Game/ECS/Systems/CombatSystem.cs`)**
- Checks `RespawnComponent.IsDead` before allowing player attacks
- Checks `RespawnComponent.IsInvulnerable` before applying damage to player
- Removes "Game Over!" message (handled by DeathSystem)
- Still triggers camera shake on death

**PlayerInputSystem (`src/Game/ECS/Systems/PlayerInputSystem.cs`)**
- Checks `RespawnComponent.IsDead` before processing movement
- Sets velocity to 0 while dead
- Prevents all movement during death countdown

## Features

### Death Penalties
- **Gold Loss**: 10% of current gold (rounded down)
- **Minimum Loss**: 0 gold (can't go negative)
- **No Item Loss**: Inventory items are kept
- **Position Reset**: Player moved to respawn point

### Respawn Mechanics
- **Countdown**: 3-second delay with visual countdown
- **Full Health**: Health restored to maximum on respawn
- **Velocity Reset**: All movement stopped
- **Invulnerability**: 2-second grace period after respawn
- **Statistics**: Death count tracked for player stats

### Console Messages

**On Death:**
```
╔══════════════════════════════════════════════════════════════════╗
║                         YOU DIED!                                ║
╚══════════════════════════════════════════════════════════════════╝
[DeathSystem] Lost 5 gold (10% penalty). Remaining: 45
[DeathSystem] Respawning in 3 seconds...
```

**Countdown:**
```
[DeathSystem] Respawning in 3...
[DeathSystem] Respawning in 2...
[DeathSystem] Respawning in 1...
```

**On Respawn:**
```
[DeathSystem] Respawning player...
[DeathSystem] Health restored to 100
[DeathSystem] Respawned at (500, 150)
[DeathSystem] Invulnerable for 2 seconds
[DeathSystem] Total deaths: 1
╔══════════════════════════════════════════════════════════════════╗
║                    RESPAWN COMPLETE                              ║
╚══════════════════════════════════════════════════════════════════╝
```

### Enemy Death
```
[DeathSystem] Enemy 42 defeated at (650, 200)
```

## Save System Integration

### SaveData Extensions (`src/Game/Serialization/SaveData.cs`)
Added to `PlayerData`:
```csharp
public float RespawnX { get; set; }
public float RespawnY { get; set; }
public int DeathCount { get; set; }
```

### SaveSystem Updates (`src/Game/ECS/Systems/SaveSystem.cs`)

**Saving:**
- Captures current respawn point coordinates
- Saves total death count
- Persists with other player data

**Loading:**
- Restores respawn point location
- Restores death statistics
- Maintains death penalty configuration

## Usage

### Setup in Scene
```csharp
// 1. Add RespawnComponent to player
World.AddComponent(playerEntity, new RespawnComponent(
    respawnX: 500,           // Spawn at X=500
    respawnY: 150,           // Spawn at Y=150
    deathPenaltyPercent: 10f,  // 10% gold loss
    invulnerabilityDuration: 2f // 2 seconds invulnerability
));

// 2. Add DeathSystem after CombatSystem
World.AddSystem(new CombatSystem());
World.AddSystem(new DeathSystem());
```

### Updating Respawn Point
```csharp
var respawn = world.GetComponent<RespawnComponent>(playerEntity);
if (respawn != null)
{
    respawn.SetRespawnPoint(newX, newY);
}
```

### Checking Death State
```csharp
var respawn = world.GetComponent<RespawnComponent>(entity);
if (respawn != null && respawn.IsDead)
{
    // Player is dead, disable actions
}

if (respawn != null && respawn.IsInvulnerable)
{
    // Player is invulnerable, skip damage
}
```

## Configuration

### Respawn Point Strategies
1. **Fixed Home Base**: Default spawn point at world origin
2. **Last Safe Location**: Update respawn point when entering safe zones
3. **Checkpoint System**: Place respawn markers throughout world
4. **Bed Respawn**: Use last-used bed as respawn point

### Death Penalty Options
```csharp
// No penalty
new RespawnComponent(x, y, deathPenaltyPercent: 0f)

// Harsh penalty (50%)
new RespawnComponent(x, y, deathPenaltyPercent: 50f)

// Standard (10%)
new RespawnComponent(x, y, deathPenaltyPercent: 10f)
```

### Invulnerability Duration
```csharp
// No invulnerability
new RespawnComponent(x, y, invulnerabilityDuration: 0f)

// Extended invulnerability (5 seconds)
new RespawnComponent(x, y, invulnerabilityDuration: 5f)
```

## Testing

### Manual Testing
1. **Basic Death**: Take damage until health reaches 0
2. **Countdown**: Verify 3-second countdown displays
3. **Respawn**: Confirm player respawns at correct location
4. **Health**: Verify health is restored to full
5. **Gold Loss**: Check 10% gold penalty applied
6. **Invulnerability**: Confirm no damage for 2 seconds after respawn
7. **Movement**: Test movement is disabled during death
8. **Save/Load**: Verify respawn point persists across saves

### Enemy Death Testing
1. Attack enemy until health reaches 0
2. Verify enemy is removed from world
3. Check console for defeat message

## Future Enhancements

### Planned Features
- [ ] Visual death animation
- [ ] Fade to black on death
- [ ] Loot drop system for enemies
- [ ] Experience loss penalty
- [ ] Hardcore mode (permanent death)
- [ ] Death location marker (corpse run)
- [ ] Respawn selection UI
- [ ] Death statistics tracking (causes, locations)
- [ ] Achievement system integration

### Possible Extensions
- Difficulty-based penalties (easy = 5%, hard = 20%)
- Time-based penalties (longer respawn on repeated deaths)
- Item durability loss on death
- Curse/debuff applied after death
- Soul retrieval mechanic (return to death location)

## Performance

### Overhead
- **Minimal**: System only processes entities with HealthComponent
- **Conditional**: Death logic only runs when health <= 0
- **Timers**: Only active during death/respawn sequence
- **Memory**: ~80 bytes per entity with RespawnComponent

### Optimizations
- Death state checked before expensive operations
- Countdown messages throttled to 1 per second
- Entity removal happens immediately after death
- No frame-by-frame processing for alive entities

## Troubleshooting

### Player doesn't respawn
- Verify RespawnComponent is attached to player entity
- Check DeathSystem is added to world systems
- Ensure DeathSystem is after CombatSystem in initialization order

### Gold penalty not working
- Confirm CurrencyComponent exists on player
- Verify DeathPenaltyPercent is > 0
- Check console for "[DeathSystem] Lost X gold" message

### Player can move while dead
- Verify PlayerInputSystem checks IsDead flag
- Check that velocity is set to 0 during death

### Invulnerability not working
- Confirm RespawnComponent.InvulnerabilityTimer > 0
- Verify CombatSystem checks IsInvulnerable flag
- Ensure timer is updated in DeathSystem

### Respawn point not saved
- Check SaveSystem.SavePlayerData includes respawn data
- Verify SaveSystem.LoadPlayerData restores respawn data
- Confirm SaveData.PlayerData has RespawnX/Y fields

## Security Considerations

### Validation
- Death penalty clamped to 0-100% range
- Gold removal checks for sufficient funds
- Respawn coordinates validated against world bounds (future)

### Exploits Prevention
- Can't attack while dead
- Can't take damage while invulnerable
- Death state is authoritative (no client override)
- Countdown is server-controlled

## References

- **ECS Pattern**: `docs/ECS_ARCHITECTURE.md`
- **Combat System**: `src/Game/ECS/Systems/CombatSystem.cs`
- **Save System**: `SAVE_LOAD_SYSTEM.md`
- **Complete Game Loop**: `COMPLETE_GAME_LOOP_ANALYSIS.md`
