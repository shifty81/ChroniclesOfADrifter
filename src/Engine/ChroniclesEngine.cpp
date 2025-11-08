#include "ChroniclesEngine.h"
#include <cstdio>
#include <cstring>
#include <SDL2/SDL.h>
#include <map>
#include <chrono>

// Chronicles of a Drifter - Native Engine Implementation with SDL2

namespace {
    // Engine state
    bool g_isInitialized = false;
    bool g_isRunning = false;
    float g_deltaTime = 0.016f; // ~60 FPS
    float g_totalTime = 0.0f;
    
    // SDL state
    SDL_Window* g_window = nullptr;
    SDL_Renderer* g_renderer = nullptr;
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
    
    // Textures
    std::map<int, SDL_Texture*> g_textures;
    int g_nextTextureId = 1;
    
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
}

// ===== Engine Initialization =====

extern "C" ENGINE_API bool Engine_Initialize(int width, int height, const char* title) {
    if (g_isInitialized) {
        return true;
    }
    
    printf("[Engine] Initializing Chronicles Engine with SDL2\n");
    printf("[Engine] Window: %dx%d - %s\n", width, height, title);
    
    // Initialize SDL
    if (SDL_Init(SDL_INIT_VIDEO | SDL_INIT_EVENTS) < 0) {
        SetError(SDL_GetError());
        return false;
    }
    
    // Create window
    g_window = SDL_CreateWindow(
        title,
        SDL_WINDOWPOS_CENTERED,
        SDL_WINDOWPOS_CENTERED,
        width,
        height,
        SDL_WINDOW_SHOWN
    );
    
    if (!g_window) {
        SetError(SDL_GetError());
        SDL_Quit();
        return false;
    }
    
    // Create renderer
    g_renderer = SDL_CreateRenderer(
        g_window,
        -1,
        SDL_RENDERER_ACCELERATED | SDL_RENDERER_PRESENTVSYNC
    );
    
    if (!g_renderer) {
        SetError(SDL_GetError());
        SDL_DestroyWindow(g_window);
        SDL_Quit();
        return false;
    }
    
    // Enable alpha blending
    SDL_SetRenderDrawBlendMode(g_renderer, SDL_BLENDMODE_BLEND);
    
    g_windowWidth = width;
    g_windowHeight = height;
    g_isInitialized = true;
    g_isRunning = true;
    
    // Initialize timing
    g_lastFrameTime = std::chrono::high_resolution_clock::now();
    
    printf("[Engine] SDL2 initialization complete\n");
    return true;
}

extern "C" ENGINE_API void Engine_Shutdown() {
    if (!g_isInitialized) {
        return;
    }
    
    printf("[Engine] Shutting down\n");
    
    // Clean up textures
    for (auto& pair : g_textures) {
        if (pair.second) {
            SDL_DestroyTexture(pair.second);
        }
    }
    g_textures.clear();
    
    // Clean up SDL
    if (g_renderer) {
        SDL_DestroyRenderer(g_renderer);
        g_renderer = nullptr;
    }
    
    if (g_window) {
        SDL_DestroyWindow(g_window);
        g_window = nullptr;
    }
    
    SDL_Quit();
    
    g_isInitialized = false;
    g_isRunning = false;
    
    printf("[Engine] Shutdown complete\n");
}

extern "C" ENGINE_API bool Engine_IsRunning() {
    return g_isRunning;
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
    
    // Process SDL events
    SDL_Event event;
    while (SDL_PollEvent(&event)) {
        switch (event.type) {
            case SDL_QUIT:
                g_isRunning = false;
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
}

extern "C" ENGINE_API void Engine_EndFrame() {
    // Present the rendered frame
    SDL_RenderPresent(g_renderer);
}

extern "C" ENGINE_API float Engine_GetDeltaTime() {
    return g_deltaTime;
}

extern "C" ENGINE_API float Engine_GetTotalTime() {
    return g_totalTime;
}

// ===== Rendering =====

extern "C" ENGINE_API int Renderer_LoadTexture(const char* filePath) {
    printf("[Renderer] Loading texture: %s\n", filePath);
    
    SDL_Surface* surface = SDL_LoadBMP(filePath);
    if (!surface) {
        SetError(SDL_GetError());
        return -1;
    }
    
    SDL_Texture* texture = SDL_CreateTextureFromSurface(g_renderer, surface);
    SDL_FreeSurface(surface);
    
    if (!texture) {
        SetError(SDL_GetError());
        return -1;
    }
    
    int textureId = g_nextTextureId++;
    g_textures[textureId] = texture;
    
    return textureId;
}

extern "C" ENGINE_API void Renderer_UnloadTexture(int textureId) {
    auto it = g_textures.find(textureId);
    if (it != g_textures.end()) {
        SDL_DestroyTexture(it->second);
        g_textures.erase(it);
        printf("[Renderer] Unloaded texture: %d\n", textureId);
    }
}

extern "C" ENGINE_API void Renderer_DrawSprite(int textureId, float x, float y,
                                               float width, float height, float rotation) {
    auto it = g_textures.find(textureId);
    if (it == g_textures.end()) {
        return;
    }
    
    SDL_Rect destRect = {
        static_cast<int>(x),
        static_cast<int>(y),
        static_cast<int>(width),
        static_cast<int>(height)
    };
    
    SDL_Point center = {
        static_cast<int>(width / 2),
        static_cast<int>(height / 2)
    };
    
    double angle = rotation * (180.0 / 3.14159265359); // Convert radians to degrees
    
    SDL_RenderCopyEx(g_renderer, it->second, nullptr, &destRect, angle, &center, SDL_FLIP_NONE);
}

extern "C" ENGINE_API void Renderer_Clear(float r, float g, float b, float a) {
    SDL_SetRenderDrawColor(g_renderer,
                          static_cast<Uint8>(r * 255),
                          static_cast<Uint8>(g * 255),
                          static_cast<Uint8>(b * 255),
                          static_cast<Uint8>(a * 255));
    SDL_RenderClear(g_renderer);
}

extern "C" ENGINE_API void Renderer_DrawRect(float x, float y, float width, float height,
                                             float r, float g, float b, float a) {
    SDL_SetRenderDrawColor(g_renderer,
                          static_cast<Uint8>(r * 255),
                          static_cast<Uint8>(g * 255),
                          static_cast<Uint8>(b * 255),
                          static_cast<Uint8>(a * 255));
    
    SDL_Rect rect = {
        static_cast<int>(x),
        static_cast<int>(y),
        static_cast<int>(width),
        static_cast<int>(height)
    };
    
    SDL_RenderFillRect(g_renderer, &rect);
}

extern "C" ENGINE_API void Renderer_Present() {
    SDL_RenderPresent(g_renderer);
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
    Uint32 mouseState = SDL_GetMouseState(nullptr, nullptr);
    return (mouseState & SDL_BUTTON(button)) != 0;
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
