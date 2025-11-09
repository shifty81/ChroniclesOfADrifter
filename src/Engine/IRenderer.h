#pragma once

#include <string>

// Abstract renderer interface for backend independence
// Allows switching between SDL2, DirectX 12, Vulkan, etc.

namespace Chronicles {

enum class RendererBackend {
    SDL2,
    DirectX11,
    DirectX12,
    Vulkan
};

class IRenderer {
public:
    virtual ~IRenderer() = default;
    
    // Initialization and cleanup
    virtual bool Initialize(int width, int height, const char* title) = 0;
    virtual void Shutdown() = 0;
    
    // Frame operations
    virtual void BeginFrame() = 0;
    virtual void EndFrame() = 0;
    virtual void Present() = 0;
    
    // Rendering operations
    virtual void Clear(float r, float g, float b, float a) = 0;
    virtual void DrawRect(float x, float y, float width, float height,
                         float r, float g, float b, float a) = 0;
    virtual void DrawSprite(int textureId, float x, float y,
                          float width, float height, float rotation) = 0;
    
    // Texture operations
    virtual int LoadTexture(const char* filePath) = 0;
    virtual void UnloadTexture(int textureId) = 0;
    
    // Getters
    virtual int GetWidth() const = 0;
    virtual int GetHeight() const = 0;
    virtual bool IsRunning() const = 0;
    virtual void SetRunning(bool running) = 0;
};

} // namespace Chronicles
