# Tilesets Directory

This directory contains tileset definitions for the Chronicles of a Drifter map editor.

## What are Tilesets?

Tilesets define the visual appearance and properties of tiles used in the game. Each tileset is a JSON file that specifies:
- Tile names and display names
- Colors for rendering (RGB values 0.0-1.0)
- Collision properties
- Categories for organization
- Optional texture paths

## Included Tilesets

### `zelda_style.json`
A Zelda: A Link to the Past inspired tileset with 18 tile types:
- **Terrain**: grass (3 variants), dirt, sand
- **Water**: shallow, deep
- **Walls**: stone wall, dungeon wall
- **Vegetation**: trees, bushes
- **Decorations**: flowers (red, yellow)
- **Structures**: doors, bridges, chests
- **Dungeon**: floors, walls

Perfect for creating action-adventure style maps.

## Creating Custom Tilesets

### Basic Structure

```json
{
  "Name": "my_tileset",
  "Description": "My custom tileset for dungeons",
  "TileSize": 32,
  "Tiles": {
    "tile_name": {
      "Name": "tile_name",
      "DisplayName": "Friendly Name",
      "Color": [R, G, B],
      "IsCollidable": false,
      "Category": "terrain"
    }
  }
}
```

### Properties

- **Name** (string): Unique identifier for the tileset
- **Description** (string): Human-readable description
- **TileSize** (int): Size of each tile in pixels (typically 32)
- **Tiles** (object): Dictionary of tile definitions

### Tile Definition Properties

- **Name** (string): Internal tile identifier (matches key)
- **DisplayName** (string): User-friendly name shown in editor
- **Color** (array): RGB color values from 0.0 to 1.0
  - Example: `[0.13, 0.65, 0.13]` for bright green
- **IsCollidable** (bool): Whether entities can pass through
- **Category** (string): Organization category
  - Common: "terrain", "wall", "water", "decoration", "structure"
- **TexturePath** (string, optional): Path to sprite texture
- **TextureX** (int, optional): X position in sprite sheet
- **TextureY** (int, optional): Y position in sprite sheet

## Color Guidelines

RGB values range from 0.0 (no color) to 1.0 (full intensity).

### Common Colors

| Color | RGB Values | Usage |
|-------|------------|-------|
| Green (Grass) | [0.13, 0.65, 0.13] | Terrain |
| Blue (Water) | [0.20, 0.60, 0.85] | Water |
| Gray (Stone) | [0.50, 0.50, 0.50] | Walls |
| Brown (Dirt) | [0.55, 0.47, 0.25] | Paths |
| Yellow (Sand) | [0.93, 0.87, 0.51] | Desert |
| Red (Brick) | [0.70, 0.35, 0.25] | Buildings |
| White (Snow) | [0.95, 0.95, 1.0] | Winter |

### Converting from 0-255 RGB
If you have RGB values from 0-255 (common in image editors):
```
JSON value = (RGB value) / 255.0

Example: RGB(33, 166, 33) → [0.13, 0.65, 0.13]
```

## Category Guidelines

Organize tiles by category for easier selection:

- **terrain**: Ground tiles (grass, dirt, sand)
- **wall**: Solid barriers (stone walls, fences)
- **water**: Water tiles (rivers, oceans, puddles)
- **decoration**: Non-blocking visuals (flowers, grass tufts)
- **structure**: Buildings and constructions (doors, bridges)
- **dungeon**: Dungeon-specific tiles
- **vegetation**: Trees, bushes, plants
- **object**: Interactive objects (chests, switches)

## Loading Tilesets

Tilesets in this directory are automatically loaded when the map editor starts:

```bash
cd src/Game
dotnet run -c Release -- editor
```

The editor will scan this directory and load all `.json` files.

## Example: Creating a Desert Tileset

```json
{
  "Name": "desert",
  "Description": "Desert theme tileset",
  "TileSize": 32,
  "Tiles": {
    "sand": {
      "Name": "sand",
      "DisplayName": "Sand",
      "Color": [0.93, 0.87, 0.51],
      "IsCollidable": false,
      "Category": "terrain"
    },
    "cactus": {
      "Name": "cactus",
      "DisplayName": "Cactus",
      "Color": [0.20, 0.60, 0.30],
      "IsCollidable": true,
      "Category": "vegetation"
    },
    "sandstone": {
      "Name": "sandstone",
      "DisplayName": "Sandstone Wall",
      "Color": [0.76, 0.70, 0.50],
      "IsCollidable": true,
      "Category": "wall"
    },
    "oasis": {
      "Name": "oasis",
      "DisplayName": "Oasis Water",
      "Color": [0.30, 0.70, 0.80],
      "IsCollidable": false,
      "Category": "water"
    }
  }
}
```

Save as `desert.json` in this directory and restart the editor.

## Testing Your Tileset

1. Create your tileset JSON file
2. Place it in this directory
3. Launch the editor: `dotnet run -- editor`
4. Check console for "Loaded tileset: [name]"
5. Use `[` and `]` keys to browse tiles
6. Place tiles with Space key

## Best Practices

1. **Naming**: Use lowercase with underscores (e.g., `dark_grass`)
2. **Colors**: Test colors in the editor - adjust if needed
3. **Categories**: Use consistent category names across tilesets
4. **Display Names**: Make them clear and descriptive
5. **Collision**: Mark solid objects as collidable
6. **Organization**: Group related tiles in the same tileset
7. **Backup**: Keep backups of custom tilesets

## Troubleshooting

**Tileset not loading?**
- Check JSON syntax with a validator
- Ensure file extension is `.json`
- Verify file is in this directory
- Check console output for errors

**Colors look wrong?**
- RGB values must be 0.0 to 1.0 (not 0-255)
- Adjust values and reload editor
- Compare with included `zelda_style.json`

**Tiles not appearing in editor?**
- Verify tileset loaded (check console)
- Tile names must be unique within tileset
- Check for JSON formatting errors

## Contributing

Share your custom tilesets with the community:
1. Create a descriptive tileset
2. Test it in the editor
3. Document special features
4. Submit via GitHub pull request

## See Also

- Map Editor Documentation: `docs/MAP_EDITOR.md`
- Quick Reference: `MAP_EDITOR_QUICKREF.md`
- Sprite Assets Guide: `docs/SPRITE_ASSETS.md`

---

**Happy Mapping!** Create beautiful worlds with custom tilesets.
