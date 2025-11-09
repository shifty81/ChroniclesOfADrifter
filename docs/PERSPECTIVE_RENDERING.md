# 3/4 Perspective Rendering (2.5D Top-Down)

## Overview

Chronicles of a Drifter uses a **3/4 perspective (2.5D top-down view)**, inspired by The Legend of Zelda: A Link to the Past and Stardew Valley. This perspective provides depth and visual appeal while maintaining 2D gameplay mechanics.

## What is 3/4 Perspective?

### Definition
The 3/4 perspective (also called pseudo-3D or 2.5D) is an **angled top-down view** where:
- Camera positioned at 45-60 degrees above the character
- Shows **both the tops of objects** (like roofs, tables) **AND their vertical faces** (like walls, character profiles)
- Creates depth illusion without true 3D rendering
- Allows intuitive 2D gameplay with rich visual presentation

### NOT to be confused with:
- ❌ **Bird's-eye view**: Straight down at 90 degrees (no vertical faces visible)
- ❌ **True isometric**: Specific 30° angle with mathematical precision (axonometric projection)
- ❌ **Full 3D**: Real-time 3D rendering with free camera movement

### Games Using This Perspective:
- The Legend of Zelda: A Link to the Past (SNES)
- Stardew Valley
- Pokémon (Game Boy through DS)
- Secret of Mana
- Chrono Trigger
- Many classic JRPGs and action-adventure games

## Implementation in Chronicles of a Drifter

### Current System: Perfect for 3/4 Perspective ✅

The existing 2D sprite rendering system is **ideally suited** for this perspective style:

#### 1. 2D Coordinate System
```csharp
// World coordinates in 2D
public class PositionComponent
{
    public float X { get; set; }  // Horizontal position
    public float Y { get; set; }  // Vertical position (but represents depth too!)
}
```

In 3/4 perspective:
- **X axis**: Horizontal movement (left/right)
- **Y axis**: Combined vertical AND depth (up/down, forward/backward)
- **Z-order**: Determined by Y position for rendering order

#### 2. Sprite Art Requirements

Sprites must be drawn at the 3/4 angle to match the perspective:

**Character Sprites:**
```
    👤  ← Top of head visible
   /|\ ← Front/side of body visible
   / \ ← Feet visible
```

**Building/Object Sprites:**
```
  ┌─────┐  ← Top/roof visible
  │░░░░░│  ← Front face visible
  │░░░░░│  ← Side partially visible
  └─────┘  ← Bottom edge
```

**Terrain Tiles:**
- Ground tiles show surface texture from 3/4 angle
- Water shows surface with slight ripples
- Grass shows tops of blades with some vertical element

#### 3. Layered Rendering System

The current rendering system already supports depth sorting:

```csharp
public class RenderingSystem : ISystem
{
    public void Update(World world, float deltaTime)
    {
        // Sort entities by Y position (depth)
        var sortedEntities = world.GetEntitiesWithComponent<SpriteComponent>()
            .OrderBy(e => world.GetComponent<PositionComponent>(e).Y);
        
        // Render back-to-front for proper occlusion
        foreach (var entity in sortedEntities)
        {
            RenderSprite(entity);
        }
    }
}
```

**Rendering Order (back-to-front):**
1. Background layer (sky, distant mountains)
2. Parallax layers (clouds, distant trees) ✅ Already implemented
3. Ground tiles (sorted by Y)
4. Objects on ground (sorted by Y)
5. Characters/NPCs (sorted by Y)
6. Effects/particles (sorted by Y)
7. UI overlay

#### 4. Depth Illusion Techniques

**Already Implemented:**
- ✅ **Parallax scrolling**: Different background layers move at different speeds
- ✅ **Shadow rendering**: DrawRect can create simple shadows
- ✅ **Z-order sorting**: Entities sorted by Y position
- ✅ **Camera system**: Smooth following and zoom

**Can Be Enhanced:**
- 🔄 **Shadow sprites**: Add shadow sprites under characters/objects
- 🔄 **Elevation markers**: Visual indicators for height differences
- 🔄 **Occlusion**: Characters walk behind tall objects based on Y position

### Example: Character Movement

```csharp
// Moving "up" in 3/4 perspective
position.Y -= speed * deltaTime;  // Moves away from camera (into the screen)

// Moving "down" in 3/4 perspective
position.Y += speed * deltaTime;  // Moves toward camera (out of screen)

// Moving "right"
position.X += speed * deltaTime;  // Moves to the right

// Moving "left"
position.X -= speed * deltaTime;  // Moves to the left
```

The same 2D math works perfectly because the "depth" is represented by the Y axis!

## Sprite Asset Guidelines for 3/4 Perspective

### Character Sprites (64x64 or 128x128)

**Requirements:**
1. **Head visible from above**: Show top/back of head
2. **Body at angle**: Show front and slight side
3. **Feet visible**: Show footsteps for grounding
4. **Consistent angle**: All sprites at same 45-60° angle

**Directions (4-directional movement):**
- **Down/South**: Character facing toward camera
- **Up/North**: Character facing away from camera (see back of head)
- **Left/West**: Character facing left with slight 3/4 view
- **Right/East**: Character facing right with slight 3/4 view

### Environment Sprites

**Ground Tiles (32x32 or 64x64):**
- Grass: Top view with slight vertical blade elements
- Water: Surface view with ripple patterns
- Stone path: Top surface with depth edges visible
- Sand: Surface texture with small shadows for depth

**Objects/Buildings:**
- Show top surface AND front/side faces
- Trees: Show canopy from above AND trunk from side
- Houses: Show roof from above AND walls from side
- Furniture: Show top surface AND front faces

**Height Representation:**
- **Tall objects** (trees, buildings): Large vertical sprites
- **Medium objects** (bushes, fences): Medium sprites with visible tops
- **Low objects** (grass, flowers): Small sprites mostly showing tops

### Shadows

Shadows enhance the 3/4 perspective illusion:

```csharp
// Simple shadow example
void RenderCharacter(Entity entity)
{
    var position = GetPosition(entity);
    var sprite = GetSprite(entity);
    
    // Render shadow (slightly offset, darker, stretched)
    Renderer_DrawRect(
        position.X + 2,      // Slight X offset
        position.Y + sprite.Height - 4,  // At character's feet
        sprite.Width * 0.8f,  // Slightly narrower
        6,                    // Thin ellipse
        0, 0, 0, 0.3f        // Semi-transparent black
    );
    
    // Render character
    Renderer_DrawSprite(sprite.TextureId, position.X, position.Y, ...);
}
```

## Advantages of 3/4 Perspective (vs Full 3D)

### ✅ Pros:
1. **Performance**: 2D rendering is much faster than 3D
2. **Art Production**: Easier to create sprites than 3D models
3. **File Size**: Smaller game size with sprite assets
4. **Clarity**: Easier to see game elements and navigate
5. **Style**: Classic, timeless aesthetic
6. **Simplicity**: Easier collision detection and physics
7. **Development Speed**: Faster iteration and testing

### ❌ Cons (of not using full 3D):
1. Limited camera angles (fixed perspective)
2. Cannot rotate camera freely
3. Sprites needed for each direction
4. Height representation is visual only, not true 3D

**For Zelda/Stardew-style gameplay, the pros heavily outweigh the cons!**

## Rendering Pipeline: 2D with 3/4 Sprites

### Current DirectX 11 Implementation

```cpp
// D3D11Renderer::DrawSprite
void D3D11Renderer::DrawSprite(int textureId, float x, float y,
                               float width, float height, float rotation)
{
    // 1. Get sprite texture (drawn at 3/4 angle by artist)
    auto texture = GetTexture(textureId);
    
    // 2. Convert screen coordinates to NDC
    float ndcX = (x / screenWidth) * 2.0f - 1.0f;
    float ndcY = 1.0f - (y / screenHeight) * 2.0f;
    
    // 3. Create quad vertices with texture coordinates
    Vertex vertices[] = {
        { {ndcX, ndcY, 0.0f}, {1,1,1,1}, {0,0} },      // Top-left
        { {ndcX+w, ndcY, 0.0f}, {1,1,1,1}, {1,0} },    // Top-right
        { {ndcX, ndcY-h, 0.0f}, {1,1,1,1}, {0,1} },    // Bottom-left
        { {ndcX+w, ndcY-h, 0.0f}, {1,1,1,1}, {1,1} }   // Bottom-right
    };
    
    // 4. Apply rotation if needed (around sprite center)
    if (rotation != 0.0f) {
        ApplyRotation(vertices, rotation);
    }
    
    // 5. Render textured quad
    DrawQuad(vertices, texture);
}
```

**The sprite texture itself contains the 3/4 perspective view!** The renderer just draws it as a 2D quad.

### No 3D Transformations Needed

```
Traditional 3D Pipeline:           Our 2.5D Pipeline:
┌─────────────────────┐           ┌─────────────────────┐
│ 3D Model Data       │           │ 2D Sprite (3/4 art) │
└──────────┬──────────┘           └──────────┬──────────┘
           │                                  │
┌──────────▼──────────┐                      │
│ Model Transform     │                      │
└──────────┬──────────┘                      │
           │                                  │
┌──────────▼──────────┐                      │
│ View Transform      │                      │
└──────────┬──────────┘                      │
           │                                  │
┌──────────▼──────────┐           ┌──────────▼──────────┐
│ Projection Transform│           │ Screen Position     │
└──────────┬──────────┘           └──────────┬──────────┘
           │                                  │
┌──────────▼──────────┐           ┌──────────▼──────────┐
│ Rasterization       │           │ Rasterization       │
└──────────┬──────────┘           └──────────┬──────────┘
           │                                  │
┌──────────▼──────────┐           ┌──────────▼──────────┐
│ Pixel Shading       │           │ Pixel Shading       │
└──────────┬──────────┘           └──────────┬──────────┘
           │                                  │
           ▼                                  ▼
     Final Image                        Final Image

     ❌ Complex                           ✅ Simple
     ❌ Slow                              ✅ Fast
     ❌ Hard to debug                     ✅ Easy to debug
```

## Examples from Target Games

### The Legend of Zelda: A Link to the Past

**Perspective Characteristics:**
- Link's head and shield visible from above
- Link's face and body visible from 3/4 angle
- Trees show canopy from top AND trunk from side
- Houses show roof AND front wall
- Pots show rim AND sides

**How It Works:**
- All sprites drawn at consistent 45° angle
- Y-sorting for depth (characters behind trees)
- Parallax layers for depth illusion
- Simple shadow ellipses under characters

### Stardew Valley

**Perspective Characteristics:**
- Character shows top of hat AND front of face
- Crops show top of plants with some leaf sides visible
- Buildings show roofs AND front walls
- Animals show back AND sides

**How It Works:**
- Tile-based 2D grid
- Sprites drawn at ~50° angle
- Y-sorting for layering (walk behind buildings)
- Seasonal sprite variations
- Shadow sprites for depth

## Implementation Checklist

### ✅ Already Implemented:
- [x] 2D coordinate system
- [x] Sprite rendering (DrawSprite)
- [x] Texture loading
- [x] Camera system with zoom
- [x] Parallax scrolling
- [x] Y-sorting capability in ECS
- [x] Collision detection (2D AABB)
- [x] Animation system

### 🔄 Recommended Enhancements:
- [ ] Automatic Y-sorting in rendering system
- [ ] Shadow sprite support (ellipse or sprite-based)
- [ ] Height-based sprite scaling (objects further back = smaller)
- [ ] Sprite offset support (anchor point adjustment)
- [ ] Layer system for explicit render order control

### 🎨 Art Asset Requirements:
- [ ] Character sprites (4-8 directions at 3/4 angle)
- [ ] Environment tiles (ground, water, grass at 3/4 angle)
- [ ] Object sprites (trees, buildings, furniture at 3/4 angle)
- [ ] Effect sprites (shadows, particles)
- [ ] UI elements (can be straight-on, not 3/4)

## Conclusion

**The current 2D rendering system is perfect for 3/4 perspective rendering!**

No need for:
- ❌ 3D voxel engine
- ❌ Mesh generation
- ❌ 3D physics engine
- ❌ 3D camera controls
- ❌ Complex 3D transformations

Just need:
- ✅ 2D sprites drawn at 3/4 angle (by artists)
- ✅ Y-sorting for depth layering (easy to implement)
- ✅ Simple shadow rendering (already capable)
- ✅ Current 2D game systems (all working)

The "3D" appearance comes from the **art style**, not the rendering engine. This is the same technique used successfully in classic games for decades and is perfect for a Zelda/Stardew-inspired game!

## References

- [The Legend of Zelda: A Link to the Past](https://zelda.fandom.com/wiki/The_Legend_of_Zelda:_A_Link_to_the_Past) - Original inspiration
- [Stardew Valley](https://www.stardewvalley.net/) - Modern example of the style
- [2.5D on Wikipedia](https://en.wikipedia.org/wiki/2.5D) - Technical explanation
- [Top-down Perspective](https://en.wikipedia.org/wiki/Video_game_graphics#Top-down_perspective) - Game graphics overview
