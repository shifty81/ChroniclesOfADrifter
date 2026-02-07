using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.ECS.Systems;
using ChroniclesOfADrifter.Terrain;
using ChroniclesOfADrifter.WorldManagement;

namespace ChroniclesOfADrifter.Tests;

/// <summary>
/// Test program for the Save/Load system
/// </summary>
public class SaveLoadTest
{
    public static void RunTest()
    {
        Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║              SAVE/LOAD SYSTEM TEST                               ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝\n");
        
        // Create a world and save system
        var world = new World();
        var saveSystem = new SaveSystem();
        world.AddSystem(saveSystem);
        
        // Create chunk manager and terrain generator
        var terrainGenerator = new TerrainGenerator(seed: 12345);
        var chunkManager = new ChunkManager();
        chunkManager.SetTerrainGenerator(terrainGenerator);
        world.SetSharedResource("ChunkManager", chunkManager);
        
        // Create time and weather systems
        var timeSystem = new TimeSystem(startHour: 10);
        timeSystem.TimeScale = 60f;
        world.SetSharedResource("TimeSystem", timeSystem);
        
        var weatherSystem = new WeatherSystem(seed: 12345);
        world.SetSharedResource("WeatherSystem", weatherSystem);
        
        // Create player entity
        var player = world.CreateEntity();
        world.AddComponent(player, new PlayerComponent { Speed = 150.0f });
        world.AddComponent(player, new PositionComponent(500, 150));
        world.AddComponent(player, new HealthComponent(100) { CurrentHealth = 75 });
        world.AddComponent(player, new CurrencyComponent(250));
        
        var inventory = new InventoryComponent(40);
        inventory.AddItem(TileType.Wood, 50);
        inventory.AddItem(TileType.Stone, 30);
        inventory.AddItem(TileType.IronOre, 10);
        world.AddComponent(player, inventory);
        
        world.AddComponent(player, new ToolComponent(ToolType.Pickaxe, ToolMaterial.Iron));
        
        var questComponent = new QuestComponent();
        var quest = new Quest("test_quest", "Test Quest", "A test quest", QuestType.Gathering)
        {
            CurrentProgress = 3,
            RequiredProgress = 10,
            GoldReward = 100
        };
        questComponent.AcceptQuest(quest);
        world.AddComponent(player, questComponent);
        
        // Modify some chunks
        chunkManager.SetTile(500, 5, TileType.Torch);
        chunkManager.SetTile(501, 5, TileType.Cobblestone);
        chunkManager.SetTile(502, 5, TileType.Wood);
        
        // Display initial state
        Console.WriteLine("=== INITIAL STATE ===");
        DisplayGameState(world, player, timeSystem, weatherSystem);
        
        // Save the game
        Console.WriteLine("\n=== SAVING GAME ===");
        bool saveSuccess = saveSystem.SaveGame("test_save", gameTime: 123.5f);
        
        if (!saveSuccess)
        {
            Console.WriteLine("❌ Save failed!");
            return;
        }
        
        Console.WriteLine("✓ Save successful!");
        
        // Modify the state
        Console.WriteLine("\n=== MODIFYING STATE ===");
        var pos = world.GetComponent<PositionComponent>(player);
        if (pos != null)
        {
            pos.X = 1000;
            pos.Y = 200;
        }
        
        var health = world.GetComponent<HealthComponent>(player);
        if (health != null)
        {
            health.CurrentHealth = 25;
        }
        
        var currency = world.GetComponent<CurrencyComponent>(player);
        if (currency != null)
        {
            currency.Gold = 50;
        }
        
        var inv = world.GetComponent<InventoryComponent>(player);
        if (inv != null)
        {
            inv.Clear();
            inv.AddItem(TileType.Dirt, 5);
        }
        
        timeSystem.SetTime(18);
        chunkManager.SetTile(500, 5, TileType.Air);
        
        Console.WriteLine("Modified player position, health, gold, inventory, time, and world");
        DisplayGameState(world, player, timeSystem, weatherSystem);
        
        // Load the game
        Console.WriteLine("\n=== LOADING GAME ===");
        bool loadSuccess = saveSystem.LoadGame("test_save", out float loadedTime);
        
        if (!loadSuccess)
        {
            Console.WriteLine("❌ Load failed!");
            return;
        }
        
        Console.WriteLine($"✓ Load successful! Game time: {loadedTime}");
        
        // Display restored state
        Console.WriteLine("\n=== RESTORED STATE ===");
        DisplayGameState(world, player, timeSystem, weatherSystem);
        
        // Test quick save/load
        Console.WriteLine("\n=== TESTING QUICK SAVE/LOAD ===");
        
        // Modify position
        if (pos != null)
        {
            pos.X = 2000;
            pos.Y = 300;
        }
        
        Console.WriteLine($"Modified position to ({pos?.X}, {pos?.Y})");
        
        saveSystem.QuickSave(gameTime: 200f);
        Console.WriteLine("Quick saved");
        
        if (pos != null)
        {
            pos.X = 3000;
            pos.Y = 400;
        }
        Console.WriteLine($"Changed position to ({pos?.X}, {pos?.Y})");
        
        saveSystem.QuickLoad(out float quickLoadTime);
        Console.WriteLine($"Quick loaded - position restored to ({pos?.X}, {pos?.Y})");
        
        // List saves
        Console.WriteLine("\n=== LISTING SAVES ===");
        var saves = saveSystem.ListSaves();
        foreach (var save in saves)
        {
            Console.WriteLine($"  • {save.name} - {save.date}");
        }
        
        // Test delete
        Console.WriteLine("\n=== TESTING DELETE ===");
        bool deleteSuccess = saveSystem.DeleteSave("test_save");
        Console.WriteLine(deleteSuccess ? "✓ Save deleted" : "❌ Delete failed");
        
        Console.WriteLine("\n╔══════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║              SAVE/LOAD SYSTEM TEST COMPLETE                      ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");
    }
    
    private static void DisplayGameState(World world, Entity player, TimeSystem timeSystem, WeatherSystem weatherSystem)
    {
        var pos = world.GetComponent<PositionComponent>(player);
        var health = world.GetComponent<HealthComponent>(player);
        var currency = world.GetComponent<CurrencyComponent>(player);
        var inventory = world.GetComponent<InventoryComponent>(player);
        var quests = world.GetComponent<QuestComponent>(player);
        
        Console.WriteLine($"  Player Position: ({pos?.X}, {pos?.Y})");
        Console.WriteLine($"  Health: {health?.CurrentHealth}/{health?.MaxHealth}");
        Console.WriteLine($"  Gold: {currency?.Gold}");
        Console.WriteLine($"  Inventory Items: {inventory?.GetAllItems().Count}");
        
        if (inventory != null)
        {
            foreach (var item in inventory.GetAllItems())
            {
                Console.WriteLine($"    - {item.Key}: x{item.Value}");
            }
        }
        
        Console.WriteLine($"  Active Quests: {quests?.ActiveQuests.Count}");
        if (quests != null)
        {
            foreach (var quest in quests.ActiveQuests)
            {
                Console.WriteLine($"    - {quest.Name}: {quest.CurrentProgress}/{quest.RequiredProgress}");
            }
        }
        
        Console.WriteLine($"  Time: {timeSystem.CurrentHour:D2}:{timeSystem.CurrentMinute:D2} (Day {timeSystem.DayCount})");
        Console.WriteLine($"  Weather: {weatherSystem.CurrentWeather} ({weatherSystem.CurrentIntensity})");
    }
}
