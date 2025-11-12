# C++ Reflection System

## Overview

The Chronicles of a Drifter reflection system provides runtime type information (RTTI) for C++ classes and structures. This enables the editor to inspect and modify game objects generically without hard-coding editor logic for every class.

## Features

- **Runtime Type Inspection**: Query types, fields, and properties at runtime
- **Generic Property Access**: Get/set values without knowing types at compile time
- **Cross-Language Support**: C API for C# and Python integration
- **Minimal Overhead**: Opt-in registration with zero overhead for unregistered types
- **Easy Registration**: Simple macros for registering types and fields

## Architecture

### Components

1. **Reflection.h**: Core reflection system (header-only)
   - `TypeInfo`: Stores information about a registered type
   - `FieldInfo`: Stores information about a field
   - `ReflectionRegistry`: Central registry for all types
   - `TypeRegistrar`: Helper for registering types

2. **ReflectionAPI.h/cpp**: C API for cross-language integration
   - Query functions for types and fields
   - Value access functions (get/set)
   - String marshaling helpers

3. **ReflectionSystem.cs**: C# wrapper for reflection API
   - Managed wrapper over P/Invoke calls
   - Type-safe value access
   - LINQ-friendly interfaces

## Usage

### Registering a Type (C++)

```cpp
#include "Reflection.h"

// Define your structure
struct Transform {
    float x = 0.0f;
    float y = 0.0f;
    float rotation = 0.0f;
    float scale = 1.0f;
};

// Register the type and its fields
static auto registerTransform = Chronicles::Reflection::TypeRegistrar<Transform>("Transform")
    REFLECT_FIELD(Transform, x, Float)
    REFLECT_FIELD(Transform, y, Float)
    REFLECT_FIELD(Transform, rotation, Float)
    REFLECT_FIELD(Transform, scale, Float);
```

### Using Reflection (C++)

```cpp
using namespace Chronicles::Reflection;

// Create an instance
Transform transform;
transform.x = 100.0f;

// Get type information
auto typeInfo = ReflectionRegistry::Instance().GetType("Transform");
if (typeInfo) {
    std::cout << "Type: " << typeInfo->GetName() << std::endl;
    std::cout << "Size: " << typeInfo->GetSize() << " bytes" << std::endl;
    
    // Iterate fields
    for (const auto& field : typeInfo->GetFields()) {
        std::cout << "Field: " << field.GetName() << std::endl;
    }
    
    // Get/set value using reflection
    auto xField = typeInfo->GetField("x");
    if (xField) {
        float value = xField->GetValue<float>(&transform);
        std::cout << "x = " << value << std::endl;
        
        xField->SetValue<float>(&transform, 200.0f);
    }
}
```

### Using Reflection (C#)

```csharp
using ChroniclesOfADrifter.Engine.Reflection;

// Get all registered types
var types = ReflectionSystem.GetAllTypes();
foreach (var typeName in types) {
    Console.WriteLine($"Type: {typeName}");
}

// Get type information
var typeInfo = ReflectionSystem.GetTypeInfo("Transform");
if (typeInfo != null) {
    Console.WriteLine($"Type: {typeInfo.Name}");
    Console.WriteLine($"Size: {typeInfo.Size} bytes");
    
    // Iterate fields
    foreach (var field in typeInfo.Fields) {
        Console.WriteLine($"  {field.Name}: {field.Type}");
    }
}

// Get/set values (assuming you have an IntPtr to a C++ Transform instance)
IntPtr instancePtr = /* ... */;
float xValue = (float)ReflectionSystem.GetValue("Transform", "x", instancePtr);
ReflectionSystem.SetValue("Transform", "x", instancePtr, 300.0f);
```

## Supported Property Types

- **Bool**: Boolean values
- **Int**: 32-bit integers
- **Float**: Single-precision floating point
- **Double**: Double-precision floating point
- **String**: UTF-8 strings (std::string)
- **Vector2**: 2D vectors (future)
- **Vector3**: 3D vectors (future)
- **Color**: Color values (future)
- **Custom**: User-defined types (future)

## Editor Integration

The reflection system is the foundation for editor features:

### Property Inspector

The editor can display and edit properties of any reflected type:

```csharp
// Property Inspector pseudocode
var typeInfo = ReflectionSystem.GetTypeInfo(selectedObject.TypeName);
foreach (var field in typeInfo.Fields) {
    // Create appropriate UI control based on field.Type
    switch (field.Type) {
        case PropertyType.Float:
            ShowFloatSlider(field.Name, instance);
            break;
        case PropertyType.Bool:
            ShowCheckbox(field.Name, instance);
            break;
        case PropertyType.String:
            ShowTextBox(field.Name, instance);
            break;
    }
}
```

### Scene Serialization

Reflection enables automatic serialization:

```csharp
// Serialize an object to JSON
var typeInfo = ReflectionSystem.GetTypeInfo(typeName);
var json = new JsonObject();
foreach (var field in typeInfo.Fields) {
    var value = ReflectionSystem.GetValue(typeName, field.Name, instance);
    json[field.Name] = value;
}
```

## Performance Considerations

### Registration Overhead

- Registration happens at static initialization time (before main())
- Zero runtime overhead for unregistered types
- Registry lookup is O(log n) using std::map

### Value Access

- Direct pointer arithmetic (no virtual calls)
- Type checking at runtime for safety
- Consider caching TypeInfo/FieldInfo pointers for hot paths

### Best Practices

1. **Register only editor-visible types**: Don't register internal implementation types
2. **Cache lookups**: Store TypeInfo/FieldInfo pointers when possible
3. **Batch operations**: Group multiple field accesses together
4. **Use templates for C++**: Direct access is faster than reflection

## Extending the System

### Adding New Property Types

1. Add enum value to `PropertyType` in Reflection.h
2. Add get/set functions to ReflectionAPI.h/cpp
3. Update C# PropertyType enum and ReflectionSystem class

Example for Vector2:

```cpp
// In Reflection.h
enum class PropertyType {
    // ... existing types ...
    Vector2,
};

// In ReflectionAPI.h
extern "C" {
    ENGINE_API void Reflection_GetVector2Value(const char* typeName, 
        const char* fieldName, void* instance, float* outX, float* outY);
    ENGINE_API void Reflection_SetVector2Value(const char* typeName,
        const char* fieldName, void* instance, float x, float y);
}
```

### Custom Type Serializers

For complex types, implement custom serialization:

```cpp
// Register a custom serializer
ReflectionRegistry::Instance().RegisterSerializer("CustomType",
    [](void* instance) -> std::string {
        // Serialize to string
    },
    [](void* instance, const std::string& data) {
        // Deserialize from string
    }
);
```

## Future Enhancements

- [ ] Support for arrays and collections
- [ ] Support for nested objects
- [ ] Custom attributes/metadata on fields
- [ ] Function/method reflection
- [ ] Virtual property support
- [ ] Enum reflection
- [ ] Inheritance information
- [ ] Validation and constraints

## Security Considerations

1. **Type Safety**: The system performs runtime type checking
2. **Bounds Checking**: Field offsets are validated
3. **Null Checks**: All API functions check for null pointers
4. **String Safety**: All string operations use bounded copies

## Debugging

Enable reflection debugging by defining `REFLECTION_DEBUG`:

```cpp
#define REFLECTION_DEBUG
#include "Reflection.h"
```

This will output registration information to stdout.

## Examples

See `src/Engine/ReflectionExample.cpp` for complete working examples.

## Integration with Other Systems

### Lua Scripting

Expose reflection to Lua for dynamic property access:

```lua
-- Access C++ properties from Lua
local transform = game:GetTransform(entityId)
print("Position: " .. reflection.get(transform, "x") .. ", " .. reflection.get(transform, "y"))
reflection.set(transform, "x", 100.0)
```

### Python Tooling

Use reflection for automated testing and validation:

```python
# Python asset validation tool
for type_name in reflection.get_all_types():
    type_info = reflection.get_type_info(type_name)
    print(f"Validating {type_name}...")
    # Perform validation checks
```

## Conclusion

The reflection system provides a solid foundation for editor integration while maintaining performance and type safety. It enables powerful editor features without coupling the engine to specific editor implementations.
