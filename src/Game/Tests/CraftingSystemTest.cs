using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.ECS.Systems;

namespace ChroniclesOfADrifter.Tests;

/// <summary>
/// Test suite for the crafting system
/// </summary>
public static class CraftingSystemTest
{
    public static void RunTests()
    {
        Console.WriteLine("\n===========================================");
        Console.WriteLine("  Crafting System Tests");
        Console.WriteLine("===========================================\n");
        
        TestBasicCrafting();
        TestInsufficientMaterials();
        TestInventoryFull();
        TestCraftableRecipes();
        TestRecipeCategories();
        TestWoodPlankCrafting();
        TestToolCrafting();
        
        Console.WriteLine("\n===========================================");
        Console.WriteLine("  All Crafting Tests Completed!");
        Console.WriteLine("===========================================\n");
    }
    
    private static void TestBasicCrafting()
    {
        Console.WriteLine("[Test] Basic Crafting");
        
        var world = new World();
        var craftingSystem = new CraftingSystem();
        craftingSystem.Initialize(world);
        
        // Create an inventory with materials
        var inventory = new InventoryComponent(40);
        inventory.AddItem(TileType.TreeOak, 5);
        
        Console.WriteLine($"  Initial inventory: TreeOak x{inventory.GetItemCount(TileType.TreeOak)}");
        
        // Craft wood planks
        var recipe = craftingSystem.GetAllRecipes().First(r => r.Name == "Wood Planks");
        bool success = craftingSystem.TryCraft(recipe, inventory);
        
        Console.WriteLine($"  Crafting success: {success}");
        Console.WriteLine($"  WoodPlank count: {inventory.GetItemCount(TileType.WoodPlank)}");
        Console.WriteLine($"  TreeOak remaining: {inventory.GetItemCount(TileType.TreeOak)}");
        
        if (success && inventory.GetItemCount(TileType.WoodPlank) == 4 && 
            inventory.GetItemCount(TileType.TreeOak) == 4)
        {
            Console.WriteLine("  ✓ Test passed\n");
        }
        else
        {
            Console.WriteLine("  ✗ Test failed\n");
        }
    }
    
    private static void TestInsufficientMaterials()
    {
        Console.WriteLine("[Test] Insufficient Materials");
        
        var world = new World();
        var craftingSystem = new CraftingSystem();
        craftingSystem.Initialize(world);
        
        // Create an inventory without enough materials
        var inventory = new InventoryComponent(40);
        inventory.AddItem(TileType.Stone, 1);
        
        Console.WriteLine($"  Initial inventory: Stone x{inventory.GetItemCount(TileType.Stone)}");
        
        // Try to craft brick (requires 2 stone + 2 dirt)
        var recipe = craftingSystem.GetAllRecipes().First(r => r.Name == "Brick");
        bool canCraft = craftingSystem.CanCraft(recipe, inventory);
        bool success = craftingSystem.TryCraft(recipe, inventory);
        
        Console.WriteLine($"  Can craft: {canCraft}");
        Console.WriteLine($"  Crafting attempted: {success}");
        Console.WriteLine($"  Stone count after: {inventory.GetItemCount(TileType.Stone)}");
        
        if (!canCraft && !success && inventory.GetItemCount(TileType.Stone) == 1)
        {
            Console.WriteLine("  ✓ Test passed\n");
        }
        else
        {
            Console.WriteLine("  ✗ Test failed\n");
        }
    }
    
    private static void TestInventoryFull()
    {
        Console.WriteLine("[Test] Inventory Full");
        
        var world = new World();
        var craftingSystem = new CraftingSystem();
        craftingSystem.Initialize(world);
        
        // Create a small inventory and fill it
        var inventory = new InventoryComponent(2);
        inventory.AddItem(TileType.TreeOak, 10);
        inventory.AddItem(TileType.Stone, 10);
        
        Console.WriteLine($"  Inventory slots: 2/2 (full)");
        
        // Try to craft (would create a new item type)
        var recipe = craftingSystem.GetAllRecipes().First(r => r.Name == "Wood Planks");
        bool success = craftingSystem.TryCraft(recipe, inventory);
        
        Console.WriteLine($"  Crafting success: {success}");
        Console.WriteLine($"  TreeOak remaining: {inventory.GetItemCount(TileType.TreeOak)}");
        
        // Materials should be returned if crafting fails
        if (!success && inventory.GetItemCount(TileType.TreeOak) == 10)
        {
            Console.WriteLine("  ✓ Test passed (materials returned)\n");
        }
        else
        {
            Console.WriteLine("  ✗ Test failed\n");
        }
    }
    
    private static void TestCraftableRecipes()
    {
        Console.WriteLine("[Test] Craftable Recipes");
        
        var world = new World();
        var craftingSystem = new CraftingSystem();
        craftingSystem.Initialize(world);
        
        // Create an inventory with materials for some recipes
        var inventory = new InventoryComponent(40);
        inventory.AddItem(TileType.TreeOak, 10);
        inventory.AddItem(TileType.Stone, 5);
        inventory.AddItem(TileType.Dirt, 5);
        
        var craftable = craftingSystem.GetCraftableRecipes(inventory);
        
        Console.WriteLine($"  Available materials: TreeOak x10, Stone x5, Dirt x5");
        Console.WriteLine($"  Craftable recipes: {craftable.Count}");
        
        foreach (var recipe in craftable)
        {
            Console.WriteLine($"    - {recipe.Name}");
        }
        
        if (craftable.Count > 0 && craftable.Any(r => r.Name.Contains("Wood Planks")))
        {
            Console.WriteLine("  ✓ Test passed\n");
        }
        else
        {
            Console.WriteLine("  ✗ Test failed\n");
        }
    }
    
    private static void TestRecipeCategories()
    {
        Console.WriteLine("[Test] Recipe Categories");
        
        var world = new World();
        var craftingSystem = new CraftingSystem();
        craftingSystem.Initialize(world);
        
        var categories = craftingSystem.GetCategories();
        
        Console.WriteLine($"  Total categories: {categories.Count}");
        foreach (var category in categories)
        {
            var recipes = craftingSystem.GetRecipesByCategory(category);
            Console.WriteLine($"    - {category}: {recipes.Count} recipes");
        }
        
        if (categories.Contains("Building") && categories.Contains("Tools"))
        {
            Console.WriteLine("  ✓ Test passed\n");
        }
        else
        {
            Console.WriteLine("  ✗ Test failed\n");
        }
    }
    
    private static void TestWoodPlankCrafting()
    {
        Console.WriteLine("[Test] Wood Plank Crafting Chain");
        
        var world = new World();
        var craftingSystem = new CraftingSystem();
        craftingSystem.Initialize(world);
        
        var inventory = new InventoryComponent(40);
        inventory.AddItem(TileType.TreeOak, 2);
        
        Console.WriteLine($"  Starting materials: TreeOak x2");
        
        // Craft wood planks from oak
        craftingSystem.TryCraftByName("Wood Planks", inventory);
        Console.WriteLine($"  After first craft: WoodPlank x{inventory.GetItemCount(TileType.WoodPlank)}, TreeOak x{inventory.GetItemCount(TileType.TreeOak)}");
        
        // Craft wood block from planks
        craftingSystem.TryCraftByName("Wood Block", inventory);
        Console.WriteLine($"  After second craft: Wood x{inventory.GetItemCount(TileType.Wood)}, WoodPlank x{inventory.GetItemCount(TileType.WoodPlank)}");
        
        if (inventory.GetItemCount(TileType.Wood) == 1 && 
            inventory.GetItemCount(TileType.WoodPlank) == 0)
        {
            Console.WriteLine("  ✓ Test passed\n");
        }
        else
        {
            Console.WriteLine("  ✗ Test failed\n");
        }
    }
    
    private static void TestToolCrafting()
    {
        Console.WriteLine("[Test] Tool Crafting");
        
        var world = new World();
        var craftingSystem = new CraftingSystem();
        craftingSystem.Initialize(world);
        
        var inventory = new InventoryComponent(40);
        inventory.AddItem(TileType.WoodPlank, 1);
        inventory.AddItem(TileType.CopperOre, 2);
        
        Console.WriteLine($"  Materials: WoodPlank x1, CopperOre x2");
        
        // Try to craft torches
        var success = craftingSystem.TryCraftByName("Torch", inventory);
        
        Console.WriteLine($"  Crafting torches: {success}");
        Console.WriteLine($"  Torch count: {inventory.GetItemCount(TileType.Torch)}");
        Console.WriteLine($"  Materials remaining: WoodPlank x{inventory.GetItemCount(TileType.WoodPlank)}, CopperOre x{inventory.GetItemCount(TileType.CopperOre)}");
        
        if (success && inventory.GetItemCount(TileType.Torch) == 4)
        {
            Console.WriteLine("  ✓ Test passed\n");
        }
        else
        {
            Console.WriteLine("  ✗ Test failed\n");
        }
    }
}
