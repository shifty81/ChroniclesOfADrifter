# C++/C# Integration Guide

## Overview

This document explains how to integrate C++ (core engine) with C# (.NET 9) (game logic) for Chronicles of a Drifter. The integration enables high-performance native code to work seamlessly with managed game logic.

## Integration Strategies

### Option 1: P/Invoke (Recommended)

**Platform Invoke (P/Invoke)** allows C# to call native C++ functions through DllImport attributes.

#### Advantages
- Simple to implement
- No additional dependencies
- Direct function calls
- Good performance for most use cases

#### Disadvantages
- Manual marshaling required
- No C++ class support (only C-style functions)
- Callbacks require special handling

### Option 2: C++/CLI

**C++/CLI** creates a managed wrapper layer that bridges C++ and C#.

#### Advantages
- Full C++ class support
- Automatic marshaling
- Can expose C++ objects to C#

#### Disadvantages
- More complex build setup
- Windows-only
- Additional compilation step

### Option 3: COM Interop

**COM (Component Object Model)** provides object-oriented interop.

#### Advantages
- Object-oriented interface
- Standard Microsoft technology

#### Disadvantages
- Complex to implement
- Overhead from COM marshaling
- Windows-only

**Recommendation**: Use **P/Invoke** for Chronicles of a Drifter due to simplicity and cross-platform potential.

## P/Invoke Implementation

### C++ Engine Side

#### Step 1: Create C-Compatible Export Header

**File**: `src/Engine/ChroniclesEngine.h`

```cpp
#pragma once

#ifdef _WIN32
    #ifdef ENGINE_EXPORTS
        #define ENGINE_API __declspec(dllexport)
    #else
        #define ENGINE_API __declspec(dllimport)
    #endif
#else
    #define ENGINE_API
#endif

// Use extern "C" to prevent name mangling
extern "C" {
    // Engine Initialization
    ENGINE_API bool Engine_Initialize(int width, int height, const char* title);
    ENGINE_API void Engine_Shutdown();
    ENGINE_API bool Engine_IsRunning();
    
    // Game Loop
    ENGINE_API void Engine_BeginFrame();
    ENGINE_API void Engine_EndFrame();
    ENGINE_API float Engine_GetDeltaTime();
    
    // Rendering
    ENGINE_API int Renderer_LoadTexture(const char* filePath);
    ENGINE_API void Renderer_UnloadTexture(int textureId);
    ENGINE_API void Renderer_DrawSprite(int textureId, float x, float y, float width, float height, float rotation);
    ENGINE_API void Renderer_DrawTile(int textureId, int tileX, int tileY, float x, float y, float scale);
    ENGINE_API void Renderer_Clear(float r, float g, float b, float a);
    ENGINE_API void Renderer_Present();
    
    // Input
    ENGINE_API bool Input_IsKeyPressed(int keyCode);
    ENGINE_API bool Input_IsKeyDown(int keyCode);
    ENGINE_API bool Input_IsKeyReleased(int keyCode);
    ENGINE_API void Input_GetMousePosition(float* outX, float* outY);
    ENGINE_API bool Input_IsMouseButtonPressed(int button);
    
    // Audio
    ENGINE_API int Audio_LoadSound(const char* filePath);
    ENGINE_API void Audio_PlaySound(int soundId, float volume);
    ENGINE_API void Audio_PlayMusic(const char* filePath, float volume, bool loop);
    ENGINE_API void Audio_StopMusic();
    
    // Physics
    ENGINE_API void Physics_SetGravity(float x, float y);
    ENGINE_API bool Physics_CheckCollision(float x1, float y1, float w1, float h1, 
                                           float x2, float y2, float w2, float h2);
    
    // Callbacks (C# -> C++)
    typedef void (*InputCallbackFn)(int keyCode, bool isPressed);
    typedef void (*CollisionCallbackFn)(int entity1, int entity2);
    
    ENGINE_API void Engine_RegisterInputCallback(InputCallbackFn callback);
    ENGINE_API void Engine_RegisterCollisionCallback(CollisionCallbackFn callback);
}
```

#### Step 2: Implement C++ Functions

**File**: `src/Engine/ChroniclesEngine.cpp`

```cpp
#include "ChroniclesEngine.h"
#include "Renderer.h"
#include "Input.h"
#include "Audio.h"
#include "Physics.h"
#include <memory>

// Internal engine state
namespace {
    std::unique_ptr<Engine> g_engine;
    InputCallbackFn g_inputCallback = nullptr;
    CollisionCallbackFn g_collisionCallback = nullptr;
}

// Engine Initialization
extern "C" ENGINE_API bool Engine_Initialize(int width, int height, const char* title) {
    try {
        g_engine = std::make_unique<Engine>();
        return g_engine->Initialize(width, height, title);
    }
    catch (...) {
        return false;
    }
}

extern "C" ENGINE_API void Engine_Shutdown() {
    if (g_engine) {
        g_engine->Shutdown();
        g_engine.reset();
    }
}

extern "C" ENGINE_API bool Engine_IsRunning() {
    return g_engine && g_engine->IsRunning();
}

// Game Loop
extern "C" ENGINE_API void Engine_BeginFrame() {
    if (g_engine) {
        g_engine->BeginFrame();
        
        // Process input and trigger callbacks
        if (g_inputCallback) {
            auto& input = g_engine->GetInput();
            // Trigger callbacks for any key state changes
            for (int key = 0; key < 512; ++key) {
                if (input.WasKeyPressed(key)) {
                    g_inputCallback(key, true);
                }
                if (input.WasKeyReleased(key)) {
                    g_inputCallback(key, false);
                }
            }
        }
    }
}

extern "C" ENGINE_API void Engine_EndFrame() {
    if (g_engine) {
        g_engine->EndFrame();
    }
}

extern "C" ENGINE_API float Engine_GetDeltaTime() {
    return g_engine ? g_engine->GetDeltaTime() : 0.0f;
}

// Rendering
extern "C" ENGINE_API int Renderer_LoadTexture(const char* filePath) {
    if (!g_engine) return -1;
    return g_engine->GetRenderer().LoadTexture(filePath);
}

extern "C" ENGINE_API void Renderer_DrawSprite(int textureId, float x, float y, 
                                               float width, float height, float rotation) {
    if (g_engine) {
        g_engine->GetRenderer().DrawSprite(textureId, x, y, width, height, rotation);
    }
}

// Input
extern "C" ENGINE_API bool Input_IsKeyPressed(int keyCode) {
    return g_engine && g_engine->GetInput().IsKeyPressed(keyCode);
}

// Callbacks
extern "C" ENGINE_API void Engine_RegisterInputCallback(InputCallbackFn callback) {
    g_inputCallback = callback;
}

extern "C" ENGINE_API void Engine_RegisterCollisionCallback(CollisionCallbackFn callback) {
    g_collisionCallback = callback;
}

// Trigger collision callback from C++
void TriggerCollisionCallback(int entity1, int entity2) {
    if (g_collisionCallback) {
        g_collisionCallback(entity1, entity2);
    }
}
```

### C# Game Logic Side

#### Step 1: Create P/Invoke Wrapper

**File**: `src/Game/Engine/EngineInterop.cs`

```csharp
using System;
using System.Runtime.InteropServices;

namespace ChroniclesOfADrifter.Engine
{
    /// <summary>
    /// P/Invoke wrapper for the native C++ engine
    /// </summary>
    public static class EngineInterop
    {
        private const string DllName = "ChroniclesEngine.dll";
        
        // Engine Initialization
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool Engine_Initialize(int width, int height, 
            [MarshalAs(UnmanagedType.LPStr)] string title);
        
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Engine_Shutdown();
        
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool Engine_IsRunning();
        
        // Game Loop
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Engine_BeginFrame();
        
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Engine_EndFrame();
        
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float Engine_GetDeltaTime();
        
        // Rendering
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Renderer_LoadTexture(
            [MarshalAs(UnmanagedType.LPStr)] string filePath);
        
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Renderer_UnloadTexture(int textureId);
        
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Renderer_DrawSprite(int textureId, float x, float y, 
            float width, float height, float rotation);
        
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Renderer_Clear(float r, float g, float b, float a);
        
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Renderer_Present();
        
        // Input
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool Input_IsKeyPressed(int keyCode);
        
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool Input_IsKeyDown(int keyCode);
        
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Input_GetMousePosition(out float x, out float y);
        
        // Audio
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Audio_LoadSound(
            [MarshalAs(UnmanagedType.LPStr)] string filePath);
        
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Audio_PlaySound(int soundId, float volume);
        
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Audio_PlayMusic(
            [MarshalAs(UnmanagedType.LPStr)] string filePath, 
            float volume, 
            [MarshalAs(UnmanagedType.I1)] bool loop);
        
        // Callbacks
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void InputCallbackDelegate(int keyCode, 
            [MarshalAs(UnmanagedType.I1)] bool isPressed);
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CollisionCallbackDelegate(int entity1, int entity2);
        
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Engine_RegisterInputCallback(InputCallbackDelegate callback);
        
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Engine_RegisterCollisionCallback(CollisionCallbackDelegate callback);
    }
}
```

#### Step 2: Create Managed Wrapper Layer

**File**: `src/Game/Engine/NativeEngine.cs`

```csharp
using System;

namespace ChroniclesOfADrifter.Engine
{
    /// <summary>
    /// High-level managed wrapper around the native engine
    /// </summary>
    public class NativeEngine : IDisposable
    {
        private bool _isInitialized;
        private EngineInterop.InputCallbackDelegate _inputCallback;
        private EngineInterop.CollisionCallbackDelegate _collisionCallback;
        
        public event Action<int, bool> OnInput;
        public event Action<int, int> OnCollision;
        
        public float DeltaTime { get; private set; }
        public bool IsRunning => _isInitialized && EngineInterop.Engine_IsRunning();
        
        public bool Initialize(int width, int height, string title)
        {
            if (_isInitialized)
                return true;
            
            _isInitialized = EngineInterop.Engine_Initialize(width, height, title);
            
            if (_isInitialized)
            {
                RegisterCallbacks();
            }
            
            return _isInitialized;
        }
        
        private void RegisterCallbacks()
        {
            // Keep delegates alive to prevent garbage collection
            _inputCallback = OnInputCallback;
            _collisionCallback = OnCollisionCallback;
            
            EngineInterop.Engine_RegisterInputCallback(_inputCallback);
            EngineInterop.Engine_RegisterCollisionCallback(_collisionCallback);
        }
        
        private void OnInputCallback(int keyCode, bool isPressed)
        {
            OnInput?.Invoke(keyCode, isPressed);
        }
        
        private void OnCollisionCallback(int entity1, int entity2)
        {
            OnCollision?.Invoke(entity1, entity2);
        }
        
        public void BeginFrame()
        {
            EngineInterop.Engine_BeginFrame();
            DeltaTime = EngineInterop.Engine_GetDeltaTime();
        }
        
        public void EndFrame()
        {
            EngineInterop.Engine_EndFrame();
        }
        
        public void Dispose()
        {
            if (_isInitialized)
            {
                EngineInterop.Engine_Shutdown();
                _isInitialized = false;
            }
        }
    }
}
```

#### Step 3: Use in Game Application

**File**: `src/Game/Program.cs`

```csharp
using ChroniclesOfADrifter.Engine;
using ChroniclesOfADrifter.Game;

namespace ChroniclesOfADrifter
{
    class Program
    {
        static void Main(string[] args)
        {
            using var engine = new NativeEngine();
            
            if (!engine.Initialize(1920, 1080, "Chronicles of a Drifter"))
            {
                Console.WriteLine("Failed to initialize engine");
                return;
            }
            
            // Set up event handlers
            engine.OnInput += HandleInput;
            engine.OnCollision += HandleCollision;
            
            // Create game systems
            var gameWorld = new GameWorld();
            var renderer = new GameRenderer();
            
            // Main game loop
            while (engine.IsRunning)
            {
                engine.BeginFrame();
                
                // Update game logic
                gameWorld.Update(engine.DeltaTime);
                
                // Render
                renderer.Render(gameWorld);
                
                engine.EndFrame();
            }
        }
        
        static void HandleInput(int keyCode, bool isPressed)
        {
            Console.WriteLine($"Key {keyCode} {(isPressed ? "pressed" : "released")}");
        }
        
        static void HandleCollision(int entity1, int entity2)
        {
            Console.WriteLine($"Collision between {entity1} and {entity2}");
        }
    }
}
```

## Data Marshaling

### Passing Complex Data Structures

#### C++ Side

```cpp
// Define a struct with explicit layout
struct Vector2 {
    float x;
    float y;
};

struct EntityData {
    int id;
    Vector2 position;
    Vector2 velocity;
    float health;
    int entityType;
};

extern "C" ENGINE_API void Entity_GetData(int entityId, EntityData* outData) {
    // Fill the struct with entity data
    auto entity = GetEntity(entityId);
    outData->id = entity->id;
    outData->position = entity->position;
    outData->velocity = entity->velocity;
    outData->health = entity->health;
    outData->entityType = static_cast<int>(entity->type);
}

extern "C" ENGINE_API void Entity_SetData(int entityId, const EntityData* data) {
    auto entity = GetEntity(entityId);
    entity->position = data->position;
    entity->velocity = data->velocity;
    entity->health = data->health;
}
```

#### C# Side

```csharp
[StructLayout(LayoutKind.Sequential)]
public struct Vector2
{
    public float X;
    public float Y;
}

[StructLayout(LayoutKind.Sequential)]
public struct EntityData
{
    public int Id;
    public Vector2 Position;
    public Vector2 Velocity;
    public float Health;
    public int EntityType;
}

[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern void Entity_GetData(int entityId, out EntityData outData);

[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern void Entity_SetData(int entityId, ref EntityData data);

// Usage
EntityData data;
Entity_GetData(123, out data);
Console.WriteLine($"Entity position: {data.Position.X}, {data.Position.Y}");
```

### Passing Arrays

#### C++ Side

```cpp
extern "C" ENGINE_API void Renderer_DrawBatch(const EntityData* entities, int count) {
    for (int i = 0; i < count; ++i) {
        DrawEntity(entities[i]);
    }
}
```

#### C# Side

```csharp
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern void Renderer_DrawBatch(
    [MarshalAs(UnmanagedType.LPArray)] EntityData[] entities, 
    int count);

// Usage
EntityData[] entities = new EntityData[100];
// Fill entities...
Renderer_DrawBatch(entities, entities.Length);
```

### Passing Strings

#### C++ to C#

```cpp
extern "C" ENGINE_API const char* Entity_GetName(int entityId) {
    static std::string name;
    name = GetEntity(entityId)->name;
    return name.c_str();  // WARNING: Lifetime management issue!
}

// Better: Use fixed buffer
extern "C" ENGINE_API void Entity_GetName(int entityId, char* buffer, int bufferSize) {
    auto name = GetEntity(entityId)->name;
    strncpy(buffer, name.c_str(), bufferSize - 1);
    buffer[bufferSize - 1] = '\0';
}
```

#### C# Side

```csharp
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern void Entity_GetName(int entityId, 
    [MarshalAs(UnmanagedType.LPStr)] StringBuilder buffer, 
    int bufferSize);

// Usage
var buffer = new StringBuilder(256);
Entity_GetName(123, buffer, buffer.Capacity);
string name = buffer.ToString();
```

## Error Handling

### C++ Side

```cpp
enum class EngineError {
    None = 0,
    InitializationFailed = 1,
    FileNotFound = 2,
    InvalidParameter = 3,
    OutOfMemory = 4
};

static EngineError g_lastError = EngineError::None;

extern "C" ENGINE_API int Engine_GetLastError() {
    return static_cast<int>(g_lastError);
}

extern "C" ENGINE_API const char* Engine_GetErrorMessage() {
    switch (g_lastError) {
        case EngineError::InitializationFailed: return "Initialization failed";
        case EngineError::FileNotFound: return "File not found";
        // ... other cases
        default: return "No error";
    }
}
```

### C# Side

```csharp
public enum EngineError
{
    None = 0,
    InitializationFailed = 1,
    FileNotFound = 2,
    InvalidParameter = 3,
    OutOfMemory = 4
}

[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern int Engine_GetLastError();

[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
[return: MarshalAs(UnmanagedType.LPStr)]
public static extern string Engine_GetErrorMessage();

// Usage
if (!EngineInterop.Engine_Initialize(800, 600, "Game"))
{
    var error = (EngineError)EngineInterop.Engine_GetLastError();
    var message = EngineInterop.Engine_GetErrorMessage();
    throw new Exception($"Engine error {error}: {message}");
}
```

## Performance Considerations

### Minimize Interop Calls

```csharp
// Bad: Many interop calls
for (int i = 0; i < 1000; i++)
{
    Renderer_DrawSprite(texId, entities[i].X, entities[i].Y, 32, 32, 0);
}

// Good: Batch call
Renderer_DrawBatch(entities, entities.Length);
```

### Use Blittable Types

Blittable types can be passed without marshaling overhead:
- Primitives: int, float, double, byte, etc.
- Structs containing only blittable types
- Fixed-size arrays of blittable types

Non-blittable types require marshaling:
- bool (use byte instead)
- string
- Arrays of non-blittable types

### Keep Callbacks Simple

```csharp
// Good: Simple callback
_inputCallback = (keyCode, isPressed) => {
    _inputQueue.Enqueue(new InputEvent(keyCode, isPressed));
};

// Bad: Complex logic in callback
_inputCallback = (keyCode, isPressed) => {
    var entity = world.GetPlayer();
    entity.HandleInput(keyCode, isPressed);
    // ... more logic
};
```

## Build Configuration

### CMakeLists.txt (C++)

```cmake
cmake_minimum_required(VERSION 3.20)
project(ChroniclesEngine)

set(CMAKE_CXX_STANDARD 20)

# Engine library
add_library(ChroniclesEngine SHARED
    src/Engine/ChroniclesEngine.cpp
    src/Engine/Renderer.cpp
    src/Engine/Input.cpp
    src/Engine/Audio.cpp
)

target_include_directories(ChroniclesEngine PUBLIC
    ${CMAKE_CURRENT_SOURCE_DIR}/src/Engine
)

# Define export macro
target_compile_definitions(ChroniclesEngine PRIVATE ENGINE_EXPORTS)

# Link DirectX 12 / Vulkan
find_package(directx-headers CONFIG REQUIRED)
target_link_libraries(ChroniclesEngine PRIVATE Microsoft::DirectX-Headers)
```

### .csproj (C#)

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
    <Platforms>x64</Platforms>
  </PropertyGroup>

  <ItemGroup>
    <!-- Copy native DLL to output -->
    <None Include="..\..\build\Release\ChroniclesEngine.dll">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="NLua" Version="1.7.3" />
  </ItemGroup>
</Project>
```

## Testing Interop

### Unit Test Example

```csharp
using Xunit;

public class EngineInteropTests
{
    [Fact]
    public void Engine_ShouldInitializeSuccessfully()
    {
        using var engine = new NativeEngine();
        bool result = engine.Initialize(800, 600, "Test");
        
        Assert.True(result);
        Assert.True(engine.IsRunning);
    }
    
    [Fact]
    public void Renderer_ShouldLoadTexture()
    {
        using var engine = new NativeEngine();
        engine.Initialize(800, 600, "Test");
        
        int textureId = EngineInterop.Renderer_LoadTexture("test.png");
        
        Assert.True(textureId >= 0);
    }
}
```

## Debugging Tips

1. **Use Mixed-Mode Debugging** in Visual Studio 2022
2. **Enable Native Debugging** in project properties
3. **Check DLL Loading** with Process Monitor
4. **Validate Marshaling** with structure sizes
5. **Use DebugView** to capture native debug output

## Conclusion

P/Invoke provides an effective bridge between C++ performance and C# productivity for Chronicles of a Drifter. By following these patterns and best practices, you can create a robust integration layer that leverages the strengths of both languages.
