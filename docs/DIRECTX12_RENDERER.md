# DirectX 12 Rendering Engine

This document describes the DirectX 12 rendering backend implementation for Chronicles of a Drifter.

## Overview

The engine now supports multiple rendering backends through an abstracted interface:
- **SDL2** (default, cross-platform)
- **DirectX 12** (Windows-optimized, high-performance)

## Architecture

### Abstraction Layer

The rendering system uses an `IRenderer` interface that both SDL2 and DirectX 12 implementations inherit from. This allows the engine to switch between backends at runtime without changing the game logic.

```
IRenderer (interface)
├── SDL2Renderer (cross-platform)
└── D3D12Renderer (Windows DirectX 12)
```

### Files

- `src/Engine/IRenderer.h` - Abstract renderer interface
- `src/Engine/SDL2Renderer.h/cpp` - SDL2 implementation
- `src/Engine/D3D12Renderer.h/cpp` - DirectX 12 implementation
- `src/Engine/ChroniclesEngine.cpp` - Updated to use abstracted renderer

## Using DirectX 12

### Selecting the Renderer Backend

By default, the engine uses SDL2. To use DirectX 12 on Windows, set the `CHRONICLES_RENDERER` environment variable:

#### Windows Command Prompt:
```cmd
set CHRONICLES_RENDERER=dx12
cd src/Game
dotnet run -c Release
```

#### Windows PowerShell:
```powershell
$env:CHRONICLES_RENDERER="dx12"
cd src/Game
dotnet run -c Release
```

#### Linux/macOS:
```bash
export CHRONICLES_RENDERER=sdl2  # SDL2 is the only option on non-Windows
cd src/Game
dotnet run -c Release
```

### Build Requirements

#### Windows (DirectX 12)
- Windows 10 or later
- Visual Studio 2022 with C++ Desktop Development workload
- Windows 10 SDK (10.0.19041.0 or later)
- DirectX 12 compatible GPU (most GPUs from 2015+)

#### All Platforms (SDL2)
- CMake 3.20+
- SDL2 development libraries
- C++20 compatible compiler

## DirectX 12 Implementation Details

### Features Implemented

1. **Device Creation**
   - Hardware acceleration with fallback to WARP
   - Debug layer support in debug builds
   - Adapter enumeration and selection

2. **Swap Chain**
   - Double buffering (2 frame buffers)
   - Flip model presentation
   - VSync support

3. **Rendering Pipeline**
   - Root signature for texture and constant binding
   - Graphics pipeline state with alpha blending
   - Vertex and pixel shaders for 2D rendering

4. **Resource Management**
   - Descriptor heaps (RTV, DSV, SRV)
   - Command allocators per frame
   - Command list recording and execution

5. **Synchronization**
   - Fence-based frame synchronization
   - GPU wait operations
   - Frame pacing for smooth rendering

6. **Depth Buffering**
   - 32-bit float depth buffer
   - Depth testing for proper layering

### Current Limitations

The DirectX 12 renderer is a work in progress. Currently implemented:
- ✅ Window creation and management
- ✅ Device and swap chain initialization
- ✅ Basic rendering pipeline setup
- ✅ Clear and present operations
- ⚠️ Rectangle drawing (stub)
- ⚠️ Sprite/texture rendering (stub)
- ⚠️ Texture loading (stub)

### Performance Characteristics

DirectX 12 provides:
- **Lower CPU overhead** compared to SDL2
- **Better GPU utilization** through explicit command lists
- **Reduced draw call overhead** through batching opportunities
- **Advanced features** like async compute and ray tracing (future)

Tradeoffs:
- **More complex** implementation and debugging
- **Windows-only** (not cross-platform)
- **Requires modern GPU** (2015 or later)

## API Compatibility

Both renderers implement the same `IRenderer` interface, ensuring that C# game code works identically regardless of backend:

```cpp
// Common rendering operations work with both backends
Renderer_Clear(0.0f, 0.0f, 0.0f, 1.0f);  // Clear to black
Renderer_DrawSprite(textureId, x, y, w, h, rotation);
Renderer_Present();  // Show the frame
```

## Debugging

### Visual Studio Graphics Debugger

DirectX 12 frames can be captured and analyzed:

1. Debug → Graphics → Start Diagnostics
2. Capture a frame (Print Screen)
3. Analyze draw calls, resources, and pipeline state

### PIX for Windows

Microsoft's advanced GPU profiler:

1. Download PIX from Microsoft
2. Launch and attach to ChroniclesOfADrifter.exe
3. Capture GPU trace
4. Analyze performance bottlenecks

### Debug Layer

Enable the D3D12 debug layer in debug builds:

```cpp
#ifdef _DEBUG
ComPtr<ID3D12Debug> debugController;
D3D12GetDebugInterface(IID_PPV_ARGS(&debugController));
debugController->EnableDebugLayer();
#endif
```

This provides detailed validation messages in the Visual Studio Output window.

## Future Enhancements

Planned improvements for the DirectX 12 renderer:

1. **Complete texture loading** with WIC (Windows Imaging Component)
2. **Sprite batching** for efficient 2D rendering
3. **Constant buffers** for per-draw parameters
4. **Multi-threading** for command list recording
5. **HDR rendering** support
6. **Ray tracing** for advanced lighting (optional)

## Troubleshooting

### "Failed to create D3D12 device"

**Cause:** GPU doesn't support DirectX 12 or drivers are outdated.

**Solution:** 
- Update GPU drivers
- Check GPU compatibility (most 2015+ GPUs support DX12)
- System will fall back to WARP (software renderer)

### "Failed to create swap chain"

**Cause:** Window creation failed or display configuration issue.

**Solution:**
- Check window dimensions are valid
- Ensure display is connected properly
- Try windowed mode instead of fullscreen

### Poor Performance

**Cause:** Running on WARP (software renderer) or debug build.

**Solution:**
- Ensure GPU supports DirectX 12
- Use Release build for performance testing
- Check GPU utilization in Task Manager

## References

- [DirectX 12 Programming Guide](https://docs.microsoft.com/en-us/windows/win32/direct3d12/directx-12-programming-guide)
- [DirectX Graphics Samples](https://github.com/microsoft/DirectX-Graphics-Samples)
- [Learning DirectX 12](https://www.3dgep.com/learning-directx-12-1/)

## Contributing

When working on the DirectX 12 renderer:

1. Test on multiple GPU vendors (NVIDIA, AMD, Intel)
2. Profile performance with PIX
3. Validate with debug layer enabled
4. Ensure SDL2 backend still works (for CI/CD)
5. Document any new features or changes
