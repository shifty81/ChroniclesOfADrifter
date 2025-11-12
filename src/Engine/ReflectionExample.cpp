#include "Reflection.h"
#include <iostream>

// Example structures to demonstrate reflection

namespace Examples {

struct Transform {
    float x = 0.0f;
    float y = 0.0f;
    float rotation = 0.0f;
    float scale = 1.0f;
};

struct GameObject {
    std::string name = "GameObject";
    int id = 0;
    bool active = true;
    Transform transform;
};

// Register Transform type
static auto registerTransform = Chronicles::Reflection::TypeRegistrar<Transform>("Transform")
    REFLECT_FIELD(Transform, x, Float)
    REFLECT_FIELD(Transform, y, Float)
    REFLECT_FIELD(Transform, rotation, Float)
    REFLECT_FIELD(Transform, scale, Float);

// Register GameObject type  
static auto registerGameObject = Chronicles::Reflection::TypeRegistrar<GameObject>("GameObject")
    REFLECT_FIELD(GameObject, name, String)
    REFLECT_FIELD(GameObject, id, Int)
    REFLECT_FIELD(GameObject, active, Bool);

// Example function to demonstrate reflection usage
void DemonstrateReflection() {
    using namespace Chronicles::Reflection;
    
    std::cout << "=== Reflection System Demo ===" << std::endl;
    
    // Get all registered types
    auto typeNames = ReflectionRegistry::Instance().GetAllTypeNames();
    std::cout << "Registered types: " << typeNames.size() << std::endl;
    for (const auto& name : typeNames) {
        std::cout << "  - " << name << std::endl;
    }
    
    // Create an instance and use reflection
    Transform transform;
    transform.x = 10.0f;
    transform.y = 20.0f;
    transform.rotation = 45.0f;
    
    // Get type info
    auto typeInfo = ReflectionRegistry::Instance().GetType("Transform");
    if (typeInfo) {
        std::cout << "\nTransform type info:" << std::endl;
        std::cout << "  Size: " << typeInfo->GetSize() << " bytes" << std::endl;
        std::cout << "  Fields: " << typeInfo->GetFields().size() << std::endl;
        
        // Read values using reflection
        for (const auto& field : typeInfo->GetFields()) {
            std::cout << "  - " << field.GetName() << " (";
            
            switch (field.GetType()) {
                case PropertyType::Float:
                    std::cout << "float): " << field.GetValue<float>(&transform);
                    break;
                case PropertyType::Int:
                    std::cout << "int): " << field.GetValue<int>(&transform);
                    break;
                case PropertyType::Bool:
                    std::cout << "bool): " << (field.GetValue<bool>(&transform) ? "true" : "false");
                    break;
                case PropertyType::String:
                    std::cout << "string): " << field.GetValue<std::string>(&transform);
                    break;
                default:
                    std::cout << "unknown)";
            }
            std::cout << std::endl;
        }
        
        // Modify value using reflection
        auto xField = typeInfo->GetField("x");
        if (xField) {
            xField->SetValue<float>(&transform, 100.0f);
            std::cout << "\nAfter setting x to 100: " << transform.x << std::endl;
        }
    }
    
    std::cout << "\n=== End Demo ===" << std::endl;
}

} // namespace Examples
