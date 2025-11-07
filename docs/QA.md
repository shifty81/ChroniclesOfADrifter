# Q&A: Chronicles of a Drifter Implementation

This document answers specific questions about implementing Chronicles of a Drifter using C++, .NET 9, and Lua.

## Question 1: What procedural generation algorithms are good for Zelda-like dungeons?

### Recommended Approach: Hybrid Algorithm

For a Zelda-like dungeon experience, we recommend a **hybrid approach** combining three algorithms:

### 1. Binary Space Partitioning (BSP) - Primary Structure

**Why it's perfect for Zelda dungeons:**
- Creates well-defined rooms like in classic Zelda dungeons
- Guarantees no overlapping spaces
- Natural corridor placement between rooms
- Easy to designate special rooms (entrance, boss, treasure)

**How it works:**
```
1. Start with full dungeon rectangle
2. Recursively split into sub-rectangles
3. Create rooms in leaf nodes
4. Connect rooms with corridors
5. Place doors at corridor junctions
```

**Tuning for Zelda feel:**
- Min room size: 5-10 tiles (intimate spaces)
- Max room size: 15-25 tiles (large boss arenas)
- Corridor width: 2-3 tiles (allows combat)
- Split ratio: 0.45-0.55 (balanced layout)

### 2. Random Walker - Secret Passages

**Purpose:**
- Create hidden paths between treasure rooms
- Generate winding, organic secret areas
- Connect optional challenges

**Implementation:**
- 30% chance per treasure room to generate secret path
- Use weighted directions toward nearby rooms
- Can be hidden behind breakable walls

### 3. Cellular Automata - Cave Sections

**Purpose:**
- Add variety with natural cave areas
- Create claustrophobic underground sections
- Generate areas with different feel from constructed dungeons

**When to use:**
- 40-50% of dungeons include cave section
- Use as connecting tissue between BSP rooms
- Ideal for mines, natural caverns themes

### Complete Algorithm Flow

```csharp
public Dungeon GenerateZeldaDungeon()
{
    // 1. BSP for main structure
    var bsp = new BSPNode(100, 100);
    bsp.Split(minSize: 8);
    var rooms = bsp.GetAllRooms();
    
    // 2. Designate special rooms
    var entrance = rooms[0];
    var boss = rooms[rooms.Count - 1];
    var key = rooms[rooms.Count / 2];
    var treasures = SelectTreasureRooms(rooms, 3);
    
    // 3. Add locked doors
    PlaceLockedDoor(boss, key);
    
    // 4. Generate secret passages
    foreach (var treasure in treasures)
    {
        if (Random.value < 0.3f)
        {
            var walker = new RandomWalker();
            walker.GenerateSecret(treasure, FindNearby(treasure));
        }
    }
    
    // 5. Add cave section (optional)
    if (Random.value < 0.5f)
    {
        var ca = new CellularAutomata();
        var caveArea = SelectEmptyArea(50, 50);
        ca.Generate(caveArea);
        ConnectToMainDungeon(caveArea);
    }
    
    // 6. Place content
    PlaceEnemies(rooms, boss);
    PlacePuzzles(rooms);
    PlaceChests(treasures);
    
    return dungeon;
}
```

### Additional Zelda-Specific Features

**Entrance Concealment:**
- Overgrowth: Requires machete/sword to clear
- Boulders: Requires bombs to clear
- Hidden: Visible only with special lens item
- Puzzle: Environmental puzzle to reveal

**Progressive Difficulty:**
```csharp
private void PlaceEnemies(List<Room> rooms, Room bossRoom)
{
    for (int i = 0; i < rooms.Count; i++)
    {
        float difficulty = (float)i / rooms.Count;
        int enemyCount = 2 + (int)(difficulty * 5);
        string enemyType = SelectEnemyForDifficulty(difficulty);
        
        for (int j = 0; j < enemyCount; j++)
        {
            SpawnEnemy(enemyType, rooms[i].GetRandomPosition());
        }
    }
    
    // Boss in final room
    SpawnBoss("skeleton_king", bossRoom.Center);
}
```

See [docs/PROCEDURAL_GENERATION.md](docs/PROCEDURAL_GENERATION.md) for complete implementation details.

---

## Question 2: Give me examples of how Lua could script enemy behaviors

Lua provides powerful runtime scripting for enemy AI. Here are comprehensive examples:

### Example 1: Basic Patrol Enemy (Goblin)

```lua
-- Goblin with state machine AI
local State = { PATROL = 1, CHASE = 2, ATTACK = 3, RETREAT = 4 }

local config = {
    detectionRange = 8.0,
    attackRange = 1.5,
    patrolSpeed = 2.0,
    chaseSpeed = 4.0,
    retreatHealthPercent = 0.3
}

local state = {
    currentState = State.PATROL,
    currentWaypoint = 1,
    lastAttackTime = 0
}

function OnUpdate(entity, deltaTime)
    local player = GetNearestPlayer(entity)
    local distance = Vector2.Distance(entity.position, player.position)
    
    -- State machine
    if state.currentState == State.PATROL then
        if distance < config.detectionRange then
            state.currentState = State.CHASE
            PlaySound("goblin_alert")
        else
            Patrol(entity, deltaTime)
        end
    elseif state.currentState == State.CHASE then
        if entity.health / entity.maxHealth < config.retreatHealthPercent then
            state.currentState = State.RETREAT
        elseif distance < config.attackRange then
            state.currentState = State.ATTACK
        else
            Chase(entity, player, deltaTime)
        end
    elseif state.currentState == State.ATTACK then
        Attack(entity, player, deltaTime)
    elseif state.currentState == State.RETREAT then
        Retreat(entity, player, deltaTime)
    end
end
```

### Example 2: Flying Enemy with Ranged Attacks

```lua
-- Bat that flies in circles and shoots projectiles
local config = {
    circleRadius = 5.0,
    circleSpeed = 2.0,
    shootCooldown = 2.0,
    burstCount = 3,
    burstDelay = 0.3
}

local state = {
    angle = 0,
    centerPoint = nil,
    shotsFired = 0
}

function OnUpdate(entity, deltaTime)
    local player = GetNearestPlayer(entity)
    
    -- Move center point toward player
    state.centerPoint = Vector2.Lerp(
        state.centerPoint, 
        player.position, 
        deltaTime * 0.5
    )
    
    -- Fly in circle
    state.angle = state.angle + config.circleSpeed * deltaTime
    local offset = Vector2.new(
        math.cos(state.angle) * config.circleRadius,
        math.sin(state.angle) * config.circleRadius
    )
    entity.position = state.centerPoint + offset
    
    -- Shoot projectiles in bursts
    if CanShoot() then
        ShootAtPlayer(entity, player)
        state.shotsFired = state.shotsFired + 1
    end
end

function ShootAtPlayer(entity, player)
    local direction = Vector2.Normalize(player.position - entity.position)
    SpawnProjectile("bat_projectile", entity.position, direction * 8.0)
    PlaySound("bat_shoot")
end
```

### Example 3: Boss with Multiple Phases

```lua
-- Skeleton King Boss with phase transitions
local Phase = { PHASE_1 = 1, PHASE_2 = 2, PHASE_3 = 3 }
local Attack = {
    MELEE_COMBO = 1,
    GROUND_SLAM = 2,
    SUMMON_SKELETONS = 3,
    BONE_BARRAGE = 4
}

function OnUpdate(entity, deltaTime)
    local healthPercent = entity.health / entity.maxHealth
    
    -- Phase transitions
    if healthPercent < 0.33 and currentPhase ~= Phase.PHASE_3 then
        TransitionToPhase3(entity)
    elseif healthPercent < 0.66 and currentPhase == Phase.PHASE_1 then
        TransitionToPhase2(entity)
    end
    
    -- Choose and execute attack
    if not attackInProgress then
        local availableAttacks = GetAttacksForPhase(currentPhase)
        local attack = availableAttacks[math.random(#availableAttacks)]
        ExecuteAttack(entity, attack)
    end
end

function TransitionToPhase2(entity)
    currentPhase = Phase.PHASE_2
    PlaySound("boss_roar")
    ShowMessage("The Skeleton King summons his army!")
    SummonSkeletons(entity, 4)
    entity.attackSpeed = entity.attackSpeed * 1.2
end

function ExecuteAttack(entity, attackType)
    if attackType == Attack.GROUND_SLAM then
        -- Charge up animation
        SetAnimation(entity, "ground_slam_charge")
        ScheduleCallback(1.0, function()
            CameraShake(0.5, 10.0)
            DealDamageInRadius(entity.position, 5.0, 40.0)
            SpawnEffect("ground_slam_effect", entity.position)
        end)
    elseif attackType == Attack.SUMMON_SKELETONS then
        for i = 1, 3 do
            local offset = Vector2.FromAngle(i * 120 * DEG_TO_RAD) * 3
            SpawnEnemy("skeleton_minion", entity.position + offset)
        end
    end
end
```

### Example 4: Behavior Tree Enemy

```lua
-- More complex AI using behavior tree pattern
local BehaviorTree = {
    -- Selector: tries children until one succeeds
    Selector = function(behaviors)
        return function(entity, deltaTime)
            for _, behavior in ipairs(behaviors) do
                if behavior(entity, deltaTime) then
                    return true
                end
            end
            return false
        end
    end,
    
    -- Sequence: runs children in order until one fails
    Sequence = function(behaviors)
        return function(entity, deltaTime)
            for _, behavior in ipairs(behaviors) do
                if not behavior(entity, deltaTime) then
                    return false
                end
            end
            return true
        end
    end
}

-- Build the behavior tree
local rootBehavior = BehaviorTree.Selector({
    -- Try to flee if low health
    BehaviorTree.Sequence({
        IsHealthLow,
        Flee
    }),
    
    -- Try to attack if in range
    BehaviorTree.Sequence({
        IsPlayerInRange,
        AttackPlayer
    }),
    
    -- Chase if player detected
    BehaviorTree.Sequence({
        IsPlayerDetected,
        ChasePlayer
    }),
    
    -- Default: patrol
    Patrol
})

function OnUpdate(entity, deltaTime)
    rootBehavior(entity, deltaTime)
end
```

### Key Advantages of Lua Scripting

1. **Hot Reloading**: Edit scripts while game runs
2. **No Recompilation**: Instant iteration
3. **Designer-Friendly**: Non-programmers can tweak AI
4. **Mod Support**: Players can create custom enemies
5. **Debugging**: `print()` statements for quick debugging

See [docs/LUA_SCRIPTING.md](docs/LUA_SCRIPTING.md) for weapon behaviors, quest scripts, and complete API documentation.

---

## Question 3: How would you handle C++ and C# integration for the game engine?

We use **P/Invoke (Platform Invoke)** as our integration strategy. Here's the complete approach:

### Architecture Overview

```
C# Game Logic (.NET 9)           P/Invoke Boundary          C++ Engine (Native DLL)
─────────────────────            ─────────────────          ──────────────────────
                                                           
Game.Update()                                               Engine Loop
   ↓                                                           ↓
NativeEngine.BeginFrame()    →  DllImport call   →       Engine_BeginFrame()
   ↓                                                           ↓
Renderer.DrawSprite()        →  DllImport call   →       Renderer_DrawSprite()
   ↓                                                           ↓
NativeEngine.EndFrame()      →  DllImport call   →       Engine_EndFrame()
                                                              ↓
InputCallback()              ←  Function Pointer ←      Input Events
```

### Step 1: C++ Export Header

```cpp
// ChroniclesEngine.h
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

extern "C" {
    // Use extern "C" to prevent C++ name mangling
    
    ENGINE_API bool Engine_Initialize(int width, int height, const char* title);
    ENGINE_API void Engine_Shutdown();
    ENGINE_API void Engine_BeginFrame();
    ENGINE_API float Engine_GetDeltaTime();
    
    ENGINE_API void Renderer_DrawSprite(
        int textureId, float x, float y, 
        float width, float height, float rotation
    );
    
    // Callbacks for C++ → C# communication
    typedef void (*InputCallbackFn)(int keyCode, bool isPressed);
    ENGINE_API void Engine_RegisterInputCallback(InputCallbackFn callback);
}
```

### Step 2: C++ Implementation

```cpp
// ChroniclesEngine.cpp
#include "ChroniclesEngine.h"
#include <memory>

namespace {
    std::unique_ptr<Engine> g_engine;
    InputCallbackFn g_inputCallback = nullptr;
}

extern "C" ENGINE_API bool Engine_Initialize(int width, int height, const char* title)
{
    g_engine = std::make_unique<Engine>();
    return g_engine->Initialize(width, height, title);
}

extern "C" ENGINE_API void Engine_BeginFrame()
{
    g_engine->BeginFrame();
    
    // Trigger callbacks for input events
    if (g_inputCallback)
    {
        auto& input = g_engine->GetInput();
        for (int key = 0; key < 512; ++key)
        {
            if (input.WasKeyPressed(key))
                g_inputCallback(key, true);
        }
    }
}

extern "C" ENGINE_API void Engine_RegisterInputCallback(InputCallbackFn callback)
{
    g_inputCallback = callback;
}
```

### Step 3: C# P/Invoke Wrapper

```csharp
// EngineInterop.cs
using System.Runtime.InteropServices;

public static class EngineInterop
{
    private const string DllName = "ChroniclesEngine.dll";
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool Engine_Initialize(
        int width, 
        int height, 
        [MarshalAs(UnmanagedType.LPStr)] string title
    );
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void Engine_BeginFrame();
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float Engine_GetDeltaTime();
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void Renderer_DrawSprite(
        int textureId, float x, float y, 
        float width, float height, float rotation
    );
    
    // Callback delegate
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void InputCallbackDelegate(
        int keyCode, 
        [MarshalAs(UnmanagedType.I1)] bool isPressed
    );
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void Engine_RegisterInputCallback(
        InputCallbackDelegate callback
    );
}
```

### Step 4: C# Managed Wrapper

```csharp
// NativeEngine.cs
public class NativeEngine : IDisposable
{
    private EngineInterop.InputCallbackDelegate _inputCallback;
    
    public event Action<int, bool> OnInput;
    
    public bool Initialize(int width, int height, string title)
    {
        bool success = EngineInterop.Engine_Initialize(width, height, title);
        
        if (success)
        {
            // Keep delegate alive to prevent GC
            _inputCallback = HandleInputCallback;
            EngineInterop.Engine_RegisterInputCallback(_inputCallback);
        }
        
        return success;
    }
    
    private void HandleInputCallback(int keyCode, bool isPressed)
    {
        OnInput?.Invoke(keyCode, isPressed);
    }
    
    public void BeginFrame()
    {
        EngineInterop.Engine_BeginFrame();
    }
    
    public void Dispose()
    {
        EngineInterop.Engine_Shutdown();
    }
}
```

### Step 5: Usage in Game

```csharp
// Program.cs
using var engine = new NativeEngine();
engine.Initialize(1920, 1080, "Chronicles of a Drifter");

engine.OnInput += (keyCode, isPressed) =>
{
    if (keyCode == Keys.Space && isPressed)
        player.Attack();
};

while (engine.IsRunning)
{
    engine.BeginFrame();
    
    // Update game logic in C#
    gameWorld.Update(engine.DeltaTime);
    
    // Render via C++ engine
    foreach (var sprite in gameWorld.Sprites)
    {
        EngineInterop.Renderer_DrawSprite(
            sprite.TextureId,
            sprite.Position.X,
            sprite.Position.Y,
            sprite.Width,
            sprite.Height,
            sprite.Rotation
        );
    }
    
    engine.EndFrame();
}
```

### Data Marshaling for Complex Types

```csharp
// Define matching structs on both sides
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
}

// C++ side
struct Vector2 { float x, y; };
struct EntityData { int id; Vector2 position; Vector2 velocity; float health; };

extern "C" ENGINE_API void Entity_GetData(int entityId, EntityData* outData);

// C# side
[DllImport(DllName)]
public static extern void Entity_GetData(int entityId, out EntityData outData);

// Usage
EntityData data;
EngineInterop.Entity_GetData(123, out data);
```

### Performance Optimization

1. **Batch Calls**: Group operations to minimize interop overhead
2. **Blittable Types**: Use simple types (int, float) when possible
3. **Keep Delegates Alive**: Store as class members to prevent GC
4. **Minimize String Marshaling**: Strings are expensive to marshal

### Build Configuration

**CMakeLists.txt:**
```cmake
add_library(ChroniclesEngine SHARED
    src/Engine/ChroniclesEngine.cpp
)
target_compile_definitions(ChroniclesEngine PRIVATE ENGINE_EXPORTS)
```

**.csproj:**
```xml
<ItemGroup>
  <None Include="..\..\build\bin\ChroniclesEngine.dll">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

### Debugging

Visual Studio 2022 supports **mixed-mode debugging**:
1. Enable "Enable native code debugging" in C# project properties
2. Set breakpoints in both C++ and C# code
3. Step seamlessly between languages

See [docs/CPP_CSHARP_INTEGRATION.md](docs/CPP_CSHARP_INTEGRATION.md) for complete details including error handling, marshaling patterns, and advanced scenarios.

---

## Summary

The Chronicles of a Drifter engine leverages the strengths of each technology:

- **C++**: High-performance rendering, physics, and audio
- **C#**: Productive development of game logic and systems
- **Lua**: Flexible, hot-reloadable content scripting

This architecture provides the performance of native code with the productivity of managed languages and the flexibility of dynamic scripting.
