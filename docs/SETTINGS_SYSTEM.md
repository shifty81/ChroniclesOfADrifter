# Game Settings System

## Overview

The Chronicles of a Drifter game includes a comprehensive settings system that allows users to customize various aspects of the game, including the renderer backend.

## Windows-Only Configuration

This project is configured for **Windows-only** builds with **DirectX 11 as the default renderer**. This ensures:
- Broad hardware compatibility
- Optimal performance on Windows
- Easy deployment without external dependencies

## Renderer Settings

### Default Configuration
- **Platform:** Windows 10/11
- **Default Renderer:** DirectX 11
- **Alternative Renderers:** DirectX 12 (high-performance), SDL2 (optional, cross-platform)

### Changing Renderer Backend

#### Method 1: Settings File (settings.json)
The game uses a `settings.json` file located in the game directory. You can manually edit this file to change the renderer:

```json
{
  "renderer": {
    "backend": "dx11",
    "windowWidth": 1920,
    "windowHeight": 1080,
    "vsync": true,
    "fullscreen": false
  }
}
```

Valid backend values:
- `dx11` - DirectX 11 (default, broad compatibility)
- `dx12` - DirectX 12 (high-performance, requires compatible GPU)
- `sdl2` - SDL2 renderer (optional, for cross-platform testing)

#### Method 2: Environment Variable
You can override the renderer at runtime using the `CHRONICLES_RENDERER` environment variable:

**Windows Command Prompt:**
```cmd
set CHRONICLES_RENDERER=dx12
dotnet run -c Release
```

**Windows PowerShell:**
```powershell
$env:CHRONICLES_RENDERER="dx12"
dotnet run -c Release
```

#### Method 3: In-Game Settings Menu (Future)
The game will include an in-game settings menu where users can change the renderer. When changing the renderer:
1. User selects new renderer in settings menu
2. Settings are saved to `settings.json`
3. Game automatically restarts with the new renderer

## Settings Manager API

The `SettingsManager` class provides the following functionality:

### Loading Settings
```csharp
var settings = SettingsManager.LoadSettings();
Console.WriteLine($"Current renderer: {settings.Renderer.Backend}");
```

### Saving Settings
```csharp
settings.Renderer.Backend = "dx12";
SettingsManager.SaveSettings(settings);
```

### Applying Renderer Settings
```csharp
SettingsManager.ApplyRendererSettings(settings);
// This sets the CHRONICLES_RENDERER environment variable
```

### Changing Renderer (with restart)
```csharp
SettingsManager.ChangeRendererBackend("dx12");
// This saves settings and notifies user to restart
```

## Configuration Options

### Renderer Settings
- **backend**: Renderer type (dx11, dx12, sdl2)
- **windowWidth**: Window width in pixels (default: 1920)
- **windowHeight**: Window height in pixels (default: 1080)
- **vsync**: Enable vertical sync (default: true)
- **fullscreen**: Run in fullscreen mode (default: false)

### Graphics Settings
- **quality**: Graphics quality preset (low, medium, high, ultra)
- **antialiasing**: Enable antialiasing (default: true)
- **shadows**: Enable shadow rendering (default: true)

### Audio Settings
- **masterVolume**: Master volume level (0.0 - 1.0)
- **musicVolume**: Background music volume (0.0 - 1.0)
- **sfxVolume**: Sound effects volume (0.0 - 1.0)

### Gameplay Settings
- **difficulty**: Game difficulty (easy, normal, hard)
- **showTutorials**: Display tutorial messages (default: true)

## Platform Requirements

### Windows (Primary Platform)
- Windows 10 or Windows 11
- DirectX 11 (included with Windows)
- DirectX 12 (optional, for high-performance rendering)
- Visual Studio 2022 with C++ Desktop Development workload
- .NET 9 SDK

### Build System
- CMake 3.20 or later
- MSVC compiler (Visual Studio 2022)

### Testing
All testing should be performed on Windows with DirectX 11 as the baseline. Testing with DirectX 12 is recommended for high-performance validation.

## Implementation Notes

1. **Default Behavior**: On Windows, DirectX 11 is automatically selected as the default renderer if no configuration is present.

2. **Settings Persistence**: Settings are saved in `settings.json` in the game directory. This file is created automatically with default values on first run.

3. **Renderer Changes**: Changing the renderer backend requires a game restart to properly initialize the new rendering system.

4. **Platform Enforcement**: The build system enforces Windows-only builds through CMake. Attempting to build on non-Windows platforms will result in a configuration error.

5. **Backward Compatibility**: The environment variable `CHRONICLES_RENDERER` continues to work and takes precedence over settings.json for quick testing.

## Future Enhancements

The following features are planned for future updates:

1. **In-Game Settings Menu**: Full UI for changing settings without editing JSON files
2. **Graphics Presets**: Quick selection of graphics quality presets (low, medium, high, ultra)
3. **Resolution Scaling**: Dynamic resolution adjustment for performance
4. **Advanced Graphics Options**: More granular control over rendering features
5. **Profile Management**: Multiple settings profiles for different use cases
6. **Cloud Settings Sync**: Optional cloud-based settings synchronization

## Troubleshooting

### Renderer Not Changing
1. Verify settings.json has the correct backend value
2. Ensure environment variable is not overriding settings
3. Check that the selected renderer is supported on your system

### DirectX 11 Not Available
- DirectX 11 is included with Windows 10/11 by default
- Update Windows to ensure latest DirectX runtime is installed

### DirectX 12 Not Working
- Ensure GPU supports DirectX 12
- Update graphics drivers to latest version
- Check Windows 10 version (requires Windows 10 1809 or later)

### SDL2 Renderer Issues
- SDL2 is optional and requires separate installation
- Used primarily for cross-platform testing
- Not recommended as primary renderer on Windows

## See Also

- [Build Setup](BUILD_SETUP.md) - Build instructions and setup guide
- [Visual Studio 2022 Setup](VS2022_SETUP.md) - VS2022 debugging guide
- [DirectX 11 Renderer](DIRECTX11_RENDERER.md) - DirectX 11 implementation details
- [DirectX 12 Renderer](DIRECTX12_RENDERER.md) - DirectX 12 implementation details
