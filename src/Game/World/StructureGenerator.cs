using ChroniclesOfADrifter.ECS.Components;

namespace ChroniclesOfADrifter.Terrain;

/// <summary>
/// Types of structures that can generate in the world
/// </summary>
public enum StructureType
{
    // Surface structures
    Village,
    SmallHouse,
    Tower,
    Ruin,
    Campsite,
    Well,
    
    // Underground structures
    TreasureRoom,
    Crypt,
    MineShaft,
    SecretChamber,
    AbandonedBase,
    
    // Dungeon entrances
    CaveEntrance,
    TrapdoorEntrance,
    RuinEntrance
}

/// <summary>
/// Structure template defining how a structure should be built
/// </summary>
public class StructureTemplate
{
    public StructureType Type { get; set; }
    public string Name { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public ECS.Components.TileType[,] Tiles { get; set; }
    public List<(int x, int y, CreatureType creature)> Creatures { get; set; }
    public List<(int x, int y, string item)> Loot { get; set; }
    
    public StructureTemplate(StructureType type, string name, int width, int height)
    {
        Type = type;
        Name = name;
        Width = width;
        Height = height;
        Tiles = new ECS.Components.TileType[width, height];
        Creatures = new List<(int x, int y, CreatureType creature)>();
        Loot = new List<(int x, int y, string item)>();
    }
}

/// <summary>
/// Generates structures like villages, ruins, and treasure rooms
/// </summary>
public class StructureGenerator
{
    private Random random;
    private Dictionary<StructureType, List<StructureTemplate>> templates;
    
    public StructureGenerator(int seed)
    {
        random = new Random(seed);
        templates = new Dictionary<StructureType, List<StructureTemplate>>();
        InitializeTemplates();
    }
    
    /// <summary>
    /// Attempts to place a structure at the given location
    /// Returns true if structure was placed successfully
    /// </summary>
    public bool TryPlaceStructure(ChunkManager chunkManager, StructureType type, int worldX, int worldY)
    {
        if (!templates.ContainsKey(type) || templates[type].Count == 0)
        {
            return false;
        }
        
        // Select a random template for this structure type
        var template = templates[type][random.Next(templates[type].Count)];
        
        // Check if there's enough space (simple check - could be more sophisticated)
        if (!CanPlaceStructure(chunkManager, worldX, worldY, template))
        {
            return false;
        }
        
        // Place the structure
        PlaceStructure(chunkManager, worldX, worldY, template);
        
        return true;
    }
    
    /// <summary>
    /// Checks if a structure can be placed at the given location
    /// </summary>
    private bool CanPlaceStructure(ChunkManager chunkManager, int worldX, int worldY, StructureTemplate template)
    {
        // For now, just check if the area is above ground
        for (int x = 0; x < template.Width; x++)
        {
            for (int y = 0; y < template.Height; y++)
            {
                int checkX = worldX + x;
                int checkY = worldY + y;
                
                // Make sure we're not placing in bedrock or too deep
                if (checkY < 0 || checkY >= Chunk.CHUNK_HEIGHT - 1)
                {
                    return false;
                }
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// Places a structure at the given location
    /// </summary>
    private void PlaceStructure(ChunkManager chunkManager, int worldX, int worldY, StructureTemplate template)
    {
        // Place tiles
        for (int x = 0; x < template.Width; x++)
        {
            for (int y = 0; y < template.Height; y++)
            {
                var tileType = template.Tiles[x, y];
                if (tileType != ECS.Components.TileType.Air)  // Don't overwrite with air
                {
                    chunkManager.SetTile(worldX + x, worldY + y, tileType);
                }
            }
        }
        
        Console.WriteLine($"[StructureGen] Placed {template.Name} at ({worldX}, {worldY})");
        
        // Creatures and loot will be handled by other systems
        // Store structure metadata for later spawning
    }
    
    /// <summary>
    /// Generates structures for a chunk based on biome and depth
    /// </summary>
    public void GenerateStructuresForChunk(ChunkManager chunkManager, Chunk chunk, BiomeType biome, int chunkX)
    {
        int startX = chunk.GetWorldStartX();
        
        // Chance of structure spawning depends on biome
        float structureChance = GetStructureChanceForBiome(biome);
        
        if (random.NextDouble() > structureChance)
        {
            return;  // No structure in this chunk
        }
        
        // Determine structure type based on biome
        var possibleStructures = GetStructureTypesForBiome(biome);
        if (possibleStructures.Count == 0)
        {
            return;
        }
        
        // Pick a random structure type
        var structureType = possibleStructures[random.Next(possibleStructures.Count)];
        
        // Find a suitable location in the chunk (on the surface)
        int surfaceY = FindSurfaceY(chunkManager, startX + Chunk.CHUNK_WIDTH / 2);
        if (surfaceY < 0)
        {
            return;
        }
        
        // Try to place the structure
        TryPlaceStructure(chunkManager, structureType, startX + 5, surfaceY + 1);
    }
    
    /// <summary>
    /// Finds the surface Y coordinate at a given X position
    /// </summary>
    private int FindSurfaceY(ChunkManager chunkManager, int worldX)
    {
        for (int y = 0; y < Chunk.CHUNK_HEIGHT; y++)
        {
            var tile = chunkManager.GetTile(worldX, y);
            if (tile != ECS.Components.TileType.Air)
            {
                return y - 1;  // Return the air block above the surface
            }
        }
        return -1;
    }
    
    /// <summary>
    /// Gets the chance of structure generation for a biome
    /// </summary>
    private float GetStructureChanceForBiome(BiomeType biome)
    {
        return biome switch
        {
            BiomeType.Plains => 0.15f,
            BiomeType.Forest => 0.10f,
            BiomeType.Desert => 0.20f,
            BiomeType.Snow => 0.08f,
            BiomeType.Swamp => 0.12f,
            BiomeType.Rocky => 0.18f,
            BiomeType.Jungle => 0.05f,
            BiomeType.Beach => 0.10f,
            _ => 0.10f
        };
    }
    
    /// <summary>
    /// Gets possible structure types for a biome
    /// </summary>
    private List<StructureType> GetStructureTypesForBiome(BiomeType biome)
    {
        var structures = new List<StructureType>();
        
        switch (biome)
        {
            case BiomeType.Plains:
                structures.Add(StructureType.SmallHouse);
                structures.Add(StructureType.Village);
                structures.Add(StructureType.Well);
                break;
                
            case BiomeType.Forest:
                structures.Add(StructureType.Tower);
                structures.Add(StructureType.Campsite);
                structures.Add(StructureType.SmallHouse);
                break;
                
            case BiomeType.Desert:
                structures.Add(StructureType.Ruin);
                structures.Add(StructureType.Tower);
                break;
                
            case BiomeType.Snow:
                structures.Add(StructureType.SmallHouse);
                structures.Add(StructureType.Tower);
                break;
                
            case BiomeType.Swamp:
                structures.Add(StructureType.Ruin);
                structures.Add(StructureType.Campsite);
                break;
                
            case BiomeType.Rocky:
                structures.Add(StructureType.CaveEntrance);
                structures.Add(StructureType.Tower);
                break;
                
            case BiomeType.Jungle:
                structures.Add(StructureType.Ruin);
                structures.Add(StructureType.Campsite);
                break;
                
            case BiomeType.Beach:
                structures.Add(StructureType.Well);
                structures.Add(StructureType.Campsite);
                break;
        }
        
        return structures;
    }
    
    /// <summary>
    /// Initializes structure templates
    /// </summary>
    private void InitializeTemplates()
    {
        // Small house template
        CreateSmallHouseTemplate();
        
        // Campsite template
        CreateCampsiteTemplate();
        
        // Well template
        CreateWellTemplate();
        
        // Ruin template
        CreateRuinTemplate();
        
        // Tower template
        CreateTowerTemplate();
        
        // Treasure room template
        CreateTreasureRoomTemplate();
    }
    
    private void CreateSmallHouseTemplate()
    {
        var template = new StructureTemplate(StructureType.SmallHouse, "Small House", 7, 5);
        
        // Floor
        for (int x = 1; x < 6; x++)
        {
            template.Tiles[x, 4] = ECS.Components.TileType.Wood;
        }
        
        // Walls
        for (int y = 1; y < 4; y++)
        {
            template.Tiles[0, y] = ECS.Components.TileType.Wood;
            template.Tiles[6, y] = ECS.Components.TileType.Wood;
        }
        for (int x = 0; x < 7; x++)
        {
            template.Tiles[x, 0] = ECS.Components.TileType.Wood;
        }
        
        // Door
        template.Tiles[3, 4] = ECS.Components.TileType.Air;
        
        AddTemplate(StructureType.SmallHouse, template);
    }
    
    private void CreateCampsiteTemplate()
    {
        var template = new StructureTemplate(StructureType.Campsite, "Campsite", 5, 3);
        
        // Simple campfire
        template.Tiles[2, 2] = ECS.Components.TileType.Stone;
        
        AddTemplate(StructureType.Campsite, template);
    }
    
    private void CreateWellTemplate()
    {
        var template = new StructureTemplate(StructureType.Well, "Well", 3, 3);
        
        // Well structure
        template.Tiles[0, 1] = ECS.Components.TileType.Stone;
        template.Tiles[2, 1] = ECS.Components.TileType.Stone;
        template.Tiles[1, 2] = ECS.Components.TileType.Water;
        
        AddTemplate(StructureType.Well, template);
    }
    
    private void CreateRuinTemplate()
    {
        var template = new StructureTemplate(StructureType.Ruin, "Ancient Ruin", 9, 4);
        
        // Partial walls
        template.Tiles[0, 2] = ECS.Components.TileType.Stone;
        template.Tiles[0, 1] = ECS.Components.TileType.Stone;
        template.Tiles[8, 2] = ECS.Components.TileType.Stone;
        template.Tiles[8, 1] = ECS.Components.TileType.Stone;
        
        // Floor
        for (int x = 1; x < 8; x++)
        {
            template.Tiles[x, 3] = ECS.Components.TileType.Stone;
        }
        
        // Add some goblin spawns
        template.Creatures.Add((4, 2, CreatureType.Goblin));
        
        AddTemplate(StructureType.Ruin, template);
    }
    
    private void CreateTowerTemplate()
    {
        var template = new StructureTemplate(StructureType.Tower, "Tower", 5, 7);
        
        // Walls
        for (int y = 1; y < 7; y++)
        {
            template.Tiles[0, y] = ECS.Components.TileType.Stone;
            template.Tiles[4, y] = ECS.Components.TileType.Stone;
        }
        
        // Floor levels
        template.Tiles[1, 6] = ECS.Components.TileType.Wood;
        template.Tiles[2, 6] = ECS.Components.TileType.Wood;
        template.Tiles[3, 6] = ECS.Components.TileType.Wood;
        
        template.Tiles[1, 3] = ECS.Components.TileType.Wood;
        template.Tiles[2, 3] = ECS.Components.TileType.Wood;
        template.Tiles[3, 3] = ECS.Components.TileType.Wood;
        
        // Top
        for (int x = 0; x < 5; x++)
        {
            template.Tiles[x, 0] = ECS.Components.TileType.Stone;
        }
        
        AddTemplate(StructureType.Tower, template);
    }
    
    private void CreateTreasureRoomTemplate()
    {
        var template = new StructureTemplate(StructureType.TreasureRoom, "Treasure Room", 7, 5);
        
        // Walls
        for (int x = 0; x < 7; x++)
        {
            template.Tiles[x, 0] = ECS.Components.TileType.Stone;
            template.Tiles[x, 4] = ECS.Components.TileType.Stone;
        }
        for (int y = 0; y < 5; y++)
        {
            template.Tiles[0, y] = ECS.Components.TileType.Stone;
            template.Tiles[6, y] = ECS.Components.TileType.Stone;
        }
        
        // Entrance
        template.Tiles[3, 4] = ECS.Components.TileType.Air;
        
        // Treasure markers (will be replaced with actual treasure)
        template.Loot.Add((3, 2, "treasure_chest"));
        
        // Guardian
        template.Creatures.Add((3, 1, CreatureType.Skeleton));
        
        AddTemplate(StructureType.TreasureRoom, template);
    }
    
    private void AddTemplate(StructureType type, StructureTemplate template)
    {
        if (!templates.ContainsKey(type))
        {
            templates[type] = new List<StructureTemplate>();
        }
        templates[type].Add(template);
    }
}
