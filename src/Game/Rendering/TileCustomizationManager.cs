using ChroniclesOfADrifter.ECS.Components;

namespace ChroniclesOfADrifter.Rendering;

/// <summary>
/// Manages runtime customization of tile appearances, including color overrides,
/// tileset mappings, and theme switching
/// </summary>
public class TileCustomizationManager
{
    private TilesetManager tilesetManager;
    private Dictionary<TileType, string> tileToTilesetMapping;
    private Dictionary<TileType, float[]> colorOverrides;
    private string currentTheme;
    
    // Predefined themes
    private static readonly Dictionary<string, Dictionary<TileType, float[]>> Themes = new()
    {
        ["Default"] = new(),
        ["Zelda"] = new()
        {
            [TileType.Grass] = new float[] { 0.13f, 0.65f, 0.13f },
            [TileType.Dirt] = new float[] { 0.55f, 0.47f, 0.25f },
            [TileType.Stone] = new float[] { 0.45f, 0.45f, 0.45f },
            [TileType.Water] = new float[] { 0.30f, 0.70f, 0.90f },
            [TileType.Sand] = new float[] { 0.93f, 0.87f, 0.51f },
        },
        ["Dark"] = new()
        {
            [TileType.Grass] = new float[] { 0.08f, 0.40f, 0.08f },
            [TileType.Dirt] = new float[] { 0.35f, 0.27f, 0.15f },
            [TileType.Stone] = new float[] { 0.25f, 0.25f, 0.25f },
            [TileType.Water] = new float[] { 0.15f, 0.35f, 0.45f },
            [TileType.Sand] = new float[] { 0.70f, 0.65f, 0.40f },
        },
        ["Bright"] = new()
        {
            [TileType.Grass] = new float[] { 0.20f, 0.85f, 0.20f },
            [TileType.Dirt] = new float[] { 0.75f, 0.67f, 0.45f },
            [TileType.Stone] = new float[] { 0.70f, 0.70f, 0.70f },
            [TileType.Water] = new float[] { 0.45f, 0.85f, 1.0f },
            [TileType.Sand] = new float[] { 1.0f, 0.95f, 0.70f },
        },
        ["Volcanic"] = new()
        {
            [TileType.Grass] = new float[] { 0.20f, 0.15f, 0.10f },
            [TileType.Dirt] = new float[] { 0.40f, 0.25f, 0.15f },
            [TileType.Stone] = new float[] { 0.35f, 0.20f, 0.15f },
            [TileType.Water] = new float[] { 0.85f, 0.35f, 0.15f },
            [TileType.Sand] = new float[] { 0.60f, 0.35f, 0.20f },
        }
    };
    
    public TileCustomizationManager(TilesetManager tilesetManager)
    {
        this.tilesetManager = tilesetManager;
        this.tileToTilesetMapping = new Dictionary<TileType, string>();
        this.colorOverrides = new Dictionary<TileType, float[]>();
        this.currentTheme = "Default";
        
        InitializeDefaultMappings();
    }
    
    /// <summary>
    /// Initialize default tile-to-tileset mappings
    /// </summary>
    private void InitializeDefaultMappings()
    {
        // Map TileTypes to tileset tile names
        tileToTilesetMapping[TileType.Grass] = "grass";
        tileToTilesetMapping[TileType.Dirt] = "dirt";
        tileToTilesetMapping[TileType.Stone] = "stone";
        tileToTilesetMapping[TileType.Water] = "water";
        tileToTilesetMapping[TileType.Sand] = "sand";
        tileToTilesetMapping[TileType.Snow] = "snow";
        tileToTilesetMapping[TileType.Wood] = "wood";
        tileToTilesetMapping[TileType.Brick] = "brick";
        tileToTilesetMapping[TileType.TreeOak] = "tree";
        tileToTilesetMapping[TileType.Bush] = "bush";
        tileToTilesetMapping[TileType.Flower] = "flower_red";
    }
    
    /// <summary>
    /// Get the visual definition for a tile type
    /// </summary>
    public TileDefinition? GetTileDefinition(TileType tileType)
    {
        // Check if there's a custom tileset mapping
        if (tileToTilesetMapping.TryGetValue(tileType, out string? tilesetTileName))
        {
            var tileset = tilesetManager.GetActiveTileset();
            if (tileset != null)
            {
                var tileDef = tileset.GetTile(tilesetTileName);
                if (tileDef != null)
                {
                    // Apply color override if exists
                    if (colorOverrides.TryGetValue(tileType, out float[]? colorOverride))
                    {
                        return new TileDefinition
                        {
                            Name = tileDef.Name,
                            DisplayName = tileDef.DisplayName,
                            Color = colorOverride,
                            TexturePath = tileDef.TexturePath,
                            TextureX = tileDef.TextureX,
                            TextureY = tileDef.TextureY,
                            IsCollidable = tileDef.IsCollidable,
                            Category = tileDef.Category
                        };
                    }
                    return tileDef;
                }
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Set a custom color override for a tile type
    /// </summary>
    public void SetColorOverride(TileType tileType, float r, float g, float b)
    {
        colorOverrides[tileType] = new float[] { r, g, b };
        Console.WriteLine($"[TileCustomization] Set color override for {tileType}: ({r}, {g}, {b})");
    }
    
    /// <summary>
    /// Remove color override for a tile type
    /// </summary>
    public void ClearColorOverride(TileType tileType)
    {
        colorOverrides.Remove(tileType);
        Console.WriteLine($"[TileCustomization] Cleared color override for {tileType}");
    }
    
    /// <summary>
    /// Clear all color overrides
    /// </summary>
    public void ClearAllColorOverrides()
    {
        colorOverrides.Clear();
        Console.WriteLine("[TileCustomization] Cleared all color overrides");
    }
    
    /// <summary>
    /// Set custom tileset mapping for a tile type
    /// </summary>
    public void SetTilesetMapping(TileType tileType, string tilesetTileName)
    {
        tileToTilesetMapping[tileType] = tilesetTileName;
        Console.WriteLine($"[TileCustomization] Mapped {tileType} to tileset tile '{tilesetTileName}'");
    }
    
    /// <summary>
    /// Apply a predefined theme
    /// </summary>
    public bool ApplyTheme(string themeName)
    {
        if (!Themes.ContainsKey(themeName))
        {
            Console.WriteLine($"[TileCustomization] Theme '{themeName}' not found");
            return false;
        }
        
        ClearAllColorOverrides();
        
        var theme = Themes[themeName];
        foreach (var kvp in theme)
        {
            colorOverrides[kvp.Key] = kvp.Value;
        }
        
        currentTheme = themeName;
        Console.WriteLine($"[TileCustomization] Applied theme: {themeName}");
        return true;
    }
    
    /// <summary>
    /// Get the current theme name
    /// </summary>
    public string GetCurrentTheme()
    {
        return currentTheme;
    }
    
    /// <summary>
    /// Get all available theme names
    /// </summary>
    public IEnumerable<string> GetAvailableThemes()
    {
        return Themes.Keys;
    }
    
    /// <summary>
    /// Get color for a tile type (with override applied if exists)
    /// </summary>
    public (float r, float g, float b) GetTileColor(TileType tileType)
    {
        var tileDef = GetTileDefinition(tileType);
        if (tileDef != null)
        {
            return tileDef.GetColor();
        }
        
        // Fallback to default console colors
        return (1.0f, 1.0f, 1.0f);
    }
}
