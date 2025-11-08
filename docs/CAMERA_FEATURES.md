# Parallax Scrolling and Camera Look-Ahead Systems

## Overview

This document describes the parallax scrolling and camera look-ahead systems implemented in Chronicles of a Drifter to enhance the visual depth and camera behavior of the game.

## Parallax Scrolling System

### Purpose

The parallax scrolling system creates an illusion of depth in 2D scenes by moving background layers at different speeds relative to the camera. This technique is commonly used in 2D games to create a more immersive and visually appealing experience.

### Components

#### ParallaxLayerComponent

Located in: `src/Game/ECS/Components/ParallaxLayerComponent.cs`

This component represents a single parallax layer in the scene.

**Properties:**

- `ParallaxFactor` (float): Controls how fast this layer moves relative to the camera
  - `0.0` = Static (no movement)
  - `< 1.0` = Moves slower than camera (background layers)
  - `1.0` = Moves with camera (gameplay layer)
  - `> 1.0` = Moves faster than camera (foreground layers)

- `AutoScrollX` (float): Automatic horizontal scroll speed (e.g., for clouds)
- `AutoScrollY` (float): Automatic vertical scroll speed
- `ZOrder` (int): Rendering order (lower values rendered first/behind)
- `IsVisible` (bool): Whether this layer is currently visible
- `Name` (string): Layer name for debugging
- `AccumulatedTime` (float): Internal time accumulator for auto-scroll calculations

#### ParallaxSystem

Located in: `src/Game/ECS/Systems/ParallaxSystem.cs`

This system updates all parallax layers each frame based on camera movement.

**Key Methods:**

- `Update(World world, float deltaTime)`: Updates all parallax layer positions
- `CreateParallaxLayer(...)`: Static helper to create a new parallax layer entity

### Usage Example

```csharp
// Create background layers in your scene
private void CreateParallaxLayers()
{
    // Far background (moves slowest, 20% of camera speed)
    var farBg = ParallaxSystem.CreateParallaxLayer(World, "Far Background", 
        parallaxFactor: 0.2f, 
        zOrder: -100,
        autoScrollX: 2.0f);  // Slowly scrolls like distant clouds
    
    // Mid background (50% of camera speed)
    var midBg = ParallaxSystem.CreateParallaxLayer(World, "Mid Background",
        parallaxFactor: 0.5f,
        zOrder: -50);
    
    // Near background (80% of camera speed)
    var nearBg = ParallaxSystem.CreateParallaxLayer(World, "Near Background",
        parallaxFactor: 0.8f,
        zOrder: -10);
}
```

### Best Practices

1. **Background Layers**: Use `parallaxFactor < 1.0` for background layers to create depth
2. **Foreground Layers**: Use `parallaxFactor > 1.0` for foreground elements (trees, etc.)
3. **Main Gameplay**: Always use `parallaxFactor = 1.0` for the main gameplay layer
4. **Z-Order**: Organize layers with negative Z values for backgrounds, positive for foregrounds
5. **Auto-Scroll**: Use subtle auto-scroll values (1-5) for atmospheric effects like clouds

### Layer Structure Example

```
Layer 0: Far mountains      (parallax: 0.2, z: -100) - slowest
Layer 1: Distant trees      (parallax: 0.4, z: -50)
Layer 2: Mid-ground         (parallax: 0.7, z: -10)
Layer 3: Ground/Terrain     (parallax: 1.0, z: 0)     - camera speed
Layer 4: Player/Entities    (parallax: 1.0, z: 0)
Layer 5: Foreground trees   (parallax: 1.2, z: 10)    - faster than camera
```

## Camera Look-Ahead System

### Purpose

The camera look-ahead system shifts the camera focus in the direction of player movement, showing more of what's ahead. This improves the gameplay experience by giving players better visibility of threats and obstacles in their movement direction.

### Components

#### CameraLookAheadComponent

Located in: `src/Game/ECS/Components/CameraLookAheadComponent.cs`

This component stores the state for camera look-ahead behavior.

**Properties:**

- `LookAheadDistance` (float): Distance in world units to look ahead (default: 100.0)
- `LookAheadSpeed` (float): Speed at which the look-ahead offset adjusts (default: 3.0)
- `CurrentOffsetX` (float): Current look-ahead offset X (internal state)
- `CurrentOffsetY` (float): Current look-ahead offset Y (internal state)
- `IsEnabled` (bool): Whether look-ahead is enabled
- `MinVelocityThreshold` (float): Minimum velocity to activate look-ahead (default: 0.1)
- `OffsetScale` (float): Scale factor for subtle effect (default: 0.1)

#### CameraLookAheadSystem

Located in: `src/Game/ECS/Systems/CameraLookAheadSystem.cs`

This system calculates and applies look-ahead offsets based on target entity velocity.

**Key Methods:**

- `Update(World world, float deltaTime)`: Updates camera look-ahead for all cameras
- `EnableLookAhead(...)`: Static helper to enable look-ahead for a camera
- `DisableLookAhead(...)`: Static helper to disable look-ahead for a camera

### Usage Example

```csharp
// In your scene setup, after creating the camera
var camera = World.CreateEntity();
World.AddComponent(camera, new CameraComponent(1920, 1080));
CameraSystem.SetFollowTarget(World, camera, player);

// Enable look-ahead with custom settings
CameraLookAheadSystem.EnableLookAhead(World, camera, 
    lookAheadDistance: 100.0f,  // Look 100 units ahead
    lookAheadSpeed: 3.0f,        // Smooth adjustment speed
    offsetScale: 0.15f);         // 15% of the calculated offset (subtle)
```

### How It Works

1. **Velocity Detection**: The system reads the target entity's velocity component
2. **Direction Calculation**: Normalizes the velocity to get movement direction
3. **Offset Calculation**: Multiplies direction by `LookAheadDistance`
4. **Smooth Interpolation**: Uses exponential smoothing for natural movement
5. **Centering**: When the target stops, gradually returns camera to center
6. **Application**: Applies the offset (scaled) to the camera position after normal following

### Configuration Guidelines

#### LookAheadDistance

- **Short (50-75)**: Subtle effect, good for slow-paced games
- **Medium (100-150)**: ✅ **Recommended** - Balanced for most games
- **Long (200+)**: Strong effect, good for fast-paced action games

#### LookAheadSpeed

- **Slow (1-2)**: Cinematic, deliberate camera movement
- **Medium (3-5)**: ✅ **Recommended** - Responsive but smooth
- **Fast (6-10)**: Snappy response, good for competitive gameplay

#### OffsetScale

- **Subtle (0.05-0.10)**: Very gentle look-ahead
- **Medium (0.10-0.20)**: ✅ **Recommended** - Noticeable but not distracting
- **Strong (0.25-0.50)**: Aggressive look-ahead, may be disorienting

### When to Use Look-Ahead

✅ **Good for:**
- Fast-paced action games
- Racing/running games
- Platformers
- Top-down shooters

❌ **Not recommended for:**
- Slow exploration games (can be disorienting)
- Puzzle games (unnecessary)
- Turn-based games (no continuous movement)

## System Integration

### System Update Order

The systems must be updated in the correct order:

```csharp
World.AddSystem(new PlayerInputSystem());      // 1. Process input
World.AddSystem(new MovementSystem());         // 2. Apply movement
World.AddSystem(new CameraSystem());           // 3. Follow target
World.AddSystem(new CameraLookAheadSystem());  // 4. Apply look-ahead
World.AddSystem(new ParallaxSystem());         // 5. Update parallax layers
World.AddSystem(new RenderingSystem());        // 6. Render everything
```

### Combining Both Systems

Both systems work together seamlessly:

```csharp
public override void OnLoad()
{
    // Add systems in order
    World.AddSystem(new CameraSystem());
    World.AddSystem(new CameraLookAheadSystem());
    World.AddSystem(new ParallaxSystem());
    
    // Create camera with following
    var camera = World.CreateEntity();
    World.AddComponent(camera, new CameraComponent(1920, 1080));
    CameraSystem.SetFollowTarget(World, camera, player, followSpeed: 8.0f);
    
    // Enable look-ahead
    CameraLookAheadSystem.EnableLookAhead(World, camera);
    
    // Create parallax layers
    CreateParallaxLayers();
}
```

## Performance Considerations

### Parallax System

- **Cost**: Low - Only updates entity positions based on camera
- **Optimization**: Layers with `IsVisible = false` are skipped
- **Best Practice**: Limit to 5-7 parallax layers for optimal performance

### Camera Look-Ahead System

- **Cost**: Very Low - Simple vector math per frame
- **Optimization**: Only processes cameras with `IsEnabled = true`
- **Best Practice**: Enable only for the active camera

## Technical Details

### Parallax Calculation

```csharp
// For each layer:
offsetX = cameraX * parallaxFactor + autoScrollX * time
offsetY = cameraY * parallaxFactor + autoScrollY * time
```

### Look-Ahead Calculation

```csharp
// If target is moving:
direction = normalize(velocity)
targetOffset = direction * lookAheadDistance

// Smooth interpolation:
t = 1.0 - exp(-lookAheadSpeed * deltaTime)
currentOffset = lerp(currentOffset, targetOffset, t)

// Apply to camera:
camera.position += currentOffset * offsetScale
```

## Future Enhancements

Potential improvements for these systems:

1. **Parallax Textures**: Add actual texture/sprite support to layers
2. **Dynamic Parallax**: Adjust factors based on zoom level
3. **Look-Ahead Zones**: Different settings for different game areas
4. **Screen Shake Integration**: Combine with look-ahead for impact effects
5. **Vertical Parallax**: Enhanced support for vertical scrolling games

## References

- ROADMAP.md - Phase 4: Camera System for Semi-Angled Top-Down View
- CameraSystem.cs - Core camera following logic
- MovementSystem.cs - Velocity application

## Changelog

### Version 1.0 (Initial Implementation)

- Added ParallaxLayerComponent and ParallaxSystem
- Added CameraLookAheadComponent and CameraLookAheadSystem
- Integrated both systems into PlayableDemoScene
- Documented usage patterns and best practices
