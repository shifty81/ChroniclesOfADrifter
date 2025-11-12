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

// Chronicles of a Drifter - Python Integration API
// C-compatible API for Python bindings (using ctypes or cffi)

extern "C" {
    // ===== Python Interpreter =====
    
    /// <summary>
    /// Initialize Python interpreter
    /// </summary>
    /// <returns>true if successful</returns>
    ENGINE_API bool Python_Initialize();
    
    /// <summary>
    /// Shutdown Python interpreter
    /// </summary>
    ENGINE_API void Python_Shutdown();
    
    /// <summary>
    /// Check if Python is initialized
    /// </summary>
    ENGINE_API bool Python_IsInitialized();
    
    // ===== Script Execution =====
    
    /// <summary>
    /// Execute Python script string
    /// </summary>
    /// <param name="script">Python code to execute</param>
    /// <returns>true if execution succeeded</returns>
    ENGINE_API bool Python_ExecuteString(const char* script);
    
    /// <summary>
    /// Execute Python script file
    /// </summary>
    /// <param name="filePath">Path to .py file</param>
    /// <returns>true if execution succeeded</returns>
    ENGINE_API bool Python_ExecuteFile(const char* filePath);
    
    /// <summary>
    /// Call Python function
    /// </summary>
    /// <param name="moduleName">Module containing the function</param>
    /// <param name="functionName">Function to call</param>
    /// <param name="args">JSON-encoded arguments</param>
    /// <param name="resultBuffer">Buffer for JSON-encoded result</param>
    /// <param name="bufferSize">Size of result buffer</param>
    /// <returns>true if call succeeded</returns>
    ENGINE_API bool Python_CallFunction(const char* moduleName, const char* functionName,
                                       const char* args, char* resultBuffer, int bufferSize);
    
    // ===== Error Handling =====
    
    /// <summary>
    /// Get last Python error message
    /// </summary>
    ENGINE_API void Python_GetLastError(char* buffer, int bufferSize);
    
    /// <summary>
    /// Clear Python error state
    /// </summary>
    ENGINE_API void Python_ClearError();
}
