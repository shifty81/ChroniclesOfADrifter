# Death & Respawn System - Quick Reference

## Quick Setup

### 1. Add to Player
```csharp
World.AddComponent(playerEntity, new RespawnComponent(
    respawnX: 500,
    respawnY: 150,
    deathPenaltyPercent: 10f,
    invulnerabilityDuration: 2f
));
```

### 2. Add System
```csharp
World.AddSystem(new DeathSystem());  // After CombatSystem
```

## Key Properties

### RespawnComponent
| Property | Type | Description |
|----------|------|-------------|
| RespawnX/Y | float | Respawn coordinates |
| DeathPenaltyPercent | float | Gold loss % (0-100) |
| DeathCount | int | Total deaths |
| IsDead | bool | Currently dead |
| RespawnTimer | float | Time until respawn |
| IsInvulnerable | bool | Immune to damage |

## Common Operations

### Update Respawn Point
```csharp
respawn.SetRespawnPoint(newX, newY);
```

### Check Death State
```csharp
if (respawn.IsDead) { /* Player is dead */ }
if (respawn.IsInvulnerable) { /* Skip damage */ }
```

### Get Death Statistics
```csharp
int deaths = respawn.DeathCount;
```

## Death Flow

1. **Health → 0**: CombatSystem reduces health to 0
2. **Death Detected**: DeathSystem detects death
3. **Penalties Applied**: 10% gold loss
4. **Countdown**: 3-second timer with messages
5. **Respawn**: Player moved to respawn point
6. **Recovery**: Health restored, 2s invulnerability

## Default Settings

- **Death Penalty**: 10% of gold
- **Respawn Delay**: 3 seconds
- **Invulnerability**: 2 seconds
- **Respawn Point**: (500, 150)

## Console Messages

**Death:**
```
╔════════════════════════════════════════════════════╗
║               YOU DIED!                            ║
╚════════════════════════════════════════════════════╝
[DeathSystem] Lost 5 gold (10% penalty). Remaining: 45
```

**Countdown:**
```
[DeathSystem] Respawning in 3...
[DeathSystem] Respawning in 2...
[DeathSystem] Respawning in 1...
```

**Respawn:**
```
[DeathSystem] Health restored to 100
[DeathSystem] Respawned at (500, 150)
[DeathSystem] Invulnerable for 2 seconds
╔════════════════════════════════════════════════════╗
║            RESPAWN COMPLETE                        ║
╚════════════════════════════════════════════════════╝
```

## Save System

### Saved Data
- Respawn point (X, Y)
- Death count

### Auto-Saves
- Respawn location persists across sessions
- Death statistics saved with player data

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Player doesn't respawn | Add DeathSystem after CombatSystem |
| Can move while dead | PlayerInputSystem checks IsDead |
| No gold penalty | Ensure DeathPenaltyPercent > 0 |
| No invulnerability | CombatSystem checks IsInvulnerable |

## Integration Checklist

- [x] RespawnComponent added to player
- [x] DeathSystem added to World
- [x] CombatSystem checks IsDead/IsInvulnerable
- [x] PlayerInputSystem prevents movement when dead
- [x] SaveSystem saves/loads respawn data

## See Also

- Full Documentation: `DEATH_RESPAWN_SYSTEM.md`
- Combat System: `src/Game/ECS/Systems/CombatSystem.cs`
- Save System: `SAVE_LOAD_SYSTEM.md`
