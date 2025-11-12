#include "ReflectionAPI.h"
#include <cstring>
#include <algorithm>

using namespace Chronicles::Reflection;

// ===== Type Query Functions =====

extern "C" ENGINE_API int Reflection_GetTypeCount() {
    return static_cast<int>(ReflectionRegistry::Instance().GetAllTypeNames().size());
}

extern "C" ENGINE_API void Reflection_GetTypeName(int index, char* buffer, int bufferSize) {
    if (!buffer || bufferSize <= 0) return;
    
    auto names = ReflectionRegistry::Instance().GetAllTypeNames();
    if (index >= 0 && index < static_cast<int>(names.size())) {
        std::strncpy(buffer, names[index].c_str(), bufferSize - 1);
        buffer[bufferSize - 1] = '\0';
    } else {
        buffer[0] = '\0';
    }
}

extern "C" ENGINE_API int Reflection_GetTypeSize(const char* typeName) {
    if (!typeName) return 0;
    
    auto typeInfo = ReflectionRegistry::Instance().GetType(typeName);
    return typeInfo ? static_cast<int>(typeInfo->GetSize()) : 0;
}

extern "C" ENGINE_API int Reflection_GetFieldCount(const char* typeName) {
    if (!typeName) return 0;
    
    auto typeInfo = ReflectionRegistry::Instance().GetType(typeName);
    return typeInfo ? static_cast<int>(typeInfo->GetFields().size()) : 0;
}

// ===== Field Query Functions =====

extern "C" ENGINE_API void Reflection_GetFieldName(const char* typeName, int fieldIndex, 
                                                   char* buffer, int bufferSize) {
    if (!typeName || !buffer || bufferSize <= 0) return;
    
    auto typeInfo = ReflectionRegistry::Instance().GetType(typeName);
    if (typeInfo) {
        const auto& fields = typeInfo->GetFields();
        if (fieldIndex >= 0 && fieldIndex < static_cast<int>(fields.size())) {
            std::strncpy(buffer, fields[fieldIndex].GetName().c_str(), bufferSize - 1);
            buffer[bufferSize - 1] = '\0';
            return;
        }
    }
    
    buffer[0] = '\0';
}

extern "C" ENGINE_API int Reflection_GetFieldType(const char* typeName, const char* fieldName) {
    if (!typeName || !fieldName) return -1;
    
    auto typeInfo = ReflectionRegistry::Instance().GetType(typeName);
    if (!typeInfo) return -1;
    
    auto field = typeInfo->GetField(fieldName);
    return field ? static_cast<int>(field->GetType()) : -1;
}

extern "C" ENGINE_API int Reflection_GetFieldOffset(const char* typeName, const char* fieldName) {
    if (!typeName || !fieldName) return -1;
    
    auto typeInfo = ReflectionRegistry::Instance().GetType(typeName);
    if (!typeInfo) return -1;
    
    auto field = typeInfo->GetField(fieldName);
    return field ? static_cast<int>(field->GetOffset()) : -1;
}

// ===== Value Access Functions =====

extern "C" ENGINE_API float Reflection_GetFloatValue(const char* typeName, const char* fieldName, void* instance) {
    if (!typeName || !fieldName || !instance) return 0.0f;
    
    auto typeInfo = ReflectionRegistry::Instance().GetType(typeName);
    if (!typeInfo) return 0.0f;
    
    auto field = typeInfo->GetField(fieldName);
    if (!field || field->GetType() != PropertyType::Float) return 0.0f;
    
    return field->GetValue<float>(instance);
}

extern "C" ENGINE_API void Reflection_SetFloatValue(const char* typeName, const char* fieldName, 
                                                    void* instance, float value) {
    if (!typeName || !fieldName || !instance) return;
    
    auto typeInfo = ReflectionRegistry::Instance().GetType(typeName);
    if (!typeInfo) return;
    
    auto field = typeInfo->GetField(fieldName);
    if (!field || field->GetType() != PropertyType::Float) return;
    
    field->SetValue<float>(instance, value);
}

extern "C" ENGINE_API int Reflection_GetIntValue(const char* typeName, const char* fieldName, void* instance) {
    if (!typeName || !fieldName || !instance) return 0;
    
    auto typeInfo = ReflectionRegistry::Instance().GetType(typeName);
    if (!typeInfo) return 0;
    
    auto field = typeInfo->GetField(fieldName);
    if (!field || field->GetType() != PropertyType::Int) return 0;
    
    return field->GetValue<int>(instance);
}

extern "C" ENGINE_API void Reflection_SetIntValue(const char* typeName, const char* fieldName, 
                                                  void* instance, int value) {
    if (!typeName || !fieldName || !instance) return;
    
    auto typeInfo = ReflectionRegistry::Instance().GetType(typeName);
    if (!typeInfo) return;
    
    auto field = typeInfo->GetField(fieldName);
    if (!field || field->GetType() != PropertyType::Int) return;
    
    field->SetValue<int>(instance, value);
}

extern "C" ENGINE_API bool Reflection_GetBoolValue(const char* typeName, const char* fieldName, void* instance) {
    if (!typeName || !fieldName || !instance) return false;
    
    auto typeInfo = ReflectionRegistry::Instance().GetType(typeName);
    if (!typeInfo) return false;
    
    auto field = typeInfo->GetField(fieldName);
    if (!field || field->GetType() != PropertyType::Bool) return false;
    
    return field->GetValue<bool>(instance);
}

extern "C" ENGINE_API void Reflection_SetBoolValue(const char* typeName, const char* fieldName, 
                                                   void* instance, bool value) {
    if (!typeName || !fieldName || !instance) return;
    
    auto typeInfo = ReflectionRegistry::Instance().GetType(typeName);
    if (!typeInfo) return;
    
    auto field = typeInfo->GetField(fieldName);
    if (!field || field->GetType() != PropertyType::Bool) return;
    
    field->SetValue<bool>(instance, value);
}

extern "C" ENGINE_API void Reflection_GetStringValue(const char* typeName, const char* fieldName, 
                                                     void* instance, char* buffer, int bufferSize) {
    if (!typeName || !fieldName || !instance || !buffer || bufferSize <= 0) return;
    
    auto typeInfo = ReflectionRegistry::Instance().GetType(typeName);
    if (!typeInfo) {
        buffer[0] = '\0';
        return;
    }
    
    auto field = typeInfo->GetField(fieldName);
    if (!field || field->GetType() != PropertyType::String) {
        buffer[0] = '\0';
        return;
    }
    
    const auto& str = field->GetValue<std::string>(instance);
    std::strncpy(buffer, str.c_str(), bufferSize - 1);
    buffer[bufferSize - 1] = '\0';
}

extern "C" ENGINE_API void Reflection_SetStringValue(const char* typeName, const char* fieldName, 
                                                     void* instance, const char* value) {
    if (!typeName || !fieldName || !instance || !value) return;
    
    auto typeInfo = ReflectionRegistry::Instance().GetType(typeName);
    if (!typeInfo) return;
    
    auto field = typeInfo->GetField(fieldName);
    if (!field || field->GetType() != PropertyType::String) return;
    
    field->SetValue<std::string>(instance, std::string(value));
}
