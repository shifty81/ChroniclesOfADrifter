#include "SDL2Renderer.h"
#include <cstdio>

namespace Chronicles {

SDL2Renderer::SDL2Renderer()
    : m_window(nullptr)
    , m_renderer(nullptr)
    , m_windowWidth(0)
    , m_windowHeight(0)
    , m_isRunning(false)
    , m_nextTextureId(1)
{
}

SDL2Renderer::~SDL2Renderer() {
    Shutdown();
}

bool SDL2Renderer::Initialize(int width, int height, const char* title) {
    printf("[SDL2Renderer] Initializing SDL2 renderer\n");
    printf("[SDL2Renderer] Window: %dx%d - %s\n", width, height, title);
    
    // Initialize SDL
    if (SDL_Init(SDL_INIT_VIDEO | SDL_INIT_EVENTS) < 0) {
        printf("[SDL2Renderer] ERROR: SDL_Init failed: %s\n", SDL_GetError());
        return false;
    }
    
    // Create window
    m_window = SDL_CreateWindow(
        title,
        SDL_WINDOWPOS_CENTERED,
        SDL_WINDOWPOS_CENTERED,
        width,
        height,
        SDL_WINDOW_SHOWN
    );
    
    if (!m_window) {
        printf("[SDL2Renderer] ERROR: SDL_CreateWindow failed: %s\n", SDL_GetError());
        SDL_Quit();
        return false;
    }
    
    // Create renderer
    m_renderer = SDL_CreateRenderer(
        m_window,
        -1,
        SDL_RENDERER_ACCELERATED | SDL_RENDERER_PRESENTVSYNC
    );
    
    if (!m_renderer) {
        printf("[SDL2Renderer] ERROR: SDL_CreateRenderer failed: %s\n", SDL_GetError());
        SDL_DestroyWindow(m_window);
        SDL_Quit();
        return false;
    }
    
    // Enable alpha blending
    SDL_SetRenderDrawBlendMode(m_renderer, SDL_BLENDMODE_BLEND);
    
    m_windowWidth = width;
    m_windowHeight = height;
    m_isRunning = true;
    
    printf("[SDL2Renderer] Initialization complete\n");
    return true;
}

void SDL2Renderer::Shutdown() {
    if (!m_renderer) {
        return;
    }
    
    printf("[SDL2Renderer] Shutting down\n");
    
    // Clean up textures
    for (auto& pair : m_textures) {
        if (pair.second) {
            SDL_DestroyTexture(pair.second);
        }
    }
    m_textures.clear();
    
    // Clean up SDL
    if (m_renderer) {
        SDL_DestroyRenderer(m_renderer);
        m_renderer = nullptr;
    }
    
    if (m_window) {
        SDL_DestroyWindow(m_window);
        m_window = nullptr;
    }
    
    SDL_Quit();
    
    m_isRunning = false;
    
    printf("[SDL2Renderer] Shutdown complete\n");
}

void SDL2Renderer::BeginFrame() {
    // SDL2 doesn't need explicit frame begin
}

void SDL2Renderer::EndFrame() {
    // Frame end is handled by Present()
}

void SDL2Renderer::Present() {
    SDL_RenderPresent(m_renderer);
}

void SDL2Renderer::Clear(float r, float g, float b, float a) {
    SDL_SetRenderDrawColor(m_renderer,
                          static_cast<Uint8>(r * 255),
                          static_cast<Uint8>(g * 255),
                          static_cast<Uint8>(b * 255),
                          static_cast<Uint8>(a * 255));
    SDL_RenderClear(m_renderer);
}

void SDL2Renderer::DrawRect(float x, float y, float width, float height,
                            float r, float g, float b, float a) {
    SDL_SetRenderDrawColor(m_renderer,
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
    
    SDL_RenderFillRect(m_renderer, &rect);
}

void SDL2Renderer::DrawSprite(int textureId, float x, float y,
                              float width, float height, float rotation) {
    auto it = m_textures.find(textureId);
    if (it == m_textures.end()) {
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
    
    SDL_RenderCopyEx(m_renderer, it->second, nullptr, &destRect, angle, &center, SDL_FLIP_NONE);
}

int SDL2Renderer::LoadTexture(const char* filePath) {
    printf("[SDL2Renderer] Loading texture: %s\n", filePath);
    
    SDL_Surface* surface = SDL_LoadBMP(filePath);
    if (!surface) {
        printf("[SDL2Renderer] ERROR: SDL_LoadBMP failed: %s\n", SDL_GetError());
        return -1;
    }
    
    SDL_Texture* texture = SDL_CreateTextureFromSurface(m_renderer, surface);
    SDL_FreeSurface(surface);
    
    if (!texture) {
        printf("[SDL2Renderer] ERROR: SDL_CreateTextureFromSurface failed: %s\n", SDL_GetError());
        return -1;
    }
    
    int textureId = m_nextTextureId++;
    m_textures[textureId] = texture;
    
    return textureId;
}

void SDL2Renderer::UnloadTexture(int textureId) {
    auto it = m_textures.find(textureId);
    if (it != m_textures.end()) {
        SDL_DestroyTexture(it->second);
        m_textures.erase(it);
        printf("[SDL2Renderer] Unloaded texture: %d\n", textureId);
    }
}

} // namespace Chronicles
