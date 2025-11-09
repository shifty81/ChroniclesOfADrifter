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
    SilverOre,      // Silver ore deposits
    CoalOre,        // Coal ore deposits
    DiamondOre,     // Diamond ore deposits
    DeepWater,      // Deep water blocks
    Sandstone,      // Sandstone blocks
    Limestone,      // Limestone blocks
    
    // Building materials
    Wood,           // Wood planks for building
    WoodPlank,      // Processed wood planks
    Cobblestone,    // Cobblestone blocks
    Brick,          // Brick blocks
    
    // Vegetation types
    TreeOak,        // Oak tree (Forest/Plains biome)
    TreePine,       // Pine tree (generic)
    TreePalm,       // Palm tree (Desert biome)
    TallGrass,      // Tall grass decoration
    Bush,           // Bush decoration
    Cactus,         // Desert cactus
    Flower,         // Small flower decoration
    
    // Light sources
    Torch,          // Placed torch (emits light)
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
            TileType.DeepWater => false,
            TileType.TallGrass => false,
            TileType.Flower => false,
            TileType.Torch => false,
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
            TileType.SilverOre => 'S',
            TileType.CoalOre => 'O',
            TileType.DiamondOre => 'D',
            TileType.DeepWater => '≋',
            TileType.Sandstone => '▒',
            TileType.Limestone => '░',
            TileType.Wood => '╬',
            TileType.WoodPlank => '═',
            TileType.Cobblestone => '▒',
            TileType.Brick => '▓',
            TileType.TreeOak => '♣',
            TileType.TreePine => '♠',
            TileType.TreePalm => 'Ψ',
            TileType.TallGrass => '"',
            TileType.Bush => '♠',
            TileType.Cactus => '‡',
            TileType.Flower => '✿',
            TileType.Torch => '☼',
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
            TileType.SilverOre => ConsoleColor.Gray,
            TileType.CoalOre => ConsoleColor.Black,
            TileType.DiamondOre => ConsoleColor.Cyan,
            TileType.DeepWater => ConsoleColor.DarkBlue,
            TileType.Sandstone => ConsoleColor.Yellow,
            TileType.Limestone => ConsoleColor.White,
            TileType.Wood => ConsoleColor.DarkYellow,
            TileType.WoodPlank => ConsoleColor.Yellow,
            TileType.Cobblestone => ConsoleColor.Gray,
            TileType.Brick => ConsoleColor.DarkRed,
            TileType.TreeOak => ConsoleColor.DarkGreen,
            TileType.TreePine => ConsoleColor.DarkGreen,
            TileType.TreePalm => ConsoleColor.Green,
            TileType.TallGrass => ConsoleColor.Green,
            TileType.Bush => ConsoleColor.DarkGreen,
            TileType.Cactus => ConsoleColor.Green,
            TileType.Flower => ConsoleColor.Magenta,
            TileType.Torch => ConsoleColor.Yellow,
            _ => ConsoleColor.White
        };
    }
    
    /// <summary>
    /// Checks if a tile type is vegetation
    /// </summary>
    public static bool IsVegetation(this TileType type)
    {
        return type switch
        {
            TileType.TreeOak => true,
            TileType.TreePine => true,
            TileType.TreePalm => true,
            TileType.TallGrass => true,
            TileType.Bush => true,
            TileType.Cactus => true,
            TileType.Flower => true,
            _ => false
        };
    }
    
    /// <summary>
    /// Checks if a tile type emits light
    /// </summary>
    public static bool IsLightSource(this TileType type)
    {
        return type switch
        {
            TileType.Torch => true,
            _ => false
        };
    }
    
    /// <summary>
    /// Gets the light radius emitted by this tile type (in blocks)
    /// </summary>
    public static float GetLightRadius(this TileType type)
    {
        return type switch
        {
            TileType.Torch => 8.0f,
            _ => 0f
        };
    }
    
    /// <summary>
    /// Gets the light intensity emitted by this tile type (0.0 to 1.0)
    /// </summary>
    public static float GetLightIntensity(this TileType type)
    {
        return type switch
        {
            TileType.Torch => 1.0f,
            _ => 0f
        };
    }
    
    /// <summary>
    /// Gets the hardness of a block (time to mine in seconds with bare hands)
    /// </summary>
    public static float GetHardness(this TileType type)
    {
        return type switch
        {
            TileType.Air => 0f,
            TileType.TallGrass => 0.1f,
            TileType.Flower => 0.1f,
            TileType.Bush => 0.3f,
            TileType.Dirt => 0.5f,
            TileType.Sand => 0.5f,
            TileType.Grass => 0.6f,
            TileType.Snow => 0.6f,
            TileType.TreeOak => 2.0f,
            TileType.TreePine => 2.0f,
            TileType.TreePalm => 2.0f,
            TileType.Cactus => 1.5f,
            TileType.Stone => 5.0f,
            TileType.CopperOre => 6.0f,
            TileType.IronOre => 8.0f,
            TileType.DeepStone => 10.0f,
            TileType.GoldOre => 12.0f,
            TileType.SilverOre => 10.0f,
            TileType.CoalOre => 4.0f,
            TileType.DiamondOre => 15.0f,
            TileType.Sandstone => 4.5f,
            TileType.Limestone => 4.0f,
            TileType.Wood => 2.5f,
            TileType.WoodPlank => 2.0f,
            TileType.Cobblestone => 4.0f,
            TileType.Brick => 3.5f,
            TileType.Bedrock => float.PositiveInfinity, // Unbreakable
            TileType.Water => 0f,
            TileType.Torch => 0.5f,
            _ => 1.0f
        };
    }
    
    /// <summary>
    /// Gets the required tool type to efficiently mine this block
    /// </summary>
    public static ToolType GetRequiredToolType(this TileType type)
    {
        return type switch
        {
            TileType.Stone => ToolType.Pickaxe,
            TileType.DeepStone => ToolType.Pickaxe,
            TileType.CopperOre => ToolType.Pickaxe,
            TileType.IronOre => ToolType.Pickaxe,
            TileType.GoldOre => ToolType.Pickaxe,
            TileType.SilverOre => ToolType.Pickaxe,
            TileType.CoalOre => ToolType.Pickaxe,
            TileType.DiamondOre => ToolType.Pickaxe,
            TileType.Sandstone => ToolType.Pickaxe,
            TileType.Limestone => ToolType.Pickaxe,
            TileType.Cobblestone => ToolType.Pickaxe,
            TileType.Brick => ToolType.Pickaxe,
            TileType.TreeOak => ToolType.Axe,
            TileType.TreePine => ToolType.Axe,
            TileType.TreePalm => ToolType.Axe,
            TileType.Wood => ToolType.Axe,
            TileType.WoodPlank => ToolType.Axe,
            TileType.Dirt => ToolType.Shovel,
            TileType.Sand => ToolType.Shovel,
            TileType.Grass => ToolType.Shovel,
            TileType.Snow => ToolType.Shovel,
            _ => ToolType.None
        };
    }
    
    /// <summary>
    /// Gets the minimum tool material required to mine this block
    /// </summary>
    public static ToolMaterial GetMinimumToolMaterial(this TileType type)
    {
        return type switch
        {
            TileType.Stone => ToolMaterial.Wood,
            TileType.CopperOre => ToolMaterial.Wood,
            TileType.CoalOre => ToolMaterial.Wood,
            TileType.Sandstone => ToolMaterial.Wood,
            TileType.Limestone => ToolMaterial.Wood,
            TileType.IronOre => ToolMaterial.Stone,
            TileType.DeepStone => ToolMaterial.Stone,
            TileType.SilverOre => ToolMaterial.Iron,
            TileType.GoldOre => ToolMaterial.Iron,
            TileType.DiamondOre => ToolMaterial.Iron,
            TileType.Bedrock => ToolMaterial.None, // Unbreakable
            _ => ToolMaterial.None
        };
    }
    
    /// <summary>
    /// Gets the item that drops when this block is mined
    /// </summary>
    public static TileType GetDroppedItem(this TileType type)
    {
        // Most blocks drop themselves
        return type switch
        {
            TileType.Grass => TileType.Dirt, // Grass drops dirt
            TileType.Air => TileType.Air,     // Air drops nothing
            TileType.Water => TileType.Air,   // Water drops nothing
            TileType.Bedrock => TileType.Air, // Bedrock can't be mined
            _ => type // Everything else drops itself
        };
    }
    
    /// <summary>
    /// Gets the quantity of items dropped when this block is mined
    /// </summary>
    public static int GetDropQuantity(this TileType type)
    {
        return type switch
        {
            TileType.Air => 0,
            TileType.Water => 0,
            TileType.Bedrock => 0,
            TileType.TallGrass => 0, // Grass doesn't drop items
            TileType.Flower => 0,    // Flowers don't drop items (could drop seeds later)
            _ => 1
        };
    }
    
    /// <summary>
    /// Checks if a block can be mined/destroyed
    /// </summary>
    public static bool IsMineable(this TileType type)
    {
        return type switch
        {
            TileType.Air => false,
            TileType.Water => false,
            TileType.Bedrock => false,
            _ => true
        };
    }
}
