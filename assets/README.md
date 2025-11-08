# Assets Directory

This directory contains all game assets for Chronicles of a Drifter.

## Directory Structure

```
assets/
├── sprites/           # Sprite sheets and animations
│   ├── characters/    # Character base sprites
│   ├── animations/    # Character animations
│   ├── clothing/      # Clothing layer sprites
│   └── armor/         # Armor sprites
├── sounds/            # Sound effects and music
├── fonts/             # UI fonts
└── data/              # Game data files (JSON configs)
```

## Sprite Assets

### Character Sprites
Character sprites use a high-resolution format (64x64 or 128x128 per frame) organized in sprite sheets.

See [SPRITE_ASSETS.md](../docs/SPRITE_ASSETS.md) for detailed specifications.

### Current Asset Status

**Base Characters**: Placeholder - To be implemented
- Standard resolution: 64x64 per frame
- High resolution: 128x128 per frame
- Required animations: idle, walk (4 directions), attack (4 directions)

**Clothing Layers**: Placeholder - To be implemented
- Shirts/Tops: tunic, vest, robe, peasant shirt, armor shirt
- Pants: trousers, leggings, shorts, armor pants
- Accessories: boots, gloves, hats

**Armor Sets**: Placeholder - To be implemented
- Light armor: leather
- Medium armor: chainmail, iron
- Heavy armor: plate, knight

## Creating New Assets

1. Follow the specifications in [SPRITE_ASSETS.md](../docs/SPRITE_ASSETS.md)
2. Place assets in the appropriate subdirectory
3. Use the naming convention: `item_name_resolution.png`
4. Ensure transparent backgrounds (PNG with alpha)
5. Test in-game before committing

## Asset Guidelines

- **Resolution**: High-res preferred (64x64 or 128x128 per frame)
- **Format**: PNG with alpha transparency
- **Style**: Top-down pixel art matching the game's visual style
- **Color**: Vibrant colors with good contrast
- **Animation**: Smooth 4-8 frame loops

## Placeholder Assets

Currently, the game uses texture IDs without actual sprite files loaded. The rendering system is ready to support sprite sheets once assets are available.

To add real sprite assets:
1. Create the sprite sheet according to specifications
2. Place in the appropriate directory
3. Update the texture loading system to reference the files
4. Test the animations in-game

---

See the main [README.md](../README.md) for more information about the project.
