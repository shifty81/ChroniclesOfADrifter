# Cinematic Camera System

The cinematic camera system provides scripted camera movements for cutscenes, dramatic reveals, and other non-interactive sequences in Chronicles of a Drifter.

## Overview

The system builds on the existing camera and screen shake systems to provide:
- **Smooth camera pans** between positions
- **Zoom animations** for dramatic effect
- **Waypoint-based patrol paths**
- **Multiple easing functions** for natural motion
- **Screen shake integration** for impact moments
- **Sequence composition** for complex cinematics

## Architecture

### Components

#### CinematicCameraComponent
Tracks the state of cinematic playback:
```csharp
public class CinematicCameraComponent : IComponent
{
    public bool IsPlaying { get; set; }
    public CinematicSequence? CurrentSequence { get; set; }
    public int CurrentStepIndex { get; set; }
    public float StepElapsedTime { get; set; }
    // Start position for interpolation
    public float StartX { get; set; }
    public float StartY { get; set; }
    public float StartZoom { get; set; }
    public Action? OnSequenceComplete { get; set; }
}
```

#### CinematicSequence
A container for a series of camera movements:
```csharp
public class CinematicSequence
{
    public string Name { get; set; }
    public List<CinematicStep> Steps { get; set; }
}
```

#### CinematicStep
A single camera movement:
```csharp
public class CinematicStep
{
    public float TargetX { get; set; }
    public float TargetY { get; set; }
    public float TargetZoom { get; set; } = 1.0f;
    public float Duration { get; set; } = 2.0f;
    public EasingType Easing { get; set; } = EasingType.Linear;
    public float HoldDuration { get; set; } = 0f;
    public ScreenShakeConfig? ScreenShake { get; set; }
}
```

### Systems

#### CinematicCameraSystem
Processes cinematic sequences and updates camera position/zoom over time with smooth interpolation.

## Easing Functions

The system supports multiple easing functions for natural-looking camera movements:

### Available Easing Types

| Easing Type | Description | Best Use Case |
|------------|-------------|---------------|
| `Linear` | Constant speed | Simple pans, mechanical movements |
| `EaseIn` | Starts slow, ends fast | Accelerating into action |
| `EaseOut` | Starts fast, ends slow | Settling on a target |
| `EaseInOut` | Slow-fast-slow | **Most natural for general use** |
| `EaseInQuad` | Quadratic ease in | Moderate acceleration |
| `EaseOutQuad` | Quadratic ease out | Moderate deceleration |
| `EaseInOutQuad` | Quadratic ease in-out | Smooth, professional feel |
| `EaseInCubic` | Cubic ease in | Dramatic acceleration |
| `EaseOutCubic` | Cubic ease out | Dramatic deceleration |
| `EaseInOutCubic` | Cubic ease in-out | **Cinematic, theatrical feel** |

### Easing Visualization

```
Linear:        ────────────────
               
EaseIn:        ─────────────┌──
                           ╱
EaseOut:       ──┐        ╱
                 ╲      ╱
EaseInOut:        ╲    ╱
                   ╲  ╱
                    ╲╱
```

## Usage Examples

### Simple Pan
Move camera smoothly from current position to a target:

```csharp
// Create a simple pan sequence
var sequence = CinematicCameraSystem.CreatePanSequence(
    "Pan to Enemy", 
    targetX: 500, 
    targetY: 300, 
    duration: 2.0f,
    easing: EasingType.EaseInOut
);

// Play the sequence
CinematicCameraSystem.PlaySequence(world, cameraEntity, sequence);
```

### Zoom Animation
Zoom in or out while panning:

```csharp
// Zoom in dramatically
var sequence = CinematicCameraSystem.CreateZoomSequence(
    "Boss Reveal",
    targetX: 400,
    targetY: 300,
    targetZoom: 2.5f,  // 2.5x zoom
    duration: 3.0f,
    easing: EasingType.EaseOutCubic
);

CinematicCameraSystem.PlaySequence(world, cameraEntity, sequence);
```

### Patrol Path
Camera follows a series of waypoints:

```csharp
// Create a patrol path showing different areas
var waypoints = new List<(float x, float y)>
{
    (100, 100),   // Point A
    (700, 100),   // Point B
    (700, 500),   // Point C
    (100, 500)    // Point D
};

var sequence = CinematicCameraSystem.CreatePatrolSequence(
    "Tour of Arena",
    waypoints,
    durationPerWaypoint: 2.0f,
    holdTime: 0.5f,  // Pause at each point
    easing: EasingType.EaseInOut
);

CinematicCameraSystem.PlaySequence(world, cameraEntity, sequence);
```

### Dramatic Reveal
Zoom in, hold, then zoom out to show the full scene:

```csharp
var sequence = CinematicCameraSystem.CreateRevealSequence(
    "Epic Reveal",
    startX: 400,
    startY: 300,
    endX: 600,
    endY: 400,
    startZoom: 3.0f,  // Zoom way in
    endZoom: 0.8f,    // Zoom out to show more
    duration: 4.0f
);

CinematicCameraSystem.PlaySequence(world, cameraEntity, sequence);
```

### Shake Effect
Create dramatic impact with screen shake:

```csharp
var sequence = CinematicCameraSystem.CreateShakeSequence(
    "Earthquake",
    x: 400,
    y: 300,
    shakeIntensity: 30f,
    duration: 2.0f
);

CinematicCameraSystem.PlaySequence(world, cameraEntity, sequence);
```

### Custom Complex Sequence
Build a multi-step sequence manually:

```csharp
var sequence = new CinematicSequence("Battle Intro");

// Step 1: Quick zoom to player
sequence.Steps.Add(new CinematicStep(playerX, playerY, 1.0f, EasingType.EaseOut)
{
    TargetZoom = 2.0f,
    HoldDuration = 0.5f
});

// Step 2: Pan to enemy with screen shake
sequence.Steps.Add(new CinematicStep(enemyX, enemyY, 0.8f, EasingType.EaseInOut)
{
    TargetZoom = 2.0f,
    ScreenShake = new ScreenShakeConfig(intensity: 15f, duration: 0.3f)
});

// Step 3: Zoom out to show both combatants
sequence.Steps.Add(new CinematicStep(centerX, centerY, 2.0f, EasingType.EaseInOutCubic)
{
    TargetZoom = 1.0f,
    HoldDuration = 1.0f
});

// Play with completion callback
CinematicCameraSystem.PlaySequence(world, cameraEntity, sequence, () =>
{
    Console.WriteLine("Battle cinematic complete - start gameplay!");
    // Resume player control, start battle music, etc.
});
```

## Completion Callbacks

Every sequence can have a completion callback to trigger events:

```csharp
CinematicCameraSystem.PlaySequence(world, cameraEntity, sequence, onComplete: () =>
{
    // Restore player control
    playerInputEnabled = true;
    
    // Trigger next event
    questSystem.AdvanceQuest("main_quest");
    
    // Play audio
    audioSystem.PlayMusic("battle_theme");
});
```

## Integration with Existing Systems

### With Camera Follow
Cinematic sequences automatically disable camera following:

```csharp
// Camera is following player
CameraSystem.SetFollowTarget(world, cameraEntity, playerEntity);

// Play cinematic (following is temporarily disabled)
CinematicCameraSystem.PlaySequence(world, cameraEntity, cutscene);

// After cinematic completes, re-enable following if needed
CameraSystem.SetFollowTarget(world, cameraEntity, playerEntity);
```

### With Screen Shake System
Screen shake can be triggered at specific moments in a sequence:

```csharp
var step = new CinematicStep(x, y, duration)
{
    ScreenShake = new ScreenShakeConfig(
        intensity: 25f,
        duration: 0.5f
    )
};
```

### With Camera Zones
Combine cinematic sequences with camera zones for context-sensitive cinematics:

```csharp
// When player enters a zone, play a reveal cinematic
if (PlayerEnteredBossRoom(player))
{
    var bossReveal = CinematicCameraSystem.CreateRevealSequence(
        "Boss Intro",
        bossX, bossY,
        playerX, playerY,
        startZoom: 2.5f,
        endZoom: 1.2f,
        duration: 4.0f
    );
    
    CinematicCameraSystem.PlaySequence(world, camera, bossReveal);
}
```

## Best Practices

### Timing
- **Short pans (1-2s)**: Use for quick reactions (player takes damage, item pickup)
- **Medium pans (2-4s)**: Use for scene transitions, enemy reveals
- **Long pans (4-6s)**: Use for establishing shots, story beats
- **Hold duration**: 0.3-1.0s is usually enough to let viewers process

### Easing Selection
- **EaseInOut**: Default choice, works for 90% of cases
- **EaseOutCubic**: Dramatic stops, use for reveals
- **EaseInCubic**: Dramatic starts, use for action sequences
- **Linear**: Only use for mechanical/robotic movement

### Zoom Levels
- **0.5x - 0.8x**: Wide shot, shows environment
- **1.0x**: Normal gameplay zoom
- **1.5x - 2.0x**: Medium close-up, focus on character
- **2.5x - 3.0x**: Close-up, dramatic reveals
- **Avoid > 3.5x**: Can be disorienting

### Performance
- Keep sequences under 10 steps when possible
- Use callbacks to chain long cinematics instead of one huge sequence
- Reuse sequences instead of creating new ones each time

## Testing

Run the cinematic camera tests:
```bash
dotnet run -c Release -- cinematic-test
```

Run the interactive demo:
```bash
dotnet run -c Release -- cinematic
```

## Example: Complete Cutscene

Here's a complete example of a boss battle intro cutscene:

```csharp
public void PlayBossIntro(World world, Entity camera, Vector2 bossPos, Vector2 playerPos)
{
    var sequence = new CinematicSequence("Boss Battle Intro");
    
    // 1. Start with player (establish protagonist)
    sequence.Steps.Add(new CinematicStep(playerPos.X, playerPos.Y, 1.5f, EasingType.EaseOut)
    {
        TargetZoom = 2.0f,
        HoldDuration = 0.8f
    });
    
    // 2. Quick pan to boss with dramatic shake (boss roars)
    sequence.Steps.Add(new CinematicStep(bossPos.X, bossPos.Y, 1.2f, EasingType.EaseInOut)
    {
        TargetZoom = 2.5f,
        HoldDuration = 1.0f,
        ScreenShake = new ScreenShakeConfig(20f, 0.4f)
    });
    
    // 3. Zoom out to show arena
    sequence.Steps.Add(new CinematicStep(
        (playerPos.X + bossPos.X) / 2,
        (playerPos.Y + bossPos.Y) / 2,
        2.5f,
        EasingType.EaseInOutCubic)
    {
        TargetZoom = 0.9f,
        HoldDuration = 0.5f
    });
    
    // 4. Subtle shake as boss prepares to attack
    sequence.Steps.Add(new CinematicStep(
        (playerPos.X + bossPos.X) / 2,
        (playerPos.Y + bossPos.Y) / 2,
        0.01f,
        EasingType.Linear)
    {
        TargetZoom = 1.0f,
        HoldDuration = 0.8f,
        ScreenShake = new ScreenShakeConfig(8f, 0.8f)
    });
    
    CinematicCameraSystem.PlaySequence(world, camera, sequence, () =>
    {
        // Start boss battle
        StartBossBattle();
        EnablePlayerInput();
        PlayBossMusic();
    });
}
```

## API Reference

### CinematicCameraSystem Methods

| Method | Description |
|--------|-------------|
| `PlaySequence(world, camera, sequence, callback?)` | Start playing a cinematic sequence |
| `StopSequence(world, camera)` | Stop the currently playing sequence |
| `IsPlaying(world, camera)` | Check if a sequence is currently playing |
| `CreatePanSequence(...)` | Helper to create a simple pan |
| `CreateZoomSequence(...)` | Helper to create a zoom animation |
| `CreateRevealSequence(...)` | Helper to create a dramatic reveal |
| `CreatePatrolSequence(...)` | Helper to create a waypoint patrol |
| `CreateShakeSequence(...)` | Helper to create a screen shake sequence |

## See Also

- [Camera System](CAMERA_SYSTEM.md) - Base camera functionality
- [Camera Features](CAMERA_FEATURES.md) - Advanced camera features
- [Screen Shake System](ADVANCED_CAMERA_FEATURES.md) - Screen shake effects
