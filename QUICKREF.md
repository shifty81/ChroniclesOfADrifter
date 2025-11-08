# Quick Reference Card

## Building Locally

```bash
# Quick build (recommended for local development)
./build.sh        # Linux/macOS
build.bat         # Windows

# Run the game
cd src/Game
dotnet run -c Release

# Run specific demos
dotnet run -c Release -- mining      # Mining/building demo
dotnet run -c Release -- collision   # Collision demo
dotnet run -c Release -- terrain     # Terrain demo
```

## Player Scale Reference

**The player character is 2.5 blocks tall** (80 pixels at 32px/block)

This is the fundamental scale for all game content. When creating or generating content:

```csharp
using ChroniclesOfADrifter;

// Use these constants
GameConstants.PlayerHeightInBlocks    // 2.5 blocks
GameConstants.PlayerWidthInBlocks     // 0.8 blocks
GameConstants.BlockSize               // 32 pixels
GameConstants.PlayerCollisionHeight   // 80 pixels
GameConstants.PlayerCollisionWidth    // 26 pixels
```

### Quick Scale Guidelines

| Feature | Height/Size | Notes |
|---------|-------------|-------|
| Player | 2.5 blocks | Base reference |
| Doors | 3+ blocks | Player + headroom |
| Tunnels/Caves | 3+ blocks | Comfortable navigation |
| Trees | 4-8 blocks | Visual variety |
| Room Ceilings | 4 blocks | Spacious feel |
| Small Enemies | 0.5-1.5 blocks | Goblins, rats |
| Human NPCs | 2-3 blocks | Guards, merchants |
| Boss Enemies | 8-16 blocks | Dragons, giants |

## Key Documentation

- **[SCALE_REFERENCE.md](docs/SCALE_REFERENCE.md)** - Comprehensive scale guide with examples
- **[BUILD_SETUP.md](docs/BUILD_SETUP.md)** - Detailed build instructions
- **[TERRAIN_GENERATION.md](docs/TERRAIN_GENERATION.md)** - Terrain system with scale reference
- **[PROCEDURAL_GENERATION.md](docs/PROCEDURAL_GENERATION.md)** - Dungeon generation with guidelines

## Development Status

🔧 **Active Local Development** - Focus on rapid iteration and debugging
- No CI/CD pipelines configured yet (by design)
- Build locally with provided scripts
- All systems functional and ready for prototyping

## Project Structure

```
ChroniclesOfADrifter/
├── src/
│   ├── Engine/              # C++ native engine
│   └── Game/                # C# game logic
│       ├── GameConstants.cs # ⭐ Scale constants here
│       ├── ECS/             # Entity Component System
│       ├── Scenes/          # Game scenes/demos
│       └── World/           # Terrain, chunks, generation
├── docs/                    # Documentation
├── build.sh / build.bat    # Build scripts
└── README.md               # Full project README
```

## Quick Tips

✅ **DO:**
- Use `GameConstants` for all scale-related values
- Reference player height (2.5 blocks) when generating content
- Build locally with `build.sh` or `build.bat`
- Check `SCALE_REFERENCE.md` for guidelines

❌ **DON'T:**
- Hardcode dimensions (use GameConstants instead)
- Ignore player scale in procedural generation
- Create doorways less than 3 blocks tall
- Add arbitrary scale values without consulting GameConstants

## Need Help?

- Check documentation in `docs/` directory
- See `BUILD_SETUP.md` for detailed build instructions
- Reference `SCALE_REFERENCE.md` for scale questions
- Open an issue for bugs or feature requests
