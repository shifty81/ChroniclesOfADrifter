#pragma once

#ifdef _WIN32

#include "IRenderer.h"
#include <d3d11.h>
#include <dxgi.h>
#include <wrl/client.h>
#include <map>
#include <vector>
#include <string>

// DirectX 11 Renderer Implementation
// Balanced Windows rendering backend with broad hardware support

namespace Chronicles {

using Microsoft::WRL::ComPtr;

struct D3D11Texture {
    ComPtr<ID3D11Texture2D> texture;
    ComPtr<ID3D11ShaderResourceView> shaderResourceView;
    int width;
    int height;
};

class D3D11Renderer : public IRenderer {
public:
    D3D11Renderer();
    ~D3D11Renderer() override;
    
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
    
    int GetWidth() const override { return m_width; }
    int GetHeight() const override { return m_height; }
    bool IsRunning() const override { return m_isRunning; }
    void SetRunning(bool running) override { m_isRunning = running; }

private:
    // Initialization helpers
    bool CreateDevice();
    bool CreateSwapChain(HWND hwnd);
    bool CreateRenderTargetView();
    bool CreateDepthStencilBuffer();
    bool CreateDepthStencilState();
    bool CreateRasterizerState();
    bool CreateBlendState();
    bool CreateShadersAndInputLayout();
    bool CreateConstantBuffers();
    
    // Window management
    bool CreateAppWindow(const char* title);
    static LRESULT CALLBACK WindowProc(HWND hwnd, UINT message, WPARAM wParam, LPARAM lParam);
    
    // Constants
    static constexpr UINT MaxTextures = 1024;
    
    // D3D11 objects
    ComPtr<ID3D11Device> m_device;
    ComPtr<ID3D11DeviceContext> m_deviceContext;
    ComPtr<IDXGISwapChain> m_swapChain;
    ComPtr<ID3D11RenderTargetView> m_renderTargetView;
    ComPtr<ID3D11Texture2D> m_depthStencilBuffer;
    ComPtr<ID3D11DepthStencilView> m_depthStencilView;
    ComPtr<ID3D11DepthStencilState> m_depthStencilState;
    ComPtr<ID3D11RasterizerState> m_rasterizerState;
    ComPtr<ID3D11BlendState> m_blendState;
    
    // Shaders and input layout
    ComPtr<ID3D11VertexShader> m_vertexShader;
    ComPtr<ID3D11PixelShader> m_pixelShader;
    ComPtr<ID3D11InputLayout> m_inputLayout;
    
    // Constant buffers
    ComPtr<ID3D11Buffer> m_constantBuffer;
    
    // Vertex buffer for dynamic rendering
    ComPtr<ID3D11Buffer> m_vertexBuffer;
    ComPtr<ID3D11Buffer> m_indexBuffer;
    
    // Window
    HWND m_hwnd;
    int m_width;
    int m_height;
    bool m_isRunning;
    
    // Textures
    std::map<int, D3D11Texture> m_textures;
    int m_nextTextureId;
    
    // Viewport
    D3D11_VIEWPORT m_viewport;
    
    // Clear color
    float m_clearColor[4];
};

} // namespace Chronicles

#endif // _WIN32
