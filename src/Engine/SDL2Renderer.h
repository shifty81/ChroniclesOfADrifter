#pragma once

#include "IRenderer.h"
#include <SDL2/SDL.h>
#include <map>
#include <chrono>

// SDL2 Renderer Implementation
// Cross-platform rendering backend

namespace Chronicles {

class SDL2Renderer : public IRenderer {
public:
    SDL2Renderer();
    ~SDL2Renderer() override;
    
    // IRenderer implementation
    bool Initialize(int width, int height, const char* title) override;
    void Shutdown() override;
    void BeginFrame() override;
    void EndFrame() override;
    void Present() override;
    void Clear(float r, float g, float b, float a) override;
    void DrawRect(float x, float y, float width, float height,
                 float r, float g, float b, float a) override;
    void DrawSprite(int textureId, float x, float y,
                   float width, float height, float rotation) override;
    int LoadTexture(const char* filePath) override;
    void UnloadTexture(int textureId) override;
    
    int GetWidth() const override { return m_windowWidth; }
    int GetHeight() const override { return m_windowHeight; }
    bool IsRunning() const override { return m_isRunning; }
    void SetRunning(bool running) override { m_isRunning = running; }

private:
    SDL_Window* m_window;
    SDL_Renderer* m_renderer;
    int m_windowWidth;
    int m_windowHeight;
    bool m_isRunning;
    
    std::map<int, SDL_Texture*> m_textures;
    int m_nextTextureId;
};

} // namespace Chronicles
