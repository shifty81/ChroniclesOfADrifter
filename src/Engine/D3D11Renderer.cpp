#ifdef _WIN32

#include "D3D11Renderer.h"
#include <d3dcompiler.h>
#include <cstdio>
#include <cstring>
#include <vector>
#include <DirectXMath.h>
#include <wincodec.h>
#include <wrl/client.h>

#pragma comment(lib, "d3d11.lib")
#pragma comment(lib, "dxgi.lib")
#pragma comment(lib, "d3dcompiler.lib")
#pragma comment(lib, "windowscodecs.lib")

namespace Chronicles {

// Vertex structure for 2D rendering
struct Vertex {
    DirectX::XMFLOAT3 position;
    DirectX::XMFLOAT4 color;
    DirectX::XMFLOAT2 texCoord;
};

// Constant buffer structure
struct ConstantBufferData {
    DirectX::XMMATRIX worldViewProjection;
    DirectX::XMFLOAT4 tintColor;
};

// Simple vertex shader for 2D rendering
static const char* g_vertexShaderSource = R"(
cbuffer ConstantBuffer : register(b0)
{
    matrix worldViewProjection;
    float4 tintColor;
};

struct VS_INPUT
{
    float3 position : POSITION;
    float4 color : COLOR;
    float2 texCoord : TEXCOORD;
};

struct PS_INPUT
{
    float4 position : SV_POSITION;
    float4 color : COLOR;
    float2 texCoord : TEXCOORD;
};

PS_INPUT main(VS_INPUT input)
{
    PS_INPUT output;
    output.position = mul(float4(input.position, 1.0f), worldViewProjection);
    output.color = input.color * tintColor;
    output.texCoord = input.texCoord;
    return output;
}
)";

// Simple pixel shader for 2D rendering
static const char* g_pixelShaderSource = R"(
Texture2D shaderTexture : register(t0);
SamplerState samplerState : register(s0);

struct PS_INPUT
{
    float4 position : SV_POSITION;
    float4 color : COLOR;
    float2 texCoord : TEXCOORD;
};

float4 main(PS_INPUT input) : SV_TARGET
{
    float4 textureColor = shaderTexture.Sample(samplerState, input.texCoord);
    return textureColor * input.color;
}
)";

D3D11Renderer::D3D11Renderer()
    : m_hwnd(nullptr)
    , m_width(0)
    , m_height(0)
    , m_isRunning(false)
    , m_nextTextureId(1)
{
    m_clearColor[0] = 0.0f;
    m_clearColor[1] = 0.0f;
    m_clearColor[2] = 0.0f;
    m_clearColor[3] = 1.0f;
}

D3D11Renderer::~D3D11Renderer() {
    Shutdown();
}

bool D3D11Renderer::Initialize(int width, int height, const char* title) {
    printf("[D3D11Renderer] Initializing DirectX 11 renderer\n");
    
    m_width = width;
    m_height = height;
    
    // Create window
    if (!CreateAppWindow(title)) {
        printf("[D3D11Renderer] Failed to create window\n");
        return false;
    }
    
    // Create D3D11 device and context
    if (!CreateDevice()) {
        printf("[D3D11Renderer] Failed to create device\n");
        return false;
    }
    
    // Create swap chain
    if (!CreateSwapChain(m_hwnd)) {
        printf("[D3D11Renderer] Failed to create swap chain\n");
        return false;
    }
    
    // Create render target view
    if (!CreateRenderTargetView()) {
        printf("[D3D11Renderer] Failed to create render target view\n");
        return false;
    }
    
    // Create depth stencil buffer
    if (!CreateDepthStencilBuffer()) {
        printf("[D3D11Renderer] Failed to create depth stencil buffer\n");
        return false;
    }
    
    // Create depth stencil state
    if (!CreateDepthStencilState()) {
        printf("[D3D11Renderer] Failed to create depth stencil state\n");
        return false;
    }
    
    // Create rasterizer state
    if (!CreateRasterizerState()) {
        printf("[D3D11Renderer] Failed to create rasterizer state\n");
        return false;
    }
    
    // Create blend state
    if (!CreateBlendState()) {
        printf("[D3D11Renderer] Failed to create blend state\n");
        return false;
    }
    
    // Create shaders and input layout
    if (!CreateShadersAndInputLayout()) {
        printf("[D3D11Renderer] Failed to create shaders\n");
        return false;
    }
    
    // Create constant buffers
    if (!CreateConstantBuffers()) {
        printf("[D3D11Renderer] Failed to create constant buffers\n");
        return false;
    }
    
    // Create sampler state
    if (!CreateSamplerState()) {
        printf("[D3D11Renderer] Failed to create sampler state\n");
        return false;
    }
    
    // Create white texture for solid color rendering
    if (!CreateWhiteTexture()) {
        printf("[D3D11Renderer] Failed to create white texture\n");
        return false;
    }
    
    // Set viewport
    m_viewport.TopLeftX = 0.0f;
    m_viewport.TopLeftY = 0.0f;
    m_viewport.Width = static_cast<float>(m_width);
    m_viewport.Height = static_cast<float>(m_height);
    m_viewport.MinDepth = 0.0f;
    m_viewport.MaxDepth = 1.0f;
    m_deviceContext->RSSetViewports(1, &m_viewport);
    
    // Set render targets
    m_deviceContext->OMSetRenderTargets(1, m_renderTargetView.GetAddressOf(), m_depthStencilView.Get());
    
    m_isRunning = true;
    printf("[D3D11Renderer] DirectX 11 renderer initialized successfully\n");
    return true;
}

void D3D11Renderer::Shutdown() {
    if (!m_isRunning) {
        return;
    }
    
    printf("[D3D11Renderer] Shutting down DirectX 11 renderer\n");
    
    // Unload all textures
    m_textures.clear();
    
    // Release D3D11 resources (ComPtr handles this automatically)
    
    // Destroy window
    if (m_hwnd) {
        DestroyWindow(m_hwnd);
        m_hwnd = nullptr;
    }
    
    m_isRunning = false;
}

void D3D11Renderer::BeginFrame() {
    // Process Windows messages
    MSG msg = {};
    while (PeekMessage(&msg, nullptr, 0, 0, PM_REMOVE)) {
        if (msg.message == WM_QUIT) {
            m_isRunning = false;
        }
        TranslateMessage(&msg);
        DispatchMessage(&msg);
    }
}

void D3D11Renderer::EndFrame() {
    // Nothing to do here for D3D11
}

void D3D11Renderer::Present() {
    // Present the frame
    m_swapChain->Present(1, 0); // VSync enabled (1), no flags (0)
}

void D3D11Renderer::Clear(float r, float g, float b, float a) {
    m_clearColor[0] = r;
    m_clearColor[1] = g;
    m_clearColor[2] = b;
    m_clearColor[3] = a;
    
    // Clear the render target
    m_deviceContext->ClearRenderTargetView(m_renderTargetView.Get(), m_clearColor);
    
    // Clear the depth stencil view
    m_deviceContext->ClearDepthStencilView(m_depthStencilView.Get(), 
                                          D3D11_CLEAR_DEPTH | D3D11_CLEAR_STENCIL, 
                                          1.0f, 0);
}

void D3D11Renderer::DrawRect(float x, float y, float width, float height,
                             float r, float g, float b, float a) {
    // Create vertices for a rectangle (2 triangles = 6 vertices)
    // Convert screen coordinates to normalized device coordinates (NDC)
    // NDC: X ranges from -1 (left) to +1 (right), Y ranges from -1 (top) to +1 (bottom)
    float left = (x / m_width) * 2.0f - 1.0f;
    float right = ((x + width) / m_width) * 2.0f - 1.0f;
    float top = 1.0f - (y / m_height) * 2.0f;
    float bottom = 1.0f - ((y + height) / m_height) * 2.0f;
    
    Vertex vertices[] = {
        // Triangle 1
        { DirectX::XMFLOAT3(left, top, 0.0f), DirectX::XMFLOAT4(r, g, b, a), DirectX::XMFLOAT2(0.0f, 0.0f) },
        { DirectX::XMFLOAT3(right, top, 0.0f), DirectX::XMFLOAT4(r, g, b, a), DirectX::XMFLOAT2(1.0f, 0.0f) },
        { DirectX::XMFLOAT3(left, bottom, 0.0f), DirectX::XMFLOAT4(r, g, b, a), DirectX::XMFLOAT2(0.0f, 1.0f) },
        // Triangle 2
        { DirectX::XMFLOAT3(right, top, 0.0f), DirectX::XMFLOAT4(r, g, b, a), DirectX::XMFLOAT2(1.0f, 0.0f) },
        { DirectX::XMFLOAT3(right, bottom, 0.0f), DirectX::XMFLOAT4(r, g, b, a), DirectX::XMFLOAT2(1.0f, 1.0f) },
        { DirectX::XMFLOAT3(left, bottom, 0.0f), DirectX::XMFLOAT4(r, g, b, a), DirectX::XMFLOAT2(0.0f, 1.0f) }
    };
    
    // Create or update vertex buffer
    if (!m_vertexBuffer) {
        D3D11_BUFFER_DESC bufferDesc = {};
        bufferDesc.ByteWidth = sizeof(vertices);
        bufferDesc.Usage = D3D11_USAGE_DYNAMIC;
        bufferDesc.BindFlags = D3D11_BIND_VERTEX_BUFFER;
        bufferDesc.CPUAccessFlags = D3D11_CPU_ACCESS_WRITE;
        
        D3D11_SUBRESOURCE_DATA initData = {};
        initData.pSysMem = vertices;
        
        HRESULT hr = m_device->CreateBuffer(&bufferDesc, &initData, m_vertexBuffer.GetAddressOf());
        if (FAILED(hr)) {
            printf("[D3D11Renderer] Failed to create vertex buffer\n");
            return;
        }
    } else {
        // Update existing vertex buffer
        D3D11_MAPPED_SUBRESOURCE mappedResource;
        HRESULT hr = m_deviceContext->Map(m_vertexBuffer.Get(), 0, D3D11_MAP_WRITE_DISCARD, 0, &mappedResource);
        if (SUCCEEDED(hr)) {
            memcpy(mappedResource.pData, vertices, sizeof(vertices));
            m_deviceContext->Unmap(m_vertexBuffer.Get(), 0);
        }
    }
    
    // Update constant buffer with identity matrix (we're using NDC directly)
    D3D11_MAPPED_SUBRESOURCE mappedResource;
    HRESULT hr = m_deviceContext->Map(m_constantBuffer.Get(), 0, D3D11_MAP_WRITE_DISCARD, 0, &mappedResource);
    if (SUCCEEDED(hr)) {
        ConstantBufferData* cbData = static_cast<ConstantBufferData*>(mappedResource.pData);
        cbData->worldViewProjection = DirectX::XMMatrixIdentity();
        cbData->tintColor = DirectX::XMFLOAT4(1.0f, 1.0f, 1.0f, 1.0f);
        m_deviceContext->Unmap(m_constantBuffer.Get(), 0);
    }
    
    // Bind white texture for solid color rendering
    m_deviceContext->PSSetShaderResources(0, 1, m_whiteTextureSRV.GetAddressOf());
    
    // Set vertex buffer
    UINT stride = sizeof(Vertex);
    UINT offset = 0;
    m_deviceContext->IASetVertexBuffers(0, 1, m_vertexBuffer.GetAddressOf(), &stride, &offset);
    
    // Draw the rectangle
    m_deviceContext->Draw(6, 0);
}

void D3D11Renderer::DrawSprite(int textureId, float x, float y,
                               float width, float height, float rotation) {
    // Find the texture
    auto it = m_textures.find(textureId);
    if (it == m_textures.end()) {
        // Texture not found, use white texture as fallback
        m_deviceContext->PSSetShaderResources(0, 1, m_whiteTextureSRV.GetAddressOf());
    } else {
        // Bind the texture
        m_deviceContext->PSSetShaderResources(0, 1, it->second.shaderResourceView.GetAddressOf());
    }
    
    // Convert screen coordinates to normalized device coordinates (NDC)
    // Calculate center position for rotation
    float centerX = x + width / 2.0f;
    float centerY = y + height / 2.0f;
    
    // Convert to NDC
    float ndcCenterX = (centerX / m_width) * 2.0f - 1.0f;
    float ndcCenterY = 1.0f - (centerY / m_height) * 2.0f;
    float ndcWidth = (width / m_width) * 2.0f;
    float ndcHeight = (height / m_height) * 2.0f;
    
    // Calculate half extents
    float halfWidth = ndcWidth / 2.0f;
    float halfHeight = ndcHeight / 2.0f;
    
    // Create rotation matrix if needed
    DirectX::XMMATRIX rotationMatrix = DirectX::XMMatrixIdentity();
    if (rotation != 0.0f) {
        // Create rotation around Z axis (rotation is in radians)
        rotationMatrix = DirectX::XMMatrixRotationZ(rotation);
    }
    
    // Define sprite corners (centered at origin for rotation)
    DirectX::XMVECTOR corners[4] = {
        DirectX::XMVectorSet(-halfWidth, halfHeight, 0.0f, 1.0f),   // Top-left
        DirectX::XMVectorSet(halfWidth, halfHeight, 0.0f, 1.0f),    // Top-right
        DirectX::XMVectorSet(-halfWidth, -halfHeight, 0.0f, 1.0f),  // Bottom-left
        DirectX::XMVectorSet(halfWidth, -halfHeight, 0.0f, 1.0f)    // Bottom-right
    };
    
    // Apply rotation and translation
    for (int i = 0; i < 4; i++) {
        corners[i] = DirectX::XMVector3Transform(corners[i], rotationMatrix);
        float cornerX = DirectX::XMVectorGetX(corners[i]) + ndcCenterX;
        float cornerY = DirectX::XMVectorGetY(corners[i]) + ndcCenterY;
        corners[i] = DirectX::XMVectorSet(cornerX, cornerY, 0.0f, 1.0f);
    }
    
    // Create vertices for a rectangle (2 triangles = 6 vertices)
    Vertex vertices[] = {
        // Triangle 1
        { DirectX::XMFLOAT3(DirectX::XMVectorGetX(corners[0]), DirectX::XMVectorGetY(corners[0]), 0.0f), 
          DirectX::XMFLOAT4(1.0f, 1.0f, 1.0f, 1.0f), DirectX::XMFLOAT2(0.0f, 0.0f) },
        { DirectX::XMFLOAT3(DirectX::XMVectorGetX(corners[1]), DirectX::XMVectorGetY(corners[1]), 0.0f), 
          DirectX::XMFLOAT4(1.0f, 1.0f, 1.0f, 1.0f), DirectX::XMFLOAT2(1.0f, 0.0f) },
        { DirectX::XMFLOAT3(DirectX::XMVectorGetX(corners[2]), DirectX::XMVectorGetY(corners[2]), 0.0f), 
          DirectX::XMFLOAT4(1.0f, 1.0f, 1.0f, 1.0f), DirectX::XMFLOAT2(0.0f, 1.0f) },
        // Triangle 2
        { DirectX::XMFLOAT3(DirectX::XMVectorGetX(corners[1]), DirectX::XMVectorGetY(corners[1]), 0.0f), 
          DirectX::XMFLOAT4(1.0f, 1.0f, 1.0f, 1.0f), DirectX::XMFLOAT2(1.0f, 0.0f) },
        { DirectX::XMFLOAT3(DirectX::XMVectorGetX(corners[3]), DirectX::XMVectorGetY(corners[3]), 0.0f), 
          DirectX::XMFLOAT4(1.0f, 1.0f, 1.0f, 1.0f), DirectX::XMFLOAT2(1.0f, 1.0f) },
        { DirectX::XMFLOAT3(DirectX::XMVectorGetX(corners[2]), DirectX::XMVectorGetY(corners[2]), 0.0f), 
          DirectX::XMFLOAT4(1.0f, 1.0f, 1.0f, 1.0f), DirectX::XMFLOAT2(0.0f, 1.0f) }
    };
    
    // Create or update vertex buffer
    if (!m_vertexBuffer) {
        D3D11_BUFFER_DESC bufferDesc = {};
        bufferDesc.ByteWidth = sizeof(vertices);
        bufferDesc.Usage = D3D11_USAGE_DYNAMIC;
        bufferDesc.BindFlags = D3D11_BIND_VERTEX_BUFFER;
        bufferDesc.CPUAccessFlags = D3D11_CPU_ACCESS_WRITE;
        
        D3D11_SUBRESOURCE_DATA initData = {};
        initData.pSysMem = vertices;
        
        HRESULT hr = m_device->CreateBuffer(&bufferDesc, &initData, m_vertexBuffer.GetAddressOf());
        if (FAILED(hr)) {
            printf("[D3D11Renderer] Failed to create vertex buffer for sprite\n");
            return;
        }
    } else {
        // Update existing vertex buffer
        D3D11_MAPPED_SUBRESOURCE mappedResource;
        HRESULT hr = m_deviceContext->Map(m_vertexBuffer.Get(), 0, D3D11_MAP_WRITE_DISCARD, 0, &mappedResource);
        if (SUCCEEDED(hr)) {
            memcpy(mappedResource.pData, vertices, sizeof(vertices));
            m_deviceContext->Unmap(m_vertexBuffer.Get(), 0);
        }
    }
    
    // Update constant buffer with identity matrix (we're using NDC directly)
    D3D11_MAPPED_SUBRESOURCE mappedResource;
    HRESULT hr = m_deviceContext->Map(m_constantBuffer.Get(), 0, D3D11_MAP_WRITE_DISCARD, 0, &mappedResource);
    if (SUCCEEDED(hr)) {
        ConstantBufferData* cbData = static_cast<ConstantBufferData*>(mappedResource.pData);
        cbData->worldViewProjection = DirectX::XMMatrixIdentity();
        cbData->tintColor = DirectX::XMFLOAT4(1.0f, 1.0f, 1.0f, 1.0f);
        m_deviceContext->Unmap(m_constantBuffer.Get(), 0);
    }
    
    // Set vertex buffer
    UINT stride = sizeof(Vertex);
    UINT offset = 0;
    m_deviceContext->IASetVertexBuffers(0, 1, m_vertexBuffer.GetAddressOf(), &stride, &offset);
    
    // Draw the sprite
    m_deviceContext->Draw(6, 0);
}

int D3D11Renderer::LoadTexture(const char* filePath) {
    printf("[D3D11Renderer] Loading texture: %s\n", filePath);
    
    // Initialize WIC factory
    ComPtr<IWICImagingFactory> wicFactory;
    HRESULT hr = CoCreateInstance(
        CLSID_WICImagingFactory,
        nullptr,
        CLSCTX_INPROC_SERVER,
        IID_PPV_ARGS(&wicFactory)
    );
    
    if (FAILED(hr)) {
        printf("[D3D11Renderer] Failed to create WIC factory (HRESULT: 0x%08X)\n", hr);
        return -1;
    }
    
    // Convert file path to wide string
    int widePathLen = MultiByteToWideChar(CP_UTF8, 0, filePath, -1, nullptr, 0);
    wchar_t* widePath = new wchar_t[widePathLen];
    MultiByteToWideChar(CP_UTF8, 0, filePath, -1, widePath, widePathLen);
    
    // Create a decoder for the image
    ComPtr<IWICBitmapDecoder> decoder;
    hr = wicFactory->CreateDecoderFromFilename(
        widePath,
        nullptr,
        GENERIC_READ,
        WICDecodeMetadataCacheOnDemand,
        &decoder
    );
    
    delete[] widePath;
    
    if (FAILED(hr)) {
        printf("[D3D11Renderer] Failed to create decoder for texture: %s (HRESULT: 0x%08X)\n", filePath, hr);
        return -1;
    }
    
    // Get the first frame
    ComPtr<IWICBitmapFrameDecode> frame;
    hr = decoder->GetFrame(0, &frame);
    if (FAILED(hr)) {
        printf("[D3D11Renderer] Failed to get frame from texture\n");
        return -1;
    }
    
    // Get image dimensions
    UINT width, height;
    frame->GetSize(&width, &height);
    
    // Create format converter
    ComPtr<IWICFormatConverter> converter;
    hr = wicFactory->CreateFormatConverter(&converter);
    if (FAILED(hr)) {
        printf("[D3D11Renderer] Failed to create format converter\n");
        return -1;
    }
    
    // Convert to RGBA format
    hr = converter->Initialize(
        frame.Get(),
        GUID_WICPixelFormat32bppRGBA,
        WICBitmapDitherTypeNone,
        nullptr,
        0.0,
        WICBitmapPaletteTypeCustom
    );
    
    if (FAILED(hr)) {
        printf("[D3D11Renderer] Failed to initialize format converter\n");
        return -1;
    }
    
    // Calculate stride and image size
    UINT stride = width * 4; // 4 bytes per pixel (RGBA)
    UINT imageSize = stride * height;
    
    // Allocate memory for pixel data
    std::vector<BYTE> pixelData(imageSize);
    
    // Copy pixel data
    hr = converter->CopyPixels(
        nullptr,
        stride,
        imageSize,
        pixelData.data()
    );
    
    if (FAILED(hr)) {
        printf("[D3D11Renderer] Failed to copy pixel data\n");
        return -1;
    }
    
    // Create texture description
    D3D11_TEXTURE2D_DESC textureDesc = {};
    textureDesc.Width = width;
    textureDesc.Height = height;
    textureDesc.MipLevels = 1;
    textureDesc.ArraySize = 1;
    textureDesc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
    textureDesc.SampleDesc.Count = 1;
    textureDesc.SampleDesc.Quality = 0;
    textureDesc.Usage = D3D11_USAGE_DEFAULT;
    textureDesc.BindFlags = D3D11_BIND_SHADER_RESOURCE;
    textureDesc.CPUAccessFlags = 0;
    textureDesc.MiscFlags = 0;
    
    // Create subresource data
    D3D11_SUBRESOURCE_DATA initData = {};
    initData.pSysMem = pixelData.data();
    initData.SysMemPitch = stride;
    initData.SysMemSlicePitch = imageSize;
    
    // Create the texture
    ComPtr<ID3D11Texture2D> texture;
    hr = m_device->CreateTexture2D(&textureDesc, &initData, &texture);
    if (FAILED(hr)) {
        printf("[D3D11Renderer] Failed to create texture 2D (HRESULT: 0x%08X)\n", hr);
        return -1;
    }
    
    // Create shader resource view
    D3D11_SHADER_RESOURCE_VIEW_DESC srvDesc = {};
    srvDesc.Format = textureDesc.Format;
    srvDesc.ViewDimension = D3D11_SRV_DIMENSION_TEXTURE2D;
    srvDesc.Texture2D.MostDetailedMip = 0;
    srvDesc.Texture2D.MipLevels = 1;
    
    ComPtr<ID3D11ShaderResourceView> shaderResourceView;
    hr = m_device->CreateShaderResourceView(texture.Get(), &srvDesc, &shaderResourceView);
    if (FAILED(hr)) {
        printf("[D3D11Renderer] Failed to create shader resource view (HRESULT: 0x%08X)\n", hr);
        return -1;
    }
    
    // Store texture in map
    int textureId = m_nextTextureId++;
    D3D11Texture d3dTexture;
    d3dTexture.texture = texture;
    d3dTexture.shaderResourceView = shaderResourceView;
    d3dTexture.width = width;
    d3dTexture.height = height;
    
    m_textures[textureId] = d3dTexture;
    
    printf("[D3D11Renderer] Successfully loaded texture: %s (ID: %d, Size: %dx%d)\n", 
           filePath, textureId, width, height);
    
    return textureId;
}

void D3D11Renderer::UnloadTexture(int textureId) {
    auto it = m_textures.find(textureId);
    if (it != m_textures.end()) {
        m_textures.erase(it);
    }
}

bool D3D11Renderer::CreateDevice() {
    UINT createDeviceFlags = 0;
#ifdef _DEBUG
    createDeviceFlags |= D3D11_CREATE_DEVICE_DEBUG;
#endif
    
    D3D_FEATURE_LEVEL featureLevels[] = {
        D3D_FEATURE_LEVEL_11_1,
        D3D_FEATURE_LEVEL_11_0,
        D3D_FEATURE_LEVEL_10_1,
        D3D_FEATURE_LEVEL_10_0
    };
    
    D3D_FEATURE_LEVEL featureLevel;
    
    HRESULT hr = D3D11CreateDevice(
        nullptr,                    // Use default adapter
        D3D_DRIVER_TYPE_HARDWARE,   // Hardware acceleration
        nullptr,                    // No software module
        createDeviceFlags,          // Flags
        featureLevels,              // Feature levels
        ARRAYSIZE(featureLevels),   // Number of feature levels
        D3D11_SDK_VERSION,          // SDK version
        m_device.GetAddressOf(),    // Output device
        &featureLevel,              // Output feature level
        m_deviceContext.GetAddressOf() // Output device context
    );
    
    if (FAILED(hr)) {
        printf("[D3D11Renderer] Failed to create D3D11 device (HRESULT: 0x%08X)\n", hr);
        
        // Try with WARP (software) as fallback
        hr = D3D11CreateDevice(
            nullptr,
            D3D_DRIVER_TYPE_WARP,
            nullptr,
            createDeviceFlags,
            featureLevels,
            ARRAYSIZE(featureLevels),
            D3D11_SDK_VERSION,
            m_device.GetAddressOf(),
            &featureLevel,
            m_deviceContext.GetAddressOf()
        );
        
        if (FAILED(hr)) {
            printf("[D3D11Renderer] Failed to create WARP device (HRESULT: 0x%08X)\n", hr);
            return false;
        }
        
        printf("[D3D11Renderer] Using WARP (software) device\n");
    }
    
    printf("[D3D11Renderer] Created D3D11 device with feature level: %d\n", featureLevel);
    return true;
}

bool D3D11Renderer::CreateSwapChain(HWND hwnd) {
    // Get DXGI factory from device
    ComPtr<IDXGIDevice> dxgiDevice;
    HRESULT hr = m_device.As(&dxgiDevice);
    if (FAILED(hr)) {
        printf("[D3D11Renderer] Failed to get DXGI device\n");
        return false;
    }
    
    ComPtr<IDXGIAdapter> dxgiAdapter;
    hr = dxgiDevice->GetAdapter(dxgiAdapter.GetAddressOf());
    if (FAILED(hr)) {
        printf("[D3D11Renderer] Failed to get DXGI adapter\n");
        return false;
    }
    
    ComPtr<IDXGIFactory> dxgiFactory;
    hr = dxgiAdapter->GetParent(__uuidof(IDXGIFactory), &dxgiFactory);
    if (FAILED(hr)) {
        printf("[D3D11Renderer] Failed to get DXGI factory\n");
        return false;
    }
    
    // Create swap chain description
    DXGI_SWAP_CHAIN_DESC swapChainDesc = {};
    swapChainDesc.BufferCount = 2;
    swapChainDesc.BufferDesc.Width = m_width;
    swapChainDesc.BufferDesc.Height = m_height;
    swapChainDesc.BufferDesc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
    swapChainDesc.BufferDesc.RefreshRate.Numerator = 60;
    swapChainDesc.BufferDesc.RefreshRate.Denominator = 1;
    swapChainDesc.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
    swapChainDesc.OutputWindow = hwnd;
    swapChainDesc.SampleDesc.Count = 1;
    swapChainDesc.SampleDesc.Quality = 0;
    swapChainDesc.Windowed = TRUE;
    swapChainDesc.SwapEffect = DXGI_SWAP_EFFECT_DISCARD;
    swapChainDesc.Flags = 0;
    
    hr = dxgiFactory->CreateSwapChain(m_device.Get(), &swapChainDesc, m_swapChain.GetAddressOf());
    if (FAILED(hr)) {
        printf("[D3D11Renderer] Failed to create swap chain (HRESULT: 0x%08X)\n", hr);
        return false;
    }
    
    return true;
}

bool D3D11Renderer::CreateRenderTargetView() {
    // Get back buffer from swap chain
    ComPtr<ID3D11Texture2D> backBuffer;
    HRESULT hr = m_swapChain->GetBuffer(0, __uuidof(ID3D11Texture2D), &backBuffer);
    if (FAILED(hr)) {
        printf("[D3D11Renderer] Failed to get back buffer\n");
        return false;
    }
    
    // Create render target view
    hr = m_device->CreateRenderTargetView(backBuffer.Get(), nullptr, m_renderTargetView.GetAddressOf());
    if (FAILED(hr)) {
        printf("[D3D11Renderer] Failed to create render target view\n");
        return false;
    }
    
    return true;
}

bool D3D11Renderer::CreateDepthStencilBuffer() {
    // Create depth stencil buffer description
    D3D11_TEXTURE2D_DESC depthStencilDesc = {};
    depthStencilDesc.Width = m_width;
    depthStencilDesc.Height = m_height;
    depthStencilDesc.MipLevels = 1;
    depthStencilDesc.ArraySize = 1;
    depthStencilDesc.Format = DXGI_FORMAT_D24_UNORM_S8_UINT;
    depthStencilDesc.SampleDesc.Count = 1;
    depthStencilDesc.SampleDesc.Quality = 0;
    depthStencilDesc.Usage = D3D11_USAGE_DEFAULT;
    depthStencilDesc.BindFlags = D3D11_BIND_DEPTH_STENCIL;
    depthStencilDesc.CPUAccessFlags = 0;
    depthStencilDesc.MiscFlags = 0;
    
    HRESULT hr = m_device->CreateTexture2D(&depthStencilDesc, nullptr, m_depthStencilBuffer.GetAddressOf());
    if (FAILED(hr)) {
        printf("[D3D11Renderer] Failed to create depth stencil buffer\n");
        return false;
    }
    
    // Create depth stencil view
    hr = m_device->CreateDepthStencilView(m_depthStencilBuffer.Get(), nullptr, m_depthStencilView.GetAddressOf());
    if (FAILED(hr)) {
        printf("[D3D11Renderer] Failed to create depth stencil view\n");
        return false;
    }
    
    return true;
}

bool D3D11Renderer::CreateDepthStencilState() {
    D3D11_DEPTH_STENCIL_DESC depthStencilDesc = {};
    depthStencilDesc.DepthEnable = TRUE;
    depthStencilDesc.DepthWriteMask = D3D11_DEPTH_WRITE_MASK_ALL;
    depthStencilDesc.DepthFunc = D3D11_COMPARISON_LESS;
    depthStencilDesc.StencilEnable = FALSE;
    
    HRESULT hr = m_device->CreateDepthStencilState(&depthStencilDesc, m_depthStencilState.GetAddressOf());
    if (FAILED(hr)) {
        printf("[D3D11Renderer] Failed to create depth stencil state\n");
        return false;
    }
    
    m_deviceContext->OMSetDepthStencilState(m_depthStencilState.Get(), 1);
    return true;
}

bool D3D11Renderer::CreateRasterizerState() {
    D3D11_RASTERIZER_DESC rasterizerDesc = {};
    rasterizerDesc.FillMode = D3D11_FILL_SOLID;
    rasterizerDesc.CullMode = D3D11_CULL_BACK;
    rasterizerDesc.FrontCounterClockwise = FALSE;
    rasterizerDesc.DepthBias = 0;
    rasterizerDesc.DepthBiasClamp = 0.0f;
    rasterizerDesc.SlopeScaledDepthBias = 0.0f;
    rasterizerDesc.DepthClipEnable = TRUE;
    rasterizerDesc.ScissorEnable = FALSE;
    rasterizerDesc.MultisampleEnable = FALSE;
    rasterizerDesc.AntialiasedLineEnable = FALSE;
    
    HRESULT hr = m_device->CreateRasterizerState(&rasterizerDesc, m_rasterizerState.GetAddressOf());
    if (FAILED(hr)) {
        printf("[D3D11Renderer] Failed to create rasterizer state\n");
        return false;
    }
    
    m_deviceContext->RSSetState(m_rasterizerState.Get());
    return true;
}

bool D3D11Renderer::CreateBlendState() {
    D3D11_BLEND_DESC blendDesc = {};
    blendDesc.RenderTarget[0].BlendEnable = TRUE;
    blendDesc.RenderTarget[0].SrcBlend = D3D11_BLEND_SRC_ALPHA;
    blendDesc.RenderTarget[0].DestBlend = D3D11_BLEND_INV_SRC_ALPHA;
    blendDesc.RenderTarget[0].BlendOp = D3D11_BLEND_OP_ADD;
    blendDesc.RenderTarget[0].SrcBlendAlpha = D3D11_BLEND_ONE;
    blendDesc.RenderTarget[0].DestBlendAlpha = D3D11_BLEND_ZERO;
    blendDesc.RenderTarget[0].BlendOpAlpha = D3D11_BLEND_OP_ADD;
    blendDesc.RenderTarget[0].RenderTargetWriteMask = D3D11_COLOR_WRITE_ENABLE_ALL;
    
    HRESULT hr = m_device->CreateBlendState(&blendDesc, m_blendState.GetAddressOf());
    if (FAILED(hr)) {
        printf("[D3D11Renderer] Failed to create blend state\n");
        return false;
    }
    
    float blendFactor[4] = { 1.0f, 1.0f, 1.0f, 1.0f };
    m_deviceContext->OMSetBlendState(m_blendState.Get(), blendFactor, 0xFFFFFFFF);
    return true;
}

bool D3D11Renderer::CreateShadersAndInputLayout() {
    HRESULT hr;
    
    // Compile vertex shader
    ComPtr<ID3DBlob> vertexShaderBlob;
    ComPtr<ID3DBlob> errorBlob;
    
    hr = D3DCompile(
        g_vertexShaderSource,
        strlen(g_vertexShaderSource),
        nullptr,
        nullptr,
        nullptr,
        "main",
        "vs_5_0",
        D3DCOMPILE_ENABLE_STRICTNESS,
        0,
        vertexShaderBlob.GetAddressOf(),
        errorBlob.GetAddressOf()
    );
    
    if (FAILED(hr)) {
        if (errorBlob) {
            printf("[D3D11Renderer] Vertex shader compilation error: %s\n", 
                   (char*)errorBlob->GetBufferPointer());
        }
        return false;
    }
    
    // Create vertex shader
    hr = m_device->CreateVertexShader(
        vertexShaderBlob->GetBufferPointer(),
        vertexShaderBlob->GetBufferSize(),
        nullptr,
        m_vertexShader.GetAddressOf()
    );
    
    if (FAILED(hr)) {
        printf("[D3D11Renderer] Failed to create vertex shader\n");
        return false;
    }
    
    // Compile pixel shader
    ComPtr<ID3DBlob> pixelShaderBlob;
    
    hr = D3DCompile(
        g_pixelShaderSource,
        strlen(g_pixelShaderSource),
        nullptr,
        nullptr,
        nullptr,
        "main",
        "ps_5_0",
        D3DCOMPILE_ENABLE_STRICTNESS,
        0,
        pixelShaderBlob.GetAddressOf(),
        errorBlob.GetAddressOf()
    );
    
    if (FAILED(hr)) {
        if (errorBlob) {
            printf("[D3D11Renderer] Pixel shader compilation error: %s\n", 
                   (char*)errorBlob->GetBufferPointer());
        }
        return false;
    }
    
    // Create pixel shader
    hr = m_device->CreatePixelShader(
        pixelShaderBlob->GetBufferPointer(),
        pixelShaderBlob->GetBufferSize(),
        nullptr,
        m_pixelShader.GetAddressOf()
    );
    
    if (FAILED(hr)) {
        printf("[D3D11Renderer] Failed to create pixel shader\n");
        return false;
    }
    
    // Create input layout
    D3D11_INPUT_ELEMENT_DESC inputLayoutDesc[] = {
        { "POSITION", 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 0, D3D11_INPUT_PER_VERTEX_DATA, 0 },
        { "COLOR", 0, DXGI_FORMAT_R32G32B32A32_FLOAT, 0, 12, D3D11_INPUT_PER_VERTEX_DATA, 0 },
        { "TEXCOORD", 0, DXGI_FORMAT_R32G32_FLOAT, 0, 28, D3D11_INPUT_PER_VERTEX_DATA, 0 }
    };
    
    hr = m_device->CreateInputLayout(
        inputLayoutDesc,
        ARRAYSIZE(inputLayoutDesc),
        vertexShaderBlob->GetBufferPointer(),
        vertexShaderBlob->GetBufferSize(),
        m_inputLayout.GetAddressOf()
    );
    
    if (FAILED(hr)) {
        printf("[D3D11Renderer] Failed to create input layout\n");
        return false;
    }
    
    // Set shaders and input layout
    m_deviceContext->VSSetShader(m_vertexShader.Get(), nullptr, 0);
    m_deviceContext->PSSetShader(m_pixelShader.Get(), nullptr, 0);
    m_deviceContext->IASetInputLayout(m_inputLayout.Get());
    m_deviceContext->IASetPrimitiveTopology(D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST);
    
    return true;
}

bool D3D11Renderer::CreateConstantBuffers() {
    D3D11_BUFFER_DESC bufferDesc = {};
    bufferDesc.ByteWidth = sizeof(ConstantBufferData);
    bufferDesc.Usage = D3D11_USAGE_DYNAMIC;
    bufferDesc.BindFlags = D3D11_BIND_CONSTANT_BUFFER;
    bufferDesc.CPUAccessFlags = D3D11_CPU_ACCESS_WRITE;
    
    HRESULT hr = m_device->CreateBuffer(&bufferDesc, nullptr, m_constantBuffer.GetAddressOf());
    if (FAILED(hr)) {
        printf("[D3D11Renderer] Failed to create constant buffer\n");
        return false;
    }
    
    // Set constant buffer
    m_deviceContext->VSSetConstantBuffers(0, 1, m_constantBuffer.GetAddressOf());
    
    return true;
}

bool D3D11Renderer::CreateSamplerState() {
    D3D11_SAMPLER_DESC samplerDesc = {};
    samplerDesc.Filter = D3D11_FILTER_MIN_MAG_MIP_LINEAR;
    samplerDesc.AddressU = D3D11_TEXTURE_ADDRESS_WRAP;
    samplerDesc.AddressV = D3D11_TEXTURE_ADDRESS_WRAP;
    samplerDesc.AddressW = D3D11_TEXTURE_ADDRESS_WRAP;
    samplerDesc.MipLODBias = 0.0f;
    samplerDesc.MaxAnisotropy = 1;
    samplerDesc.ComparisonFunc = D3D11_COMPARISON_ALWAYS;
    samplerDesc.BorderColor[0] = 0.0f;
    samplerDesc.BorderColor[1] = 0.0f;
    samplerDesc.BorderColor[2] = 0.0f;
    samplerDesc.BorderColor[3] = 0.0f;
    samplerDesc.MinLOD = 0.0f;
    samplerDesc.MaxLOD = D3D11_FLOAT32_MAX;
    
    HRESULT hr = m_device->CreateSamplerState(&samplerDesc, m_samplerState.GetAddressOf());
    if (FAILED(hr)) {
        printf("[D3D11Renderer] Failed to create sampler state\n");
        return false;
    }
    
    // Set sampler state
    m_deviceContext->PSSetSamplers(0, 1, m_samplerState.GetAddressOf());
    
    return true;
}

bool D3D11Renderer::CreateWhiteTexture() {
    // Create a 1x1 white texture for solid color rendering
    D3D11_TEXTURE2D_DESC textureDesc = {};
    textureDesc.Width = 1;
    textureDesc.Height = 1;
    textureDesc.MipLevels = 1;
    textureDesc.ArraySize = 1;
    textureDesc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
    textureDesc.SampleDesc.Count = 1;
    textureDesc.SampleDesc.Quality = 0;
    textureDesc.Usage = D3D11_USAGE_DEFAULT;
    textureDesc.BindFlags = D3D11_BIND_SHADER_RESOURCE;
    textureDesc.CPUAccessFlags = 0;
    textureDesc.MiscFlags = 0;
    
    // White pixel data
    UINT whitePixel = 0xFFFFFFFF;
    D3D11_SUBRESOURCE_DATA initData = {};
    initData.pSysMem = &whitePixel;
    initData.SysMemPitch = 4;
    initData.SysMemSlicePitch = 4;
    
    HRESULT hr = m_device->CreateTexture2D(&textureDesc, &initData, m_whiteTexture.GetAddressOf());
    if (FAILED(hr)) {
        printf("[D3D11Renderer] Failed to create white texture\n");
        return false;
    }
    
    // Create shader resource view
    D3D11_SHADER_RESOURCE_VIEW_DESC srvDesc = {};
    srvDesc.Format = textureDesc.Format;
    srvDesc.ViewDimension = D3D11_SRV_DIMENSION_TEXTURE2D;
    srvDesc.Texture2D.MostDetailedMip = 0;
    srvDesc.Texture2D.MipLevels = 1;
    
    hr = m_device->CreateShaderResourceView(m_whiteTexture.Get(), &srvDesc, m_whiteTextureSRV.GetAddressOf());
    if (FAILED(hr)) {
        printf("[D3D11Renderer] Failed to create white texture SRV\n");
        return false;
    }
    
    return true;
}

bool D3D11Renderer::CreateAppWindow(const char* title) {
    WNDCLASSEX wc = {};
    wc.cbSize = sizeof(WNDCLASSEX);
    wc.style = CS_HREDRAW | CS_VREDRAW;
    wc.lpfnWndProc = WindowProc;
    wc.hInstance = GetModuleHandle(nullptr);
    wc.hCursor = LoadCursor(nullptr, IDC_ARROW);
    wc.lpszClassName = L"ChroniclesD3D11WindowClass";
    
    RegisterClassEx(&wc);
    
    // Convert title to wide string
    int titleLen = MultiByteToWideChar(CP_UTF8, 0, title, -1, nullptr, 0);
    wchar_t* wideTitle = new wchar_t[titleLen];
    MultiByteToWideChar(CP_UTF8, 0, title, -1, wideTitle, titleLen);
    
    // Calculate window size to achieve desired client area size
    RECT windowRect = { 0, 0, m_width, m_height };
    AdjustWindowRect(&windowRect, WS_OVERLAPPEDWINDOW, FALSE);
    
    m_hwnd = CreateWindowEx(
        0,
        L"ChroniclesD3D11WindowClass",
        wideTitle,
        WS_OVERLAPPEDWINDOW,
        CW_USEDEFAULT, CW_USEDEFAULT,
        windowRect.right - windowRect.left,
        windowRect.bottom - windowRect.top,
        nullptr,
        nullptr,
        GetModuleHandle(nullptr),
        this
    );
    
    delete[] wideTitle;
    
    if (!m_hwnd) {
        printf("[D3D11Renderer] Failed to create window\n");
        return false;
    }
    
    ShowWindow(m_hwnd, SW_SHOW);
    UpdateWindow(m_hwnd);
    
    return true;
}

LRESULT CALLBACK D3D11Renderer::WindowProc(HWND hwnd, UINT message, WPARAM wParam, LPARAM lParam) {
    D3D11Renderer* renderer = nullptr;
    
    if (message == WM_CREATE) {
        CREATESTRUCT* pCreate = reinterpret_cast<CREATESTRUCT*>(lParam);
        renderer = reinterpret_cast<D3D11Renderer*>(pCreate->lpCreateParams);
        SetWindowLongPtr(hwnd, GWLP_USERDATA, reinterpret_cast<LONG_PTR>(renderer));
    } else {
        renderer = reinterpret_cast<D3D11Renderer*>(GetWindowLongPtr(hwnd, GWLP_USERDATA));
    }
    
    switch (message) {
        case WM_CLOSE:
            if (renderer) {
                renderer->SetRunning(false);
            }
            return 0;
            
        case WM_DESTROY:
            PostQuitMessage(0);
            return 0;
    }
    
    return DefWindowProc(hwnd, message, wParam, lParam);
}

} // namespace Chronicles

#endif // _WIN32
