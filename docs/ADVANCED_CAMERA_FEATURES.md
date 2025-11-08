# Advanced Camera Features

This document describes the advanced camera features implemented in Chronicles of a Drifter.

## Overview

The game includes a sophisticated camera system with multiple advanced features:
- Multi-layer parallax scrolling for depth perception
- Screen shake effects for impact feedback
- Camera zones with dynamic behavior
- Smooth zoom and follow mechanics

## Multi-Layer Parallax Backgrounds

Parallax scrolling creates the illusion of depth by moving background layers at different speeds relative to the camera.

### Visual Types

The `ParallaxVisualType` enum defines different background patterns:

- **Sky**: Solid sky color background
- **Clouds**: Animated cloud patterns with auto-scroll
- **Mountains**: Distant mountain silhouettes
- **Stars**: Starfield for night sky effects
- **Mist**: Fog/mist effects in lower portions
- **Trees**: Foreground tree silhouettes

### Creating Parallax Layers

```csharp
// Sky layer - static background
var sky = ParallaxSystem.CreateParallaxLayer(World, "Sky", 
    parallaxFactor: 0.0f,      // Doesn't move (static)
    zOrder: -150,              // Rendered first (farthest back)
    visualType: ParallaxVisualType.Sky,
    color: ConsoleColor.DarkBlue,
    density: 0.5f);

// Clouds layer - slowly scrolling
var clouds = ParallaxSystem.CreateParallaxLayer(World, "Clouds",
    parallaxFactor: 0.2f,      // Moves slowly (far away)
    zOrder: -100,
    visualType: ParallaxVisualType.Clouds,
    color: ConsoleColor.DarkGray,
    density: 0.3f,
    autoScrollX: 2.0f);        // Auto-scroll for animation

// Mountains layer
var mountains = ParallaxSystem.CreateParallaxLayer(World, "Mountains",
    parallaxFactor: 0.4f,      // Moves faster (closer)
    zOrder: -75,
    visualType: ParallaxVisualType.Mountains,
    color: ConsoleColor.DarkCyan,
    density: 0.6f);
```

### Parallax Factor Guidelines

- **0.0**: Static background (doesn't move)
- **< 1.0**: Background layers (move slower than camera)
- **1.0**: Main gameplay layer (moves with camera)
- **> 1.0**: Foreground layers (move faster than camera)

## Screen Shake Effects

Screen shake provides visual feedback for impacts, attacks, and other game events.

### Adding Screen Shake to Camera

```csharp
// Add screen shake component to camera entity
World.AddComponent(camera, new ScreenShakeComponent());

// Add screen shake system to world
World.AddSystem(new ScreenShakeSystem());
```

### Triggering Screen Shake

Three intensity levels are provided:

```csharp
// Light shake (e.g., player taking damage)
ScreenShakeSystem.TriggerLightShake(world, cameraEntity);
// Intensity: 5.0f, Duration: 0.2s

// Medium shake (e.g., enemy defeated, explosion)
ScreenShakeSystem.TriggerMediumShake(world, cameraEntity);
// Intensity: 10.0f, Duration: 0.3s

// Heavy shake (e.g., boss attack, large explosion)
ScreenShakeSystem.TriggerHeavyShake(world, cameraEntity);
// Intensity: 20.0f, Duration: 0.5s
```

### Custom Screen Shake

For custom shake effects:

```csharp
ScreenShakeSystem.TriggerShake(world, cameraEntity, 
    intensity: 15.0f,   // How much to shake
    duration: 0.4f,     // How long in seconds
    frequency: 25.0f);  // How fast to shake
```

### How It Works

1. Screen shake uses sine/cosine waves for natural movement
2. Intensity decreases over time (decay)
3. Small random offsets add variation
4. Automatically resets when complete

## Camera Zones

Camera zones allow different areas of the game to have different camera behaviors.

### Creating Camera Zones

```csharp
// Safe zone - zoomed in, slow follow
CameraZoneSystem.CreateCameraZone(World, "Safe Zone",
    minX: 0, maxX: 640, 
    minY: 0, maxY: 1080,
    zoom: 1.3f,              // Zoomed in for detail
    followSpeed: 3.0f,       // Slower follow
    enableLookAhead: false,  // No look-ahead
    priority: 1);

// Combat zone - normal settings
CameraZoneSystem.CreateCameraZone(World, "Combat Zone",
    minX: 640, maxX: 1280,
    minY: 0, maxY: 1080,
    zoom: 1.0f,              // Normal zoom
    followSpeed: 8.0f,       // Fast follow
    enableLookAhead: true,
    lookAheadDistance: 100.0f,
    priority: 1);

// Boss arena - zoomed out, very responsive
CameraZoneSystem.CreateCameraZone(World, "Boss Arena",
    minX: 1280, maxX: 1920,
    minY: 0, maxY: 1080,
    zoom: 0.8f,              // Zoomed out to see more
    followSpeed: 12.0f,      // Very fast follow
    enableLookAhead: true,
    lookAheadDistance: 150.0f,
    priority: 1);
```

### Zone Properties

- **Bounds**: Define the rectangular area of the zone
- **Zoom**: Camera zoom level in this zone
- **FollowSpeed**: How quickly camera follows the player
- **EnableLookAhead**: Whether to enable camera look-ahead
- **LookAheadDistance**: How far ahead to look when moving
- **Priority**: Higher priority zones override lower ones if overlapping
- **TransitionSpeed**: How smoothly to transition (0 = instant, higher = smoother)

### How Zones Work

1. System checks player/camera position each frame
2. Finds the highest priority zone containing the position
3. Smoothly transitions camera settings when entering new zones
4. Applies zone-specific behavior (zoom, follow speed, look-ahead)

## Camera Look-Ahead

Camera look-ahead shifts the camera focus in the direction of movement, showing more of what's ahead.

### Enabling Look-Ahead

```csharp
CameraLookAheadSystem.EnableLookAhead(World, camera,
    lookAheadDistance: 100.0f,  // How far ahead to look
    lookAheadSpeed: 3.0f,        // How fast to adjust
    offsetScale: 0.15f);         // Scale factor for subtle effect
```

### When to Use

- ✅ Fast-paced action games - player needs to see threats ahead
- ✅ Platformers - helps with jump planning
- ❌ Slow exploration - can be disorienting
- ❌ Puzzle games - not needed, can be distracting

## Smooth Zoom

The camera supports smooth zoom transitions using keyboard input.

### Zoom Controls

- **+ or =**: Zoom in
- **- or _**: Zoom out

### Zoom Settings

```csharp
// In CameraInputSystem
private float _zoomSpeed = 1.0f;  // Zoom change per second

// Zoom limits
camera.Zoom = MathF.Min(camera.Zoom, 4.0f);   // Max zoom 4x
camera.Zoom = MathF.Max(camera.Zoom, 0.25f);  // Min zoom 0.25x
```

The zoom is automatically smooth because it's based on deltaTime:
```csharp
camera.Zoom += _zoomSpeed * deltaTime;  // Smooth continuous zoom
```

## Complete Example

Here's how to set up a complete advanced camera system:

```csharp
public override void OnLoad()
{
    // Add all camera systems
    World.AddSystem(new PlayerInputSystem());
    World.AddSystem(new CameraInputSystem());
    World.AddSystem(new MovementSystem());
    World.AddSystem(new CameraSystem());
    World.AddSystem(new CameraLookAheadSystem());
    World.AddSystem(new ScreenShakeSystem());
    World.AddSystem(new CameraZoneSystem());
    World.AddSystem(new ParallaxSystem());
    
    // Create player
    var player = World.CreateEntity();
    World.AddComponent(player, new PlayerComponent { Speed = 150.0f });
    World.AddComponent(player, new PositionComponent(960, 540));
    World.AddComponent(player, new VelocityComponent());
    
    // Create camera with all features
    var camera = World.CreateEntity();
    var cameraComponent = new CameraComponent(1920, 1080)
    {
        Zoom = 1.0f,
        FollowSpeed = 8.0f
    };
    World.AddComponent(camera, cameraComponent);
    World.AddComponent(camera, new PositionComponent(960, 540));
    World.AddComponent(camera, new ScreenShakeComponent());
    
    // Set up camera following
    CameraSystem.SetFollowTarget(World, camera, player, followSpeed: 8.0f);
    
    // Enable camera look-ahead
    CameraLookAheadSystem.EnableLookAhead(World, camera, 
        lookAheadDistance: 100.0f, 
        lookAheadSpeed: 3.0f, 
        offsetScale: 0.15f);
    
    // Create parallax layers
    CreateParallaxLayers();
    
    // Create camera zones
    CreateCameraZones();
}

private void CreateParallaxLayers()
{
    // Sky
    ParallaxSystem.CreateParallaxLayer(World, "Sky", 
        parallaxFactor: 0.0f, zOrder: -150,
        visualType: ParallaxVisualType.Sky,
        color: ConsoleColor.DarkBlue, density: 0.5f);
    
    // Clouds (auto-scrolling)
    ParallaxSystem.CreateParallaxLayer(World, "Clouds",
        parallaxFactor: 0.2f, zOrder: -100,
        visualType: ParallaxVisualType.Clouds,
        color: ConsoleColor.DarkGray, density: 0.3f,
        autoScrollX: 2.0f);
    
    // Mountains
    ParallaxSystem.CreateParallaxLayer(World, "Mountains",
        parallaxFactor: 0.4f, zOrder: -75,
        visualType: ParallaxVisualType.Mountains,
        color: ConsoleColor.DarkCyan, density: 0.6f);
    
    // Mist
    ParallaxSystem.CreateParallaxLayer(World, "Mist",
        parallaxFactor: 0.6f, zOrder: -25,
        visualType: ParallaxVisualType.Mist,
        color: ConsoleColor.DarkGray, density: 0.15f,
        autoScrollX: 1.0f);
}

private void CreateCameraZones()
{
    // Safe zone
    CameraZoneSystem.CreateCameraZone(World, "Safe Zone",
        minX: 0, maxX: 640, minY: 0, maxY: 1080,
        zoom: 1.3f, followSpeed: 3.0f, 
        enableLookAhead: false, priority: 1);
    
    // Combat zone
    CameraZoneSystem.CreateCameraZone(World, "Combat Zone",
        minX: 640, maxX: 1280, minY: 0, maxY: 1080,
        zoom: 1.0f, followSpeed: 8.0f,
        enableLookAhead: true, lookAheadDistance: 100.0f,
        priority: 1);
    
    // Boss arena
    CameraZoneSystem.CreateCameraZone(World, "Boss Arena",
        minX: 1280, maxX: 1920, minY: 0, maxY: 1080,
        zoom: 0.8f, followSpeed: 12.0f,
        enableLookAhead: true, lookAheadDistance: 150.0f,
        priority: 1);
}
```

## Performance Considerations

### Parallax Rendering
- Parallax layers are rendered in order by ZOrder (lowest first)
- Only layers marked as `IsVisible` are rendered
- Pattern generation uses mathematical functions (sin/cos) - very efficient
- No texture loading required for console rendering

### Screen Shake
- Minimal performance impact - just offset calculations
- Automatic cleanup when shake completes
- No per-frame allocations

### Camera Zones
- Zone detection is fast - simple point-in-rectangle checks
- Only checks zones marked as `IsActive`
- Priority system prevents unnecessary comparisons
- Smooth transitions use exponential smoothing (no frame jumps)

## Testing

The camera features are tested in `CameraFeaturesTest.cs`:

```bash
dotnet run camera-test
```

Tests include:
- Parallax layer creation and positioning
- Camera look-ahead offset calculations
- Zone detection and transitions
- Camera bounds enforcement
- Multi-layer parallax movement verification

## Debugging Tips

### Parallax Not Visible
- Check `IsVisible` property on layers
- Verify ZOrder (negative for backgrounds)
- Ensure layers are created before rendering starts
- Check parallax factor values

### Screen Shake Not Working
- Verify `ScreenShakeComponent` is on camera entity
- Check if shake duration hasn't already elapsed
- Ensure `ScreenShakeSystem` is added before `CameraSystem`
- Confirm shake intensity is > 0

### Camera Zones Not Activating
- Verify zone bounds contain the camera position
- Check zone priority values (higher overrides lower)
- Ensure `IsActive` is true
- Confirm `CameraZoneSystem` is added to world
- Check console output for zone entry messages

### Look-Ahead Not Functioning
- Verify `CameraLookAheadComponent` exists on camera
- Check `IsEnabled` property
- Ensure entity has velocity (look-ahead requires movement)
- Confirm system is added after `MovementSystem`

## Future Enhancements

Potential improvements for the camera system:
- Cinematic camera paths for cutscenes
- Camera shake variations (horizontal-only, vertical-only, circular)
- Dynamic parallax based on time of day
- Camera zoom zones (automatic zoom based on area)
- Camera boundaries per zone
- Multiple camera support for split-screen
- Camera interpolation modes (linear, ease-in-out, spring)

## See Also

- [CAMERA_SYSTEM.md](CAMERA_SYSTEM.md) - Basic camera system documentation
- [CAMERA_FEATURES.md](CAMERA_FEATURES.md) - Original camera features (look-ahead, parallax basics)
- [ROADMAP.md](../ROADMAP.md) - Development roadmap
- PlayableDemoScene.cs - Example implementation
