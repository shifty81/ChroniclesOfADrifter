#include "ChroniclesEngine.h"
#ifdef HAS_SDL2
#include "SDL2Renderer.h"
#endif
#ifdef _WIN32
#include "D3D11Renderer.h"
#include "D3D12Renderer.h"
#endif
#include <cstdio>
#include <cstring>
#ifdef HAS_SDL2
#include <SDL2/SDL.h>
#endif
#include <map>
#include <chrono>
#include <memory>

// Chronicles of a Drifter - Native Engine Implementation with Multiple Renderer Backends

namespace {
    // Engine state
    bool g_isInitialized = false;
    bool g_isRunning = false;
    float g_deltaTime = 0.016f; // ~60 FPS
    float g_totalTime = 0.0f;
    
    // Renderer backend
    std::unique_ptr<Chronicles::IRenderer> g_renderer;
    int g_windowWidth = 0;
    int g_windowHeight = 0;
    
    // Timing
    std::chrono::high_resolution_clock::time_point g_lastFrameTime;
    
    // Input state
    std::map<int, bool> g_keyStates;
    std::map<int, bool> g_keyPressed;
    std::map<int, bool> g_keyReleased;
    float g_mouseX = 0.0f;
    float g_mouseY = 0.0f;
    
    // Callbacks
    InputCallbackFn g_inputCallback = nullptr;
    CollisionCallbackFn g_collisionCallback = nullptr;
    
    // Error handling
    int g_lastError = 0;
    char g_errorMessage[256] = "No error";
    
    void SetError(const char* message) {
        strncpy(g_errorMessage, message, sizeof(g_errorMessage) - 1);
        g_errorMessage[sizeof(g_errorMessage) - 1] = '\0';
        printf("[Engine] ERROR: %s\n", message);
    }
    
    // Environment variable to select renderer backend
    // Set CHRONICLES_RENDERER=dx11 for DirectX 11 (Windows only)
    // Set CHRONICLES_RENDERER=dx12 for DirectX 12 (Windows only)
    // Set CHRONICLES_RENDERER=sdl2 for SDL2 (cross-platform, if available)
    Chronicles::RendererBackend GetRendererBackend() {
        const char* rendererEnv = std::getenv("CHRONICLES_RENDERER");
        if (rendererEnv) {
            std::string backend(rendererEnv);
            if (backend == "dx11" || backend == "directx11" || backend == "d3d11") {
#ifdef _WIN32
                return Chronicles::RendererBackend::DirectX11;
#else
                printf("[Engine] WARNING: DirectX 11 not available on this platform\n");
#ifdef HAS_SDL2
                printf("[Engine] Using SDL2 as fallback\n");
                return Chronicles::RendererBackend::SDL2;
#else
                printf("[Engine] ERROR: No renderer backend available\n");
                return Chronicles::RendererBackend::SDL2; // Will fail gracefully
#endif
#endif
            }
            if (backend == "dx12" || backend == "directx12" || backend == "d3d12") {
#ifdef _WIN32
                return Chronicles::RendererBackend::DirectX12;
#else
                printf("[Engine] WARNING: DirectX 12 not available on this platform\n");
#ifdef HAS_SDL2
                printf("[Engine] Using SDL2 as fallback\n");
                return Chronicles::RendererBackend::SDL2;
#else
                printf("[Engine] ERROR: No renderer backend available\n");
                return Chronicles::RendererBackend::SDL2; // Will fail gracefully
#endif
#endif
            }
        }
        
        // Default to SDL2 if available, otherwise DirectX 11 on Windows
#ifdef HAS_SDL2
        return Chronicles::RendererBackend::SDL2;
#elif defined(_WIN32)
        printf("[Engine] SDL2 not available, using DirectX 11 as default\n");
        return Chronicles::RendererBackend::DirectX11;
#else
        printf("[Engine] ERROR: No renderer backend available\n");
        return Chronicles::RendererBackend::SDL2; // Will fail gracefully
#endif
    }
}

// ===== Engine Initialization =====

extern "C" ENGINE_API bool Engine_Initialize(int width, int height, const char* title) {
    if (g_isInitialized) {
        return true;
    }
    
    printf("[Engine] Initializing Chronicles Engine\n");
    printf("[Engine] Window: %dx%d - %s\n", width, height, title);
    
    // Determine renderer backend
    Chronicles::RendererBackend backend = GetRendererBackend();
    
    // Create renderer based on backend
    try {
        switch (backend) {
            case Chronicles::RendererBackend::DirectX11:
#ifdef _WIN32
                printf("[Engine] Using DirectX 11 renderer backend\n");
                g_renderer = std::make_unique<Chronicles::D3D11Renderer>();
#else
                SetError("DirectX 11 not available on this platform");
                return false;
#endif
                break;
            
            case Chronicles::RendererBackend::DirectX12:
#ifdef _WIN32
                printf("[Engine] Using DirectX 12 renderer backend\n");
                g_renderer = std::make_unique<Chronicles::D3D12Renderer>();
#else
                SetError("DirectX 12 not available on this platform");
                return false;
#endif
                break;
            
            case Chronicles::RendererBackend::SDL2:
            default:
#ifdef HAS_SDL2
                printf("[Engine] Using SDL2 renderer backend\n");
                g_renderer = std::make_unique<Chronicles::SDL2Renderer>();
#else
                SetError("SDL2 not available. Install SDL2 development libraries or use DirectX on Windows.");
                return false;
#endif
                break;
        }
    }
    catch (const std::exception& e) {
        SetError(e.what());
        return false;
    }
    
    // Initialize the renderer
    if (!g_renderer->Initialize(width, height, title)) {
        SetError("Renderer initialization failed");
        g_renderer.reset();
        return false;
    }
    
    g_windowWidth = width;
    g_windowHeight = height;
    g_isInitialized = true;
    g_isRunning = true;
    
    // Initialize timing
    g_lastFrameTime = std::chrono::high_resolution_clock::now();
    
    // Initialize SDL for input (even if using DirectX for rendering)
#ifdef HAS_SDL2
    if (backend == Chronicles::RendererBackend::DirectX11 || backend == Chronicles::RendererBackend::DirectX12) {
        if (SDL_Init(SDL_INIT_EVENTS) < 0) {
            printf("[Engine] WARNING: SDL input initialization failed: %s\n", SDL_GetError());
        }
    }
#endif
    
    printf("[Engine] Initialization complete\n");
    return true;
}

extern "C" ENGINE_API void Engine_Shutdown() {
    if (!g_isInitialized) {
        return;
    }
    
    printf("[Engine] Shutting down\n");
    
    // Shutdown renderer
    if (g_renderer) {
        g_renderer->Shutdown();
        g_renderer.reset();
    }
    
    // Quit SDL if it was initialized
#ifdef HAS_SDL2
    SDL_Quit();
#endif
    
    g_isInitialized = false;
    g_isRunning = false;
    
    printf("[Engine] Shutdown complete\n");
}

extern "C" ENGINE_API bool Engine_IsRunning() {
    return g_isRunning && g_renderer && g_renderer->IsRunning();
}

// ===== Game Loop =====

extern "C" ENGINE_API void Engine_BeginFrame() {
    // Calculate delta time
    auto currentTime = std::chrono::high_resolution_clock::now();
    std::chrono::duration<float> elapsed = currentTime - g_lastFrameTime;
    g_deltaTime = elapsed.count();
    g_lastFrameTime = currentTime;
    g_totalTime += g_deltaTime;
    
    // Clear previous frame input states
    g_keyPressed.clear();
    g_keyReleased.clear();
    
#ifdef HAS_SDL2
    // Process SDL events (for input and window management)
    SDL_Event event;
    while (SDL_PollEvent(&event)) {
        switch (event.type) {
            case SDL_QUIT:
                g_isRunning = false;
                if (g_renderer) {
                    g_renderer->SetRunning(false);
                }
                break;
                
            case SDL_KEYDOWN:
                if (!event.key.repeat) {
                    g_keyStates[event.key.keysym.sym] = true;
                    g_keyPressed[event.key.keysym.sym] = true;
                    if (g_inputCallback) {
                        g_inputCallback(event.key.keysym.sym, true);
                    }
                }
                break;
                
            case SDL_KEYUP:
                g_keyStates[event.key.keysym.sym] = false;
                g_keyReleased[event.key.keysym.sym] = true;
                if (g_inputCallback) {
                    g_inputCallback(event.key.keysym.sym, false);
                }
                break;
                
            case SDL_MOUSEMOTION:
                g_mouseX = static_cast<float>(event.motion.x);
                g_mouseY = static_cast<float>(event.motion.y);
                break;
        }
    }
#endif
    
    // Begin renderer frame
    if (g_renderer) {
        g_renderer->BeginFrame();
    }
}

extern "C" ENGINE_API void Engine_EndFrame() {
    // End renderer frame
    if (g_renderer) {
        g_renderer->EndFrame();
    }
}

extern "C" ENGINE_API float Engine_GetDeltaTime() {
    return g_deltaTime;
}

extern "C" ENGINE_API float Engine_GetTotalTime() {
    return g_totalTime;
}

// ===== Rendering =====

extern "C" ENGINE_API int Renderer_LoadTexture(const char* filePath) {
    if (!g_renderer) return -1;
    return g_renderer->LoadTexture(filePath);
}

extern "C" ENGINE_API void Renderer_UnloadTexture(int textureId) {
    if (!g_renderer) return;
    g_renderer->UnloadTexture(textureId);
}

extern "C" ENGINE_API void Renderer_DrawSprite(int textureId, float x, float y,
                                               float width, float height, float rotation) {
    if (!g_renderer) return;
    g_renderer->DrawSprite(textureId, x, y, width, height, rotation);
}

extern "C" ENGINE_API void Renderer_Clear(float r, float g, float b, float a) {
    if (!g_renderer) return;
    g_renderer->Clear(r, g, b, a);
}

extern "C" ENGINE_API void Renderer_DrawRect(float x, float y, float width, float height,
                                             float r, float g, float b, float a) {
    if (!g_renderer) return;
    g_renderer->DrawRect(x, y, width, height, r, g, b, a);
}

extern "C" ENGINE_API void Renderer_Present() {
    if (!g_renderer) return;
    g_renderer->Present();
}

// ===== Input =====

extern "C" ENGINE_API bool Input_IsKeyPressed(int keyCode) {
    return g_keyPressed.find(keyCode) != g_keyPressed.end();
}

extern "C" ENGINE_API bool Input_IsKeyDown(int keyCode) {
    auto it = g_keyStates.find(keyCode);
    return it != g_keyStates.end() && it->second;
}

extern "C" ENGINE_API bool Input_IsKeyReleased(int keyCode) {
    return g_keyReleased.find(keyCode) != g_keyReleased.end();
}

extern "C" ENGINE_API void Input_GetMousePosition(float* outX, float* outY) {
    if (outX) *outX = g_mouseX;
    if (outY) *outY = g_mouseY;
}

extern "C" ENGINE_API bool Input_IsMouseButtonPressed(int button) {
#ifdef HAS_SDL2
    Uint32 mouseState = SDL_GetMouseState(nullptr, nullptr);
    return (mouseState & SDL_BUTTON(button)) != 0;
#else
    (void)button;
    return false; // Not supported without SDL2
#endif
}

// ===== Audio =====

extern "C" ENGINE_API int Audio_LoadSound(const char* filePath) {
    printf("[Audio] Loading sound: %s\n", filePath);
    // TODO: Load sound file
    return 0; // Stub: return sound ID
}

extern "C" ENGINE_API void Audio_PlaySound(int soundId, float volume) {
    // TODO: Play sound effect
}

extern "C" ENGINE_API void Audio_PlayMusic(const char* filePath, float volume, bool loop) {
    printf("[Audio] Playing music: %s (volume: %.2f, loop: %d)\n", filePath, volume, loop);
    // TODO: Play background music
}

extern "C" ENGINE_API void Audio_StopMusic() {
    // TODO: Stop music
}

// ===== Physics =====

extern "C" ENGINE_API void Physics_SetGravity(float x, float y) {
    // TODO: Set physics gravity
}

extern "C" ENGINE_API bool Physics_CheckCollision(float x1, float y1, float w1, float h1,
                                                  float x2, float y2, float w2, float h2) {
    // Basic AABB collision check
    return (x1 < x2 + w2 &&
            x1 + w1 > x2 &&
            y1 < y2 + h2 &&
            y1 + h1 > y2);
}

// ===== Callbacks =====

extern "C" ENGINE_API void Engine_RegisterInputCallback(InputCallbackFn callback) {
    g_inputCallback = callback;
    printf("[Engine] Input callback registered\n");
}

extern "C" ENGINE_API void Engine_RegisterCollisionCallback(CollisionCallbackFn callback) {
    g_collisionCallback = callback;
    printf("[Engine] Collision callback registered\n");
}

// ===== Error Handling =====

extern "C" ENGINE_API int Engine_GetLastError() {
    return g_lastError;
}

extern "C" ENGINE_API const char* Engine_GetErrorMessage() {
    return g_errorMessage;
}
