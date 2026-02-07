using System.Text.Json;
using System.Text.Json.Serialization;
using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.Serialization;
using ChroniclesOfADrifter.Terrain;
using ChroniclesOfADrifter.WorldManagement;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// System for saving and loading game state
/// </summary>
public class SaveSystem : ISystem
{
    private World? _world;
    private string _saveDirectory;
    private const string SAVE_EXTENSION = ".json";
    private const string QUICKSAVE_NAME = "quicksave";
    private const int CURRENT_SAVE_FORMAT_VERSION = 1;
    
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    
    public SaveSystem(string? saveDirectory = null)
    {
        _saveDirectory = saveDirectory ?? Path.Combine(Directory.GetCurrentDirectory(), "saves");
        EnsureSaveDirectoryExists();
    }
    
    public void Initialize(World world)
    {
        _world = world;
    }
    
    public void Update(World world, float deltaTime)
    {
        // SaveSystem doesn't need per-frame updates
    }
    
    private void EnsureSaveDirectoryExists()
    {
        if (!Directory.Exists(_saveDirectory))
        {
            Directory.CreateDirectory(_saveDirectory);
        }
    }
    
    /// <summary>
    /// Saves the current game state
    /// </summary>
    public bool SaveGame(string saveName, float gameTime = 0f)
    {
        if (_world == null)
        {
            LogError("World not initialized");
            return false;
        }
        
        try
        {
            LogInfo($"Saving game: {saveName}...");
            
            var saveData = new SaveData
            {
                SaveName = saveName,
                SaveDate = DateTime.Now,
                GameVersion = "1.0.0",
                SaveFormatVersion = CURRENT_SAVE_FORMAT_VERSION,
                GameTime = gameTime
            };
            
            // Save player data
            saveData.Player = SavePlayerData();
            
            // Save world data
            saveData.World = SaveWorldData();
            
            // Save time data
            saveData.Time = SaveTimeData();
            
            // Save weather data
            saveData.Weather = SaveWeatherData();
            
            // Save NPC data
            saveData.NPCs = SaveNPCData();
            
            // Serialize to JSON
            string json = JsonSerializer.Serialize(saveData, _jsonOptions);
            string filePath = GetSaveFilePath(saveName);
            File.WriteAllText(filePath, json);
            
            LogInfo($"Game saved successfully to {filePath}");
            return true;
        }
        catch (Exception ex)
        {
            LogError($"Error saving game: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Loads a saved game state
    /// </summary>
    public bool LoadGame(string saveName, out float gameTime)
    {
        gameTime = 0f;
        
        if (_world == null)
        {
            LogError("World not initialized");
            return false;
        }
        
        string filePath = GetSaveFilePath(saveName);
        
        if (!File.Exists(filePath))
        {
            LogError($"Save file not found: {filePath}");
            return false;
        }
        
        try
        {
            LogInfo($"Loading game: {saveName}...");
            
            string json = File.ReadAllText(filePath);
            var saveData = JsonSerializer.Deserialize<SaveData>(json, _jsonOptions);
            
            if (saveData == null)
            {
                LogError("Failed to deserialize save data");
                return false;
            }
            
            // Validate save format version
            if (!ValidateSaveFormat(saveData))
            {
                LogError($"Incompatible save format. Current version: {CURRENT_SAVE_FORMAT_VERSION}, Save version: {saveData.SaveFormatVersion}");
                return false;
            }
            
            gameTime = saveData.GameTime;
            
            // Load player data
            if (saveData.Player != null)
            {
                if (!LoadPlayerData(saveData.Player))
                {
                    LogError("Failed to load player data");
                    return false;
                }
            }
            
            // Load world data
            if (saveData.World != null)
            {
                LoadWorldData(saveData.World);
            }
            
            // Load time data
            if (saveData.Time != null)
            {
                LoadTimeData(saveData.Time);
            }
            
            // Load weather data
            if (saveData.Weather != null)
            {
                LoadWeatherData(saveData.Weather);
            }
            
            // Load NPC data
            if (saveData.NPCs.Count > 0)
            {
                LoadNPCData(saveData.NPCs);
            }
            
            LogInfo($"Game loaded successfully from {filePath}");
            return true;
        }
        catch (Exception ex)
        {
            LogError($"Error loading game: {ex.Message}\n{ex.StackTrace}");
            return false;
        }
    }
    
    /// <summary>
    /// Quick save - saves to a predefined slot
    /// </summary>
    public bool QuickSave(float gameTime = 0f)
    {
        return SaveGame(QUICKSAVE_NAME, gameTime);
    }
    
    /// <summary>
    /// Quick load - loads from the predefined slot
    /// </summary>
    public bool QuickLoad(out float gameTime)
    {
        return LoadGame(QUICKSAVE_NAME, out gameTime);
    }
    
    /// <summary>
    /// Deletes a save file
    /// </summary>
    public bool DeleteSave(string saveName)
    {
        string filePath = GetSaveFilePath(saveName);
        
        if (!File.Exists(filePath))
        {
            LogError($"Save file not found: {filePath}");
            return false;
        }
        
        try
        {
            File.Delete(filePath);
            LogInfo($"Save file deleted: {filePath}");
            return true;
        }
        catch (Exception ex)
        {
            LogError($"Error deleting save: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Lists all available save files
    /// </summary>
    public List<(string name, DateTime date)> ListSaves()
    {
        var saves = new List<(string name, DateTime date)>();
        
        if (!Directory.Exists(_saveDirectory))
        {
            return saves;
        }
        
        var files = Directory.GetFiles(_saveDirectory, $"*{SAVE_EXTENSION}");
        
        foreach (var file in files)
        {
            try
            {
                string json = File.ReadAllText(file);
                var saveData = JsonSerializer.Deserialize<SaveData>(json, _jsonOptions);
                
                if (saveData != null)
                {
                    saves.Add((saveData.SaveName, saveData.SaveDate));
                }
            }
            catch
            {
                // Skip corrupted save files
            }
        }
        
        return saves.OrderByDescending(s => s.date).ToList();
    }
    
    private string GetSaveFilePath(string saveName)
    {
        // Sanitize filename
        string fileName = string.Join("_", saveName.Split(Path.GetInvalidFileNameChars()));
        return Path.Combine(_saveDirectory, fileName + SAVE_EXTENSION);
    }
    
    private PlayerData? SavePlayerData()
    {
        if (_world == null) return null;
        
        // Find player entity
        var playerEntities = _world.GetEntitiesWithComponent<PlayerComponent>().ToList();
        if (playerEntities.Count == 0)
        {
            LogWarning("No player entity found");
            return null;
        }
        
        var playerEntity = playerEntities[0];
        var playerData = new PlayerData { EntityId = playerEntity.Id };
        
        // Save position
        var position = _world.GetComponent<PositionComponent>(playerEntity);
        if (position != null)
        {
            playerData.X = position.X;
            playerData.Y = position.Y;
        }
        
        // Save player component
        var player = _world.GetComponent<PlayerComponent>(playerEntity);
        if (player != null)
        {
            playerData.Speed = player.Speed;
        }
        
        // Save health
        var health = _world.GetComponent<HealthComponent>(playerEntity);
        if (health != null)
        {
            playerData.MaxHealth = health.MaxHealth;
            playerData.CurrentHealth = health.CurrentHealth;
        }
        
        // Save currency
        var currency = _world.GetComponent<CurrencyComponent>(playerEntity);
        if (currency != null)
        {
            playerData.Gold = currency.Gold;
        }
        
        // Save inventory
        var inventory = _world.GetComponent<InventoryComponent>(playerEntity);
        if (inventory != null)
        {
            foreach (var item in inventory.GetAllItems())
            {
                playerData.Inventory[item.Key.ToString()] = item.Value;
            }
        }
        
        // Save tool
        var tool = _world.GetComponent<ToolComponent>(playerEntity);
        if (tool != null)
        {
            playerData.ToolType = tool.Type;
            playerData.ToolMaterial = tool.Material;
        }
        
        // Save quests
        var questComponent = _world.GetComponent<QuestComponent>(playerEntity);
        if (questComponent != null)
        {
            foreach (var quest in questComponent.ActiveQuests)
            {
                playerData.ActiveQuests.Add(ConvertQuestToData(quest));
            }
            
            foreach (var quest in questComponent.CompletedQuests)
            {
                playerData.CompletedQuests.Add(ConvertQuestToData(quest));
            }
        }
        
        // Save respawn data
        var respawn = _world.GetComponent<RespawnComponent>(playerEntity);
        if (respawn != null)
        {
            playerData.RespawnX = respawn.RespawnX;
            playerData.RespawnY = respawn.RespawnY;
            playerData.DeathCount = respawn.DeathCount;
        }
        
        // Save experience data
        var experience = _world.GetComponent<ExperienceComponent>(playerEntity);
        if (experience != null)
        {
            playerData.CurrentXP = experience.CurrentXP;
            playerData.Level = experience.Level;
            playerData.TotalXPEarned = experience.TotalXPEarned;
            playerData.AttackBonus = experience.AttackBonus;
            playerData.DefenseBonus = experience.DefenseBonus;
            playerData.MaxHealthBonus = experience.MaxHealthBonus;
            playerData.SpeedBonus = experience.SpeedBonus;
            playerData.StatPointsAvailable = experience.StatPointsAvailable;
        }
        
        return playerData;
    }
    
    private bool LoadPlayerData(PlayerData playerData)
    {
        if (_world == null) return false;
        
        // Validate player data
        if (!ValidatePlayerData(playerData))
        {
            LogError("Invalid player data");
            return false;
        }
        
        // Find or create player entity
        var playerEntities = _world.GetEntitiesWithComponent<PlayerComponent>().ToList();
        Entity playerEntity;
        
        if (playerEntities.Count > 0)
        {
            playerEntity = playerEntities[0];
        }
        else
        {
            LogWarning("No player entity found to load into");
            return false;
        }
        
        // Load position
        var position = _world.GetComponent<PositionComponent>(playerEntity);
        if (position != null)
        {
            position.X = playerData.X;
            position.Y = playerData.Y;
        }
        
        // Load player component
        var player = _world.GetComponent<PlayerComponent>(playerEntity);
        if (player != null)
        {
            player.Speed = playerData.Speed;
        }
        
        // Load health
        var health = _world.GetComponent<HealthComponent>(playerEntity);
        if (health != null)
        {
            health.MaxHealth = playerData.MaxHealth;
            health.CurrentHealth = playerData.CurrentHealth;
        }
        
        // Load currency
        var currency = _world.GetComponent<CurrencyComponent>(playerEntity);
        if (currency != null)
        {
            currency.Gold = playerData.Gold;
        }
        
        // Load inventory atomically
        var inventory = _world.GetComponent<InventoryComponent>(playerEntity);
        if (inventory != null)
        {
            var tempInventory = new Dictionary<TileType, int>();
            bool inventoryValid = true;
            
            foreach (var item in playerData.Inventory)
            {
                if (Enum.TryParse<TileType>(item.Key, out var tileType))
                {
                    if (item.Value < 0)
                    {
                        LogError($"Invalid inventory item count: {item.Key}={item.Value}");
                        inventoryValid = false;
                        break;
                    }
                    tempInventory[tileType] = item.Value;
                }
            }
            
            if (inventoryValid)
            {
                inventory.Clear();
                foreach (var item in tempInventory)
                {
                    inventory.AddItem(item.Key, item.Value);
                }
            }
            else
            {
                LogWarning("Inventory data invalid, keeping existing inventory");
            }
        }
        
        // Load tool
        var tool = _world.GetComponent<ToolComponent>(playerEntity);
        if (tool != null)
        {
            tool.Type = playerData.ToolType;
            tool.Material = playerData.ToolMaterial;
        }
        
        // Load quests
        var questComponent = _world.GetComponent<QuestComponent>(playerEntity);
        if (questComponent != null)
        {
            questComponent.ActiveQuests.Clear();
            questComponent.CompletedQuests.Clear();
            
            foreach (var questData in playerData.ActiveQuests)
            {
                var quest = ConvertDataToQuest(questData);
                questComponent.ActiveQuests.Add(quest);
            }
            
            foreach (var questData in playerData.CompletedQuests)
            {
                var quest = ConvertDataToQuest(questData);
                questComponent.CompletedQuests.Add(quest);
            }
        }
        
        // Load respawn data
        var respawn = _world.GetComponent<RespawnComponent>(playerEntity);
        if (respawn != null)
        {
            respawn.SetRespawnPoint(playerData.RespawnX, playerData.RespawnY);
            respawn.DeathCount = playerData.DeathCount;
        }
        
        // Load experience data
        var experience = _world.GetComponent<ExperienceComponent>(playerEntity);
        if (experience != null)
        {
            experience.CurrentXP = playerData.CurrentXP;
            experience.Level = Math.Max(1, playerData.Level);
            experience.TotalXPEarned = playerData.TotalXPEarned;
            experience.AttackBonus = playerData.AttackBonus;
            experience.DefenseBonus = playerData.DefenseBonus;
            experience.MaxHealthBonus = playerData.MaxHealthBonus;
            experience.SpeedBonus = playerData.SpeedBonus;
            experience.StatPointsAvailable = playerData.StatPointsAvailable;
        }
        
        return true;
    }
    
    private WorldData? SaveWorldData()
    {
        if (_world == null) return null;
        
        var worldData = new WorldData();
        
        // Get chunk manager from shared resources
        var chunkManager = _world.GetSharedResource<ChunkManager>("ChunkManager");
        if (chunkManager != null)
        {
            worldData.ModifiedChunks = SaveModifiedChunks(chunkManager);
            
            // Try to get world seed from terrain generator
            var terrainGen = _world.GetSharedResource<TerrainGenerator>("TerrainGenerator");
            if (terrainGen != null)
            {
                worldData.WorldSeed = terrainGen.Seed;
            }
        }
        
        return worldData;
    }
    
    private void LoadWorldData(WorldData worldData)
    {
        if (_world == null) return;
        
        var chunkManager = _world.GetSharedResource<ChunkManager>("ChunkManager");
        if (chunkManager != null)
        {
            LoadModifiedChunks(chunkManager, worldData.ModifiedChunks);
        }
    }
    
    private List<ChunkData> SaveModifiedChunks(ChunkManager chunkManager)
    {
        var chunksData = new List<ChunkData>();
        
        // Get all loaded chunks and save only modified ones
        var loadedChunks = chunkManager.GetLoadedChunks();
        
        foreach (var chunk in loadedChunks)
        {
            if (!chunk.IsModified) continue;
            
            var chunkData = new ChunkData { ChunkX = chunk.ChunkX };
            
            // Save modified tiles
            for (int x = 0; x < Chunk.CHUNK_WIDTH; x++)
            {
                for (int y = 0; y < Chunk.CHUNK_HEIGHT; y++)
                {
                    var tile = chunk.GetTile(x, y);
                    string key = $"{x},{y}";
                    chunkData.ModifiedTiles[key] = tile.ToString();
                }
                
                // Save vegetation
                var vegetation = chunk.GetVegetation(x);
                if (vegetation.HasValue)
                {
                    chunkData.ModifiedVegetation[x] = vegetation.Value.ToString();
                }
            }
            
            chunksData.Add(chunkData);
        }
        
        return chunksData;
    }
    
    private void LoadModifiedChunks(ChunkManager chunkManager, List<ChunkData> chunksData)
    {
        foreach (var chunkData in chunksData)
        {
            var chunk = chunkManager.GetChunkSync(chunkData.ChunkX);
            
            // Restore modified tiles
            foreach (var tile in chunkData.ModifiedTiles)
            {
                var coords = tile.Key.Split(',');
                if (coords.Length == 2 && 
                    int.TryParse(coords[0], out int x) && 
                    int.TryParse(coords[1], out int y) &&
                    Enum.TryParse<TileType>(tile.Value, out var tileType))
                {
                    chunk.SetTile(x, y, tileType);
                }
            }
            
            // Restore vegetation
            foreach (var veg in chunkData.ModifiedVegetation)
            {
                if (veg.Value != null && Enum.TryParse<TileType>(veg.Value, out var tileType))
                {
                    chunk.SetVegetation(veg.Key, tileType);
                }
                else
                {
                    chunk.SetVegetation(veg.Key, null);
                }
            }
        }
    }
    
    private TimeData? SaveTimeData()
    {
        if (_world == null) return null;
        
        var timeSystem = _world.GetSharedResource<TimeSystem>("TimeSystem");
        if (timeSystem == null) return null;
        
        return new TimeData
        {
            CurrentTime = timeSystem.CurrentTime,
            TimeScale = timeSystem.TimeScale,
            DayCount = timeSystem.DayCount
        };
    }
    
    private void LoadTimeData(TimeData timeData)
    {
        if (_world == null) return;
        
        var timeSystem = _world.GetSharedResource<TimeSystem>("TimeSystem");
        if (timeSystem == null) return;
        
        // Use public method instead of reflection
        timeSystem.RestoreTimeState(timeData.CurrentTime, timeData.DayCount);
        timeSystem.TimeScale = timeData.TimeScale;
    }
    
    private WeatherData? SaveWeatherData()
    {
        if (_world == null) return null;
        
        var weatherSystem = _world.GetSharedResource<WeatherSystem>("WeatherSystem");
        if (weatherSystem == null) return null;
        
        // Access public properties only
        return new WeatherData
        {
            CurrentWeather = weatherSystem.CurrentWeather.ToString(),
            CurrentIntensity = weatherSystem.CurrentIntensity.ToString(),
            WeatherTimer = 0f,
            WeatherDuration = 300f
        };
    }
    
    private void LoadWeatherData(WeatherData weatherData)
    {
        if (_world == null) return;
        
        var weatherSystem = _world.GetSharedResource<WeatherSystem>("WeatherSystem");
        if (weatherSystem == null) return;
        
        if (Enum.TryParse<WeatherType>(weatherData.CurrentWeather, out var weatherType) &&
            Enum.TryParse<WeatherIntensity>(weatherData.CurrentIntensity, out var intensity))
        {
            // Use public method instead of reflection
            weatherSystem.RestoreWeatherState(weatherType, intensity, weatherData.WeatherTimer, weatherData.WeatherDuration);
        }
    }
    
    private List<NPCData> SaveNPCData()
    {
        if (_world == null) return new List<NPCData>();
        
        var npcsData = new List<NPCData>();
        var npcEntities = _world.GetEntitiesWithComponent<NPCComponent>();
        
        foreach (var npcEntity in npcEntities)
        {
            var npc = _world.GetComponent<NPCComponent>(npcEntity);
            var position = _world.GetComponent<PositionComponent>(npcEntity);
            
            if (npc == null || position == null) continue;
            
            var npcData = new NPCData
            {
                EntityId = npcEntity.Id,
                Name = npc.Name,
                Role = npc.Role.ToString(),
                X = position.X,
                Y = position.Y,
                CurrentActivity = npc.CurrentActivity
            };
            
            // Save shop inventory
            foreach (var item in npc.ShopInventory)
            {
                npcData.ShopInventory[item.Key.ToString()] = item.Value;
            }
            
            // Save shop prices
            foreach (var price in npc.ShopPrices)
            {
                npcData.ShopPrices[price.Key.ToString()] = price.Value;
            }
            
            npcsData.Add(npcData);
        }
        
        return npcsData;
    }
    
    private void LoadNPCData(List<NPCData> npcsData)
    {
        if (_world == null) return;
        
        // Match NPCs by name (since entity IDs might change)
        var npcEntities = _world.GetEntitiesWithComponent<NPCComponent>().ToList();
        
        foreach (var npcData in npcsData)
        {
            Entity? targetEntity = null;
            
            // Find matching NPC by name
            foreach (var npcEntity in npcEntities)
            {
                var npc = _world.GetComponent<NPCComponent>(npcEntity);
                if (npc != null && npc.Name == npcData.Name)
                {
                    targetEntity = npcEntity;
                    break;
                }
            }
            
            if (targetEntity == null) continue;
            
            var npcComponent = _world.GetComponent<NPCComponent>(targetEntity.Value);
            var position = _world.GetComponent<PositionComponent>(targetEntity.Value);
            
            if (npcComponent != null)
            {
                npcComponent.CurrentActivity = npcData.CurrentActivity;
                
                // Restore shop inventory
                npcComponent.ShopInventory.Clear();
                foreach (var item in npcData.ShopInventory)
                {
                    if (Enum.TryParse<TileType>(item.Key, out var tileType))
                    {
                        npcComponent.ShopInventory[tileType] = item.Value;
                    }
                }
                
                // Restore shop prices
                npcComponent.ShopPrices.Clear();
                foreach (var price in npcData.ShopPrices)
                {
                    if (Enum.TryParse<TileType>(price.Key, out var tileType))
                    {
                        npcComponent.ShopPrices[tileType] = price.Value;
                    }
                }
            }
            
            if (position != null)
            {
                position.X = npcData.X;
                position.Y = npcData.Y;
            }
        }
    }
    
    private QuestData ConvertQuestToData(Quest quest)
    {
        var questData = new QuestData
        {
            Id = quest.Id,
            Name = quest.Name,
            Description = quest.Description,
            Type = quest.Type.ToString(),
            Status = quest.Status.ToString(),
            CurrentProgress = quest.CurrentProgress,
            RequiredProgress = quest.RequiredProgress,
            GoldReward = quest.GoldReward,
            ExperienceReward = quest.ExperienceReward,
            UnlockAbility = quest.UnlockAbility,
            UnlockArea = quest.UnlockArea,
            RequiredAbility = quest.RequiredAbility,
            RequiredLevel = quest.RequiredLevel
        };
        
        foreach (var item in quest.ItemRewards)
        {
            questData.ItemRewards[item.Key.ToString()] = item.Value;
        }
        
        return questData;
    }
    
    private Quest ConvertDataToQuest(QuestData questData)
    {
        Enum.TryParse<QuestType>(questData.Type, out var questType);
        Enum.TryParse<QuestStatus>(questData.Status, out var questStatus);
        
        var quest = new Quest(questData.Id, questData.Name, questData.Description, questType)
        {
            Status = questStatus,
            CurrentProgress = questData.CurrentProgress,
            RequiredProgress = questData.RequiredProgress,
            GoldReward = questData.GoldReward,
            ExperienceReward = questData.ExperienceReward,
            UnlockAbility = questData.UnlockAbility,
            UnlockArea = questData.UnlockArea,
            RequiredAbility = questData.RequiredAbility,
            RequiredLevel = questData.RequiredLevel
        };
        
        foreach (var item in questData.ItemRewards)
        {
            if (Enum.TryParse<TileType>(item.Key, out var tileType))
            {
                quest.ItemRewards[tileType] = item.Value;
            }
        }
        
        return quest;
    }
    
    // Validation methods
    
    private bool ValidateSaveFormat(SaveData saveData)
    {
        return saveData.SaveFormatVersion == CURRENT_SAVE_FORMAT_VERSION;
    }
    
    private bool ValidatePlayerData(PlayerData playerData)
    {
        // Validate health values
        if (playerData.MaxHealth <= 0 || playerData.CurrentHealth < 0 || playerData.CurrentHealth > playerData.MaxHealth)
        {
            LogError($"Invalid health values: Current={playerData.CurrentHealth}, Max={playerData.MaxHealth}");
            return false;
        }
        
        // Validate currency
        if (playerData.Gold < 0)
        {
            LogError($"Invalid gold value: {playerData.Gold}");
            return false;
        }
        
        // Validate speed
        if (playerData.Speed < 0)
        {
            LogError($"Invalid speed value: {playerData.Speed}");
            return false;
        }
        
        return true;
    }
    
    // Logging methods
    
    private void LogInfo(string message)
    {
        Console.WriteLine($"[SaveSystem] {message}");
    }
    
    private void LogWarning(string message)
    {
        Console.WriteLine($"[SaveSystem] Warning: {message}");
    }
    
    private void LogError(string message)
    {
        Console.WriteLine($"[SaveSystem] Error: {message}");
    }
}
