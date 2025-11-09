# Map Editor Quick Reference

## Launch Commands

```bash
# Dedicated Map Editor
dotnet run -c Release -- editor

# Visual Demo with Editor Toggle (F1)
dotnet run -c Release -- visual
```

## Essential Controls

### Navigation
- **WASD / Arrows** - Move camera
- **+/-** - Zoom in/out

### Editing
- **Space** - Place selected tile
- **[ ]** - Previous/Next tile
- **0-9** - Quick select tile

### Map Management
- **S** - Save map
- **L** - Load map
- **N** - Clear map
- **G** - Generate terrain

### Interface
- **F1 / ~** - Toggle editor
- **Q / ESC** - Exit

## Default Tiles (Quick Select)

| Key | Tile | Color |
|-----|------|-------|
| 0 | Grass | Green |
| 1 | Dirt | Brown |
| 2 | Stone | Gray |
| 3 | Sand | Yellow |
| 4 | Snow | White |
| 5 | Water | Blue |
| 6 | Wood | Dark Brown |
| 7 | WoodPlank | Light Brown |
| 8 | Cobblestone | Gray |
| 9 | Brick | Red |

## Quick Workflow

### Create New Map
1. Launch editor: `dotnet run -- editor`
2. Clear: Press **N**
3. Select tile: Press **0-9**
4. Place: Press **Space** (repeatedly)
5. Save: Press **S**

### Edit Existing Map
1. Launch editor
2. Load: Press **L**
3. Navigate: **WASD**
4. Edit tiles: **Space** / **[ ]**
5. Save: Press **S**

### Edit in Visual Demo
1. Run: `dotnet run -- visual`
2. Enable editor: Press **F1**
3. Edit while playing
4. Disable editor: Press **F1**

## File Locations

- **Maps**: `assets/maps/*.json`
- **Tilesets**: `assets/tilesets/*.json`
- **Documentation**: `docs/MAP_EDITOR.md`

## Tileset Format (JSON)

```json
{
  "Name": "my_tileset",
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

## Map Format (JSON)

```json
{
  "Name": "my_map",
  "Width": 1024,
  "Height": 30,
  "TileSize": 32,
  "Tiles": [
    {"X": 10, "Y": 5, "Type": "Grass"}
  ]
}
```

## Tips

- **Performance**: Editor runs at 5000+ FPS
- **Autosave**: Save creates timestamped files (no overwrite)
- **Generation**: Press G for new procedural terrain
- **Testing**: Edit in visual mode to test gameplay
- **Collaboration**: Share .json map files

## Troubleshooting

**Tiles not appearing?**
- Verify tile selected (check console)
- Camera must be over terrain
- Press Space to place

**Map won't save?**
- Check `assets/maps/` exists
- Verify write permissions
- Check console for errors

**Can't move camera?**
- Use WASD/Arrow keys
- Camera speed: 400 pixels/second
- Check F1 editor mode is on

## See Also

- Full Documentation: `docs/MAP_EDITOR.md`
- How to Play: `HOW_TO_PLAY.md`
- Architecture: `docs/ARCHITECTURE.md`
