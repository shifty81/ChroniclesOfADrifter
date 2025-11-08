# Camera System Documentation

## Overview

The Camera System provides 2D camera functionality for Chronicles of a Drifter, including viewport management, smooth player following, zoom controls, and camera bounds.

## Components

### CameraComponent

The `CameraComponent` represents a 2D camera in the game world.

```csharp
public class CameraComponent : IComponent
{
    public float X { get; set; }              // Camera position X
    public float Y { get; set; }              // Camera position Y
    public float Zoom { get; set; }           // Zoom level (1.0 = normal)
    public int ViewportWidth { get; set; }    // Viewport width in pixels
    public int ViewportHeight { get; set; }   // Viewport height in pixels
    public Entity FollowTarget { get; set; }  // Entity to follow
    public float FollowSpeed { get; set; }    // Follow smoothing speed
    public float? MinX { get; set; }          // Camera bounds
    public float? MaxX { get; set; }
    public float? MinY { get; set; }
    public float? MaxY { get; set; }
    public bool IsActive { get; set; }        // Whether camera is active
}
```

**Key Features:**
- **World-to-Screen Transformation**: Converts world coordinates to screen coordinates
- **Screen-to-World Transformation**: Converts screen coordinates back to world coordinates
- **Follow Target**: Smoothly follows a target entity (e.g., player)
- **Camera Bounds**: Constrains camera movement to defined world limits
- **Zoom Support**: Allows zooming in/out (default: 1.0x)

### Creating a Camera

```csharp
// Create camera entity
var camera = World.CreateEntity();
var cameraComponent = new CameraComponent(1920, 1080)
{
    Zoom = 1.0f,
    FollowSpeed = 5.0f,
    IsActive = true
};
World.AddComponent(camera, cameraComponent);
World.AddComponent(camera, new PositionComponent(960, 540));
```

## Systems

### CameraSystem

The `CameraSystem` updates camera behavior each frame:

1. **Follow Target**: Smoothly moves camera toward the followed entity
2. **Apply Bounds**: Constrains camera position within defined world bounds

**System Order**: Should be updated after movement systems but before rendering systems.

```csharp
World.AddSystem(new PlayerInputSystem());
World.AddSystem(new MovementSystem());
World.AddSystem(new CameraSystem());        // Camera system here
World.AddSystem(new RenderingSystem());
```

### CameraInputSystem

The `CameraInputSystem` handles keyboard input for camera controls:

- **Zoom In**: `+` or `=` keys
- **Zoom Out**: `-` key
- **Zoom Range**: 0.25x to 4.0x

```csharp
World.AddSystem(new CameraInputSystem());
```

## API Reference

### CameraSystem Static Methods

#### SetFollowTarget

Set the camera to follow a specific entity:

```csharp
CameraSystem.SetFollowTarget(World world, Entity cameraEntity, Entity targetEntity, float followSpeed = 5.0f)
```

**Parameters:**
- `world`: The ECS world
- `cameraEntity`: The camera entity
- `targetEntity`: The entity to follow (typically the player)
- `followSpeed`: Follow smoothing speed (0 = instant, higher = slower/smoother)

**Example:**
```csharp
var camera = World.CreateEntity();
var player = World.CreateEntity();
// ... add components ...
CameraSystem.SetFollowTarget(World, camera, player, followSpeed: 8.0f);
```

#### SetBounds

Set camera movement bounds to constrain the camera within a world area:

```csharp
CameraSystem.SetBounds(World world, Entity cameraEntity, float? minX, float? maxX, float? minY, float? maxY)
```

**Parameters:**
- `world`: The ECS world
- `cameraEntity`: The camera entity
- `minX`, `maxX`, `minY`, `maxY`: World boundaries (null = no limit)

**Example:**
```csharp
// Create a bounded world (3840x2160, 2x viewport size)
CameraSystem.SetBounds(World, camera, 
    minX: 0, maxX: 3840,
    minY: 0, maxY: 2160);
```

#### SetZoom

Change the camera zoom level:

```csharp
CameraSystem.SetZoom(World world, Entity cameraEntity, float zoom)
```

**Parameters:**
- `world`: The ECS world
- `cameraEntity`: The camera entity
- `zoom`: Zoom level (0.1 minimum, clamped automatically)

**Example:**
```csharp
CameraSystem.SetZoom(World, camera, 2.0f); // 2x zoom
```

#### GetActiveCamera

Get the currently active camera component:

```csharp
CameraComponent? camera = CameraSystem.GetActiveCamera(World world)
```

**Returns:** The active `CameraComponent`, or null if none exists.

## Usage Examples

### Basic Camera Setup

```csharp
// Create camera
var camera = World.CreateEntity();
var cameraComponent = new CameraComponent(1920, 1080)
{
    Zoom = 1.0f,
    IsActive = true
};
World.AddComponent(camera, cameraComponent);

// Create player
var player = World.CreateEntity();
World.AddComponent(player, new PositionComponent(960, 540));
World.AddComponent(player, new PlayerComponent());

// Set camera to follow player
CameraSystem.SetFollowTarget(World, camera, player, followSpeed: 5.0f);
```

### Camera with Bounds

```csharp
// Create camera with world bounds
var camera = World.CreateEntity();
var cameraComponent = new CameraComponent(1920, 1080);
World.AddComponent(camera, cameraComponent);

// Set bounds for a larger world (3840x2160)
CameraSystem.SetBounds(World, camera,
    minX: 0, maxX: 3840,
    minY: 0, maxY: 2160);
```

### Multiple Cameras

You can have multiple cameras, but only one should be active at a time:

```csharp
// Main camera (active)
var mainCamera = World.CreateEntity();
World.AddComponent(mainCamera, new CameraComponent(1920, 1080) { IsActive = true });

// Minimap camera (inactive)
var minimapCamera = World.CreateEntity();
World.AddComponent(minimapCamera, new CameraComponent(400, 300) { IsActive = false });

// Switch cameras
var mainCam = World.GetComponent<CameraComponent>(mainCamera);
var miniCam = World.GetComponent<CameraComponent>(minimapCamera);
mainCam.IsActive = false;
miniCam.IsActive = true;
```

## Coordinate Transformations

The camera provides methods to transform between world and screen coordinates:

### World to Screen

Convert world coordinates to screen coordinates:

```csharp
var camera = CameraSystem.GetActiveCamera(World);
if (camera != null)
{
    var (screenX, screenY) = camera.WorldToScreen(worldX, worldY);
    // screenX, screenY are now in screen space
}
```

### Screen to World

Convert screen coordinates (e.g., mouse position) to world coordinates:

```csharp
var camera = CameraSystem.GetActiveCamera(World);
if (camera != null)
{
    var (worldX, worldY) = camera.ScreenToWorld(mouseX, mouseY);
    // worldX, worldY are now in world space
}
```

## Integration with Rendering

The `ConsoleRenderer` automatically uses the active camera for rendering:

```csharp
public class ConsoleRenderer
{
    private void DrawEntities(World world)
    {
        // Get active camera
        var camera = CameraSystem.GetActiveCamera(world);
        
        foreach (var entity in world.GetEntitiesWithComponent<PositionComponent>())
        {
            var position = world.GetComponent<PositionComponent>(entity);
            
            if (camera != null)
            {
                // Transform world coordinates to screen coordinates
                var (screenX, screenY) = camera.WorldToScreen(position.X, position.Y);
                // ... render at screen coordinates
            }
        }
    }
}
```

When no camera is present, the renderer falls back to direct world-to-screen mapping.

## Camera Follow Behavior

The camera follow system uses exponential smoothing for natural, smooth movement:

```csharp
// Smooth follow formula
float t = 1.0f - MathF.Exp(-followSpeed * deltaTime);
camera.X = Lerp(camera.X, target.X, t);
camera.Y = Lerp(camera.Y, target.Y, t);
```

**Follow Speed Guidelines:**
- `0`: Instant follow (no smoothing)
- `3-5`: Gentle, cinematic follow
- `8-10`: Responsive follow for action games
- `15+`: Very tight follow, almost instant

## Performance Considerations

1. **Single Active Camera**: Only one camera should be active at a time to avoid redundant calculations
2. **Follow Speed**: Higher follow speeds require more calculations but provide smoother results
3. **Bounds Checking**: Camera bounds are checked every frame; ensure bounds are reasonable

## Common Patterns

### Centered Player View

```csharp
// Camera always centered on player
CameraSystem.SetFollowTarget(World, camera, player, followSpeed: 0);
```

### Lookahead Camera

For a camera that looks ahead in the player's movement direction:

```csharp
// In your custom system:
var playerVel = World.GetComponent<VelocityComponent>(player);
var camera = CameraSystem.GetActiveCamera(World);
if (camera != null && playerVel != null)
{
    float lookahead = 100f;
    camera.FollowTarget.X = playerPos.X + playerVel.VX * lookahead;
    camera.FollowTarget.Y = playerPos.Y + playerVel.VY * lookahead;
}
```

### Screen Shake

Implement screen shake by temporarily offsetting camera position:

```csharp
var camera = CameraSystem.GetActiveCamera(World);
if (camera != null)
{
    float shakeAmount = 10f;
    camera.X += Random.Shared.NextSingle() * shakeAmount - shakeAmount / 2;
    camera.Y += Random.Shared.NextSingle() * shakeAmount - shakeAmount / 2;
}
```

## Demo Scenes

### CameraDemoScene

A complete demo showcasing camera features:
- Player movement with camera following
- Zoom controls (+/- keys)
- Camera bounds preventing camera from leaving world
- Multiple enemies scattered across a large world

To run the demo, update `Program.cs`:

```csharp
var scene = new CameraDemoScene();
scene.OnLoad();
```

### PlayableDemoScene

The playable demo has been updated to include camera support:
- Camera follows player during combat
- Zoom controls available
- Smooth camera movement enhances gameplay feel

## Troubleshooting

### Camera Not Following Player

**Problem**: Camera stays stationary while player moves.

**Solution**: Ensure `CameraSystem` is added to the world and runs after `MovementSystem`:

```csharp
World.AddSystem(new MovementSystem());
World.AddSystem(new CameraSystem());
```

### Camera Jumping/Jerky

**Problem**: Camera movement is not smooth.

**Solution**: Increase the `FollowSpeed` value:

```csharp
CameraSystem.SetFollowTarget(World, camera, player, followSpeed: 8.0f);
```

### Entities Not Visible

**Problem**: Entities disappear when camera moves.

**Solution**: Check that rendering system uses camera transformation. See "Integration with Rendering" section.

### Camera Bounds Not Working

**Problem**: Camera moves beyond intended boundaries.

**Solution**: Ensure bounds account for viewport size. The camera center must stay within bounds:

```csharp
// For a 1920x1080 viewport with 1.0 zoom:
// Camera can move from (960, 540) to (worldWidth - 960, worldHeight - 540)
CameraSystem.SetBounds(World, camera, 0, worldWidth, 0, worldHeight);
```

## Future Enhancements

Potential additions to the camera system:

1. **Camera Shake**: Built-in screen shake effects
2. **Camera Zones**: Define areas where camera behavior changes
3. **Cinematic Camera**: Scripted camera movements for cutscenes
4. **Split-Screen**: Multiple viewports for local multiplayer
5. **Camera Transitions**: Smooth transitions between different camera states
6. **Dolly Zoom**: Simultaneous zoom and position adjustment effect

## See Also

- [Architecture Documentation](ARCHITECTURE.md)
- [ECS Implementation](ECS_IMPLEMENTATION.md)
- [Animation System](ANIMATION_SYSTEM.md)
