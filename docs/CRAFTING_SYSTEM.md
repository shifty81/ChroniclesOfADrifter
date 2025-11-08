# Crafting System Documentation

## Overview

The crafting system in Chronicles of a Drifter allows players to create items, tools, and building materials from resources collected in the world. It features a recipe-based system with material requirements and produces various outputs.

## Components

### CraftingSystem

The `CraftingSystem` manages all crafting recipes and handles the crafting process.

**Key Methods:**
- `AddRecipe(CraftingRecipe recipe)` - Add a new recipe to the system
- `TryCraft(CraftingRecipe recipe, InventoryComponent inventory)` - Attempt to craft an item
- `CanCraft(CraftingRecipe recipe, InventoryComponent inventory)` - Check if a recipe can be crafted
- `GetCraftableRecipes(InventoryComponent inventory)` - Get all recipes that can be crafted
- `GetRecipesByCategory(string category)` - Get recipes in a specific category

### CraftingRecipe

Represents a single crafting recipe with inputs and outputs.

**Properties:**
- `Name` - Human-readable name of the recipe
- `Output` - The TileType that will be created
- `OutputQuantity` - Number of items produced
- `Ingredients` - Dictionary of required materials and quantities
- `Category` - Recipe category (Tools, Building, Lighting, etc.)

## Recipe Categories

### Tools
Recipes for creating tools and equipment:
- Wooden Pickaxe (placeholder - uses Wood output)

### Building
Recipes for construction materials:
- **Wood Planks** - Convert tree logs into planks (4 planks per log)
  - Oak, Pine, or Palm trees → 4 Wood Planks
- **Wood Blocks** - Solid wood construction blocks
  - 4 Wood Planks → 1 Wood Block
- **Cobblestone** - Basic stone building material
  - 1 Stone → 1 Cobblestone
- **Bricks** - Refined building material
  - 2 Stone + 2 Dirt → 4 Bricks

### Lighting
Recipes for light sources:
- **Torch** - Portable light source
  - 1 Wood Plank + 1 Copper Ore → 4 Torches

## Usage Example

```csharp
// Create the crafting system
var craftingSystem = new CraftingSystem();
craftingSystem.Initialize(world);

// Get player's inventory
var inventory = world.GetComponent<InventoryComponent>(playerEntity);

// Check what can be crafted
var craftableRecipes = craftingSystem.GetCraftableRecipes(inventory);

// Craft an item
var recipe = craftableRecipes.First();
bool success = craftingSystem.TryCraft(recipe, inventory);

// Or craft by name
bool success = craftingSystem.TryCraftByName("Wood Planks", inventory);
```

## Integration with Inventory

The crafting system integrates with the `InventoryComponent`:
- Checks for required materials using `HasItem()`
- Removes ingredients using `RemoveItem()`
- Adds crafted items using `AddItem()`
- Returns ingredients if inventory is full

## Adding New Recipes

To add a new recipe, create a `CraftingRecipe` and add it in `InitializeRecipes()`:

```csharp
AddRecipe(new CraftingRecipe(
    "Iron Pickaxe",
    TileType.IronPickaxe,  // Output item
    1,                      // Output quantity
    new Dictionary<TileType, int>
    {
        { TileType.IronOre, 3 },
        { TileType.Wood, 2 }
    },
    "Tools"
));
```

## Testing

Run crafting system tests:
```bash
dotnet run -- crafting-test
```

Run interactive crafting demo:
```bash
dotnet run -- crafting
```

## Future Enhancements

- Tool items as separate types (currently using placeholder TileTypes)
- Crafting stations (furnace, workbench, etc.)
- Recipe unlocking/discovery system
- Crafting skill progression
- Time-based crafting (items take time to craft)
- Batch crafting (craft multiple items at once)
