namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// Represents a tile/block type in the world
/// </summary>
public enum TileType
{
    Air = 0,        // Empty space
    Grass,          // Surface grass block
    Dirt,           // Topsoil (layers 0-3)
    Stone,          // Underground stone (layers 4-14)
    DeepStone,      // Deep underground stone (layers 15-19)
    Bedrock,        // Unbreakable bottom layer (layer 20)
    Sand,           // Desert/beach sand
    Water,          // Water blocks
    Snow,           // Snow biome surface
    IronOre,        // Iron ore deposits
    CopperOre,      // Copper ore deposits
    GoldOre,        // Gold ore deposits
}

/// <summary>
/// Component that represents a single tile/block in the world grid
/// </summary>
public struct TileComponent
{
    public TileType Type;
    public int X;              // World X coordinate
    public int Y;              // World Y coordinate (0 = surface, negative = underground)
    public bool IsVisible;     // Whether this tile is currently visible/discovered
    
    public TileComponent(TileType type, int x, int y)
    {
        Type = type;
        X = x;
        Y = y;
        IsVisible = y >= 0; // Surface tiles are visible by default
    }
}

/// <summary>
/// Extension methods for TileType
/// </summary>
public static class TileTypeExtensions
{
    /// <summary>
    /// Checks if a tile type is solid (blocks movement/vision)
    /// </summary>
    public static bool IsSolid(this TileType type)
    {
        return type switch
        {
            TileType.Air => false,
            TileType.Water => false,
            _ => true
        };
    }
    
    /// <summary>
    /// Gets the console character representation for a tile type
    /// </summary>
    public static char GetChar(this TileType type)
    {
        return type switch
        {
            TileType.Air => ' ',
            TileType.Grass => '#',
            TileType.Dirt => '=',
            TileType.Stone => '█',
            TileType.DeepStone => '▓',
            TileType.Bedrock => '■',
            TileType.Sand => '≈',
            TileType.Water => '~',
            TileType.Snow => '*',
            TileType.IronOre => 'I',
            TileType.CopperOre => 'C',
            TileType.GoldOre => 'G',
            _ => '?'
        };
    }
    
    /// <summary>
    /// Gets the console color for a tile type
    /// </summary>
    public static ConsoleColor GetColor(this TileType type)
    {
        return type switch
        {
            TileType.Air => ConsoleColor.Black,
            TileType.Grass => ConsoleColor.Green,
            TileType.Dirt => ConsoleColor.DarkYellow,
            TileType.Stone => ConsoleColor.Gray,
            TileType.DeepStone => ConsoleColor.DarkGray,
            TileType.Bedrock => ConsoleColor.Black,
            TileType.Sand => ConsoleColor.Yellow,
            TileType.Water => ConsoleColor.Blue,
            TileType.Snow => ConsoleColor.White,
            TileType.IronOre => ConsoleColor.DarkRed,
            TileType.CopperOre => ConsoleColor.DarkCyan,
            TileType.GoldOre => ConsoleColor.DarkYellow,
            _ => ConsoleColor.White
        };
    }
}
