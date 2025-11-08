# Sprite Asset Guidelines

## Overview

Chronicles of a Drifter uses a high-resolution sprite-based animation system with support for character customization. This document outlines the specifications and guidelines for creating sprite assets that align with the game's visual style.

## Visual Style

The game follows a **2D top-down perspective** inspired by The Legend of Zelda: A Link to the Past, with modern high-resolution sprites.

### Art Direction
- **Style**: Pixel art with modern high-resolution detail
- **Perspective**: Top-down with slight angle for depth
- **Color Palette**: Rich, vibrant colors with good contrast
- **Resolution**: High-resolution sprites (64x64 to 128x128 base size)
- **Animation**: Smooth, fluid animations with 4-8 frames per cycle

## Sprite Specifications

### Character Sprites

#### Base Character Sprite Sheet
- **Resolution**: 512x512 pixels (for 8x8 grid of 64x64 frames)
- **Frame Size**: 64x64 pixels per frame
- **Layout**: Grid layout with 8 frames per row
- **Animations Required**:
  - Idle (4 frames): Row 0, frames 0-3
  - Walk Down (4 frames): Row 1, frames 0-3
  - Walk Up (4 frames): Row 2, frames 0-3
  - Walk Left (4 frames): Row 3, frames 0-3
  - Walk Right (4 frames): Row 4, frames 0-3
  - Attack Down (4 frames): Row 5, frames 0-3
  - Attack Up (4 frames): Row 6, frames 0-3
  - Attack Side (4 frames): Row 7, frames 0-3

#### High-Resolution Option
- **Resolution**: 1024x1024 pixels (for 8x8 grid of 128x128 frames)
- **Frame Size**: 128x128 pixels per frame
- **Same layout as above, just at higher resolution**

### Clothing Layer Sprites

Each clothing item should be a separate sprite sheet with the same dimensions and layout as the character base.

#### Clothing Categories
1. **Shirts/Tops**
   - Tunic
   - Vest
   - Robe
   - Peasant shirt
   - Armor shirt

2. **Pants/Bottoms**
   - Trousers
   - Leggings
   - Shorts
   - Armor pants

3. **Accessories**
   - Boots (leather, heavy, light, traveling)
   - Gloves (fingerless, leather, cloth, armored)
   - Hats (hood, cap, helmet, bandana)

#### Clothing Sprite Requirements
- **Must match base character animations exactly**
- **Transparent background (PNG with alpha)**
- **Support for color tinting** (use grayscale base with color masks)
- **Same frame timing as base character**

### Color Customization

To support color customization, clothing sprites should be created with:

1. **Primary Color Layer**: Main body of the clothing item
2. **Secondary Color Layer**: Accents, trim, buttons, etc.
3. **Fixed Details Layer**: Non-customizable details (always rendered)

#### Color Mask Format
- Use grayscale for primary/secondary color areas
- White (255) = fully affected by color tint
- Black (0) = not affected by color tint
- Gray values = partial tint blending

## File Naming Convention

### Character Base Sprites
```
characters/
  base/
    character_base_64x64.png      # Standard resolution
    character_base_128x128.png    # High resolution
    character_base_female_64x64.png
    character_base_male_64x64.png
```

### Clothing Sprites
```
clothing/
  shirts/
    tunic_64x64.png
    tunic_128x128.png
    vest_64x64.png
    robe_64x64.png
  pants/
    trousers_64x64.png
    leggings_64x64.png
  boots/
    leather_boots_64x64.png
    heavy_boots_64x64.png
  gloves/
    fingerless_gloves_64x64.png
  hats/
    hood_64x64.png
    cap_64x64.png
```

### Armor Sprites
```
armor/
  light/
    leather_armor_64x64.png
    leather_armor_128x128.png
  medium/
    chainmail_armor_64x64.png
    iron_armor_64x64.png
  heavy/
    plate_armor_64x64.png
    knight_armor_64x64.png
```

## Animation Guidelines

### Frame Rate
- **Default**: 6-8 frames per second (0.125-0.167s per frame)
- **Idle**: 4 frames at 0.15s per frame
- **Walk**: 4-6 frames at 0.1s per frame
- **Attack**: 4-6 frames at 0.08s per frame

### Animation Principles
1. **Smooth transitions**: Ensure first and last frames connect for looping
2. **Consistent timing**: Keep frame durations consistent within animation type
3. **Anticipation**: Add slight wind-up to attacks
4. **Follow-through**: Add recovery frames after attacks
5. **Squash and stretch**: Subtle deformation for dynamic movement

## Layering System

When rendering a character, layers are drawn in this order (bottom to top):

1. **Base body** (skin, eyes, hair)
2. **Pants/Bottoms**
3. **Boots**
4. **Shirts/Tops**
5. **Gloves**
6. **Hats/Helmets**
7. **Weapons** (held items)
8. **Effects** (spell effects, status indicators)

### Armor Override
When armor is equipped, it replaces layers 2-6 with a single armor sprite.

## Color Palette Recommendations

### Skin Tones
- Pale: #FFE0D5
- Light: #F0C8B4
- Medium: #D4A373
- Tan: #C68E6E
- Brown: #A67C52
- Dark: #7A5230

### Clothing Color Palettes
- **Earth Tones**: Browns, tans, olive greens
- **Forest**: Deep greens, browns
- **Ocean**: Blues, teals, navy
- **Crimson**: Deep reds, burgundy
- **Royal**: Purples, deep blues, gold accents
- **Neutral**: Grays, blacks, whites
- **Desert**: Tans, sandy yellows, warm browns

## Export Settings

### PNG Export
- **Format**: PNG with alpha transparency
- **Color Mode**: RGBA
- **Bit Depth**: 8-bit per channel
- **Compression**: Maximum quality
- **No interlacing**

### Optimization
- Use PNG optimization tools (optipng, pngcrush) for file size
- Maintain visual quality - don't over-compress
- Target file size: 50-200 KB per sprite sheet

## Testing Checklist

Before submitting sprite assets:

- [ ] All required animations present and complete
- [ ] Frame dimensions consistent
- [ ] Transparent background properly set
- [ ] Color tinting areas properly masked
- [ ] Animations loop smoothly
- [ ] Sprite aligns correctly with collision box
- [ ] Tested at different scales (1x, 2x, 4x)
- [ ] File size optimized
- [ ] Naming convention followed

## Example Workflow

1. **Create base character sprite** (64x64 per frame, 8x8 grid)
2. **Design clothing layers** matching the base animations exactly
3. **Create color masks** for customizable areas
4. **Export as PNG** with alpha transparency
5. **Optimize file size** while maintaining quality
6. **Test in-game** with animation system
7. **Verify color customization** works correctly
8. **Check all animation states** (idle, walk, attack)

## Resources

### Recommended Tools
- **Aseprite**: Excellent for pixel art and sprite animation
- **Piskel**: Free online pixel art tool
- **GIMP**: For layer management and export
- **GraphicsGale**: Animation-focused pixel editor

### Reference Materials
- The Legend of Zelda: A Link to the Past sprites
- Stardew Valley character sprites
- Terraria character customization system
- Dead Cells animation style

## Future Considerations

### Planned Enhancements
- [ ] Support for multiple body types (slim, average, athletic, heavy)
- [ ] Skin tone variations with proper shading
- [ ] Hair styles as separate layer
- [ ] Facial expressions
- [ ] Equipment visual effects (glowing weapons, particle effects)
- [ ] Seasonal outfit variations
- [ ] Emotes and reactions

---

For questions or feedback on sprite assets, please refer to the main [ARCHITECTURE.md](ARCHITECTURE.md) document.
