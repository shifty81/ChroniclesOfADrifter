# Multi-Language Editor Integration

## Overview

This document describes the complete multi-language editor integration architecture for Chronicles of a Drifter, enabling seamless communication between C++, Lua, Python, and .NET 9.

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                     .NET 9 Editor GUI                           │
│  (Separate Process - WPF/Avalonia UI)                          │
│  - Scene Viewport                                               │
│  - Property Inspector (using Reflection)                        │
│  - Asset Browser                                                │
│  - Console/Logging                                              │
│  - Undo/Redo System                                            │
└────────────────────────┬────────────────────────────────────────┘
                         │ IPC (Named Pipes/Sockets)
                         │ JSON Messages
                         │
┌────────────────────────┴────────────────────────────────────────┐
│                 C++ Core Engine                                 │
│  ┌──────────────┐  ┌───────────────┐  ┌──────────────────┐   │
│  │  Reflection  │  │ Serialization │  │   IPC Server     │   │
│  │   System     │  │    System     │  │   (Commands)     │   │
│  └──────┬───────┘  └───────┬───────┘  └────────┬─────────┘   │
│         │                  │                    │              │
│  ┌──────┴──────────────────┴────────────────────┴─────────┐   │
│  │         Core Engine API (C Interface)                   │   │
│  │  - Rendering (DirectX 11/12, SDL2)                     │   │
│  │  - Physics                                              │   │
│  │  - Audio                                                │   │
│  └─────┬───────────────────┬──────────────────┬────────────┘   │
└────────┼───────────────────┼──────────────────┼────────────────┘
         │                   │                  │
         │                   │                  │
┌────────┴────────┐ ┌────────┴────────┐ ┌──────┴─────────┐
│  .NET 9 Game    │ │  Lua Scripts    │ │ Python Tools   │
│  Logic (C#)     │ │  (NLua)         │ │ (ctypes)       │
│  - ECS          │ │  - AI Behaviors │ │ - Asset Proc.  │
│  - Systems      │ │  - Gameplay     │ │ - Build Tools  │
│  - Scenes       │ │  - Hot-reload   │ │ - Automation   │
└─────────────────┘ └─────────────────┘ └────────────────┘
```

## Components

### 1. C++ Core Engine

#### 1.1 Reflection System

**Location:** `src/Engine/Reflection.h`, `src/Engine/ReflectionAPI.h/cpp`

**Purpose:** Provides runtime type information for editor integration.

**Features:**
- Runtime type inspection
- Generic property get/set
- Zero overhead for unregistered types
- Cross-language compatible (C API)

**Usage:**
```cpp
// Register a type
struct Transform {
    float x, y, rotation, scale;
};

static auto reg = TypeRegistrar<Transform>("Transform")
    REFLECT_FIELD(Transform, x, Float)
    REFLECT_FIELD(Transform, y, Float)
    REFLECT_FIELD(Transform, rotation, Float)
    REFLECT_FIELD(Transform, scale, Float);
```

**API Functions:**
- `Reflection_GetTypeCount()` - Get number of registered types
- `Reflection_GetTypeName()` - Get type name by index
- `Reflection_GetFieldCount()` - Get field count for a type
- `Reflection_GetFloatValue()` - Get float field value
- `Reflection_SetFloatValue()` - Set float field value

#### 1.2 Serialization System

**Location:** `src/Engine/Serialization.h`, `src/Engine/SerializationAPI.h/cpp`

**Purpose:** Automatic JSON serialization using reflection.

**Features:**
- Reflection-based serialization
- File save/load support
- Cross-language compatible
- Extensible for custom types

**Usage:**
```cpp
Transform transform;
transform.x = 100.0f;

// Serialize to JSON
std::string json = SerializeObject("Transform", &transform);

// Save to file
Serialization_SaveToFile("Transform", &transform, "transform.json");
```

**API Functions:**
- `Serialization_ToJson()` - Serialize to JSON string
- `Serialization_FromJson()` - Deserialize from JSON (stub)
- `Serialization_SaveToFile()` - Save to file
- `Serialization_LoadFromFile()` - Load from file (stub)

#### 1.3 IPC System

**Location:** `src/Engine/IPC.h`, `src/Engine/IPC.cpp`

**Purpose:** Inter-process communication between editor and engine.

**Features:**
- Named pipe communication (Windows)
- Unix domain sockets (Linux/macOS)
- JSON message protocol
- Event system for editor notifications

**Message Types:**
- **Query:** GetTypes, GetTypeInfo, GetSceneObjects, GetObjectProperties
- **Command:** SetProperty, CreateObject, DeleteObject, LoadScene, SaveScene
- **Event:** ObjectSelected, ObjectModified, SceneChanged

**Usage:**
```cpp
// Engine side - Start IPC server
IPCServer server;
server.Start("ChroniclesEngine");

// Register custom handlers
server.RegisterHandler(MessageType::SetProperty, [](const std::string& payload) {
    // Handle property change
    return "{\"success\":true}";
});

// Main loop
while (running) {
    server.Update();  // Process messages
}

// Editor side - Connect to engine
IPCClient client;
client.Connect("ChroniclesEngine");

// Send command
std::string response = client.SendCommand(
    MessageType::GetTypes, 
    "{}"
);
```

#### 1.4 Python Integration

**Location:** `src/Engine/PythonAPI.h/cpp`, `scripts/python/engine_bindings.py`

**Purpose:** Python bindings for tooling and automation.

**Features:**
- ctypes-based bindings (no compilation needed)
- Access to reflection and serialization APIs
- Asset processing tools
- Build automation

**Usage:**
```python
from engine_bindings import ReflectionSystem, SerializationSystem

# Query types
types = ReflectionSystem.get_all_types()
for type_name in types:
    print(f"Type: {type_name}")

# Get type information
type_info = ReflectionSystem.get_type_info("Transform")
for field in type_info['fields']:
    print(f"  {field['name']}: {field['type']}")
```

**Tools:**
- `asset_processor.py` - Asset processing automation
  - `python asset_processor.py process` - Process assets
  - `python asset_processor.py validate` - Validate assets
  - `python asset_processor.py manifest` - Generate manifest

#### 1.5 Enhanced Lua API

**Location:** `src/Engine/LuaEnhancedAPI.h/cpp`

**Purpose:** Extended Lua capabilities for gameplay scripting.

**Features:**
- Reflection API access from Lua
- Hot-reload support
- Debugging hooks
- Automatic marshaling

**API Functions:**
- `Lua_RegisterReflectionAPI()` - Expose reflection to Lua
- `Lua_RegisterSerializationAPI()` - Expose serialization to Lua
- `Lua_HotReloadScript()` - Reload script without restart
- `Lua_EnableDebugging()` - Enable debug hooks

### 2. .NET 9 Integration

#### 2.1 C# Reflection Wrapper

**Location:** `src/Game/Engine/Reflection/ReflectionSystem.cs`

**Features:**
- Type-safe wrappers over P/Invoke
- LINQ-friendly interfaces
- Managed memory handling

**Usage:**
```csharp
// Get all types
var types = ReflectionSystem.GetAllTypes();

// Get type info
var typeInfo = ReflectionSystem.GetTypeInfo("Transform");
foreach (var field in typeInfo.Fields) {
    Console.WriteLine($"{field.Name}: {field.Type}");
}

// Get/set values
float x = (float)ReflectionSystem.GetValue("Transform", "x", instancePtr);
ReflectionSystem.SetValue("Transform", "x", instancePtr, 200.0f);
```

#### 2.2 C# Serialization Wrapper

**Location:** `src/Game/Engine/Serialization/SerializationSystem.cs`

**Usage:**
```csharp
// Serialize to JSON
string json = SerializationSystem.ToJson("Transform", instancePtr);

// Save to file
SerializationSystem.SaveToFile("Transform", instancePtr, "transform.json");
```

#### 2.3 C# IPC Wrapper

**Location:** `src/Game/Engine/IPC/IPCSystem.cs`

**Usage:**
```csharp
// Server (in game/engine)
using var server = new IPCServer();
server.Start("ChroniclesEngine");

while (running) {
    server.Update();
    server.SendEvent(MessageType.ObjectModified, "{\"id\":123}");
}

// Client (in editor)
using var client = new IPCClient();
client.Connect("ChroniclesEngine");

string response = client.SendCommand(
    MessageType.GetTypes,
    "{}"
);
```

### 3. Editor Application (Future)

**Location:** `src/Editor/` (to be created)

**Technology:** .NET 9 + WPF or Avalonia UI

**Features:**
- **Scene Viewport:** Real-time rendering window
- **Property Inspector:** Edit object properties using reflection
- **Asset Browser:** File management and import
- **Console:** Logging and debugging
- **Undo/Redo:** Operation history
- **Hot-reload:** Live asset updates

**Planned Structure:**
```
src/Editor/
├── ChroniclesEditor.csproj
├── App.xaml / Program.cs
├── MainWindow.xaml
├── ViewModels/
│   ├── MainViewModel.cs
│   ├── SceneViewModel.cs
│   ├── PropertyInspectorViewModel.cs
│   └── AssetBrowserViewModel.cs
├── Views/
│   ├── SceneView.xaml
│   ├── PropertyInspector.xaml
│   └── AssetBrowser.xaml
└── Services/
    ├── EngineConnection.cs (IPC client)
    ├── AssetService.cs
    └── UndoRedoService.cs
```

## Communication Flow

### Editor → Engine (Commands)

1. Editor creates command message
2. Serialize command and parameters to JSON
3. Send via IPC to engine
4. Engine processes command using registered handler
5. Engine returns JSON response
6. Editor deserializes and updates UI

Example:
```
Editor: SendCommand(SetProperty, {"type":"Transform","field":"x","value":100})
Engine: Processes → Updates property → Returns {"success":true}
Editor: Updates property inspector UI
```

### Engine → Editor (Events)

1. Engine detects state change
2. Create event message with data
3. Send to all connected editors via IPC
4. Editors receive and update UI

Example:
```
Engine: Object selected in game → SendEvent(ObjectSelected, {"id":123})
Editor: Receives event → Highlights object in scene view
```

## Data Formats

### Reflection Query Response
```json
{
  "types": ["Transform", "GameObject", "Rigidbody"]
}
```

### Type Info Response
```json
{
  "name": "Transform",
  "size": 16,
  "fields": [
    {"name": "x", "type": 2, "offset": 0},
    {"name": "y", "type": 2, "offset": 4},
    {"name": "rotation", "type": 2, "offset": 8},
    {"name": "scale", "type": 2, "offset": 12}
  ]
}
```

### Property Set Command
```json
{
  "typeName": "Transform",
  "fieldName": "x",
  "instanceId": 123,
  "value": 100.0
}
```

## Building the System

### Prerequisites
- CMake 3.20+
- .NET 9 SDK
- Visual Studio 2022 (Windows) or compatible C++ compiler
- Python 3.8+ (optional, for tooling)

### Build Steps

1. **Build C++ Engine:**
```bash
mkdir build && cd build
cmake ..
cmake --build . --config Release
```

2. **Build C# Game:**
```bash
cd src/Game
dotnet build -c Release
```

3. **Test Python Bindings:**
```bash
cd scripts/python
python engine_bindings.py
```

## Testing

### Test Reflection System
```cpp
// In C++
#include "ReflectionExample.cpp"
Examples::DemonstrateReflection();
```

### Test Serialization
```csharp
// In C#
var json = SerializationSystem.ToJson("Transform", ptr);
Console.WriteLine(json);
```

### Test IPC
```csharp
// Start server in game
var server = new IPCServer();
server.Start();

// Connect client in editor
var client = new IPCClient();
client.Connect();
var response = client.SendCommand(MessageType.GetTypes, "{}");
```

## Best Practices

### Performance
1. Cache type info lookups
2. Batch IPC commands when possible
3. Use events for editor updates, not polling
4. Minimize reflection usage in hot paths

### Security
1. Validate all IPC messages
2. Sanitize file paths
3. Check permissions before file operations
4. Rate-limit IPC commands

### Extensibility
1. Register new types with reflection
2. Add custom serialization for complex types
3. Implement custom IPC handlers
4. Extend Python tools for project needs

## Future Enhancements

- [ ] Complete JSON deserialization
- [ ] Platform-specific IPC implementation (pipes/sockets)
- [ ] Full Python interpreter embedding
- [ ] Advanced reflection (arrays, nested objects, inheritance)
- [ ] Visual property editors in editor
- [ ] Gizmos for 3D manipulation
- [ ] Timeline for animations/cutscenes
- [ ] Profiler integration

## Troubleshooting

### Engine DLL Not Found
- Ensure C++ engine is built
- Check DLL/SO is in correct output directory
- Verify `ChroniclesOfADrifter.csproj` copies DLL

### Reflection Returns Empty
- Verify types are registered with REFLECT_TYPE
- Check registration happens before query
- Ensure static initialization order

### IPC Connection Fails
- Verify server is started before client connects
- Check pipe name matches
- Ensure proper permissions
- Platform-specific: Windows needs elevated privileges for global pipes

### Python Can't Find Library
- Check PYTHONPATH includes scripts directory
- Verify library path in engine_bindings.py
- Build C++ engine first

## References

- [Reflection System Documentation](REFLECTION_SYSTEM.md)
- [C++/C# Integration Guide](CPP_CSHARP_INTEGRATION.md)
- [Lua Scripting Guide](LUA_SCRIPTING.md)

## Conclusion

This multi-language editor integration provides a solid foundation for building a feature-rich editor while maintaining the performance benefits of C++ for the core engine. The architecture is modular, extensible, and follows industry best practices for editor-engine communication.
