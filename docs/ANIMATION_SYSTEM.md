# Animation System Documentation

## Overview

Chronicles of a Drifter features a sophisticated sprite-based animation system with support for:
- Frame-by-frame sprite animations
- Character customization with multiple clothing layers
- Dynamic animation state management
- High-resolution sprite support
- Color customization for clothing

## Architecture

### Core Components

#### AnimationComponent
Manages the animation state for an entity.

```csharp
public class AnimationComponent : IComponent
{
    public string CurrentAnimation { get; set; }    // Current animation state
    public int CurrentFrame { get; set; }           // Current frame index
    public float FrameTimer { get; set; }           // Time accumulator
    public float FrameDuration { get; set; }        // Frame duration in seconds
    public bool Loop { get; set; }                  // Whether to loop
    public bool IsFinished { get; set; }            // Animation finished flag
    public float PlaybackSpeed { get; set; }        // Playback speed multiplier
}
```

#### AnimatedSpriteComponent
Defines sprite sheet layout and animation definitions.

```csharp
public class AnimatedSpriteComponent : IComponent
{
    public int TextureId { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    public float Rotation { get; set; }
    public Dictionary<string, AnimationDefinition> Animations { get; set; }
    public float Scale { get; set; }
    public bool FlipHorizontal { get; set; }
    public bool FlipVertical { get; set; }
}
```

#### AnimationDefinition
Describes a single animation sequence within a sprite sheet.

```csharp
public class AnimationDefinition
{
    public int[] FrameIndices { get; set; }     // Frame order
    public int FramesPerRow { get; set; }       // Sprite sheet layout
    public int FrameWidth { get; set; }         // Frame dimensions
    public int FrameHeight { get; set; }
}
```

### Character Customization Components

#### CharacterAppearanceComponent
Manages all customizable character features.

```csharp
public class CharacterAppearanceComponent : IComponent
{
    public string SkinTone { get; set; }
    public string HairStyle { get; set; }
    public Color HairColor { get; set; }
    public Color EyeColor { get; set; }
    public string BodyType { get; set; }
    public Dictionary<string, ClothingLayer> ClothingLayers { get; set; }
    public string? EquippedArmor { get; set; }
}
```

#### ClothingLayer
Represents a single customizable clothing item.

```csharp
public class ClothingLayer
{
    public string Type { get; set; }            // "shirt", "pants", etc.
    public string Style { get; set; }           // "tunic", "robe", etc.
    public Color PrimaryColor { get; set; }     // Main color
    public Color SecondaryColor { get; set; }   // Accent color
    public int RenderOrder { get; set; }        // Layer ordering
    public bool IsVisible { get; set; }         // Visibility flag
}
```

### Systems

#### AnimationSystem
Updates animation state and advances frames based on delta time.

**Responsibilities:**
- Frame progression based on time
- Animation looping
- Animation completion detection
- Playback speed control

#### CharacterRenderingSystem
Handles rendering of customizable characters with multiple layers.

**Responsibilities:**
- Render base character
- Render clothing layers in order
- Handle armor override
- Apply color customization

#### RenderingSystem (Enhanced)
Now supports both regular sprites and animated sprites.

**Responsibilities:**
- Render animated sprites
- Render regular sprites
- Clear and present frames

## Usage Examples

### Creating an Animated Character

```csharp
// Create entity
var character = world.CreateEntity();

// Add position
world.AddComponent(character, new PositionComponent(960, 540));

// Create animated sprite with animations
var animatedSprite = new AnimatedSpriteComponent(textureId: 0, width: 64, height: 64)
{
    Scale = 2.0f  // Render at 2x size for high resolution
};

// Define idle animation (4 frames)
animatedSprite.Animations["idle"] = new AnimationDefinition(
    frameIndices: new[] { 0, 1, 2, 3 },
    framesPerRow: 8,
    frameWidth: 64,
    frameHeight: 64
);

// Define walk animation (8 frames)
animatedSprite.Animations["walk"] = new AnimationDefinition(
    frameIndices: new[] { 8, 9, 10, 11, 12, 13, 14, 15 },
    framesPerRow: 8,
    frameWidth: 64,
    frameHeight: 64
);

world.AddComponent(character, animatedSprite);

// Add animation component (starts with idle)
var animation = new AnimationComponent("idle", frameDuration: 0.15f, loop: true);
world.AddComponent(character, animation);
```

### Playing Animations

```csharp
// Play a looping animation
AnimationSystem.PlayAnimation(world, entity, "walk", loop: true);

// Play a one-shot animation
AnimationSystem.PlayAnimation(world, entity, "attack", loop: false);

// Play with custom speed
AnimationSystem.PlayAnimation(world, entity, "run", loop: true, playbackSpeed: 1.5f);
```

### Creating a Customizable Character

```csharp
// Create character with appearance
var character = world.CreateEntity();
world.AddComponent(character, new PositionComponent(960, 540));

// Set up appearance
var appearance = new CharacterAppearanceComponent
{
    SkinTone = "medium",
    HairStyle = "long",
    HairColor = new Color(100, 70, 40, 255),
    EyeColor = new Color(70, 130, 180, 255),
    BodyType = "average"
};

// Add clothing layers
appearance.ClothingLayers["shirt"] = new ClothingLayer(
    type: "shirt",
    style: "tunic",
    primaryColor: new Color(34, 139, 34, 255),
    secondaryColor: new Color(85, 107, 47, 255),
    renderOrder: 1
);

appearance.ClothingLayers["pants"] = new ClothingLayer(
    type: "pants",
    style: "trousers",
    primaryColor: new Color(70, 50, 30, 255),
    secondaryColor: new Color(50, 35, 20, 255),
    renderOrder: 0
);

world.AddComponent(character, appearance);

// Add animation components as shown above
```

### Equipping Armor

```csharp
// Get character appearance
var appearance = world.GetComponent<CharacterAppearanceComponent>(entity);

// Equip armor (hides clothing)
appearance.EquippedArmor = "iron_armor";
CharacterRenderingSystem.UpdateClothingVisibility(appearance);

// Remove armor (shows clothing)
appearance.EquippedArmor = null;
CharacterRenderingSystem.UpdateClothingVisibility(appearance);
```

### Changing Clothing Colors

```csharp
var appearance = world.GetComponent<CharacterAppearanceComponent>(entity);

// Change shirt color
if (appearance.ClothingLayers.ContainsKey("shirt"))
{
    appearance.ClothingLayers["shirt"].PrimaryColor = new Color(178, 34, 34, 255); // Red
    appearance.ClothingLayers["shirt"].SecondaryColor = new Color(139, 0, 0, 255); // Dark red
}
```

## Character Creator

The `CharacterCreatorScene` demonstrates the character customization system with interactive controls:

```
Controls:
  1/2 - Change skin tone
  3/4 - Change hair style
  5/6 - Change body type
  7/8 - Change clothing color palette
  A   - Toggle armor (shows/hides clothing)
  W   - Play walk animation
  I   - Play idle animation
```

### Available Customization Options

**Skin Tones:**
- pale, light, medium, tan, brown, dark

**Hair Styles:**
- short, long, ponytail, bald, curly, braided, spiky

**Body Types:**
- slim, average, athletic, heavy

**Clothing Styles:**
- **Shirts**: tunic, vest, robe, armor_shirt, peasant
- **Pants**: trousers, leggings, shorts, armor_pants
- **Boots**: leather, heavy, light, traveling
- **Gloves**: fingerless, leather, cloth, armored
- **Hats**: hood, cap, helmet, bandana, none

**Color Palettes:**
- Earth Tones, Forest, Ocean, Crimson, Royal, Neutral, Midnight, Desert

## System Integration

### Adding Systems to a Scene

```csharp
public override void OnLoad()
{
    // Add animation system before rendering
    World.AddSystem(new AnimationSystem());
    
    // Add character rendering for customizable characters
    World.AddSystem(new CharacterRenderingSystem());
    
    // Add general rendering system
    World.AddSystem(new RenderingSystem());
    
    // ... create entities ...
}
```

**System Order Matters:**
1. AnimationSystem updates frame indices
2. CharacterRenderingSystem renders layered characters
3. RenderingSystem handles regular sprites and presents the frame

## Performance Considerations

### Optimization Tips

1. **Sprite Sheet Packing**: Pack all animation frames into a single texture to reduce draw calls
2. **Frame Caching**: Cache frequently used frame calculations
3. **Layer Culling**: Only render visible clothing layers
4. **LOD System**: Use lower-resolution sprites for distant entities
5. **Animation Pooling**: Reuse animation definitions for similar entities

### Memory Management

- Sprite sheets are loaded once and referenced by texture ID
- Animation definitions are lightweight structs
- Clothing layers use minimal memory per character
- Color data is stored as simple byte arrays

## Future Enhancements

### Planned Features

- [ ] **Directional Animations**: Separate animations for each direction (up, down, left, right)
- [ ] **Blend Animations**: Smooth transitions between animation states
- [ ] **IK System**: Inverse kinematics for dynamic posing
- [ ] **Particle Effects**: Attach particle emitters to animation frames
- [ ] **Equipment Overlays**: Visual effects for enchanted items
- [ ] **Facial Expressions**: Dynamic expression changes
- [ ] **Emotes**: Special animation sequences for player expression

### Technical Improvements

- [ ] **GPU Sprite Batching**: Batch similar sprites for better performance
- [ ] **Shader-Based Color Tinting**: Use shaders for real-time color customization
- [ ] **Animation Events**: Trigger callbacks at specific frames
- [ ] **Animation Blending**: Smooth transitions between different animations
- [ ] **Sprite Atlas Generation**: Automatic packing of sprite sheets
- [ ] **Animation Compression**: Reduce memory footprint for large sprite sets

## Troubleshooting

### Common Issues

**Problem**: Animation not playing
- Check if AnimationSystem is added to the scene
- Verify animation name exists in AnimatedSpriteComponent.Animations
- Ensure FrameDuration > 0

**Problem**: Character not visible
- Verify entity has both AnimatedSpriteComponent and PositionComponent
- Check TextureId is valid
- Ensure Scale > 0

**Problem**: Clothing not rendering
- Check if CharacterRenderingSystem is added
- Verify ClothingLayers dictionary has items
- Ensure IsVisible is true on clothing layers

**Problem**: Armor doesn't hide clothing
- Call CharacterRenderingSystem.UpdateClothingVisibility() after setting EquippedArmor
- Check EquippedArmor is not null or empty string

## API Reference

### AnimationSystem

```csharp
// Play an animation
public static void PlayAnimation(
    World world, 
    Entity entity, 
    string animationName, 
    bool loop = true, 
    float playbackSpeed = 1.0f
)

// Get current frame index for rendering
public static int GetCurrentFrameIndex(
    AnimationComponent animation, 
    AnimatedSpriteComponent sprite
)
```

### CharacterRenderingSystem

```csharp
// Update clothing visibility based on armor state
public static void UpdateClothingVisibility(
    CharacterAppearanceComponent appearance
)
```

## Examples

See the `CharacterCreatorScene` class for a complete working example of:
- Character creation
- Animation setup
- Clothing customization
- Interactive controls
- Real-time preview

---

For sprite asset specifications, see [SPRITE_ASSETS.md](SPRITE_ASSETS.md).

For ECS architecture details, see [ECS_IMPLEMENTATION.md](ECS_IMPLEMENTATION.md).
