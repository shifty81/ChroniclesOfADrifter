namespace ChroniclesOfADrifter.Rendering;

/// <summary>
/// Manages loading and accessing tilesets
/// </summary>
public class TilesetManager
{
    private Dictionary<string, Tileset> tilesets = new();
    private string activeTilesetName = string.Empty;
    
    /// <summary>
    /// Load a tileset from file and register it
    /// </summary>
    public bool LoadTileset(string filePath)
    {
        var tileset = Tileset.LoadFromFile(filePath);
        if (tileset != null)
        {
            tilesets[tileset.Name] = tileset;
            Console.WriteLine($"[TilesetManager] Loaded tileset: {tileset.Name}");
            
            // Set as active if it's the first one
            if (string.IsNullOrEmpty(activeTilesetName))
            {
                activeTilesetName = tileset.Name;
            }
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Register a tileset
    /// </summary>
    public void RegisterTileset(Tileset tileset)
    {
        tilesets[tileset.Name] = tileset;
        
        // Set as active if it's the first one
        if (string.IsNullOrEmpty(activeTilesetName))
        {
            activeTilesetName = tileset.Name;
        }
    }
    
    /// <summary>
    /// Get a tileset by name
    /// </summary>
    public Tileset? GetTileset(string name)
    {
        return tilesets.TryGetValue(name, out var tileset) ? tileset : null;
    }
    
    /// <summary>
    /// Get the active tileset
    /// </summary>
    public Tileset? GetActiveTileset()
    {
        return GetTileset(activeTilesetName);
    }
    
    /// <summary>
    /// Set the active tileset
    /// </summary>
    public void SetActiveTileset(string name)
    {
        if (tilesets.ContainsKey(name))
        {
            activeTilesetName = name;
            Console.WriteLine($"[TilesetManager] Active tileset: {name}");
        }
    }
    
    /// <summary>
    /// Get all tileset names
    /// </summary>
    public IEnumerable<string> GetTilesetNames()
    {
        return tilesets.Keys;
    }
    
    /// <summary>
    /// Load all tilesets from a directory
    /// </summary>
    public int LoadTilesetsFromDirectory(string directoryPath)
    {
        int count = 0;
        if (Directory.Exists(directoryPath))
        {
            foreach (var file in Directory.GetFiles(directoryPath, "*.json"))
            {
                if (LoadTileset(file))
                {
                    count++;
                }
            }
            Console.WriteLine($"[TilesetManager] Loaded {count} tilesets from {directoryPath}");
        }
        return count;
    }
    
    /// <summary>
    /// Create a default tileset if none are loaded
    /// </summary>
    public void CreateDefaultTileset()
    {
        var defaultTileset = new Tileset
        {
            Name = "default",
            Description = "Default terrain tileset",
            TileSize = 32
        };
        
        // Add basic terrain tiles
        defaultTileset.Tiles["grass"] = new TileDefinition
        {
            Name = "grass",
            DisplayName = "Grass",
            Color = new float[] { 0.13f, 0.65f, 0.13f },
            IsCollidable = false,
            Category = "terrain"
        };
        
        defaultTileset.Tiles["dirt"] = new TileDefinition
        {
            Name = "dirt",
            DisplayName = "Dirt",
            Color = new float[] { 0.55f, 0.47f, 0.25f },
            IsCollidable = false,
            Category = "terrain"
        };
        
        defaultTileset.Tiles["stone"] = new TileDefinition
        {
            Name = "stone",
            DisplayName = "Stone",
            Color = new float[] { 0.50f, 0.50f, 0.50f },
            IsCollidable = true,
            Category = "terrain"
        };
        
        defaultTileset.Tiles["water"] = new TileDefinition
        {
            Name = "water",
            DisplayName = "Water",
            Color = new float[] { 0.20f, 0.60f, 0.85f },
            IsCollidable = false,
            Category = "terrain"
        };
        
        defaultTileset.Tiles["sand"] = new TileDefinition
        {
            Name = "sand",
            DisplayName = "Sand",
            Color = new float[] { 0.93f, 0.87f, 0.51f },
            IsCollidable = false,
            Category = "terrain"
        };
        
        defaultTileset.Tiles["snow"] = new TileDefinition
        {
            Name = "snow",
            DisplayName = "Snow",
            Color = new float[] { 0.95f, 0.95f, 1.0f },
            IsCollidable = false,
            Category = "terrain"
        };
        
        defaultTileset.Tiles["wood"] = new TileDefinition
        {
            Name = "wood",
            DisplayName = "Wood",
            Color = new float[] { 0.55f, 0.35f, 0.16f },
            IsCollidable = true,
            Category = "building"
        };
        
        defaultTileset.Tiles["brick"] = new TileDefinition
        {
            Name = "brick",
            DisplayName = "Brick",
            Color = new float[] { 0.70f, 0.35f, 0.25f },
            IsCollidable = true,
            Category = "building"
        };
        
        RegisterTileset(defaultTileset);
        Console.WriteLine("[TilesetManager] Created default tileset");
    }
}
