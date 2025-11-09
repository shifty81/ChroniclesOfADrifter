using System.Text.Json;

namespace ChroniclesOfADrifter.Rendering;

/// <summary>
/// Represents a tileset with visual tile definitions
/// </summary>
public class Tileset
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int TileSize { get; set; } = 32;
    public Dictionary<string, TileDefinition> Tiles { get; set; } = new();
    
    /// <summary>
    /// Load a tileset from a JSON file
    /// </summary>
    public static Tileset? LoadFromFile(string filePath)
    {
        try
        {
            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<Tileset>(json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Tileset] Failed to load tileset from {filePath}: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Save tileset to a JSON file
    /// </summary>
    public void SaveToFile(string filePath)
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(filePath, json);
            Console.WriteLine($"[Tileset] Saved tileset to {filePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Tileset] Failed to save tileset to {filePath}: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Get tile definition by name
    /// </summary>
    public TileDefinition? GetTile(string name)
    {
        return Tiles.TryGetValue(name, out var tile) ? tile : null;
    }
}

/// <summary>
/// Definition of a single tile in a tileset
/// </summary>
public class TileDefinition
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public float[] Color { get; set; } = new float[] { 1.0f, 1.0f, 1.0f };
    public string? TexturePath { get; set; }
    public int TextureX { get; set; }
    public int TextureY { get; set; }
    public bool IsCollidable { get; set; }
    public string Category { get; set; } = "default";
    
    /// <summary>
    /// Get RGB color as tuple
    /// </summary>
    public (float r, float g, float b) GetColor()
    {
        if (Color.Length >= 3)
        {
            return (Color[0], Color[1], Color[2]);
        }
        return (1.0f, 1.0f, 1.0f);
    }
}
