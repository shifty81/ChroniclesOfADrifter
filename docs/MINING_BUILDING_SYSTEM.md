# Mining and Building System

## Overview

Chronicles of a Drifter features a comprehensive block interaction system that allows players to mine resources from the terrain, collect them in their inventory, and place blocks to build structures.

## Components

### InventoryComponent

Manages the player's resource collection system.

**Features:**
- Stores up to 40 different item types (configurable)
- Tracks quantity for each item type
- Add/remove items with validation
- Query item counts and availability

**Usage:**
```csharp
// Create inventory
var inventory = new InventoryComponent(maxSlots: 40);
World.AddComponent(player, inventory);

// Add items
inventory.AddItem(TileType.Stone, 10);
inventory.AddItem(TileType.CopperOre, 3);

// Check if has items
if (inventory.HasItem(TileType.Dirt, 5))
{
    // Remove items
    inventory.RemoveItem(TileType.Dirt, 5);
}

// Get all items
var items = inventory.GetAllItems();
foreach (var item in items)
{
    Console.WriteLine($"{item.Key}: {item.Value}");
}
```

### ToolComponent

Tracks the currently equipped tool and its properties.

**Tool Types:**
- `None` - Bare hands
- `Pickaxe` - For mining stone and ores
- `Axe` - For chopping trees
- `Shovel` - For digging dirt and sand

**Tool Materials:**
- `None` (0.5x mining power) - Bare hands
- `Wood` (1.0x) - Basic wooden tools
- `Stone` (1.5x) - Stone tools
- `Iron` (2.5x) - Iron tools
- `Steel` (4.0x) - Steel tools (advanced)

**Usage:**
```csharp
// Create and equip a tool
var tool = new ToolComponent(ToolType.Pickaxe, ToolMaterial.Stone);
World.AddComponent(player, tool);

// Get mining power
float power = tool.MiningPower; // 1.5x for stone pickaxe
```

## Block Properties

### Hardness

Every block has a hardness value that determines how long it takes to mine:

| Block Type | Hardness (seconds with bare hands) |
|------------|-----------------------------------|
| Tall Grass | 0.1s |
| Flower | 0.1s |
| Bush | 0.3s |
| Dirt | 0.5s |
| Sand | 0.5s |
| Grass | 0.6s |
| Snow | 0.6s |
| Trees | 2.0s |
| Cactus | 1.5s |
| Stone | 5.0s |
| Copper Ore | 6.0s |
| Iron Ore | 8.0s |
| Deep Stone | 10.0s |
| Gold Ore | 12.0s |
| Bedrock | ∞ (unbreakable) |

**Mining Time Formula:**
```
Mining Time = Block Hardness / Tool Mining Power
```

### Tool Requirements

Some blocks require specific tools to mine efficiently:

| Block Type | Required Tool | Minimum Material |
|------------|---------------|------------------|
| Stone | Pickaxe | Wood |
| Copper Ore | Pickaxe | Wood |
| Iron Ore | Pickaxe | Stone |
| Deep Stone | Pickaxe | Stone |
| Gold Ore | Pickaxe | Iron |
| Trees | Axe | None |
| Dirt/Sand | Shovel | None |

**Mining Speed Penalties:**
- Correct tool + sufficient material: 100% speed (tool's full mining power)
- Wrong tool type: 50% speed
- Insufficient tool material: 10% speed (very slow)

### Resource Drops

When a block is mined, it drops resources:

| Block Mined | Item Dropped | Quantity |
|-------------|--------------|----------|
| Stone | Stone | 1 |
| Copper Ore | Copper Ore | 1 |
| Iron Ore | Iron Ore | 1 |
| Gold Ore | Gold Ore | 1 |
| Dirt | Dirt | 1 |
| Sand | Sand | 1 |
| Grass | Dirt | 1 |
| Trees | Tree (type) | 1 |
| Tall Grass | Nothing | 0 |
| Flowers | Nothing | 0 |

## Systems

### BlockInteractionSystem

Handles all player interactions with blocks, including mining and placement.

**Features:**
- Block mining with tool requirements
- Block placement from inventory
- Progress tracking for mining
- Range checking for interactions
- Inventory management integration

**Controls:**
- Hold **M** to mine blocks near you
- Press **P** to place selected block
- Press **1-9** to select block type from inventory

**Configuration:**
```csharp
private const float INTERACTION_REACH = 3.0f; // blocks
```

## Usage Example

### Setting Up a Player with Mining Capabilities

```csharp
// Create player
var player = World.CreateEntity();
World.AddComponent(player, new PlayerComponent { Speed = 100.0f });
World.AddComponent(player, new PositionComponent(500, 150));
World.AddComponent(player, new VelocityComponent());

// Add inventory
var inventory = new InventoryComponent(maxSlots: 40);
World.AddComponent(player, inventory);

// Equip starting tool
var tool = new ToolComponent(ToolType.Pickaxe, ToolMaterial.Stone);
World.AddComponent(player, tool);

// Add system to world
var blockSystem = new BlockInteractionSystem();
World.AddSystem(blockSystem);

// Store chunk manager as shared resource
World.SetSharedResource("ChunkManager", chunkManager);
```

### Mining Workflow

1. **Approach a block** - Get within 3 blocks range
2. **Hold M key** - Start mining
3. **Wait for completion** - Mining progress accumulates based on:
   - Block hardness
   - Tool type and material
   - Tool mining power
4. **Collect resource** - Item automatically added to inventory
5. **Block removed** - Becomes air, revealing blocks behind

### Building Workflow

1. **Mine resources** - Collect blocks to place
2. **Select block type** - Press 1-9 to choose from inventory
3. **Position yourself** - Stand where you want to place
4. **Press P** - Block placed if:
   - You have the block in inventory
   - Target position is empty (air)
   - Within interaction reach (3 blocks)
5. **Item consumed** - Block removed from inventory

## Mining Demo Scene

The `MiningDemoScene` showcases the mining and building system:

```bash
dotnet run mining
```

**Features:**
- Procedural terrain generation
- Player starts with Stone Pickaxe
- Mine blocks to collect resources
- Build structures with collected blocks
- Inventory display every 5 seconds
- Final inventory summary on exit

## Performance Considerations

### Inventory System
- Dictionary-based storage for O(1) lookups
- Maximum slot limit prevents unbounded growth
- Memory efficient: only stores non-zero quantities

### Mining System
- Only processes mining when M key is held
- Single mining operation at a time
- Progress resets if player moves away
- Minimal CPU overhead

### Block Placement
- Validates placement before modification
- Checks inventory before allowing placement
- Immediate chunk modification (no lag)

## Future Enhancements

### Planned Features
1. **Durability System** - Tools degrade with use
2. **Crafting System** - Combine resources to create tools
3. **Advanced Placement** - Directional placement (above, sides)
4. **Quick Select** - Hotbar for instant block selection
5. **Mining Speed Display** - Visual progress indicator
6. **Sound Effects** - Mining and placement sounds
7. **Particle Effects** - Visual feedback for breaking blocks
8. **Stack Limits** - Max quantity per item type
9. **Tool Switching** - Quick swap between equipped tools
10. **Auto-Mining** - Hold position to continue mining

### Technical Improvements
1. **Mouse-Based Targeting** - Click blocks directly to mine/place
2. **Raycast Selection** - Precise block targeting with cursor
3. **Range Indicators** - Visual display of interaction range
4. **Undo System** - Revert recent block changes
5. **Blueprint System** - Save and load build patterns
6. **Multiplayer Support** - Sync block changes across clients

## Integration with Other Systems

### Terrain System
- Block changes modify chunk data
- Chunks marked as modified for saving
- Underground blocks can be mined to explore deeper

### Camera System
- Follows player during mining/building
- Smooth transitions when moving between areas

### Future: Lighting System
- Underground areas will require torches
- Blocks can be light sources
- Dynamic lighting when placing/removing blocks

### Future: Collision System
- Placed blocks create collision geometry
- Mined blocks remove collision
- Building affects player movement

## Tips and Best Practices

### For Players
1. **Start Simple** - Use bare hands for soft blocks (dirt, sand)
2. **Get Tools Fast** - Craft a pickaxe to mine stone efficiently
3. **Collect Variety** - Mine different blocks for building options
4. **Plan Builds** - Check inventory before starting large projects
5. **Upgrade Tools** - Better materials = faster mining

### For Developers
1. **Balance Hardness** - Ensure mining feels satisfying, not tedious
2. **Test Tool Progression** - Verify upgrade path makes sense
3. **Clear Feedback** - Always inform player of actions/failures
4. **Validate Interactions** - Check range, inventory, and state
5. **Optimize Chunks** - Minimize chunk updates during building

## Troubleshooting

### "Inventory Full" Message
- Maximum 40 different item types
- Solution: Use existing items or expand max slots

### "Cannot Place Block - Space Occupied"
- Target location contains a block
- Solution: Mine existing block first

### "Too Far to Place Block"
- Outside 3-block interaction range
- Solution: Move closer to target location

### Slow Mining Speed
- Wrong tool type or insufficient material
- Solution: Equip correct tool with adequate material quality

### "No [BlockType] in Inventory"
- Trying to place block you don't have
- Solution: Mine that block type first

## API Reference

### TileTypeExtensions

Mining-related extension methods for TileType enum:

```csharp
// Get block properties
float hardness = TileType.Stone.GetHardness();
ToolType requiredTool = TileType.IronOre.GetRequiredToolType();
ToolMaterial minMaterial = TileType.GoldOre.GetMinimumToolMaterial();
bool canMine = TileType.Bedrock.IsMineable();

// Get drops
TileType droppedItem = TileType.Grass.GetDroppedItem(); // Returns Dirt
int quantity = TileType.Stone.GetDropQuantity(); // Returns 1
```

### BlockInteractionSystem Methods

```csharp
// Check mining state
bool isMining = blockInteractionSystem.IsMining();
float progress = blockInteractionSystem.GetMiningProgress(); // 0.0 to 1.0

// Get selected block
TileType selected = blockInteractionSystem.GetSelectedBlockType();
```

## Conclusion

The Mining and Building System provides a solid foundation for resource gathering and construction in Chronicles of a Drifter. It integrates seamlessly with the terrain generation system and provides satisfying gameplay mechanics through tool progression, block variety, and creative building opportunities.
