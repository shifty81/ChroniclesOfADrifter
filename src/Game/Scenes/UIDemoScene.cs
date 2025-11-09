using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.ECS.Systems;
using ChroniclesOfADrifter.UI;
using ChroniclesOfADrifter.Engine;

namespace ChroniclesOfADrifter.Scenes;

/// <summary>
/// Demo scene showcasing the UI framework with inventory and crafting
/// </summary>
public class UIDemoScene : Scene
{
    private CraftingSystem _craftingSystem = null!;
    private Entity _player;
    private Entity _uiEntity;
    private InventoryUI? _inventoryUI;
    private CraftingUI? _craftingUI;
    private bool _inventoryOpen = false;
    private bool _craftingOpen = false;
    
    public override void OnLoad()
    {
        Console.WriteLine("\n===========================================");
        Console.WriteLine("  UI Framework Demo");
        Console.WriteLine("===========================================\n");
        Console.WriteLine("Controls:");
        Console.WriteLine("  I - Toggle Inventory");
        Console.WriteLine("  C - Toggle Crafting");
        Console.WriteLine("  Mouse - Interact with UI elements");
        Console.WriteLine("===========================================\n");
        
        // Register UI system
        World.AddSystem(new UISystem());
        
        // Create crafting system
        _craftingSystem = new CraftingSystem();
        _craftingSystem.Initialize(World);
        
        // Create player with inventory
        _player = World.CreateEntity();
        World.AddComponent(_player, new PlayerComponent());
        World.AddComponent(_player, new PositionComponent { X = 400, Y = 300 });
        
        // Add starting materials to player inventory
        var inventory = new InventoryComponent(40);
        inventory.AddItem(TileType.TreeOak, 10);
        inventory.AddItem(TileType.TreePine, 5);
        inventory.AddItem(TileType.TreePalm, 3);
        inventory.AddItem(TileType.Stone, 15);
        inventory.AddItem(TileType.Dirt, 10);
        inventory.AddItem(TileType.CopperOre, 8);
        inventory.AddItem(TileType.IronOre, 5);
        inventory.AddItem(TileType.Coal, 6);
        inventory.AddItem(TileType.Wood, 4);
        
        World.AddComponent(_player, inventory);
        
        // Create UI entity
        _uiEntity = World.CreateEntity();
        var uiComponent = new UIComponent();
        
        // Create inventory UI (centered on screen, initially hidden)
        _inventoryUI = new InventoryUI(100, 100);
        _inventoryUI.SetInventory(inventory);
        _inventoryUI.IsVisible = false;
        uiComponent.AddElement(_inventoryUI);
        
        // Create crafting UI (to the right of inventory, initially hidden)
        _craftingUI = new CraftingUI(570, 100, 400, 450);
        _craftingUI.SetCraftingData(_craftingSystem, inventory);
        _craftingUI.IsVisible = false;
        uiComponent.AddElement(_craftingUI);
        
        World.AddComponent(_uiEntity, uiComponent);
        
        Console.WriteLine("UI Demo loaded successfully!");
        Console.WriteLine("Press 'I' to open inventory, 'C' to open crafting menu");
    }
    
    public override void OnUnload()
    {
        Console.WriteLine("UI Demo unloading...");
    }
    
    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);
        
        // Toggle inventory with 'I' key
        if (EngineInterop.Input_IsKeyPressed(73)) // 'I' key
        {
            _inventoryOpen = !_inventoryOpen;
            if (_inventoryUI != null)
            {
                _inventoryUI.IsVisible = _inventoryOpen;
            }
            Console.WriteLine($"Inventory {(_inventoryOpen ? "opened" : "closed")}");
        }
        
        // Toggle crafting with 'C' key
        if (EngineInterop.Input_IsKeyPressed(67)) // 'C' key
        {
            _craftingOpen = !_craftingOpen;
            if (_craftingUI != null)
            {
                _craftingUI.IsVisible = _craftingOpen;
            }
            Console.WriteLine($"Crafting menu {(_craftingOpen ? "opened" : "closed")}");
        }
        
        // ESC to close all UI
        if (EngineInterop.Input_IsKeyPressed(27)) // ESC key
        {
            _inventoryOpen = false;
            _craftingOpen = false;
            if (_inventoryUI != null) _inventoryUI.IsVisible = false;
            if (_craftingUI != null) _craftingUI.IsVisible = false;
            Console.WriteLine("All UI closed");
        }
    }
}
