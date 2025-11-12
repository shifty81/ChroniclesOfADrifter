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

// Chronicles of a Drifter - Enhanced Lua API
// Extended API for exposing more engine functionality to Lua

extern "C" {
    // ===== Lua Enhanced API =====
    
    /// <summary>
    /// Register reflection system with Lua
    /// Allows Lua scripts to query types and access properties
    /// </summary>
    ENGINE_API void Lua_RegisterReflectionAPI();
    
    /// <summary>
    /// Register serialization system with Lua
    /// Allows Lua scripts to serialize/deserialize objects
    /// </summary>
    ENGINE_API void Lua_RegisterSerializationAPI();
    
    /// <summary>
    /// Hot-reload a Lua script
    /// Reloads the script without restarting the application
    /// </summary>
    /// <param name="scriptPath">Path to script file to reload</param>
    /// <returns>true if reload succeeded</returns>
    ENGINE_API bool Lua_HotReloadScript(const char* scriptPath);
    
    /// <summary>
    /// Enable Lua debugging
    /// Sets up debugging hooks and logging
    /// </summary>
    ENGINE_API void Lua_EnableDebugging(bool enable);
    
    /// <summary>
    /// Get Lua stack trace
    /// Useful for debugging Lua errors
    /// </summary>
    ENGINE_API void Lua_GetStackTrace(char* buffer, int bufferSize);
    
    /// <summary>
    /// Call Lua function with reflection support
    /// Automatically marshals parameters based on reflection
    /// </summary>
    ENGINE_API bool Lua_CallFunctionWithReflection(const char* functionName,
                                                   const char* paramTypes,
                                                   void** params,
                                                   int paramCount,
                                                   void* result);
}
