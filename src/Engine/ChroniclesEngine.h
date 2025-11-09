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

// Chronicles of a Drifter - Native Engine Interface
// This header defines the C API for interop with C# game logic

extern "C" {
    // ===== Engine Initialization =====
    
    /// <summary>
    /// Initialize the game engine with specified window parameters
    /// </summary>
    /// <param name="width">Window width in pixels</param>
    /// <param name="height">Window height in pixels</param>
    /// <param name="title">Window title (UTF-8 encoded)</param>
    /// <returns>true if initialization succeeded, false otherwise</returns>
    ENGINE_API bool Engine_Initialize(int width, int height, const char* title);
    
    /// <summary>
    /// Shutdown the engine and release all resources
    /// </summary>
    ENGINE_API void Engine_Shutdown();
    
    /// <summary>
    /// Check if the engine is still running
    /// </summary>
    /// <returns>true if engine should continue running</returns>
    ENGINE_API bool Engine_IsRunning();
    
    // ===== Game Loop =====
    
    /// <summary>
    /// Begin a new frame - processes input, updates timing
    /// </summary>
    ENGINE_API void Engine_BeginFrame();
    
    /// <summary>
    /// End the current frame - presents rendered content
    /// </summary>
    ENGINE_API void Engine_EndFrame();
    
    /// <summary>
    /// Get time elapsed since last frame in seconds
    /// </summary>
    ENGINE_API float Engine_GetDeltaTime();
    
    /// <summary>
    /// Get total elapsed time since engine start in seconds
    /// </summary>
    ENGINE_API float Engine_GetTotalTime();
    
    // ===== Rendering =====
    
    /// <summary>
    /// Load a texture from file
    /// </summary>
    /// <param name="filePath">Path to image file</param>
    /// <returns>Texture ID (>= 0) or -1 on failure</returns>
    ENGINE_API int Renderer_LoadTexture(const char* filePath);
    
    /// <summary>
    /// Unload a previously loaded texture
    /// </summary>
    ENGINE_API void Renderer_UnloadTexture(int textureId);
    
    /// <summary>
    /// Draw a sprite with specified transform
    /// </summary>
    ENGINE_API void Renderer_DrawSprite(int textureId, float x, float y, 
                                       float width, float height, float rotation);
    
    /// <summary>
    /// Clear the screen with specified color
    /// </summary>
    ENGINE_API void Renderer_Clear(float r, float g, float b, float a);
    
    /// <summary>
    /// Draw a filled rectangle
    /// </summary>
    ENGINE_API void Renderer_DrawRect(float x, float y, float width, float height,
                                     float r, float g, float b, float a);
    
    /// <summary>
    /// Present the rendered frame to the screen
    /// </summary>
    ENGINE_API void Renderer_Present();
    
    // ===== Input =====
    
    /// <summary>
    /// Check if a key was pressed this frame
    /// </summary>
    ENGINE_API bool Input_IsKeyPressed(int keyCode);
    
    /// <summary>
    /// Check if a key is currently held down
    /// </summary>
    ENGINE_API bool Input_IsKeyDown(int keyCode);
    
    /// <summary>
    /// Check if a key was released this frame
    /// </summary>
    ENGINE_API bool Input_IsKeyReleased(int keyCode);
    
    /// <summary>
    /// Get current mouse position
    /// </summary>
    ENGINE_API void Input_GetMousePosition(float* outX, float* outY);
    
    /// <summary>
    /// Check if mouse button is pressed
    /// </summary>
    ENGINE_API bool Input_IsMouseButtonPressed(int button);
    
    // ===== Audio =====
    
    /// <summary>
    /// Load a sound effect from file
    /// </summary>
    ENGINE_API int Audio_LoadSound(const char* filePath);
    
    /// <summary>
    /// Play a loaded sound effect
    /// </summary>
    ENGINE_API void Audio_PlaySound(int soundId, float volume);
    
    /// <summary>
    /// Play background music
    /// </summary>
    ENGINE_API void Audio_PlayMusic(const char* filePath, float volume, bool loop);
    
    /// <summary>
    /// Stop currently playing music
    /// </summary>
    ENGINE_API void Audio_StopMusic();
    
    // ===== Physics =====
    
    /// <summary>
    /// Set global gravity vector
    /// </summary>
    ENGINE_API void Physics_SetGravity(float x, float y);
    
    /// <summary>
    /// Check collision between two axis-aligned bounding boxes
    /// </summary>
    ENGINE_API bool Physics_CheckCollision(float x1, float y1, float w1, float h1,
                                           float x2, float y2, float w2, float h2);
    
    // ===== Callbacks =====
    
    /// <summary>
    /// Input callback function type
    /// </summary>
    typedef void (*InputCallbackFn)(int keyCode, bool isPressed);
    
    /// <summary>
    /// Collision callback function type
    /// </summary>
    typedef void (*CollisionCallbackFn)(int entity1, int entity2);
    
    /// <summary>
    /// Register callback for input events
    /// </summary>
    ENGINE_API void Engine_RegisterInputCallback(InputCallbackFn callback);
    
    /// <summary>
    /// Register callback for collision events
    /// </summary>
    ENGINE_API void Engine_RegisterCollisionCallback(CollisionCallbackFn callback);
    
    // ===== Internal Input Functions (called by renderers) =====
    
    /// <summary>
    /// Internal: Set key state (called by renderer backends)
    /// </summary>
    ENGINE_API void Engine_SetKeyState(int keyCode, bool isDown, bool isPressed);
    
    /// <summary>
    /// Internal: Set mouse position (called by renderer backends)
    /// </summary>
    ENGINE_API void Engine_SetMousePosition(float x, float y);
    
    /// <summary>
    /// Internal: Set mouse button state (called by renderer backends)
    /// </summary>
    ENGINE_API void Engine_SetMouseButtonState(int button, bool isDown);
    
    // ===== Error Handling =====
    
    /// <summary>
    /// Get last error code
    /// </summary>
    ENGINE_API int Engine_GetLastError();
    
    /// <summary>
    /// Get last error message
    /// </summary>
    ENGINE_API const char* Engine_GetErrorMessage();
}
