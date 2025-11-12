#pragma once

#include <string>
#include <vector>
#include <map>
#include <memory>
#include <functional>
#include <any>

// Chronicles of a Drifter - Reflection System
// Provides runtime type information for editor integration

namespace Chronicles {
namespace Reflection {

// Forward declarations
class TypeInfo;
class PropertyInfo;
class FieldInfo;

// ===== Property Types =====
enum class PropertyType {
    Bool,
    Int,
    Float,
    Double,
    String,
    Vector2,
    Vector3,
    Color,
    Custom
};

// ===== Field Info =====
class FieldInfo {
public:
    FieldInfo(const std::string& name, PropertyType type, size_t offset)
        : m_name(name), m_type(type), m_offset(offset) {}
    
    const std::string& GetName() const { return m_name; }
    PropertyType GetType() const { return m_type; }
    size_t GetOffset() const { return m_offset; }
    
    // Generic getter/setter using void* to instance
    template<typename T>
    T GetValue(void* instance) const {
        return *reinterpret_cast<T*>(static_cast<char*>(instance) + m_offset);
    }
    
    template<typename T>
    void SetValue(void* instance, const T& value) const {
        *reinterpret_cast<T*>(static_cast<char*>(instance) + m_offset) = value;
    }
    
private:
    std::string m_name;
    PropertyType m_type;
    size_t m_offset;
};

// ===== Type Info =====
class TypeInfo {
public:
    TypeInfo(const std::string& name, size_t size)
        : m_name(name), m_size(size) {}
    
    const std::string& GetName() const { return m_name; }
    size_t GetSize() const { return m_size; }
    
    void AddField(const std::string& name, PropertyType type, size_t offset) {
        m_fields.emplace_back(name, type, offset);
    }
    
    const std::vector<FieldInfo>& GetFields() const { return m_fields; }
    
    const FieldInfo* GetField(const std::string& name) const {
        for (const auto& field : m_fields) {
            if (field.GetName() == name) {
                return &field;
            }
        }
        return nullptr;
    }
    
private:
    std::string m_name;
    size_t m_size;
    std::vector<FieldInfo> m_fields;
};

// ===== Reflection Registry =====
class ReflectionRegistry {
public:
    static ReflectionRegistry& Instance() {
        static ReflectionRegistry instance;
        return instance;
    }
    
    void RegisterType(const std::string& name, std::unique_ptr<TypeInfo> typeInfo) {
        m_types[name] = std::move(typeInfo);
    }
    
    const TypeInfo* GetType(const std::string& name) const {
        auto it = m_types.find(name);
        return it != m_types.end() ? it->second.get() : nullptr;
    }
    
    std::vector<std::string> GetAllTypeNames() const {
        std::vector<std::string> names;
        for (const auto& pair : m_types) {
            names.push_back(pair.first);
        }
        return names;
    }
    
private:
    ReflectionRegistry() = default;
    std::map<std::string, std::unique_ptr<TypeInfo>> m_types;
};

// ===== Registration Helper =====
template<typename T>
class TypeRegistrar {
public:
    TypeRegistrar(const std::string& name) {
        m_typeInfo = std::make_unique<TypeInfo>(name, sizeof(T));
    }
    
    template<typename FieldType>
    TypeRegistrar& Field(const std::string& name, FieldType T::*field, PropertyType type) {
        size_t offset = reinterpret_cast<size_t>(&(static_cast<T*>(nullptr)->*field));
        m_typeInfo->AddField(name, type, offset);
        return *this;
    }
    
    ~TypeRegistrar() {
        auto name = m_typeInfo->GetName();
        ReflectionRegistry::Instance().RegisterType(name, std::move(m_typeInfo));
    }
    
private:
    std::unique_ptr<TypeInfo> m_typeInfo;
};

// ===== Macros for Easy Registration =====
#define REFLECT_TYPE(TypeName) \
    static Chronicles::Reflection::TypeRegistrar<TypeName> s_##TypeName##_registrar(#TypeName)

#define REFLECT_FIELD(TypeName, FieldName, FieldType) \
    .Field(#FieldName, &TypeName::FieldName, Chronicles::Reflection::PropertyType::FieldType)

} // namespace Reflection
} // namespace Chronicles
