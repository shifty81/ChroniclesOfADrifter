# Chronicles of a Drifter - Architecture Documentation

## Overview

Chronicles of a Drifter is a 2D top-down action RPG built with a custom voxel game engine using C++, .NET 9, and Lua. This document describes the architecture and design decisions for the game engine.

## Technology Stack

### C++ Core Engine
- **Purpose**: Performance-critical systems
- **Components**:
  - Game loop and timing
  - 2D rendering (DirectX 12/Vulkan backend)
  - Low-level physics
  - Input processing
  - Audio system
  - Memory management

### .NET 9 (C#) Game Logic
- **Purpose**: Rapid development of game systems
- **Components**:
  - Entity Component System (ECS)
  - Scene management
  - Gameplay logic
  - UI framework
  - High-level systems coordination

### Lua Scripting
- **Purpose**: Runtime-editable content and behaviors
- **Components**:
  - Enemy AI definitions
  - Weapon behavior scripts
  - Quest logic
  - Game data configuration

## System Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     Game Application                         │
│                      (C# .NET 9)                            │
├─────────────────────────────────────────────────────────────┤
│  Scene Manager  │  ECS Manager  │  UI System  │  Scripting  │
├─────────────────────────────────────────────────────────────┤
│                   C++/C# Interop Layer                       │
│                      (P/Invoke)                             │
├─────────────────────────────────────────────────────────────┤
│                    Core Engine (C++)                         │
├─────────────────────────────────────────────────────────────┤
│  Renderer  │  Audio  │  Input  │  Physics  │  Resources    │
├─────────────────────────────────────────────────────────────┤
│              Platform Layer (Windows/DirectX 12)             │
└─────────────────────────────────────────────────────────────┘
```

## Core Systems

### 1. Entity Component System (ECS)
The game uses an ECS architecture for managing all game objects:

- **Entities**: Unique identifiers for game objects
- **Components**: Data containers (Position, Health, Sprite, etc.)
- **Systems**: Logic processors (MovementSystem, CombatSystem, etc.)

### 2. Scene Management
- Scene-based world structure
- Asynchronous scene loading/unloading
- Transition handling between scenes
- Persistence of scene state

### 3. Procedural Generation
Multiple algorithms for content generation:
- **Binary Space Partitioning (BSP)**: Dungeon room layout
- **Random Walkers**: Organic paths and cave systems
- **Cellular Automata**: Natural-looking terrain features
- **Perlin Noise**: Terrain height maps and biome placement

### 4. Combat System
- Hit detection using collision system
- Damage calculation with modifiers
- Status effects (Bleeding, Poison, Burning)
- Damage-over-time (DoT) tracking
- Weapon variety (melee, ranged, magic)

### 5. Crafting System
- Recipe-based crafting
- Material gathering and storage
- Equipment upgrading
- Crafting station interaction
- Quality tiers and randomization

### 6. Weather and Time Systems
- Real-time day/night cycle
- Seasonal progression
- Dynamic weather states (Clear, Rain, Storm, Snow)
- Weather effects on gameplay
- Lighting adaptation

## C++/C# Integration

### Interoperability Strategy
1. **Native DLL**: C++ engine compiled as native DLL
2. **P/Invoke**: C# calls native functions via DllImport
3. **Data Marshaling**: Struct-based data transfer
4. **Callback System**: C++ callbacks to C# for events

### Example Integration Points
```csharp
// C# calling C++ rendering
[DllImport("ChroniclesEngine.dll")]
private static extern void RenderSprite(int textureId, float x, float y, float rotation);

// C++ callback to C# for input
public delegate void InputCallback(int keyCode, bool isPressed);
```

## Lua Integration

### LuaJIT Integration
- Embedded LuaJIT interpreter in C# layer
- Script hot-reloading for rapid iteration
- Sandboxed script execution
- Exposed C# API for scripts

### Use Cases
1. **Enemy AI Behaviors**
2. **Weapon Special Effects**
3. **Quest Scripts**
4. **Game Balance Tuning**
5. **Procedural Generation Parameters**

## Performance Considerations

### Optimization Strategies
1. **Object Pooling**: Reduce allocations for projectiles, effects
2. **Spatial Partitioning**: Quad-tree for collision detection
3. **Batch Rendering**: Minimize draw calls
4. **Async Loading**: Non-blocking resource loading
5. **ECS Cache Optimization**: Contiguous component memory

### Target Performance
- **Frame Rate**: 60 FPS minimum
- **Scene Transition**: < 1 second
- **Combat Latency**: < 16ms input-to-visual
- **Memory Budget**: < 2GB RAM for typical scene

## Development Workflow

### Build Pipeline
1. C++ engine compiles to native DLL
2. C# projects reference the DLL
3. Lua scripts copied to output directory
4. Assets processed and packaged

### Testing Strategy
- Unit tests for individual systems (C# XUnit)
- Integration tests for interop layer
- Performance profiling
- Automated smoke tests

## Future Considerations

### Scalability
- Multi-threading for world generation
- GPU compute shaders for particle effects
- Network multiplayer foundation
- Mod support via Lua API expansion

### Maintenance
- Clear separation of concerns
- Extensive documentation
- Example Lua scripts
- Debugging tools and profilers
