#pragma once

#ifdef _WIN32

#include "IRenderer.h"
#include <d3d12.h>
#include <dxgi1_6.h>
#include <wrl/client.h>
#include <map>
#include <vector>
#include <string>

// DirectX 12 Renderer Implementation
// High-performance Windows-optimized rendering backend

namespace Chronicles {

using Microsoft::WRL::ComPtr;

struct Texture {
    ComPtr<ID3D12Resource> resource;
    ComPtr<ID3D12Resource> uploadHeap;
    D3D12_CPU_DESCRIPTOR_HANDLE srvHandle;
    int width;
    int height;
};

class D3D12Renderer : public IRenderer {
public:
    D3D12Renderer();
    ~D3D12Renderer() override;
    
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
    bool CreateCommandQueue();
    bool CreateSwapChain(HWND hwnd);
    bool CreateRenderTargets();
    bool CreateDepthStencil();
    bool CreateDescriptorHeaps();
    bool CreateRootSignature();
    bool CreatePipelineStates();
    bool CreateFence();
    
    // Frame synchronization
    void WaitForGPU();
    void MoveToNextFrame();
    
    // Window management
    bool CreateAppWindow(const char* title);
    static LRESULT CALLBACK WindowProc(HWND hwnd, UINT message, WPARAM wParam, LPARAM lParam);
    
    // Constants
    static constexpr UINT FrameCount = 2;
    static constexpr UINT MaxTextures = 1024;
    
    // D3D12 objects
    ComPtr<ID3D12Device> m_device;
    ComPtr<ID3D12CommandQueue> m_commandQueue;
    ComPtr<IDXGISwapChain3> m_swapChain;
    ComPtr<ID3D12DescriptorHeap> m_rtvHeap;
    ComPtr<ID3D12DescriptorHeap> m_dsvHeap;
    ComPtr<ID3D12DescriptorHeap> m_srvHeap;
    ComPtr<ID3D12Resource> m_renderTargets[FrameCount];
    ComPtr<ID3D12Resource> m_depthStencil;
    ComPtr<ID3D12CommandAllocator> m_commandAllocators[FrameCount];
    ComPtr<ID3D12GraphicsCommandList> m_commandList;
    ComPtr<ID3D12RootSignature> m_rootSignature;
    ComPtr<ID3D12PipelineState> m_pipelineState;
    ComPtr<ID3D12PipelineState> m_spritePipelineState;
    
    // Synchronization
    ComPtr<ID3D12Fence> m_fence;
    UINT64 m_fenceValues[FrameCount];
    HANDLE m_fenceEvent;
    
    // Frame management
    UINT m_frameIndex;
    UINT m_rtvDescriptorSize;
    UINT m_srvDescriptorSize;
    
    // Window
    HWND m_hwnd;
    int m_width;
    int m_height;
    bool m_isRunning;
    
    // Textures
    std::map<int, Texture> m_textures;
    int m_nextTextureId;
    UINT m_currentSrvDescriptor;
    
    // Viewport and scissor
    D3D12_VIEWPORT m_viewport;
    D3D12_RECT m_scissorRect;
};

} // namespace Chronicles

#endif // _WIN32
