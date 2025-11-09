# Map Editor Documentation

## Overview

The Chronicles of a Drifter Map Editor is an in-game real-time scene editing tool that allows you to:
- Create custom maps and scenes
- Edit procedurally generated terrain
- Place and remove tiles with visual feedback
- Save and load custom maps
- Use drag-and-drop style tileset support
- Test maps immediately in the game engine

## Getting Started

### Launching the Editor

```bash
cd src/Game
dotnet run -c Release -- editor
```

The editor will open in a graphical window (1280x720) with the DirectX 11 renderer by default.

### First Steps

1. The editor starts with a procedurally generated terrain
2. Use WASD or Arrow keys to move the camera around
3. Press Space to place the selected tile at the camera center
4. Use [ and ] to cycle through available tiles
5. Press G to generate new random terrain

## Controls Reference

### Camera Movement
- **WASD** or **Arrow Keys** - Move camera around the map
- **+/-** - Zoom in/out (camera zoom)
- Camera moves at 400 pixels/second

### Tile Editing
- **Space** - Place selected tile at cursor (camera center)
- **[** - Select previous tile in palette
- **]** - Select next tile in palette
- **0-9** - Quick select tile by number (0-9 in the tile list)

### Map Management
- **Ctrl+S** / **S** - Save current map to `assets/maps/map_TIMESTAMP.json`
- **Ctrl+L** / **L** - Load most recent map from `assets/maps/`
- **Ctrl+N** / **N** - New map (clear all tiles)
- **G** - Generate new procedurally generated terrain

### Editor Interface
- **F1** or **~** (Tilde) - Toggle editor UI on/off
- **Q** or **ESC** - Exit editor

## Available Tiles

The default tileset includes:

| # | Tile | Description | Category |
|---|------|-------------|----------|
| 0 | Grass | Green grass terrain | Terrain |
| 1 | Dirt | Brown dirt path | Terrain |
| 2 | Stone | Gray stone wall | Terrain |
| 3 | Sand | Yellow sand | Terrain |
| 4 | Snow | White snow | Terrain |
| 5 | Water | Blue water | Terrain |
| 6 | Wood | Brown wood planks | Building |
| 7 | WoodPlank | Light brown wood | Building |
| 8 | Cobblestone | Gray cobblestone | Building |
| 9 | Brick | Red brick | Building |
| 10+ | CoalOre, IronOre, GoldOre, Torch | Resources and lighting |

## Tileset System

### Default Tileset

The editor comes with a built-in default tileset that includes basic terrain and building materials.

### Loading Custom Tilesets

Place tileset JSON files in `assets/tilesets/` and they will be automatically loaded when the editor starts.

Example tileset structure:

```json
{
  "Name": "my_tileset",
  "Description": "My custom tileset",
  "TileSize": 32,
  "Tiles": {
    "grass": {
      "Name": "grass",
      "DisplayName": "Grass",
      "Color": [0.13, 0.65, 0.13],
      "IsCollidable": false,
      "Category": "terrain"
    }
  }
}
```

### Zelda-Style Tileset

A pre-made Zelda: A Link to the Past inspired tileset is included:
- Location: `assets/tilesets/zelda_style.json`
- Includes dungeon tiles, water, grass variations, and decorations
- 18 different tile types optimized for action-adventure games

## Map File Format

Maps are saved in JSON format in `assets/maps/`:

```json
{
  "Name": "Map_20250109_143022",
  "Width": 1024,
  "Height": 30,
  "TileSize": 32,
  "Tiles": [
    {
      "X": 10,
      "Y": 15,
      "Type": "Grass"
    }
  ]
}
```

### Map Properties
- **Name**: Unique map identifier with timestamp
- **Width**: Map width in tiles (dynamically based on loaded chunks)
- **Height**: Map height in tiles (always 30 for current system)
- **TileSize**: Size of each tile in pixels (32)
- **Tiles**: Array of placed tiles (only non-air tiles are saved)

## Workflow

### Creating a New Map

1. Launch editor: `dotnet run -c Release -- editor`
2. Clear existing terrain: Press **N**
3. Select a tile: Press **0-9** or use **[ ]**
4. Move camera: Use **WASD**
5. Place tiles: Press **Space** repeatedly
6. Save map: Press **S**

### Editing Generated Terrain

1. Launch editor (starts with generated terrain)
2. Move camera to area you want to edit
3. Select tile to place
4. Press Space to replace tiles
5. Save when done

### Loading and Modifying Existing Maps

1. Launch editor
2. Press **L** to load most recent map
3. Make changes using tile placement
4. Save with **S** (creates new timestamped file)

### Procedural Generation Integration

The editor integrates with the procedural generation system:
- Press **G** to generate completely new terrain
- Each generation uses a random seed
- Generated terrain includes all biomes, ores, and caves
- Generated terrain can be edited tile-by-tile

## Tips and Best Practices

### Performance
- The editor runs at high FPS (5000+ with DirectX 11/12)
- Chunk loading is automatic as you move the camera
- Only modified chunks need to be saved

### Map Design
- Start with a base using procedural generation (Press **G**)
- Manually refine key areas (entrances, paths, structures)
- Use different tile types for visual variety
- Test your map by exiting and playing it in the game

### Tile Placement Strategy
- Place collidable tiles (stone, walls) for boundaries
- Use non-collidable tiles (grass, dirt) for walkable areas
- Layer decorations (flowers, bushes) on top of base terrain
- Create paths with dirt or sand tiles

### Saving Best Practices
- Save frequently (Press **S**)
- Each save creates a timestamped file (no overwrites)
- Maps are stored in `assets/maps/`
- Backup important maps to another location

## Advanced Features

### Custom Tilesets

Create your own tileset JSON file:

1. Create file in `assets/tilesets/your_tileset.json`
2. Follow the tileset JSON format (see example above)
3. Restart editor to load new tileset
4. Tileset will appear in available tilesets

### Tileset Properties

- **Color**: RGB values (0.0 to 1.0) for visual representation
- **TexturePath**: Path to texture file (if using sprites)
- **TextureX/Y**: Position in sprite sheet
- **IsCollidable**: Whether entities can pass through
- **Category**: Organizational category (terrain, building, etc.)

### Extending the Editor

The editor is designed to be extended:
- Add new tile types in `TileType` enum
- Register new tiles in `TilesetManager.CreateDefaultTileset()`
- Implement custom tile behaviors in `TerrainRenderingSystem`
- Add new editor tools in `MapEditorScene.HandleEditorInput()`

## Troubleshooting

### Editor Won't Launch
- Ensure .NET 9 SDK is installed
- Check that DirectX 11 is available (Windows)
- Try SDL2 renderer: `set CHRONICLES_RENDERER=sdl2`

### Tiles Not Appearing
- Verify tile is selected (check console output)
- Ensure camera is positioned over terrain
- Check that Space key is being pressed
- Verify chunk is loaded at current position

### Map Won't Save
- Check that `assets/maps/` directory exists
- Verify write permissions
- Check console for error messages
- Ensure disk space is available

### Map Won't Load
- Verify map file exists in `assets/maps/`
- Check JSON file is valid format
- Ensure tile types in map match available tiles
- Check console for loading errors

## Keyboard Shortcuts Summary

| Key | Action |
|-----|--------|
| WASD / Arrows | Move camera |
| Space | Place tile |
| [ / ] | Previous/Next tile |
| 0-9 | Quick select tile |
| S | Save map |
| L | Load map |
| N | New map (clear) |
| G | Generate terrain |
| F1 / ~ | Toggle UI |
| Q / ESC | Exit |

## Integration with Game

Maps created in the editor can be loaded in the main game:

1. Create and save map in editor
2. Note the map filename
3. Load map in your game scene using `LoadMapFromFile()`
4. The saved map replaces procedurally generated terrain

Example code to load a map in a game scene:
```csharp
var mapEditor = new MapEditorScene();
mapEditor.LoadMapFromFile("assets/maps/map_20250109_143022.json");
```

## Future Enhancements

Planned features for future versions:
- Visual tile palette on screen
- Brush sizes (1x1, 2x2, 3x3)
- Fill tool for large areas
- Copy/paste regions
- Undo/redo support
- Multi-layer editing (background, foreground, collision)
- Sprite-based tiles (not just colors)
- Entity placement (enemies, items, NPCs)
- Trigger and event system
- Testing mode (play in editor)

## Support

For issues or questions:
- Check console output for error messages
- Review this documentation
- Check GitHub issues
- Create new issue with reproduction steps

---

**Chronicles of a Drifter Map Editor** - Real-time scene editing for 2D action RPGs
