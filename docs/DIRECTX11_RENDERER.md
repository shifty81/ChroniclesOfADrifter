# DirectX 11 Rendering Engine

This document describes the DirectX 11 rendering backend implementation for Chronicles of a Drifter.

## Overview

The engine now supports three rendering backends through an abstracted interface:
- **SDL2** (default, cross-platform)
- **DirectX 11** (Windows, broad hardware compatibility)
- **DirectX 12** (Windows, high-performance, latest hardware)

## Why DirectX 11?

DirectX 11 provides a balanced solution between modern features and hardware compatibility:

### Advantages
- **Broad Hardware Support**: Works on GPUs from 2010+ (most Windows gaming PCs)
- **Simpler than DirectX 12**: Higher-level API makes development and debugging easier
- **Excellent Performance**: Still offers great performance for 2D games
- **Wide Compatibility**: Runs on Windows 7, 8, 10, and 11
- **Feature Rich**: Sufficient features for 2D rendering needs
- **Industry Standard**: Well-documented with many examples and tools

### Compared to DirectX 12
- **Easier to Use**: Less boilerplate code, more automatic resource management
- **Better Compatibility**: Supports older hardware and operating systems
- **Faster Development**: Simpler API allows for quicker iteration
- **Good Performance**: More than adequate for 2D games like Chronicles of a Drifter

### Compared to SDL2
- **Better Windows Performance**: Direct access to GPU features
- **Native Integration**: Better integration with Windows graphics stack
- **Advanced Features**: Access to compute shaders, tessellation, etc.

## Architecture

### Abstraction Layer

The rendering system uses an `IRenderer` interface that all implementations inherit from:

```
IRenderer (interface)
├── SDL2Renderer (cross-platform)
├── D3D11Renderer (Windows DirectX 11)
└── D3D12Renderer (Windows DirectX 12)
```

### Files

- `src/Engine/IRenderer.h` - Abstract renderer interface
- `src/Engine/SDL2Renderer.h/cpp` - SDL2 implementation
- `src/Engine/D3D11Renderer.h/cpp` - DirectX 11 implementation
- `src/Engine/D3D12Renderer.h/cpp` - DirectX 12 implementation
- `src/Engine/ChroniclesEngine.cpp` - Updated to use abstracted renderer

## Using DirectX 11

### Selecting the Renderer Backend

By default, the engine uses SDL2. To use DirectX 11 on Windows, set the `CHRONICLES_RENDERER` environment variable:

#### Windows Command Prompt:
```cmd
set CHRONICLES_RENDERER=dx11
cd src/Game
dotnet run -c Release
```

#### Windows PowerShell:
```powershell
$env:CHRONICLES_RENDERER="dx11"
cd src/Game
dotnet run -c Release
```

#### Linux/macOS:
```bash
# DirectX 11 is Windows-only, SDL2 is the only option
export CHRONICLES_RENDERER=sdl2
cd src/Game
dotnet run -c Release
```

### Build Requirements

#### Windows (DirectX 11)
- Windows 7 or later (Windows 10+ recommended)
- Visual Studio 2022 with C++ Desktop Development workload
- Windows SDK (any version with DirectX 11 support)
- DirectX 11 compatible GPU (most GPUs from 2010+)

#### All Platforms (SDL2)
- CMake 3.20+
- SDL2 development libraries
- C++20 compatible compiler

## DirectX 11 Implementation Details

### Features Implemented

1. **Device Creation**
   - Hardware acceleration with fallback to WARP (software rendering)
   - Debug layer support in debug builds
   - Automatic feature level selection (11.1, 11.0, 10.1, 10.0)

2. **Swap Chain**
   - Double buffering (2 frame buffers)
   - Standard discard presentation model
   - VSync support
   - Configurable windowed/fullscreen modes

3. **Rendering Pipeline**
   - Vertex and pixel shaders for 2D rendering
   - Input layout for position, color, and texture coordinates
   - Constant buffers for transformation matrices
   - Alpha blending for transparency

4. **Resource Management**
   - Render target views
   - Depth stencil buffer and view
   - Texture resources and shader resource views
   - Vertex and index buffers for dynamic geometry

5. **Render States**
   - Depth stencil state for depth testing
   - Rasterizer state for culling and fill mode
   - Blend state for alpha blending
   - Sampler states for texture filtering

6. **Window Management**
   - Win32 window creation and management
   - Message pump for input and window events
   - Proper cleanup and resource release

### Pipeline Flow

```
Initialization
    ↓
Create Device → Create Swap Chain → Create Render Target View
    ↓                                        ↓
Create Depth Buffer → Create States → Create Shaders → Set Viewport
    ↓
Render Loop:
    BeginFrame (Process Messages)
        ↓
    Clear (Clear render target and depth buffer)
        ↓
    Draw Calls (Set shaders, bind resources, draw)
        ↓
    Present (Swap buffers)
```

### Shader System

The DirectX 11 renderer includes inline HLSL shaders:

**Vertex Shader**:
- Transforms vertices using world-view-projection matrix
- Passes through color and texture coordinates
- Applies tint color from constant buffer

**Pixel Shader**:
- Samples texture using provided coordinates
- Multiplies texture color by vertex color
- Outputs final blended color

### Current Status

✅ **Implemented**:
- Device and swap chain creation
- Render target and depth buffer setup
- Shader compilation and pipeline creation
- State management (depth, rasterizer, blend)
- Window creation and message handling
- Frame management (BeginFrame, Clear, Present)
- Error handling and fallback to software rendering

⚠️ **Stub/TODO**:
- Texture loading from files (WIC integration needed)
- DrawRect implementation with vertex buffer generation
- DrawSprite implementation with textured quad rendering
- Sprite batching for optimized rendering

## Performance Characteristics

### DirectX 11 Advantages
- **Lower CPU Overhead**: Direct GPU access without SDL abstraction layer
- **Better GPU Utilization**: Native driver communication
- **Feature Rich**: Access to compute shaders, advanced blending, etc.
- **Mature API**: Well-optimized drivers across all hardware vendors

### Comparison to Other Backends

| Feature | SDL2 | DirectX 11 | DirectX 12 |
|---------|------|------------|------------|
| Cross-Platform | ✅ Yes | ❌ Windows Only | ❌ Windows Only |
| Hardware Support | All | 2010+ GPUs | 2015+ GPUs |
| Performance (2D) | Good | Excellent | Excellent |
| Development Speed | Fast | Medium | Slow |
| Debugging | Easy | Medium | Hard |
| Code Complexity | Low | Medium | High |

## Debugging

### Visual Studio Graphics Debugger

1. Run the game with DirectX 11 backend
2. In Visual Studio, go to Debug → Graphics → Start Graphics Debugging
3. Capture frames with Print Screen key
4. Analyze pipeline stages, shaders, and resources

### Debug Layer

The DirectX 11 debug layer is enabled in debug builds:
```cpp
#ifdef _DEBUG
    createDeviceFlags |= D3D11_CREATE_DEVICE_DEBUG;
#endif
```

This provides:
- Validation of API calls
- Warnings about performance issues
- Detailed error messages
- Memory leak detection

### Common Issues

1. **Device Creation Fails**
   - Check that DirectX 11 runtime is installed
   - Verify GPU driver is up to date
   - Check debug output for specific error codes

2. **Black Screen**
   - Verify shaders compiled successfully
   - Check that render target view is properly set
   - Ensure viewport is configured correctly

3. **Low Performance**
   - Check that hardware device is being used (not WARP)
   - Profile with Visual Studio or PIX
   - Look for excessive state changes or draw calls

## Future Enhancements

### Short-Term
1. ✅ Complete texture loading with WIC
2. ✅ Implement DrawRect with vertex buffer generation
3. ✅ Implement DrawSprite with textured quad rendering
4. ✅ Add sprite batching for efficient rendering

### Medium-Term
1. Vertex buffer pooling and reuse
2. Multiple render targets for post-processing
3. Compute shaders for particle systems
4. Multi-threaded rendering support

### Long-Term
1. HDR rendering support
2. Advanced post-processing effects
3. Integration with Windows composition
4. Performance profiling and optimization tools

## Best Practices

1. **Use DirectX 11 for Windows Release Builds**
   - Better performance than SDL2 on Windows
   - Easier to debug than DirectX 12
   - Broad hardware compatibility

2. **Keep SDL2 for Development**
   - Cross-platform testing
   - Faster iteration during development
   - Simpler debugging

3. **Test on Multiple Hardware**
   - Test on both AMD and NVIDIA GPUs
   - Test on integrated graphics (Intel)
   - Test on older hardware with feature level 10

4. **Monitor Performance**
   - Use frame timing to detect issues
   - Profile with Visual Studio or PIX
   - Check for memory leaks with debug layer

## Technical Specifications

### DirectX 11 Features Used
- Feature Level: 11.1, 11.0, 10.1, or 10.0 (automatic selection)
- Shader Model: 5.0
- Texture Format: R8G8B8A8_UNORM
- Depth Format: D24_UNORM_S8_UINT
- Swap Effect: Discard
- Buffer Count: 2 (double buffering)

### System Requirements
- **OS**: Windows 7 SP1 or later
- **GPU**: Any DirectX 11 capable GPU (2010+)
- **RAM**: 2 GB minimum
- **Storage**: 10 MB for runtime libraries

## References

- [DirectX 11 Programming Guide](https://docs.microsoft.com/en-us/windows/win32/direct3d11/dx-graphics-overviews)
- [HLSL Language Reference](https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-reference)
- [Visual Studio Graphics Debugger](https://docs.microsoft.com/en-us/visualstudio/debugger/graphics/overview-of-visual-studio-graphics-diagnostics)

---

**Implementation Date**: November 9, 2025  
**Lines of Code**: ~650 (header + implementation)  
**Dependencies**: d3d11.lib, dxgi.lib, d3dcompiler.lib  
**Tested On**: Linux (compilation only, runtime requires Windows)
