using ChroniclesOfADrifter.ECS.Components;
using System.Collections.Generic;
using System.Linq;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// Crafting recipe definition
/// </summary>
public class CraftingRecipe
{
    public string Name { get; set; }
    public TileType Output { get; set; }
    public int OutputQuantity { get; set; }
    public Dictionary<TileType, int> Ingredients { get; set; }
    public string Category { get; set; }
    
    public CraftingRecipe(string name, TileType output, int outputQuantity, Dictionary<TileType, int> ingredients, string category = "General")
    {
        Name = name;
        Output = output;
        OutputQuantity = outputQuantity;
        Ingredients = ingredients;
        Category = category;
    }
}

/// <summary>
/// System that handles crafting items from recipes
/// </summary>
public class CraftingSystem : ISystem
{
    private List<CraftingRecipe> recipes;
    
    public CraftingSystem()
    {
        recipes = new List<CraftingRecipe>();
        InitializeRecipes();
    }
    
    public void Initialize(World world)
    {
        Console.WriteLine("[Crafting] Crafting system initialized with " + recipes.Count + " recipes");
    }
    
    public void Update(World world, float deltaTime)
    {
        // Crafting is done on-demand via TryCraft method, not in Update loop
    }
    
    /// <summary>
    /// Initialize all crafting recipes
    /// </summary>
    private void InitializeRecipes()
    {
        // Tool crafting recipes
        
        // Wooden tools
        AddRecipe(new CraftingRecipe(
            "Wooden Pickaxe",
            TileType.Wood, // Using Wood as placeholder for tool (would need ToolItem types later)
            1,
            new Dictionary<TileType, int>
            {
                { TileType.Wood, 3 },
                { TileType.TreeOak, 2 } // Sticks from trees
            },
            "Tools"
        ));
        
        // Building materials
        
        // Wood planks from trees
        AddRecipe(new CraftingRecipe(
            "Wood Planks",
            TileType.WoodPlank,
            4,
            new Dictionary<TileType, int>
            {
                { TileType.TreeOak, 1 }
            },
            "Building"
        ));
        
        AddRecipe(new CraftingRecipe(
            "Wood Planks (Pine)",
            TileType.WoodPlank,
            4,
            new Dictionary<TileType, int>
            {
                { TileType.TreePine, 1 }
            },
            "Building"
        ));
        
        AddRecipe(new CraftingRecipe(
            "Wood Planks (Palm)",
            TileType.WoodPlank,
            4,
            new Dictionary<TileType, int>
            {
                { TileType.TreePalm, 1 }
            },
            "Building"
        ));
        
        // Wood blocks from planks
        AddRecipe(new CraftingRecipe(
            "Wood Block",
            TileType.Wood,
            1,
            new Dictionary<TileType, int>
            {
                { TileType.WoodPlank, 4 }
            },
            "Building"
        ));
        
        // Cobblestone from stone
        AddRecipe(new CraftingRecipe(
            "Cobblestone",
            TileType.Cobblestone,
            1,
            new Dictionary<TileType, int>
            {
                { TileType.Stone, 1 }
            },
            "Building"
        ));
        
        // Bricks from stone and resources
        AddRecipe(new CraftingRecipe(
            "Brick",
            TileType.Brick,
            4,
            new Dictionary<TileType, int>
            {
                { TileType.Stone, 2 },
                { TileType.Dirt, 2 }
            },
            "Building"
        ));
        
        // Torches for lighting
        AddRecipe(new CraftingRecipe(
            "Torch",
            TileType.Torch,
            4,
            new Dictionary<TileType, int>
            {
                { TileType.WoodPlank, 1 },
                { TileType.CopperOre, 1 } // Using copper as fuel/ignition source
            },
            "Lighting"
        ));
    }
    
    /// <summary>
    /// Add a recipe to the crafting system
    /// </summary>
    public void AddRecipe(CraftingRecipe recipe)
    {
        recipes.Add(recipe);
    }
    
    /// <summary>
    /// Get all available recipes
    /// </summary>
    public IReadOnlyList<CraftingRecipe> GetAllRecipes()
    {
        return recipes.AsReadOnly();
    }
    
    /// <summary>
    /// Get recipes by category
    /// </summary>
    public List<CraftingRecipe> GetRecipesByCategory(string category)
    {
        return recipes.Where(r => r.Category == category).ToList();
    }
    
    /// <summary>
    /// Get all recipe categories
    /// </summary>
    public List<string> GetCategories()
    {
        return recipes.Select(r => r.Category).Distinct().ToList();
    }
    
    /// <summary>
    /// Check if a recipe can be crafted with the given inventory
    /// </summary>
    public bool CanCraft(CraftingRecipe recipe, InventoryComponent inventory)
    {
        foreach (var ingredient in recipe.Ingredients)
        {
            if (!inventory.HasItem(ingredient.Key, ingredient.Value))
            {
                return false;
            }
        }
        return true;
    }
    
    /// <summary>
    /// Get recipes that can be crafted with the current inventory
    /// </summary>
    public List<CraftingRecipe> GetCraftableRecipes(InventoryComponent inventory)
    {
        return recipes.Where(r => CanCraft(r, inventory)).ToList();
    }
    
    /// <summary>
    /// Attempt to craft an item from a recipe
    /// </summary>
    /// <returns>True if crafting was successful</returns>
    public bool TryCraft(CraftingRecipe recipe, InventoryComponent inventory)
    {
        // Check if we have all ingredients
        if (!CanCraft(recipe, inventory))
        {
            Console.WriteLine($"[Crafting] Cannot craft {recipe.Name}: missing ingredients");
            return false;
        }
        
        // Remove ingredients from inventory
        foreach (var ingredient in recipe.Ingredients)
        {
            if (!inventory.RemoveItem(ingredient.Key, ingredient.Value))
            {
                // This shouldn't happen if CanCraft returned true, but safety check
                Console.WriteLine($"[Crafting] Error removing {ingredient.Key} x{ingredient.Value}");
                return false;
            }
        }
        
        // Add crafted item to inventory
        if (inventory.AddItem(recipe.Output, recipe.OutputQuantity))
        {
            Console.WriteLine($"[Crafting] Crafted {recipe.Name}: {recipe.Output} x{recipe.OutputQuantity}");
            return true;
        }
        else
        {
            // Inventory full, return ingredients
            Console.WriteLine($"[Crafting] Inventory full! Cannot craft {recipe.Name}");
            foreach (var ingredient in recipe.Ingredients)
            {
                inventory.AddItem(ingredient.Key, ingredient.Value);
            }
            return false;
        }
    }
    
    /// <summary>
    /// Try to craft by recipe name
    /// </summary>
    public bool TryCraftByName(string recipeName, InventoryComponent inventory)
    {
        var recipe = recipes.FirstOrDefault(r => r.Name.Equals(recipeName, StringComparison.OrdinalIgnoreCase));
        if (recipe == null)
        {
            Console.WriteLine($"[Crafting] Recipe '{recipeName}' not found");
            return false;
        }
        
        return TryCraft(recipe, inventory);
    }
    
    /// <summary>
    /// Print all available recipes
    /// </summary>
    public void PrintRecipes()
    {
        Console.WriteLine("\n=== Available Crafting Recipes ===");
        var categories = GetCategories();
        
        foreach (var category in categories)
        {
            Console.WriteLine($"\n{category}:");
            var categoryRecipes = GetRecipesByCategory(category);
            
            foreach (var recipe in categoryRecipes)
            {
                Console.WriteLine($"  - {recipe.Name}:");
                Console.WriteLine($"    Output: {recipe.Output} x{recipe.OutputQuantity}");
                Console.WriteLine($"    Ingredients:");
                foreach (var ingredient in recipe.Ingredients)
                {
                    Console.WriteLine($"      * {ingredient.Key} x{ingredient.Value}");
                }
            }
        }
        Console.WriteLine();
    }
    
    /// <summary>
    /// Print recipes that can be crafted with current inventory
    /// </summary>
    public void PrintCraftableRecipes(InventoryComponent inventory)
    {
        Console.WriteLine("\n=== Craftable Recipes ===");
        var craftable = GetCraftableRecipes(inventory);
        
        if (craftable.Count == 0)
        {
            Console.WriteLine("  No recipes available with current materials");
            return;
        }
        
        foreach (var recipe in craftable)
        {
            Console.WriteLine($"  - {recipe.Name}: {recipe.Output} x{recipe.OutputQuantity}");
        }
        Console.WriteLine();
    }
}
