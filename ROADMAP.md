# Chronicles of a Drifter - Development Roadmap

This roadmap outlines the planned features and systems for Chronicles of a Drifter, with a focus on creating a vibrant, explorable 2D world with vertical depth for underground exploration.

## Phase 1: Core 2D World Generation System with Vertical Depth

### 1.1 Varied Terrain Generation

#### Height Map-Based Surface Terrain
- **Surface Generation**: Implement 2D Perlin/Simplex noise functions to create varied surface terrain
  - Rolling hills and flat areas using noise-based height maps
  - Mountains and valleys with elevation changes
  - Smooth terrain transitions between biomes
  - Height variation from 0-10 blocks above base level

- **Fractal Noise Layers for Surface Detail**:
  - **Continentalness Map**: Determines major landmasses and water bodies
  - **Erosion Map**: Simulates natural weathering and smoothing
  - **Peaks & Valleys (PV) Map**: Creates hills and depressions
  - **Detail Noise**: Adds small-scale surface variation
  - Layered noise at multiple frequencies for natural-looking terrain

#### Underground Layers (20 Blocks Deep)
- **Layered Underground System**:
  - **Layer 0-3**: Topsoil and surface rock
  - **Layer 4-8**: Stone layer with occasional ore veins
  - **Layer 9-14**: Deep stone with rarer ores
  - **Layer 15-19**: Bedrock transition with valuable treasures
  - **Layer 20**: Unbreakable bedrock floor

- **Underground Cave Pockets**: 
  - Small cave systems (3-8 blocks wide) using cellular automata
  - Hidden treasure rooms
  - Underground water pockets
  - Ore vein generation using noise functions

#### Biome System
- **Distinct Biomes** with seamless transitions:
  - **Desert**: Sandy surface, cacti, minimal vegetation, sandstone underground
  - **Forest**: Grass surface, dense tree coverage, dirt and stone underground
  - **Snowy Mountains**: Higher elevation, snow-covered surface, stone underground
  - **Beaches**: Transition zones between land and water, sandy surface and underground
  - **Plains**: Grassy surface, scattered trees, standard underground layers
  - **Jungle**: Dense vegetation on surface, clay deposits underground
  - **Swamp**: Water-logged surface, peat and mud underground layers
  - **Rocky**: Minimal topsoil, stone closer to surface, exposed ore veins
- Biome blending system for natural transitions
- Temperature and moisture maps to determine biome placement
- Biome-specific surface blocks and underground composition

#### Water Bodies
- **Rivers**: Flowing water on surface
  - Carves shallow channels in terrain
  - Natural meandering patterns using noise
  - Connects to lakes and ocean biomes
- **Lakes**: Natural depression-based water bodies
  - Various sizes from ponds to large lakes
  - Shallow depth (1-3 blocks below surface)
  - Shore generation with beaches
- **Ocean**: Large water biome
  - Surface water blocks
  - Sandy/rocky bottom (3-5 blocks deep)
  - Coastal features (beaches, cliffs)

### 1.2 Natural Elements (Flora & Fauna)

#### Vegetation System
- **Trees**: Multiple tree types per biome
  - 2D sprite-based trees on surface
  - Different sizes (small, medium, large)
  - Seasonal variation support
  - Biome-specific species (oak, pine, palm, jungle trees)
  - Root systems that extend 1-2 blocks underground
- **Grass and Ground Cover**:
  - Grass sprites on surface blocks
  - Flowers and small plants
  - Bushes and shrubs (can be obstacles)
  - Biome-specific color variations
- **Surface Decoration System**:
  - Rocks and boulders
  - Fallen logs
  - Mushrooms in dark/damp areas
  - Surface details for visual richness

#### Creatures and Characters
- **2D Sprite-Based Characters**:
  - ✅ Already implemented character system with animations
  - ✅ Layered clothing and customization system
  - Movement on surface and ability to enter underground areas
- **Creature Varieties**:
  - Surface creatures (deer, rabbits, birds)
  - Underground creatures (bats, cave spiders, rats)
  - Hostile enemies (goblins, bandits, skeletons in deep caves)
- **AI Integration**:
  - ✅ Lua-scriptable behaviors (already implemented)
  - Pathfinding on 2D terrain with height awareness
  - Dynamic spawning based on biome and depth

### 1.3 Artificial Structures and Points of Interest

- **Surface Structures**:
  - Villages and towns with buildings
  - Ruins and abandoned structures
  - Watchtowers and guard posts
  - Campsites and temporary shelters
- **Underground Structures**:
  - Hidden treasure chambers (depth 10-19)
  - Ancient crypts and burial chambers
  - Abandoned mine shafts
  - Secret underground rooms
- **Dungeon Entrances**:
  - Cave openings on surface
  - Hidden trapdoors requiring tools to open
  - Overgrown ruins with underground access
  - Wells that lead to deeper levels

## Phase 2: Essential 2D World System Features

### 2.1 Performance and Scale

#### Horizontal Infinite World Generation
- **Chunk-Based System**:
  - Dynamic chunk loading/unloading horizontally
  - Chunk size: 32 blocks wide x 30 blocks tall (10 surface + 20 underground)
  - Chunk priority system based on player proximity
  - Fixed vertical size (always 30 blocks) for consistency
  
#### Multithreading
- **Background Generation**:
  - Separate threads for terrain generation
  - Thread pool for chunk processing
  - Lock-free data structures for performance
- **Async Loading**:
  - Non-blocking chunk generation
  - Smooth gameplay during world generation
  
#### GPU Optimization
- **GPU-Accelerated Rendering**:
  - Batch rendering of 2D tiles
  - Sprite atlases for efficient texture access
  - Efficient layer rendering (background, terrain, entities, foreground)

#### Region-Based Data Management
- **Save/Load System**:
  - Region files for efficient horizontal storage
  - Stores both surface and 20-layer underground data
  - Incremental saving of modified chunks
  - World modification persistence (dug blocks, placed blocks)
- **Memory Management**:
  - Chunk pooling to reduce allocations
  - Aggressive unloading of distant chunks
  - Compression for stored chunk data

### 2.2 Rendering Optimization for 2D with Depth

#### Layer-Based Rendering
- **Rendering Strategy**:
  - Background layer (sky, distant mountains)
  - Terrain layer (surface blocks)
  - Underground layer (when visible/dug out)
  - Entity layer (characters, items)
  - Foreground layer (trees, structures)
  
#### Distance-Based Detail
- **LOD for Distant Chunks**:
  - Full detail: Nearby chunks (0-3 chunks away)
  - Reduced detail: Medium distance (4-8 chunks away)
  - Simplified rendering: Far distance (9-16 chunks away)
- **Underground Visibility**:
  - Only render underground blocks when player has dug them out
  - Fog of war for unexplored underground areas
  - Lighting affects visibility underground

### 2.3 Block and Material Systems

#### Block Types and Properties
- **Surface Blocks**:
  - Grass, dirt, sand, snow, stone (surface layer)
  - Each has specific texture and properties
- **Underground Blocks**:
  - Dirt (layers 0-3)
  - Stone (layers 4-14)
  - Deep stone/hardened rock (layers 15-19)
  - Bedrock (layer 20 - unbreakable)
  - Ore blocks: Coal, copper, iron, silver, gold, gems
  - Special blocks: Clay, gravel, underground water
- **Block Properties**:
  - Hardness (affects digging time)
  - Tool requirement (some blocks need specific tools)
  - Drop items (what you get when mining)
  - Light blocking (affects underground visibility)

#### Tile Rendering
- **Texture System**:
  - 16x16 or 32x32 pixel tiles
  - Texture atlases for efficiency
  - Tile variations for same block type
  - Connected textures for natural look
- **Visual Variation**:
  - Color tinting for biome-specific blocks
  - Random texture rotation for variety
  - Ambient occlusion for depth perception

### 2.4 World Interactivity and Digging System

#### Block Modification
- **Digging/Mining**:
  - Remove blocks by digging with tools
  - Progressive damage before block breaks
  - Tool requirements for harder materials
  - Drop resources when blocks are destroyed
- **Placing Blocks**:
  - Place blocks from inventory
  - Build structures on surface
  - Fill in holes underground
  - Crafting stations and structures
- **Digging Mechanics**:
  - Can dig downward to explore underground
  - Can dig horizontally to create tunnels
  - Cannot dig through bedrock (layer 20)
  - Digging time varies by block hardness and tool quality

#### Underground Exploration
- **Lighting System**:
  - Darkness underground (no sunlight below surface)
  - Torches and light sources required
  - Dynamic lighting for exploration atmosphere
  - Light sources can be placed while digging
- **Cave-ins (Optional)**:
  - Structural integrity for large underground chambers
  - Support beams to prevent collapse
  - Physics-based falling blocks

## Phase 3: Research and Reference Implementation

### 3.1 Games Using 2D Terrain with Vertical Depth - What They Do Well

#### Terraria (2011 - Present)
**What It Does Well:**
- **Deep Underground System**: Multiple distinct underground layers with unique content
- **Biome Diversity**: Surface biomes extend underground with unique caves
- **Mining Progression**: Tools get better, allowing access to deeper, harder materials
- **Hidden Treasures**: Chests, rare ores, and secrets reward exploration
- **Cave Systems**: Organic cave generation feels natural and explorable
- **Building**: Can build anywhere, both above and below ground

**Technical Highlights:**
- 2D tile-based world (8000+ blocks wide, 2400+ blocks deep)
- Perlin noise for surface terrain
- Cellular automata for cave generation
- Multiple underground "depth zones" with different content
- Ore veins using noise-based placement

**Underground Layers:**
```
Surface (0-50 blocks): Grass, trees, normal enemies
Underground (50-400): Stone caves, common ores, basic enemies
Cavern (400-1200): Large caves, better ores, tougher enemies
Underworld (1200+): Lava, hellstone, hardmode enemies
```

#### Starbound (2016)
**What It Does Well:**
- **Planet Generation**: Each planet has unique surface and underground
- **Background Layers**: Multiple visual layers create depth perception
- **Biome Variation**: Underground reflects surface biome characteristics
- **Ore Distribution**: Logical ore placement by depth
- **Underground Structures**: Ruins, bunkers, and dungeons underground

**Technical Highlights:**
- Procedural 2D worlds with seamless wrapping
- Noise-based terrain and cave generation
- JSON-defined biomes for easy modding
- Layer-based rendering system
- Quest markers guide exploration

#### Dig Dug (Classic Reference)
**What It Does Well (Despite Simplicity):**
- **Simple Depth Mechanic**: Clear understanding of digging down
- **Enemy Spawning**: Different enemies at different depths
- **Score by Depth**: Deeper = more valuable
- **Path Creation**: Player creates their own tunnels

#### Motherload (Flash Game)
**What It Does Well:**
- **Depth-Based Progression**: Must upgrade to dig deeper
- **Resource Management**: Fuel and cargo space force surface trips
- **Hazards by Depth**: Different dangers at different levels
- **Ore Value Scaling**: Deeper = more valuable resources

**Core Loop:**
```
1. Dig down to find valuable ores
2. Return to surface to sell
3. Upgrade drill, fuel tank, cargo
4. Dig deeper for better resources
5. Repeat
```

#### SteamWorld Dig (2013)
**What It Does Well:**
- **Metroidvania Digging**: Upgrades unlock deeper areas
- **Permanent Changes**: Digging is destructive and permanent
- **Light Management**: Darkness increases with depth
- **Vertical Platforming**: Climbing back up is part of the challenge
- **Strategic Digging**: Plan your path carefully (limited resources)

**Technical Highlights:**
- Hand-crafted main path with procedural side areas
- Lighting system affects gameplay
- Upgrade-gated progression
- Risk/reward depth mechanics

**Depth System:**
```
Level 1 (0-50m): Tutorial area, basic enemies, common ores
Level 2 (50-150m): Tougher enemies, need better drill
Level 3 (150-300m): Lava hazards, rare ores, boss enemies
```

#### Minecraft (2D Perspective)
**What It Does Well:**
- **Logical Layers**: Dirt → Stone → Deep slate → Bedrock
- **Ore Distribution**: Each ore has optimal depth range
- **Cave Systems**: Pre-generated caves to discover while mining
- **Risk vs Reward**: Deeper = better loot but harder enemies
- **Lighting Mechanics**: Torches required for safe mining

**Ore Distribution by Depth:**
```
Y 64+  : Coal, emerald (mountains)
Y 0-64 : Coal, iron, gold, redstone
Y 0-16 : Diamond, lapis, redstone (abundant)
Y -64  : Deep slate, ancient cities
```

### 3.2 Games Using Fractal Noise Maps for 2D Terrain - Implementation Details

#### Terraria - Multi-Layer Noise Approach
**Implementation:**
- **Surface Height**: Perlin noise creates rolling hills
  - Base frequency: 0.01 (gentle slopes)
  - Octaves: 4-5 for detail
  - Height variation: 20-40 blocks
  
- **Cave Systems**: Multiple noise layers create organic caves
  - Large cave noise: 0.02 frequency (big chambers)
  - Small cave noise: 0.05 frequency (tunnels)
  - Worley noise: Creates cellular patterns
  
- **Ore Veins**: Perlin noise determines ore placement
  - Each ore has unique noise seed
  - Depth-based probability threshold
  - Blob-like vein shapes

**Code Concept:**
```csharp
float GenerateSurfaceHeight(int x)
{
    float height = 100f; // Base level
    height += PerlinNoise(x * 0.01f) * 30f; // Major hills
    height += PerlinNoise(x * 0.05f) * 10f; // Small variation
    height += PerlinNoise(x * 0.1f) * 3f;   // Fine detail
    return height;
}

bool IsCave(int x, int y)
{
    float caveNoise = PerlinNoise(x * 0.02f, y * 0.02f);
    float threshold = 0.5f + (y * 0.001f); // More caves deeper
    return caveNoise > threshold;
}

bool IsOre(int x, int y, OreType oreType)
{
    if (y < oreType.MinDepth || y > oreType.MaxDepth)
        return false;
        
    float oreNoise = PerlinNoise(x * 0.08f, y * 0.08f, oreType.Seed);
    return oreNoise > oreType.Threshold;
}
```

#### Starbound - Planet-Specific Noise
**Implementation:**
- **Biome Selection**: Low-frequency noise determines biome zones
- **Terrain Shape**: Medium-frequency noise creates surface contours
- **Detail Layer**: High-frequency noise adds small variations
- **Underground Composition**: Noise determines block types by depth

**Biome Blending:**
```csharp
Biome GetBiomeAt(int x)
{
    float biomeNoise = PerlinNoise(x * 0.001f);
    float moistureNoise = PerlinNoise(x * 0.0015f + 1000f);
    
    if (biomeNoise < 0.3f) return Biome.Desert;
    if (biomeNoise < 0.6f && moistureNoise > 0.5f) return Biome.Forest;
    if (biomeNoise < 0.6f) return Biome.Plains;
    return Biome.Mountains;
}
```

#### No Man's Sky (2D) - Advanced Layering
**Implementation:**
- **Continental Scale**: Very low frequency (0.0001-0.0005)
- **Regional Features**: Low frequency (0.001-0.005)
- **Local Terrain**: Medium frequency (0.01-0.05)
- **Surface Detail**: High frequency (0.1-0.5)

**Domain Warping for Organic Shapes:**
```csharp
float DomainWarpedHeight(int x)
{
    // Warp the input coordinates
    float warpX = PerlinNoise(x * 0.01f) * 50f;
    float actualX = x + warpX;
    
    // Sample height at warped position
    float height = PerlinNoise(actualX * 0.005f) * 40f;
    return height;
}
```

#### Spelunky - Controlled Randomness
**Implementation:**
- **Room Templates**: Pre-designed room types
- **Noise for Variation**: Slight variations within templates
- **Path Guaranteed**: Ensures a valid path through level
- **Challenge Placement**: Noise determines trap/enemy positions

**Hybrid Approach:**
```csharp
void GenerateLevel()
{
    // Use grid-based layout
    Room[,] rooms = new Room[4, 4];
    
    // Perlin noise selects room types
    for (int x = 0; x < 4; x++)
    {
        for (int y = 0; y < 4; y++)
        {
            float noise = PerlinNoise(x * 0.25f, y * 0.25f);
            rooms[x, y] = SelectRoomType(noise);
        }
    }
    
    // Add noise-based decorations
    AddNoiseDecorations(rooms);
}
```

### 3.3 Examples of 2D Biomes with Underground Variations

#### Example 1: Terraria-Style Biomes

**Forest Biome (Surface + Underground):**
```
Surface:
- Blocks: Grass, dirt, mud
- Trees: Oak and maple (60% coverage)
- Features: Flowers, mushrooms, small ponds
- Creatures: Slimes, bunnies, birds

Underground (Layer 4-10):
- Blocks: Dirt (4-6), stone (7-10)
- Ores: Copper, tin (common)
- Features: Dirt caves, roots from trees
- Creatures: Cave bats, worms
- Lighting: Minimal, requires torches

Deep Underground (Layer 11-19):
- Blocks: Stone, hard stone
- Ores: Iron, silver (uncommon), gold (rare)
- Features: Larger cave pockets, underground water
- Creatures: Cave spiders, skeletons
- Special: Occasional treasure rooms
```

**Desert Biome:**
```
Surface:
- Blocks: Sand, sandstone
- Vegetation: Cacti, dead bushes (5% coverage)
- Features: Sand dunes, quicksand pits
- Creatures: Scorpions, vultures

Underground (Layer 4-10):
- Blocks: Sand (4-7), sandstone (8-10)
- Ores: Copper, topaz gems
- Features: Sand pockets (can fall), air gaps
- Creatures: Antlions, sand worms
- Hazard: Collapsing sand

Deep Underground (Layer 11-19):
- Blocks: Sandstone, limestone
- Ores: Gold, turquoise
- Features: Ancient tombs, mummy chambers
- Creatures: Mummies, sand elementals
- Special: Desert pyramid connections
```

**Snow/Ice Biome:**
```
Surface:
- Blocks: Snow, ice
- Trees: Pine trees (30% coverage)
- Features: Frozen ponds, snowdrifts
- Creatures: Penguins, ice slimes

Underground (Layer 4-10):
- Blocks: Frozen dirt (4-6), ice (7-10)
- Ores: Silver, sapphire gems
- Features: Ice caves, frozen water pockets
- Creatures: Ice bats, frozen zombies
- Special: Slippery ice surfaces

Deep Underground (Layer 11-19):
- Blocks: Ice, packed ice, stone
- Ores: Platinum, diamonds (rare)
- Features: Crystal caves, underground ice lakes
- Creatures: Ice elementals, yetis
- Special: Magical ice formations
```

#### Example 2: Resource Distribution by Depth

**Chronicle of a Drifter - Proposed System:**

```
Layer 0-3 (Topsoil):
- Primary: Dirt, grass, sand (biome dependent)
- Resources: Clay, flint
- Difficulty: Very Easy
- Tool: Bare hands or basic shovel
- Treasure: None

Layer 4-8 (Shallow Underground):
- Primary: Stone, compacted dirt
- Ores: Coal (common), Copper (common)
- Features: Small cave pockets (3-5 blocks)
- Difficulty: Easy
- Tool: Wooden pickaxe
- Treasure: Wooden chests (basic loot)

Layer 9-14 (Deep Underground):
- Primary: Hard stone, mineral deposits
- Ores: Iron (uncommon), Silver (uncommon), Tin (common)
- Features: Medium caves (5-8 blocks), underground streams
- Difficulty: Medium
- Tool: Iron pickaxe
- Treasure: Iron chests (good loot), rare gems
- Enemies: Cave creatures, undead

Layer 15-19 (Very Deep):
- Primary: Dense stone, crystal formations
- Ores: Gold (rare), Platinum (rare), Gems (uncommon)
- Features: Large chambers, treasure rooms
- Difficulty: Hard
- Tool: Steel pickaxe or better
- Treasure: Treasure chests (excellent loot), artifacts
- Enemies: Dangerous creatures, boss enemies
- Special: Ancient ruins, hidden chambers

Layer 20 (Bedrock):
- Primary: Indestructible bedrock
- Purpose: World floor boundary
- Cannot dig through
```

#### Example 3: Cave Generation Patterns

**Small Pocket Cave (Layer 4-8):**
```
     SSSS        S = Stone
    SCCCCS       C = Cave (Air)
   SCCCCCS       O = Ore
   SCCCCCS
    SCOOCS
     SSSS
```

**Medium Chamber (Layer 9-14):**
```
      SSSSSSS
    SSSCCCCCSS      T = Treasure Chest
   SSCCCCCCCCSS     W = Water
  SSCCCCCCCCCCS     
  SCCCCCTCCCCCS
  SCCCCCCCCCCCS
   SSCCWWWCCSS
     SSSSSSS
```

**Large Treasure Room (Layer 15-19):**
```
        SSSSSSSSSSS
      SSSCCCCCCCCSSS
    SSSCCCCCCCCCCCSS     T = Major Treasure
   SSCCCCCCCCCCCCCCSS    G = Gems/Ores
   SCCCCCOOOOOCCCCCS     B = Boss Enemy
   SCCCCOGGTGGOCCCS      L = Light Source
   SCCCCOGTBTGOCCCS
   SCCCCOGGTGOCCCS
   SCCCCCOOOOOCCCS
    SSCCLLCLLCCSS
      SSSSSSSSS
```

#### Example 4: Biome Transition Underground

**Forest → Desert Transition:**
```
Horizontal Position:  0    10   20   30   40   50
                      |Forest |Trans|Desert|

Surface Blocks:      Grass→Mixed→Sand
Surface Trees:       Oak→Sparse→Cacti

Layer 4-8:
Forest Side:         Dirt + stone
Transition:          Mixed dirt/sand + sandstone  
Desert Side:         Sand + sandstone

Layer 9-14:
Forest Side:         Stone + copper/iron
Transition:          Stone + mixed ores
Desert Side:         Sandstone + topaz/copper

Layer 15-19:
All Areas:           Hard stone + rare ores
                     (Deeper layers are more uniform)
```

## Phase 4: Implementation Priorities

### High Priority (Phase 1)
1. ✅ Basic ECS system (COMPLETED)
2. ✅ Lua scripting integration (COMPLETED)
3. ✅ 2D camera system with smooth following (COMPLETED)
4. ✅ Orthographic projection camera (COMPLETED - inherent in 2D design)
5. ✅ Parallax scrolling system for depth illusion (COMPLETED)
6. ✅ Camera look-ahead based on player movement (COMPLETED)
7. ⬜ Semi-angled sprite art (25-45° perspective in sprites)
8. ✅ Basic 2D tile/block system (32 tiles wide × 30 tiles tall per chunk) (COMPLETED)
9. ✅ Simple terrain generation with Perlin noise for surface (COMPLETED)
10. ✅ Basic biome system (3-4 biomes) (COMPLETED - Forest, Plains, Desert)
11. ✅ Underground layer generation (20 blocks deep) (COMPLETED)
12. ✅ Chunk loading/unloading (horizontal) (COMPLETED)

### Medium Priority (Phase 2)
1. ✅ Multi-layer parallax backgrounds (3-5 layers) (COMPLETED - Sky, Clouds, Mountains, Stars, Mist)
2. ✅ Advanced camera features (screen shake, camera zones) (COMPLETED)
3. ✅ Underground cave pocket generation (COMPLETED)
4. ✅ Multiple biome types (8+ biomes) (COMPLETED - 8 biomes implemented)
5. ✅ Tree and vegetation generation on surface (COMPLETED - 7 vegetation types)
6. ✅ Water body generation (rivers, lakes, oceans) (COMPLETED - 3 water types)
7. ✅ Block digging/mining system (COMPLETED)
8. ✅ Underground lighting and fog of war (COMPLETED)
9. ✅ Resource drops and inventory (COMPLETED)
10. ⬜ Multithreaded chunk generation

### Lower Priority (Phase 3)
1. ✅ Advanced parallax effects (animated clouds, weather) (COMPLETED - Clouds and mist with auto-scroll)
2. ⬜ Cinematic camera movements for cutscenes
3. ⬜ Creature spawning system by biome and depth
4. ⬜ Structure generation (ruins, villages, treasure rooms)
5. ✅ Advanced lighting effects (torches, dynamic shadows) (COMPLETED - Light sources with distance falloff)
6. ⬜ Weather and atmospheric effects (rain, snow)
7. ⬜ Seasonal changes affecting surface appearance
8. ✅ Tool progression system (better tools = dig faster/deeper) (COMPLETED)
9. ⬜ Structural integrity for large underground chambers

## Phase 4: Camera System for Semi-Angled Top-Down View

### 4.1 Orthographic Projection Camera

**Core Principle**: Use an orthographic camera looking straight down (not perspective). The "semi-angled" look comes from the art/sprites themselves, not from camera trickery.

#### Why Orthographic?
- **Consistent Scale**: Objects don't appear smaller when "further away"
- **Predictable Gameplay**: Players can judge distances accurately
- **2D-Friendly**: Perfect for top-down and isometric-style games
- **Performance**: Simpler rendering calculations than perspective

#### Implementation
```csharp
// Chronicles of a Drifter already uses orthographic projection
// The CameraComponent uses direct 2D transformations:
var (screenX, screenY) = camera.WorldToScreen(worldX, worldY);

// No perspective division, just:
// screenX = (worldX - cameraX) * zoom + viewportWidth / 2
// screenY = (worldY - cameraY) * zoom + viewportHeight / 2
```

### 4.2 Semi-Angled Art Style (Not Camera Angle!)

**Key Concept**: The camera looks straight down (0°), but sprites and tiles are drawn at a fixed angle.

#### Recommended Angles
- **45°**: Classic isometric (equal x and y projection)
- **30°**: Steeper isometric (more top-down feeling)
- **25°**: Zelda-style (slight angle, mostly top-down)
- **60°**: Shallow angle (more side-view)

#### Art Pipeline
```
1. Create sprites/tiles at chosen angle (e.g., 45°)
2. Export as 2D images
3. Render with orthographic camera
4. Result: Consistent "2.5D" appearance
```

#### Example Sprite Specifications
```
Character Sprite (45° angle):
- Base: 32×32 pixels
- Head drawn at 45° angle
- Shoulders visible from top-down perspective
- Feet at bottom of sprite
- Shadow underneath (circular)

Tile Sprite (45° angle):
- Size: 32×32 pixels
- Top face visible
- Front edge visible at angle
- Consistent lighting from top-left
```

### 4.3 Parallax Scrolling System

**Purpose**: Create illusion of depth by moving background layers at different speeds.

#### Layer Structure (Back to Front)
```
Layer 0: Far mountains      (parallax factor: 0.2 - slowest)
Layer 1: Distant trees      (parallax factor: 0.4)
Layer 2: Mid-ground         (parallax factor: 0.7)
Layer 3: Ground/Terrain     (parallax factor: 1.0 - moves with camera)
Layer 4: Player/Entities    (parallax factor: 1.0)
Layer 5: Foreground trees   (parallax factor: 1.2 - faster than camera)
```

#### Implementation
```csharp
public class ParallaxLayer
{
    public Texture Texture { get; set; }
    public float ParallaxFactor { get; set; }  // 0.0 to 2.0
    public float ScrollSpeedX { get; set; }    // Optional auto-scroll
    public float ScrollSpeedY { get; set; }
    public int ZOrder { get; set; }            // Rendering order
}

public class ParallaxSystem : ISystem
{
    private List<ParallaxLayer> _layers;
    
    public void Update(World world, float deltaTime)
    {
        var camera = CameraSystem.GetActiveCamera(world);
        if (camera == null) return;
        
        foreach (var layer in _layers.OrderBy(l => l.ZOrder))
        {
            // Calculate parallax offset
            float offsetX = camera.X * layer.ParallaxFactor;
            float offsetY = camera.Y * layer.ParallaxFactor;
            
            // Optional auto-scroll (e.g., for clouds)
            offsetX += layer.ScrollSpeedX * totalTime;
            offsetY += layer.ScrollSpeedY * totalTime;
            
            // Render layer at calculated offset
            RenderLayer(layer, offsetX, offsetY);
        }
    }
}
```

#### Best Practices
- **Background layers**: Use parallax < 1.0 (move slower than camera)
- **Foreground layers**: Use parallax > 1.0 (move faster than camera)
- **Main gameplay layer**: Always use parallax = 1.0
- **Vertical parallax**: More effective for top-down games to show height
- **Repeating textures**: Use seamless tiling for infinite backgrounds

### 4.4 Smooth Following (Damped Follow)

**Already Implemented**: Chronicles of a Drifter uses exponential smoothing via Lerp.

#### Current Implementation
```csharp
// In CameraSystem.cs
float t = 1.0f - MathF.Exp(-followSpeed * deltaTime);
camera.X = Lerp(camera.X, target.X, t);
camera.Y = Lerp(camera.Y, target.Y, t);
```

#### Follow Speed Recommendations
- **Slow (2-4)**: Cinematic, exploration games
- **Medium (5-8)**: ✅ **Recommended for Chronicles of a Drifter**
- **Fast (10-15)**: Action games, combat focus
- **Instant (0)**: Puzzle games, strategic games

### 4.5 Look-Ahead Feature

**Purpose**: Shift camera focus in the direction of player movement, showing more of what's ahead.

#### Implementation
```csharp
public class CameraLookAheadSystem : ISystem
{
    private float _lookAheadDistance = 100f;  // How far ahead to look
    private float _lookAheadSpeed = 3f;       // How fast to adjust
    private Vector2 _currentLookAhead;
    
    public void Update(World world, float deltaTime)
    {
        var camera = CameraSystem.GetActiveCamera(world);
        if (camera?.FollowTarget == null) return;
        
        var targetPos = world.GetComponent<PositionComponent>(camera.FollowTarget);
        var targetVel = world.GetComponent<VelocityComponent>(camera.FollowTarget);
        
        if (targetPos != null && targetVel != null)
        {
            // Calculate desired look-ahead offset based on velocity
            float speed = MathF.Sqrt(targetVel.VX * targetVel.VX + targetVel.VY * targetVel.VY);
            
            if (speed > 0.1f)  // Only look ahead if moving
            {
                Vector2 direction = new Vector2(
                    targetVel.VX / speed,
                    targetVel.VY / speed
                );
                
                Vector2 targetLookAhead = direction * _lookAheadDistance;
                
                // Smoothly interpolate current look-ahead to target
                float t = 1.0f - MathF.Exp(-_lookAheadSpeed * deltaTime);
                _currentLookAhead.X = Lerp(_currentLookAhead.X, targetLookAhead.X, t);
                _currentLookAhead.Y = Lerp(_currentLookAhead.Y, targetLookAhead.Y, t);
            }
            else
            {
                // Return to center when stopped
                float t = 1.0f - MathF.Exp(-_lookAheadSpeed * deltaTime);
                _currentLookAhead.X = Lerp(_currentLookAhead.X, 0, t);
                _currentLookAhead.Y = Lerp(_currentLookAhead.Y, 0, t);
            }
            
            // Adjust camera follow target with look-ahead offset
            // Note: Don't modify target entity, modify camera's target position
            camera.X += _currentLookAhead.X / 10f;  // Scale down for subtle effect
            camera.Y += _currentLookAhead.Y / 10f;
        }
    }
}
```

#### When to Use Look-Ahead
- ✅ **Fast-paced action**: Player needs to see threats ahead
- ✅ **Racing/running games**: Essential for forward visibility
- ✅ **Platformers**: Helps with jump planning
- ❌ **Slow exploration**: Can be disorienting
- ❌ **Puzzle games**: Not needed, can be distracting

### 4.6 Camera Bounds

**Already Implemented**: Chronicles of a Drifter has camera bounds via `SetBounds()`.

#### Implementation Considerations for 2D World
```csharp
// For a world with chunks that are 32 blocks wide × 30 blocks tall
// With 16 chunks loaded (512 blocks wide)
int worldWidth = 512 * blockSize;  // e.g., 512 * 32 = 16,384 pixels
int worldHeight = 30 * blockSize;  // e.g., 30 * 32 = 960 pixels

// Camera bounds should account for viewport size
float halfViewportWidth = camera.ViewportWidth / (2.0f * camera.Zoom);
float halfViewportHeight = camera.ViewportHeight / (2.0f * camera.Zoom);

CameraSystem.SetBounds(world, cameraEntity,
    minX: halfViewportWidth,
    maxX: worldWidth - halfViewportWidth,
    minY: halfViewportHeight,
    maxY: worldHeight - halfViewportHeight
);
```

#### Dynamic Bounds for Infinite Worlds
```csharp
// Update bounds as player explores
public void UpdateCameraBounds(World world, Entity camera, int chunksLoaded)
{
    int worldWidth = chunksLoaded * chunkWidth * blockSize;
    
    var cam = world.GetComponent<CameraComponent>(camera);
    float halfViewportWidth = cam.ViewportWidth / (2.0f * cam.Zoom);
    float halfViewportHeight = cam.ViewportHeight / (2.0f * cam.Zoom);
    
    // Expand bounds as world grows
    CameraSystem.SetBounds(world, camera,
        minX: halfViewportWidth,
        maxX: worldWidth - halfViewportWidth,
        minY: halfViewportHeight,
        maxY: (30 * blockSize) - halfViewportHeight  // Fixed vertical
    );
}
```

### 4.7 Advanced Camera Features

#### Screen Shake
```csharp
public class ScreenShakeEffect
{
    private float _intensity;
    private float _duration;
    private float _elapsed;
    
    public void Trigger(float intensity, float duration)
    {
        _intensity = intensity;
        _duration = duration;
        _elapsed = 0;
    }
    
    public Vector2 GetOffset(float deltaTime)
    {
        if (_elapsed >= _duration) return Vector2.Zero;
        
        _elapsed += deltaTime;
        float progress = _elapsed / _duration;
        float currentIntensity = _intensity * (1.0f - progress);  // Fade out
        
        return new Vector2(
            (Random.Shared.NextSingle() * 2 - 1) * currentIntensity,
            (Random.Shared.NextSingle() * 2 - 1) * currentIntensity
        );
    }
}

// Usage in CameraSystem:
var shakeOffset = _screenShake.GetOffset(deltaTime);
camera.X += shakeOffset.X;
camera.Y += shakeOffset.Y;
```

#### Camera Zones
```csharp
public class CameraZone
{
    public Rectangle Bounds { get; set; }
    public float FollowSpeed { get; set; }
    public float Zoom { get; set; }
    public bool EnableLookAhead { get; set; }
}

// Change camera behavior when entering different zones
// E.g., slower follow in safe town, fast follow in combat area
```

### 4.8 Performance Optimization

#### Rendering with Camera
```csharp
// Only render what's visible in camera viewport
public void RenderVisibleTiles(CameraComponent camera)
{
    // Calculate visible tile range
    int minTileX = (int)((camera.X - viewportHalfWidth) / tileSize);
    int maxTileX = (int)((camera.X + viewportHalfWidth) / tileSize) + 1;
    int minTileY = (int)((camera.Y - viewportHalfHeight) / tileSize);
    int maxTileY = (int)((camera.Y + viewportHalfHeight) / tileSize) + 1;
    
    // Only render tiles in visible range
    for (int y = minTileY; y <= maxTileY; y++)
    {
        for (int x = minTileX; x <= maxTileX; x++)
        {
            RenderTile(x, y);
        }
    }
}
```

#### Parallax Layer Optimization
```csharp
// Don't update/render parallax layers every frame if they barely move
if (Math.Abs(camera.X - _lastCameraX) > 5f)
{
    UpdateParallaxLayers();
    _lastCameraX = camera.X;
}
```

## Technical Resources

### Noise Libraries
- **FastNoiseLite**: Excellent C++ noise library with many algorithms
- **libnoise**: Classic noise generation library
- **OpenSimplex2**: Improved Simplex noise implementation

### 2D Tile Rendering References
- **Sprite Batching**: Combine multiple sprites into single draw calls
- **Tile Culling**: Only render tiles visible in camera viewport
- **Texture Atlases**: Pack multiple tiles into single texture for efficiency

### Recommended Reading
- "Procedural Generation in Game Design" - Tanya X. Short
- Terraria's world generation techniques
- Starbound's modding documentation
- "2D Game Development with Unity" - Jared Halpern

## Success Metrics

### Performance Targets
- Chunk generation: <30ms per chunk (32 blocks wide × 30 blocks tall)
- Render distance: 16-32 chunks horizontally
- Frame rate: 60 FPS minimum at 1080p
- Memory: <2GB for world data (2D is lighter than 3D)
- Underground rendering: No performance drop when digging

### Quality Targets
- Biome diversity: 8-12 distinct surface biomes
- Underground layers: 20 blocks deep with distinct content per layer
- Vegetation coverage: Natural-looking density per biome
- Visual quality: Comparable to Terraria/Starbound
- Camera feel: Smooth following with optional parallax depth

---

**Note**: This roadmap is a living document and will be updated as development progresses. The 2D world generation system with vertical depth builds upon the existing 2D camera and ECS systems already implemented in Chronicles of a Drifter.
