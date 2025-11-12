#pragma once

#include "Reflection.h"

#ifdef _WIN32
    #ifdef ENGINE_EXPORTS
        #define ENGINE_API __declspec(dllexport)
    #else
        #define ENGINE_API __declspec(dllimport)
    #endif
#else
    #define ENGINE_API
#endif

// Chronicles of a Drifter - Reflection API for C# and Python
// C-compatible API for accessing reflection data

extern "C" {
    // ===== Type Query Functions =====
    
    /// <summary>
    /// Get the number of registered types
    /// </summary>
    ENGINE_API int Reflection_GetTypeCount();
    
    /// <summary>
    /// Get type name by index
    /// </summary>
    /// <param name="index">Type index (0 to count-1)</param>
    /// <param name="buffer">Output buffer for type name</param>
    /// <param name="bufferSize">Size of output buffer</param>
    ENGINE_API void Reflection_GetTypeName(int index, char* buffer, int bufferSize);
    
    /// <summary>
    /// Get type size in bytes
    /// </summary>
    ENGINE_API int Reflection_GetTypeSize(const char* typeName);
    
    /// <summary>
    /// Get number of fields in a type
    /// </summary>
    ENGINE_API int Reflection_GetFieldCount(const char* typeName);
    
    // ===== Field Query Functions =====
    
    /// <summary>
    /// Get field name by index
    /// </summary>
    ENGINE_API void Reflection_GetFieldName(const char* typeName, int fieldIndex, 
                                            char* buffer, int bufferSize);
    
    /// <summary>
    /// Get field type
    /// </summary>
    /// <returns>PropertyType enum value</returns>
    ENGINE_API int Reflection_GetFieldType(const char* typeName, const char* fieldName);
    
    /// <summary>
    /// Get field offset in bytes
    /// </summary>
    ENGINE_API int Reflection_GetFieldOffset(const char* typeName, const char* fieldName);
    
    // ===== Value Access Functions =====
    
    /// <summary>
    /// Get float field value from instance
    /// </summary>
    ENGINE_API float Reflection_GetFloatValue(const char* typeName, const char* fieldName, void* instance);
    
    /// <summary>
    /// Set float field value on instance
    /// </summary>
    ENGINE_API void Reflection_SetFloatValue(const char* typeName, const char* fieldName, 
                                             void* instance, float value);
    
    /// <summary>
    /// Get int field value from instance
    /// </summary>
    ENGINE_API int Reflection_GetIntValue(const char* typeName, const char* fieldName, void* instance);
    
    /// <summary>
    /// Set int field value on instance
    /// </summary>
    ENGINE_API void Reflection_SetIntValue(const char* typeName, const char* fieldName, 
                                           void* instance, int value);
    
    /// <summary>
    /// Get bool field value from instance
    /// </summary>
    ENGINE_API bool Reflection_GetBoolValue(const char* typeName, const char* fieldName, void* instance);
    
    /// <summary>
    /// Set bool field value on instance
    /// </summary>
    ENGINE_API void Reflection_SetBoolValue(const char* typeName, const char* fieldName, 
                                            void* instance, bool value);
    
    /// <summary>
    /// Get string field value from instance
    /// </summary>
    ENGINE_API void Reflection_GetStringValue(const char* typeName, const char* fieldName, 
                                              void* instance, char* buffer, int bufferSize);
    
    /// <summary>
    /// Set string field value on instance
    /// </summary>
    ENGINE_API void Reflection_SetStringValue(const char* typeName, const char* fieldName, 
                                              void* instance, const char* value);
}
