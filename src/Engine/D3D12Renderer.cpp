#ifdef _WIN32

#include "D3D12Renderer.h"
#include <stdexcept>
#include <d3dcompiler.h>
#include <DirectXMath.h>
#include <wincodec.h>
#include <cstdio>

#pragma comment(lib, "d3d12.lib")
#pragma comment(lib, "dxgi.lib")
#pragma comment(lib, "d3dcompiler.lib")

namespace Chronicles {

using namespace DirectX;

// Helper function to check HRESULT
inline void ThrowIfFailed(HRESULT hr, const char* message = "D3D12 operation failed") {
    if (FAILED(hr)) {
        char buffer[256];
        snprintf(buffer, sizeof(buffer), "%s (HRESULT: 0x%08X)", message, hr);
        throw std::runtime_error(buffer);
    }
}

// Vertex structure for rendering
struct Vertex {
    XMFLOAT3 position;
    XMFLOAT2 texcoord;
    XMFLOAT4 color;
};

D3D12Renderer::D3D12Renderer()
    : m_hwnd(nullptr)
    , m_width(0)
    , m_height(0)
    , m_isRunning(false)
    , m_frameIndex(0)
    , m_rtvDescriptorSize(0)
    , m_srvDescriptorSize(0)
    , m_fenceEvent(nullptr)
    , m_nextTextureId(1)
    , m_currentSrvDescriptor(0)
{
    for (UINT i = 0; i < FrameCount; i++) {
        m_fenceValues[i] = 0;
    }
}

D3D12Renderer::~D3D12Renderer() {
    Shutdown();
}

bool D3D12Renderer::Initialize(int width, int height, const char* title) {
    printf("[D3D12Renderer] Initializing DirectX 12 renderer\n");
    printf("[D3D12Renderer] Window: %dx%d - %s\n", width, height, title);
    
    m_width = width;
    m_height = height;
    
    try {
        // Create window
        if (!CreateAppWindow(title)) {
            return false;
        }
        
        // Initialize D3D12
        if (!CreateDevice()) return false;
        if (!CreateCommandQueue()) return false;
        if (!CreateSwapChain(m_hwnd)) return false;
        if (!CreateDescriptorHeaps()) return false;
        if (!CreateRenderTargets()) return false;
        if (!CreateDepthStencil()) return false;
        if (!CreateRootSignature()) return false;
        if (!CreatePipelineStates()) return false;
        if (!CreateFence()) return false;
        
        // Create command allocators and command list
        for (UINT i = 0; i < FrameCount; i++) {
            ThrowIfFailed(
                m_device->CreateCommandAllocator(
                    D3D12_COMMAND_LIST_TYPE_DIRECT,
                    IID_PPV_ARGS(&m_commandAllocators[i])),
                "Failed to create command allocator");
        }
        
        ThrowIfFailed(
            m_device->CreateCommandList(
                0,
                D3D12_COMMAND_LIST_TYPE_DIRECT,
                m_commandAllocators[m_frameIndex].Get(),
                m_pipelineState.Get(),
                IID_PPV_ARGS(&m_commandList)),
            "Failed to create command list");
        
        ThrowIfFailed(m_commandList->Close(), "Failed to close command list");
        
        // Set up viewport and scissor rect
        m_viewport.TopLeftX = 0;
        m_viewport.TopLeftY = 0;
        m_viewport.Width = static_cast<float>(m_width);
        m_viewport.Height = static_cast<float>(m_height);
        m_viewport.MinDepth = 0.0f;
        m_viewport.MaxDepth = 1.0f;
        
        m_scissorRect.left = 0;
        m_scissorRect.top = 0;
        m_scissorRect.right = m_width;
        m_scissorRect.bottom = m_height;
        
        m_isRunning = true;
        
        printf("[D3D12Renderer] Initialization complete\n");
        return true;
    }
    catch (const std::exception& e) {
        printf("[D3D12Renderer] ERROR: %s\n", e.what());
        return false;
    }
}

void D3D12Renderer::Shutdown() {
    if (!m_device) {
        return;
    }
    
    printf("[D3D12Renderer] Shutting down\n");
    
    // Wait for GPU to finish
    WaitForGPU();
    
    // Clean up textures
    m_textures.clear();
    
    // Close fence event
    if (m_fenceEvent) {
        CloseHandle(m_fenceEvent);
        m_fenceEvent = nullptr;
    }
    
    // Destroy window
    if (m_hwnd) {
        DestroyWindow(m_hwnd);
        m_hwnd = nullptr;
    }
    
    m_isRunning = false;
    
    printf("[D3D12Renderer] Shutdown complete\n");
}

bool D3D12Renderer::CreateDevice() {
    UINT dxgiFactoryFlags = 0;
    
#ifdef _DEBUG
    // Enable debug layer
    ComPtr<ID3D12Debug> debugController;
    if (SUCCEEDED(D3D12GetDebugInterface(IID_PPV_ARGS(&debugController)))) {
        debugController->EnableDebugLayer();
        dxgiFactoryFlags |= DXGI_CREATE_FACTORY_DEBUG;
        printf("[D3D12Renderer] Debug layer enabled\n");
    }
#endif
    
    ComPtr<IDXGIFactory4> factory;
    ThrowIfFailed(CreateDXGIFactory2(dxgiFactoryFlags, IID_PPV_ARGS(&factory)),
                 "Failed to create DXGI factory");
    
    // Try to create hardware device
    ComPtr<IDXGIAdapter1> hardwareAdapter;
    for (UINT adapterIndex = 0; ; ++adapterIndex) {
        ComPtr<IDXGIAdapter1> adapter;
        if (factory->EnumAdapters1(adapterIndex, &adapter) == DXGI_ERROR_NOT_FOUND) {
            break;
        }
        
        DXGI_ADAPTER_DESC1 desc;
        adapter->GetDesc1(&desc);
        
        // Skip software adapter
        if (desc.Flags & DXGI_ADAPTER_FLAG_SOFTWARE) {
            continue;
        }
        
        // Try to create device with this adapter
        if (SUCCEEDED(D3D12CreateDevice(adapter.Get(), D3D_FEATURE_LEVEL_11_0,
                                       IID_PPV_ARGS(&m_device)))) {
            hardwareAdapter = adapter;
            printf("[D3D12Renderer] Using adapter: %ls\n", desc.Description);
            break;
        }
    }
    
    if (!m_device) {
        printf("[D3D12Renderer] WARNING: No hardware adapter found, using WARP\n");
        ComPtr<IDXGIAdapter> warpAdapter;
        ThrowIfFailed(factory->EnumWarpAdapter(IID_PPV_ARGS(&warpAdapter)),
                     "Failed to get WARP adapter");
        ThrowIfFailed(D3D12CreateDevice(warpAdapter.Get(), D3D_FEATURE_LEVEL_11_0,
                                       IID_PPV_ARGS(&m_device)),
                     "Failed to create D3D12 device");
    }
    
    return true;
}

bool D3D12Renderer::CreateCommandQueue() {
    D3D12_COMMAND_QUEUE_DESC queueDesc = {};
    queueDesc.Type = D3D12_COMMAND_LIST_TYPE_DIRECT;
    queueDesc.Flags = D3D12_COMMAND_QUEUE_FLAG_NONE;
    
    ThrowIfFailed(m_device->CreateCommandQueue(&queueDesc, IID_PPV_ARGS(&m_commandQueue)),
                 "Failed to create command queue");
    
    return true;
}

bool D3D12Renderer::CreateSwapChain(HWND hwnd) {
    ComPtr<IDXGIFactory4> factory;
    ThrowIfFailed(CreateDXGIFactory2(0, IID_PPV_ARGS(&factory)),
                 "Failed to create DXGI factory for swap chain");
    
    DXGI_SWAP_CHAIN_DESC1 swapChainDesc = {};
    swapChainDesc.BufferCount = FrameCount;
    swapChainDesc.Width = m_width;
    swapChainDesc.Height = m_height;
    swapChainDesc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
    swapChainDesc.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
    swapChainDesc.SwapEffect = DXGI_SWAP_EFFECT_FLIP_DISCARD;
    swapChainDesc.SampleDesc.Count = 1;
    
    ComPtr<IDXGISwapChain1> swapChain;
    ThrowIfFailed(factory->CreateSwapChainForHwnd(
        m_commandQueue.Get(),
        hwnd,
        &swapChainDesc,
        nullptr,
        nullptr,
        &swapChain),
        "Failed to create swap chain");
    
    // Disable Alt+Enter fullscreen toggle
    ThrowIfFailed(factory->MakeWindowAssociation(hwnd, DXGI_MWA_NO_ALT_ENTER),
                 "Failed to disable Alt+Enter");
    
    ThrowIfFailed(swapChain.As(&m_swapChain), "Failed to cast swap chain");
    m_frameIndex = m_swapChain->GetCurrentBackBufferIndex();
    
    return true;
}

bool D3D12Renderer::CreateDescriptorHeaps() {
    // RTV heap
    D3D12_DESCRIPTOR_HEAP_DESC rtvHeapDesc = {};
    rtvHeapDesc.NumDescriptors = FrameCount;
    rtvHeapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE_RTV;
    rtvHeapDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_NONE;
    ThrowIfFailed(m_device->CreateDescriptorHeap(&rtvHeapDesc, IID_PPV_ARGS(&m_rtvHeap)),
                 "Failed to create RTV heap");
    
    m_rtvDescriptorSize = m_device->GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE_RTV);
    
    // DSV heap
    D3D12_DESCRIPTOR_HEAP_DESC dsvHeapDesc = {};
    dsvHeapDesc.NumDescriptors = 1;
    dsvHeapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE_DSV;
    dsvHeapDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_NONE;
    ThrowIfFailed(m_device->CreateDescriptorHeap(&dsvHeapDesc, IID_PPV_ARGS(&m_dsvHeap)),
                 "Failed to create DSV heap");
    
    // SRV heap for textures
    D3D12_DESCRIPTOR_HEAP_DESC srvHeapDesc = {};
    srvHeapDesc.NumDescriptors = MaxTextures;
    srvHeapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV;
    srvHeapDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_SHADER_VISIBLE;
    ThrowIfFailed(m_device->CreateDescriptorHeap(&srvHeapDesc, IID_PPV_ARGS(&m_srvHeap)),
                 "Failed to create SRV heap");
    
    m_srvDescriptorSize = m_device->GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
    
    return true;
}

bool D3D12Renderer::CreateRenderTargets() {
    D3D12_CPU_DESCRIPTOR_HANDLE rtvHandle = m_rtvHeap->GetCPUDescriptorHandleForHeapStart();
    
    for (UINT i = 0; i < FrameCount; i++) {
        ThrowIfFailed(m_swapChain->GetBuffer(i, IID_PPV_ARGS(&m_renderTargets[i])),
                     "Failed to get swap chain buffer");
        m_device->CreateRenderTargetView(m_renderTargets[i].Get(), nullptr, rtvHandle);
        rtvHandle.ptr += m_rtvDescriptorSize;
    }
    
    return true;
}

bool D3D12Renderer::CreateDepthStencil() {
    D3D12_HEAP_PROPERTIES heapProps = {};
    heapProps.Type = D3D12_HEAP_TYPE_DEFAULT;
    
    D3D12_RESOURCE_DESC depthDesc = {};
    depthDesc.Dimension = D3D12_RESOURCE_DIMENSION_TEXTURE2D;
    depthDesc.Width = m_width;
    depthDesc.Height = m_height;
    depthDesc.DepthOrArraySize = 1;
    depthDesc.MipLevels = 1;
    depthDesc.Format = DXGI_FORMAT_D32_FLOAT;
    depthDesc.SampleDesc.Count = 1;
    depthDesc.Flags = D3D12_RESOURCE_FLAG_ALLOW_DEPTH_STENCIL;
    
    D3D12_CLEAR_VALUE clearValue = {};
    clearValue.Format = DXGI_FORMAT_D32_FLOAT;
    clearValue.DepthStencil.Depth = 1.0f;
    
    ThrowIfFailed(m_device->CreateCommittedResource(
        &heapProps,
        D3D12_HEAP_FLAG_NONE,
        &depthDesc,
        D3D12_RESOURCE_STATE_DEPTH_WRITE,
        &clearValue,
        IID_PPV_ARGS(&m_depthStencil)),
        "Failed to create depth stencil");
    
    D3D12_DEPTH_STENCIL_VIEW_DESC dsvDesc = {};
    dsvDesc.Format = DXGI_FORMAT_D32_FLOAT;
    dsvDesc.ViewDimension = D3D12_DSV_DIMENSION_TEXTURE2D;
    
    m_device->CreateDepthStencilView(m_depthStencil.Get(), &dsvDesc,
                                     m_dsvHeap->GetCPUDescriptorHandleForHeapStart());
    
    return true;
}

bool D3D12Renderer::CreateRootSignature() {
    // Simple root signature for 2D rendering
    D3D12_ROOT_PARAMETER rootParameters[1] = {};
    
    // Descriptor table for SRVs (textures)
    D3D12_DESCRIPTOR_RANGE descriptorRange = {};
    descriptorRange.RangeType = D3D12_DESCRIPTOR_RANGE_TYPE_SRV;
    descriptorRange.NumDescriptors = 1;
    descriptorRange.BaseShaderRegister = 0;
    descriptorRange.RegisterSpace = 0;
    descriptorRange.OffsetInDescriptorsFromTableStart = D3D12_DESCRIPTOR_RANGE_OFFSET_APPEND;
    
    rootParameters[0].ParameterType = D3D12_ROOT_PARAMETER_TYPE_DESCRIPTOR_TABLE;
    rootParameters[0].DescriptorTable.NumDescriptorRanges = 1;
    rootParameters[0].DescriptorTable.pDescriptorRanges = &descriptorRange;
    rootParameters[0].ShaderVisibility = D3D12_SHADER_VISIBILITY_PIXEL;
    
    // Static sampler
    D3D12_STATIC_SAMPLER_DESC sampler = {};
    sampler.Filter = D3D12_FILTER_MIN_MAG_MIP_LINEAR;
    sampler.AddressU = D3D12_TEXTURE_ADDRESS_MODE_WRAP;
    sampler.AddressV = D3D12_TEXTURE_ADDRESS_MODE_WRAP;
    sampler.AddressW = D3D12_TEXTURE_ADDRESS_MODE_WRAP;
    sampler.MipLODBias = 0;
    sampler.MaxAnisotropy = 0;
    sampler.ComparisonFunc = D3D12_COMPARISON_FUNC_NEVER;
    sampler.BorderColor = D3D12_STATIC_BORDER_COLOR_TRANSPARENT_BLACK;
    sampler.MinLOD = 0.0f;
    sampler.MaxLOD = D3D12_FLOAT32_MAX;
    sampler.ShaderRegister = 0;
    sampler.RegisterSpace = 0;
    sampler.ShaderVisibility = D3D12_SHADER_VISIBILITY_PIXEL;
    
    D3D12_ROOT_SIGNATURE_DESC rootSignatureDesc = {};
    rootSignatureDesc.NumParameters = _countof(rootParameters);
    rootSignatureDesc.pParameters = rootParameters;
    rootSignatureDesc.NumStaticSamplers = 1;
    rootSignatureDesc.pStaticSamplers = &sampler;
    rootSignatureDesc.Flags = D3D12_ROOT_SIGNATURE_FLAG_ALLOW_INPUT_ASSEMBLER_INPUT_LAYOUT;
    
    ComPtr<ID3DBlob> signature;
    ComPtr<ID3DBlob> error;
    HRESULT hr = D3D12SerializeRootSignature(&rootSignatureDesc, D3D_ROOT_SIGNATURE_VERSION_1,
                                             &signature, &error);
    if (FAILED(hr)) {
        if (error) {
            printf("[D3D12Renderer] Root signature serialization error: %s\n",
                   (char*)error->GetBufferPointer());
        }
        throw std::runtime_error("Failed to serialize root signature");
    }
    
    ThrowIfFailed(m_device->CreateRootSignature(0, signature->GetBufferPointer(),
                                                signature->GetBufferSize(),
                                                IID_PPV_ARGS(&m_rootSignature)),
                 "Failed to create root signature");
    
    return true;
}

bool D3D12Renderer::CreatePipelineStates() {
    // Simple shaders for 2D rendering
    const char* vertexShaderCode = R"(
        struct VSInput {
            float3 position : POSITION;
            float2 texcoord : TEXCOORD;
            float4 color : COLOR;
        };
        
        struct PSInput {
            float4 position : SV_POSITION;
            float2 texcoord : TEXCOORD;
            float4 color : COLOR;
        };
        
        PSInput main(VSInput input) {
            PSInput output;
            output.position = float4(input.position, 1.0f);
            output.texcoord = input.texcoord;
            output.color = input.color;
            return output;
        }
    )";
    
    const char* pixelShaderCode = R"(
        struct PSInput {
            float4 position : SV_POSITION;
            float2 texcoord : TEXCOORD;
            float4 color : COLOR;
        };
        
        Texture2D g_texture : register(t0);
        SamplerState g_sampler : register(s0);
        
        float4 main(PSInput input) : SV_TARGET {
            return input.color * g_texture.Sample(g_sampler, input.texcoord);
        }
    )";
    
    ComPtr<ID3DBlob> vertexShader;
    ComPtr<ID3DBlob> pixelShader;
    ComPtr<ID3DBlob> error;
    
    // Compile vertex shader
    HRESULT hr = D3DCompile(vertexShaderCode, strlen(vertexShaderCode), nullptr, nullptr, nullptr,
                            "main", "vs_5_0", 0, 0, &vertexShader, &error);
    if (FAILED(hr)) {
        if (error) {
            printf("[D3D12Renderer] Vertex shader compilation error: %s\n",
                   (char*)error->GetBufferPointer());
        }
        return false;
    }
    
    // Compile pixel shader
    hr = D3DCompile(pixelShaderCode, strlen(pixelShaderCode), nullptr, nullptr, nullptr,
                    "main", "ps_5_0", 0, 0, &pixelShader, &error);
    if (FAILED(hr)) {
        if (error) {
            printf("[D3D12Renderer] Pixel shader compilation error: %s\n",
                   (char*)error->GetBufferPointer());
        }
        return false;
    }
    
    // Define input layout
    D3D12_INPUT_ELEMENT_DESC inputElementDescs[] = {
        { "POSITION", 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 0, D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA, 0 },
        { "TEXCOORD", 0, DXGI_FORMAT_R32G32_FLOAT, 0, 12, D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA, 0 },
        { "COLOR", 0, DXGI_FORMAT_R32G32B32A32_FLOAT, 0, 20, D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA, 0 }
    };
    
    // Create pipeline state
    D3D12_GRAPHICS_PIPELINE_STATE_DESC psoDesc = {};
    psoDesc.InputLayout = { inputElementDescs, _countof(inputElementDescs) };
    psoDesc.pRootSignature = m_rootSignature.Get();
    psoDesc.VS = { vertexShader->GetBufferPointer(), vertexShader->GetBufferSize() };
    psoDesc.PS = { pixelShader->GetBufferPointer(), pixelShader->GetBufferSize() };
    psoDesc.RasterizerState.FillMode = D3D12_FILL_MODE_SOLID;
    psoDesc.RasterizerState.CullMode = D3D12_CULL_MODE_NONE;
    psoDesc.BlendState.RenderTarget[0].BlendEnable = TRUE;
    psoDesc.BlendState.RenderTarget[0].SrcBlend = D3D12_BLEND_SRC_ALPHA;
    psoDesc.BlendState.RenderTarget[0].DestBlend = D3D12_BLEND_INV_SRC_ALPHA;
    psoDesc.BlendState.RenderTarget[0].BlendOp = D3D12_BLEND_OP_ADD;
    psoDesc.BlendState.RenderTarget[0].SrcBlendAlpha = D3D12_BLEND_ONE;
    psoDesc.BlendState.RenderTarget[0].DestBlendAlpha = D3D12_BLEND_ZERO;
    psoDesc.BlendState.RenderTarget[0].BlendOpAlpha = D3D12_BLEND_OP_ADD;
    psoDesc.BlendState.RenderTarget[0].RenderTargetWriteMask = D3D12_COLOR_WRITE_ENABLE_ALL;
    psoDesc.DepthStencilState.DepthEnable = TRUE;
    psoDesc.DepthStencilState.DepthWriteMask = D3D12_DEPTH_WRITE_MASK_ALL;
    psoDesc.DepthStencilState.DepthFunc = D3D12_COMPARISON_FUNC_LESS;
    psoDesc.SampleMask = UINT_MAX;
    psoDesc.PrimitiveTopologyType = D3D12_PRIMITIVE_TOPOLOGY_TYPE_TRIANGLE;
    psoDesc.NumRenderTargets = 1;
    psoDesc.RTVFormats[0] = DXGI_FORMAT_R8G8B8A8_UNORM;
    psoDesc.DSVFormat = DXGI_FORMAT_D32_FLOAT;
    psoDesc.SampleDesc.Count = 1;
    
    ThrowIfFailed(m_device->CreateGraphicsPipelineState(&psoDesc, IID_PPV_ARGS(&m_pipelineState)),
                 "Failed to create pipeline state");
    
    return true;
}

bool D3D12Renderer::CreateFence() {
    ThrowIfFailed(m_device->CreateFence(0, D3D12_FENCE_FLAG_NONE, IID_PPV_ARGS(&m_fence)),
                 "Failed to create fence");
    
    m_fenceEvent = CreateEvent(nullptr, FALSE, FALSE, nullptr);
    if (!m_fenceEvent) {
        throw std::runtime_error("Failed to create fence event");
    }
    
    return true;
}

void D3D12Renderer::BeginFrame() {
    // Reset command allocator and list
    ThrowIfFailed(m_commandAllocators[m_frameIndex]->Reset(),
                 "Failed to reset command allocator");
    ThrowIfFailed(m_commandList->Reset(m_commandAllocators[m_frameIndex].Get(),
                                      m_pipelineState.Get()),
                 "Failed to reset command list");
    
    // Set necessary state
    m_commandList->SetGraphicsRootSignature(m_rootSignature.Get());
    m_commandList->RSSetViewports(1, &m_viewport);
    m_commandList->RSSetScissorRects(1, &m_scissorRect);
    
    // Transition render target to render target state
    D3D12_RESOURCE_BARRIER barrier = {};
    barrier.Type = D3D12_RESOURCE_BARRIER_TYPE_TRANSITION;
    barrier.Transition.pResource = m_renderTargets[m_frameIndex].Get();
    barrier.Transition.StateBefore = D3D12_RESOURCE_STATE_PRESENT;
    barrier.Transition.StateAfter = D3D12_RESOURCE_STATE_RENDER_TARGET;
    barrier.Transition.Subresource = D3D12_RESOURCE_BARRIER_ALL_SUBRESOURCES;
    m_commandList->ResourceBarrier(1, &barrier);
    
    // Set render target
    D3D12_CPU_DESCRIPTOR_HANDLE rtvHandle = m_rtvHeap->GetCPUDescriptorHandleForHeapStart();
    rtvHandle.ptr += m_frameIndex * m_rtvDescriptorSize;
    D3D12_CPU_DESCRIPTOR_HANDLE dsvHandle = m_dsvHeap->GetCPUDescriptorHandleForHeapStart();
    m_commandList->OMSetRenderTargets(1, &rtvHandle, FALSE, &dsvHandle);
}

void D3D12Renderer::EndFrame() {
    // Transition render target to present state
    D3D12_RESOURCE_BARRIER barrier = {};
    barrier.Type = D3D12_RESOURCE_BARRIER_TYPE_TRANSITION;
    barrier.Transition.pResource = m_renderTargets[m_frameIndex].Get();
    barrier.Transition.StateBefore = D3D12_RESOURCE_STATE_RENDER_TARGET;
    barrier.Transition.StateAfter = D3D12_RESOURCE_STATE_PRESENT;
    barrier.Transition.Subresource = D3D12_RESOURCE_BARRIER_ALL_SUBRESOURCES;
    m_commandList->ResourceBarrier(1, &barrier);
    
    ThrowIfFailed(m_commandList->Close(), "Failed to close command list");
    
    // Execute command list
    ID3D12CommandList* commandLists[] = { m_commandList.Get() };
    m_commandQueue->ExecuteCommandLists(_countof(commandLists), commandLists);
}

void D3D12Renderer::Present() {
    ThrowIfFailed(m_swapChain->Present(1, 0), "Failed to present");
    MoveToNextFrame();
}

void D3D12Renderer::Clear(float r, float g, float b, float a) {
    D3D12_CPU_DESCRIPTOR_HANDLE rtvHandle = m_rtvHeap->GetCPUDescriptorHandleForHeapStart();
    rtvHandle.ptr += m_frameIndex * m_rtvDescriptorSize;
    
    float clearColor[4] = { r, g, b, a };
    m_commandList->ClearRenderTargetView(rtvHandle, clearColor, 0, nullptr);
    
    D3D12_CPU_DESCRIPTOR_HANDLE dsvHandle = m_dsvHeap->GetCPUDescriptorHandleForHeapStart();
    m_commandList->ClearDepthStencilView(dsvHandle, D3D12_CLEAR_FLAG_DEPTH, 1.0f, 0, 0, nullptr);
}

void D3D12Renderer::DrawRect(float x, float y, float width, float height,
                             float r, float g, float b, float a) {
    // TODO: Implement rectangle drawing with vertex buffer
    // For now, this is a stub
}

void D3D12Renderer::DrawSprite(int textureId, float x, float y,
                               float width, float height, float rotation) {
    // TODO: Implement sprite drawing with texture
    // For now, this is a stub
}

int D3D12Renderer::LoadTexture(const char* filePath) {
    printf("[D3D12Renderer] Loading texture: %s\n", filePath);
    // TODO: Implement texture loading with WIC
    // For now, return a placeholder ID
    return m_nextTextureId++;
}

void D3D12Renderer::UnloadTexture(int textureId) {
    auto it = m_textures.find(textureId);
    if (it != m_textures.end()) {
        m_textures.erase(it);
        printf("[D3D12Renderer] Unloaded texture: %d\n", textureId);
    }
}

void D3D12Renderer::WaitForGPU() {
    ThrowIfFailed(m_commandQueue->Signal(m_fence.Get(), m_fenceValues[m_frameIndex]),
                 "Failed to signal fence");
    ThrowIfFailed(m_fence->SetEventOnCompletion(m_fenceValues[m_frameIndex], m_fenceEvent),
                 "Failed to set event on completion");
    WaitForSingleObject(m_fenceEvent, INFINITE);
    m_fenceValues[m_frameIndex]++;
}

void D3D12Renderer::MoveToNextFrame() {
    const UINT64 currentFenceValue = m_fenceValues[m_frameIndex];
    ThrowIfFailed(m_commandQueue->Signal(m_fence.Get(), currentFenceValue),
                 "Failed to signal fence");
    
    m_frameIndex = m_swapChain->GetCurrentBackBufferIndex();
    
    if (m_fence->GetCompletedValue() < m_fenceValues[m_frameIndex]) {
        ThrowIfFailed(m_fence->SetEventOnCompletion(m_fenceValues[m_frameIndex], m_fenceEvent),
                     "Failed to set event on completion");
        WaitForSingleObject(m_fenceEvent, INFINITE);
    }
    
    m_fenceValues[m_frameIndex] = currentFenceValue + 1;
}

bool D3D12Renderer::CreateAppWindow(const char* title) {
    // Register window class
    WNDCLASSEX wc = {};
    wc.cbSize = sizeof(WNDCLASSEX);
    wc.style = CS_HREDRAW | CS_VREDRAW;
    wc.lpfnWndProc = WindowProc;
    wc.hInstance = GetModuleHandle(nullptr);
    wc.hCursor = LoadCursor(nullptr, IDC_ARROW);
    wc.lpszClassName = L"ChroniclesD3D12WindowClass";
    
    if (!RegisterClassEx(&wc)) {
        printf("[D3D12Renderer] ERROR: Failed to register window class\n");
        return false;
    }
    
    // Convert title to wide string
    wchar_t wideTitle[256];
    MultiByteToWideChar(CP_UTF8, 0, title, -1, wideTitle, 256);
    
    // Calculate window size with client area
    RECT rect = { 0, 0, m_width, m_height };
    AdjustWindowRect(&rect, WS_OVERLAPPEDWINDOW, FALSE);
    
    // Create window
    m_hwnd = CreateWindowEx(
        0,
        L"ChroniclesD3D12WindowClass",
        wideTitle,
        WS_OVERLAPPEDWINDOW,
        CW_USEDEFAULT, CW_USEDEFAULT,
        rect.right - rect.left,
        rect.bottom - rect.top,
        nullptr,
        nullptr,
        GetModuleHandle(nullptr),
        this
    );
    
    if (!m_hwnd) {
        printf("[D3D12Renderer] ERROR: Failed to create window\n");
        return false;
    }
    
    ShowWindow(m_hwnd, SW_SHOW);
    UpdateWindow(m_hwnd);
    
    return true;
}

LRESULT CALLBACK D3D12Renderer::WindowProc(HWND hwnd, UINT message, WPARAM wParam, LPARAM lParam) {
    D3D12Renderer* renderer = reinterpret_cast<D3D12Renderer*>(GetWindowLongPtr(hwnd, GWLP_USERDATA));
    
    switch (message) {
        case WM_CREATE: {
            CREATESTRUCT* pCreate = reinterpret_cast<CREATESTRUCT*>(lParam);
            renderer = reinterpret_cast<D3D12Renderer*>(pCreate->lpCreateParams);
            SetWindowLongPtr(hwnd, GWLP_USERDATA, reinterpret_cast<LONG_PTR>(renderer));
            break;
        }
        
        case WM_DESTROY:
            if (renderer) {
                renderer->SetRunning(false);
            }
            PostQuitMessage(0);
            break;
        
        case WM_CLOSE:
            if (renderer) {
                renderer->SetRunning(false);
            }
            DestroyWindow(hwnd);
            break;
        
        default:
            return DefWindowProc(hwnd, message, wParam, lParam);
    }
    
    return 0;
}

} // namespace Chronicles

#endif // _WIN32
