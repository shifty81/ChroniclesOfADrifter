# Maps Directory

This directory contains saved map files created with the Chronicles of a Drifter map editor.

## What are Map Files?

Map files are JSON documents that store the layout of tiles in a game scene. They allow you to:
- Save your custom maps
- Load and edit existing maps
- Share maps with other players
- Version control your level designs
- Backup important areas

## File Format

Maps are saved in JSON format with the following structure:

```json
{
  "Name": "map_name",
  "Width": 1024,
  "Height": 30,
  "TileSize": 32,
  "Tiles": [
    {"X": 10, "Y": 5, "Type": "Grass"},
    {"X": 11, "Y": 5, "Type": "Stone"}
  ]
}
```

### Properties

- **Name**: Unique identifier for the map (auto-generated with timestamp)
- **Width**: Map width in tiles (based on loaded chunks)
- **Height**: Map height in tiles (always 30 in current system)
- **TileSize**: Size of each tile in pixels (always 32)
- **Tiles**: Array of tile placements (only non-air tiles are saved)

### Tile Properties

Each tile in the array has:
- **X**: Horizontal position in tile coordinates
- **Y**: Vertical position in tile coordinates (0 = top, 29 = bottom)
- **Type**: Tile type name (must match TileType enum)

## Included Maps

### `example_small_area.json`
A small example map demonstrating basic map structure:
- 5x5 grass area
- Dirt path through the center
- Stone walls around perimeter
- Wooden door entrance
- Small water pond nearby

Perfect for understanding the map format and testing the editor.

## Creating Maps

### Using the Map Editor

1. Launch editor:
   ```bash
   dotnet run -c Release -- editor
   ```

2. Create your map:
   - Move camera with WASD
   - Select tiles with 0-9 or [ ]
   - Place tiles with Space
   - Build your design

3. Save the map:
   - Press **S** to save
   - Map saved to `assets/maps/map_TIMESTAMP.json`
   - Timestamp format: YYYYMMDD_HHMMSS

### Editing Existing Maps

1. Launch editor
2. Press **L** to load most recent map
3. Make changes
4. Press **S** to save (creates new file)

## Loading Maps in Game

To load a map in your game code:

```csharp
var mapEditor = new MapEditorScene();
mapEditor.LoadMapFromFile("assets/maps/example_small_area.json");
```

Or integrate into your scene:

```csharp
public override void OnLoad()
{
    // ... initialize scene ...
    
    // Load map
    LoadMapFromFile("assets/maps/my_custom_map.json");
}
```

## Map Coordinate System

- **X-axis**: Horizontal, increases to the right
- **Y-axis**: Vertical, 0 at top, 29 at bottom
- **Tile coordinates**: Integer grid positions
- **World coordinates**: Multiply by 32 (tile size)

Example conversions:
- Tile (10, 5) = World (320, 160)
- Tile (0, 0) = World (0, 0)
- Tile (31, 29) = World (992, 928)

## File Naming Convention

Maps saved by the editor use automatic naming:
```
map_YYYYMMDD_HHMMSS.json
```

Examples:
- `map_20250109_143022.json` - Saved Jan 9, 2025 at 2:30:22 PM
- `map_20250110_091545.json` - Saved Jan 10, 2025 at 9:15:45 AM

This prevents accidental overwrites and maintains version history.

## Best Practices

### Organization
- Use descriptive names for custom maps
- Keep map files in this directory
- Create subdirectories for large projects
- Document special features in comments (add Description field)

### Versioning
- Keep old versions for rollback
- Use git to track changes
- Tag important milestones
- Note changes in commit messages

### Performance
- Larger maps (more tiles) take longer to load
- Empty areas (air tiles) are not saved
- Optimize by removing unnecessary tiles
- Test map load times during development

### Sharing
- Include tileset requirements in documentation
- Test maps on fresh install
- Provide screenshots
- Document special mechanics or triggers

## Troubleshooting

**Map won't load?**
- Check JSON syntax with validator
- Ensure all tile types exist in TileType enum
- Verify file path is correct
- Check console for error messages

**Tiles missing after load?**
- Tile types must match exactly (case-sensitive)
- Ensure tilesets are loaded
- Check tile coordinates are within bounds (0-29 for Y)
- Verify map format matches specification

**Map file too large?**
- Remove redundant tiles (use default terrain instead)
- Split into multiple smaller maps
- Use procedural generation for large areas
- Save only important structures

**Changes not saving?**
- Check write permissions
- Verify disk space
- Ensure maps directory exists
- Check console for save errors

## Advanced Features

### Manual Editing

Maps are plain JSON, so you can edit them manually:

1. Open map file in text editor
2. Add/modify tile entries
3. Save file
4. Load in editor to test

Useful for:
- Bulk changes
- Precise positioning
- Scripted generation
- Version control merging

### Map Generation Scripts

Create maps programmatically:

```csharp
var map = new MapData
{
    Name = "generated_map",
    Width = 64,
    Height = 30,
    TileSize = 32,
    Tiles = new List<TileData>()
};

// Generate 10x10 grass area
for (int x = 0; x < 10; x++)
{
    for (int y = 0; y < 10; y++)
    {
        map.Tiles.Add(new TileData
        {
            X = x,
            Y = y,
            Type = "Grass"
        });
    }
}

// Save to file
File.WriteAllText("assets/maps/generated.json", 
    JsonSerializer.Serialize(map, new JsonSerializerOptions { WriteIndented = true }));
```

### Integration with Procedural Generation

Combine saved maps with procedural generation:

1. Generate base terrain procedurally
2. Load map file
3. Override tiles where map has data
4. Result: Hand-crafted areas in procedural world

## Contributing

Share your amazing maps:
1. Create interesting designs
2. Test thoroughly
3. Document features and requirements
4. Submit via GitHub with screenshots

## See Also

- Map Editor Documentation: `docs/MAP_EDITOR.md`
- Quick Reference: `MAP_EDITOR_QUICKREF.md`
- Tileset Documentation: `assets/tilesets/README.md`
- Terrain Generation: `docs/TERRAIN_GENERATION.md`

---

**Happy Mapping!** Build worlds, tell stories, create adventures.
