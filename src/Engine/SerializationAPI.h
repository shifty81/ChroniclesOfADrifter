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

// Chronicles of a Drifter - Serialization API for C# and Python
// C-compatible API for serializing/deserializing objects

extern "C" {
    // ===== Serialization Functions =====
    
    /// <summary>
    /// Serialize an object to JSON string
    /// </summary>
    /// <param name="typeName">Name of the type to serialize</param>
    /// <param name="instance">Pointer to object instance</param>
    /// <param name="buffer">Output buffer for JSON string</param>
    /// <param name="bufferSize">Size of output buffer</param>
    /// <returns>Length of JSON string (excluding null terminator), or -1 on error</returns>
    ENGINE_API int Serialization_ToJson(const char* typeName, void* instance, 
                                        char* buffer, int bufferSize);
    
    /// <summary>
    /// Deserialize an object from JSON string
    /// </summary>
    /// <param name="typeName">Name of the type to deserialize</param>
    /// <param name="instance">Pointer to object instance to populate</param>
    /// <param name="json">JSON string to deserialize from</param>
    /// <returns>true if successful, false otherwise</returns>
    ENGINE_API bool Serialization_FromJson(const char* typeName, void* instance, 
                                           const char* json);
    
    /// <summary>
    /// Save object to JSON file
    /// </summary>
    ENGINE_API bool Serialization_SaveToFile(const char* typeName, void* instance, 
                                             const char* filePath);
    
    /// <summary>
    /// Load object from JSON file
    /// </summary>
    ENGINE_API bool Serialization_LoadFromFile(const char* typeName, void* instance,
                                               const char* filePath);
}
