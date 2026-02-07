using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.WorldManagement;

namespace ChroniclesOfADrifter.Serialization;

/// <summary>
/// Root save data structure
/// </summary>
public class SaveData
{
    public string SaveName { get; set; } = "";
    public DateTime SaveDate { get; set; }
    public string GameVersion { get; set; } = "1.0.0";
    public int SaveFormatVersion { get; set; } = 1;
    public float GameTime { get; set; }
    
    public PlayerData? Player { get; set; }
    public WorldData? World { get; set; }
    public TimeData? Time { get; set; }
    public WeatherData? Weather { get; set; }
    public List<NPCData> NPCs { get; set; } = new();
}

/// <summary>
/// Player state data
/// </summary>
public class PlayerData
{
    public int EntityId { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Speed { get; set; }
    public float MaxHealth { get; set; }
    public float CurrentHealth { get; set; }
    public int Gold { get; set; }
    public Dictionary<string, int> Inventory { get; set; } = new();
    public ToolType ToolType { get; set; }
    public ToolMaterial ToolMaterial { get; set; }
    public List<QuestData> ActiveQuests { get; set; } = new();
    public List<QuestData> CompletedQuests { get; set; } = new();
    public float RespawnX { get; set; }
    public float RespawnY { get; set; }
    public int DeathCount { get; set; }
}

/// <summary>
/// Quest data
/// </summary>
public class QuestData
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Type { get; set; } = "";
    public string Status { get; set; } = "";
    public int CurrentProgress { get; set; }
    public int RequiredProgress { get; set; }
    public int GoldReward { get; set; }
    public int ExperienceReward { get; set; }
    public Dictionary<string, int> ItemRewards { get; set; } = new();
    public string? UnlockAbility { get; set; }
    public string? UnlockArea { get; set; }
    public string? RequiredAbility { get; set; }
    public int RequiredLevel { get; set; }
}

/// <summary>
/// World state data
/// </summary>
public class WorldData
{
    public int WorldSeed { get; set; }
    public List<ChunkData> ModifiedChunks { get; set; } = new();
}

/// <summary>
/// Chunk modification data
/// </summary>
public class ChunkData
{
    public int ChunkX { get; set; }
    public Dictionary<string, string> ModifiedTiles { get; set; } = new();
    public Dictionary<int, string?> ModifiedVegetation { get; set; } = new();
}

/// <summary>
/// Time system data
/// </summary>
public class TimeData
{
    public float CurrentTime { get; set; }
    public float TimeScale { get; set; }
    public int DayCount { get; set; }
}

/// <summary>
/// Weather system data
/// </summary>
public class WeatherData
{
    public string CurrentWeather { get; set; } = "";
    public string CurrentIntensity { get; set; } = "";
    public float WeatherTimer { get; set; }
    public float WeatherDuration { get; set; }
}

/// <summary>
/// NPC data
/// </summary>
public class NPCData
{
    public int EntityId { get; set; }
    public string Name { get; set; } = "";
    public string Role { get; set; } = "";
    public float X { get; set; }
    public float Y { get; set; }
    public string CurrentActivity { get; set; } = "";
    public Dictionary<string, int> ShopInventory { get; set; } = new();
    public Dictionary<string, int> ShopPrices { get; set; } = new();
}
