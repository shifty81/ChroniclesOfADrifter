using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.ECS.Systems;

namespace ChroniclesOfADrifter.Scenes;

/// <summary>
/// Demo scene showcasing the crafting system
/// </summary>
public class CraftingDemoScene : Scene
{
    private CraftingSystem craftingSystem = null!;
    private Entity player;
    
    public override void OnLoad()
    {
        Console.WriteLine("\n===========================================");
        Console.WriteLine("  Crafting System Demo");
        Console.WriteLine("===========================================\n");
        
        // Create crafting system
        craftingSystem = new CraftingSystem();
        craftingSystem.Initialize(World);
        
        // Create player with inventory
        player = World.CreateEntity();
        World.AddComponent(player, new PlayerComponent());
        World.AddComponent(player, new PositionComponent { X = 0, Y = 0 });
        
        // Add starting materials to player inventory
        var inventory = new InventoryComponent(40);
        inventory.AddItem(TileType.TreeOak, 10);
        inventory.AddItem(TileType.TreePine, 5);
        inventory.AddItem(TileType.Stone, 15);
        inventory.AddItem(TileType.Dirt, 10);
        inventory.AddItem(TileType.CopperOre, 8);
        inventory.AddItem(TileType.IronOre, 3);
        
        World.AddComponent(player, inventory);
        
        Console.WriteLine("Player inventory initialized with starting materials:");
        PrintInventory(inventory);
        
        // Show all available recipes
        craftingSystem.PrintRecipes();
        
        // Show craftable recipes
        craftingSystem.PrintCraftableRecipes(inventory);
        
        // Demo crafting sequence
        DemoCraftingSequence();
        
        Console.WriteLine("\n===========================================");
        Console.WriteLine("  Crafting Demo Complete!");
        Console.WriteLine("===========================================\n");
    }
    
    private void DemoCraftingSequence()
    {
        Console.WriteLine("\n=== Crafting Demo Sequence ===\n");
        
        var inventory = World.GetComponent<InventoryComponent>(player);
        if (inventory == null) return;
        
        // 1. Craft wood planks from oak trees
        Console.WriteLine("[Step 1] Crafting wood planks from oak trees...");
        craftingSystem.TryCraftByName("Wood Planks", inventory);
        PrintInventory(inventory);
        
        // 2. Craft wood blocks from planks
        Console.WriteLine("\n[Step 2] Crafting wood blocks from planks...");
        craftingSystem.TryCraftByName("Wood Block", inventory);
        PrintInventory(inventory);
        
        // 3. Craft cobblestone from stone
        Console.WriteLine("\n[Step 3] Crafting cobblestone from stone...");
        craftingSystem.TryCraftByName("Cobblestone", inventory);
        PrintInventory(inventory);
        
        // 4. Craft bricks
        Console.WriteLine("\n[Step 4] Crafting bricks from stone and dirt...");
        craftingSystem.TryCraftByName("Brick", inventory);
        PrintInventory(inventory);
        
        // 5. Craft torches
        Console.WriteLine("\n[Step 5] Crafting torches for lighting...");
        craftingSystem.TryCraftByName("Torch", inventory);
        PrintInventory(inventory);
        
        // 6. Show what we can still craft
        Console.WriteLine("\n[Step 6] Remaining craftable recipes:");
        craftingSystem.PrintCraftableRecipes(inventory);
    }
    
    private void PrintInventory(InventoryComponent inventory)
    {
        Console.WriteLine("\nCurrent Inventory:");
        var items = inventory.GetAllItems();
        
        if (items.Count == 0)
        {
            Console.WriteLine("  (empty)");
            return;
        }
        
        foreach (var item in items.OrderBy(kvp => kvp.Key.ToString()))
        {
            Console.WriteLine($"  {item.Key}: x{item.Value}");
        }
    }
    
    public override void OnUnload()
    {
        Console.WriteLine("Crafting demo scene shutdown");
    }
}
