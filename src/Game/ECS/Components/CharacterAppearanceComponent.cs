namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// Character appearance component - manages customizable character features
/// </summary>
public class CharacterAppearanceComponent : IComponent
{
    /// <summary>
    /// Skin tone identifier
    /// </summary>
    public string SkinTone { get; set; }
    
    /// <summary>
    /// Hair style identifier
    /// </summary>
    public string HairStyle { get; set; }
    
    /// <summary>
    /// Hair color (RGBA)
    /// </summary>
    public Color HairColor { get; set; }
    
    /// <summary>
    /// Eye color (RGBA)
    /// </summary>
    public Color EyeColor { get; set; }
    
    /// <summary>
    /// Body type identifier
    /// </summary>
    public string BodyType { get; set; }
    
    /// <summary>
    /// Currently equipped clothing layers
    /// </summary>
    public Dictionary<string, ClothingLayer> ClothingLayers { get; set; }
    
    /// <summary>
    /// Currently equipped armor (when equipped, overrides clothing visibility)
    /// </summary>
    public string? EquippedArmor { get; set; }
    
    public CharacterAppearanceComponent()
    {
        SkinTone = "medium";
        HairStyle = "short";
        HairColor = new Color(100, 70, 40, 255);
        EyeColor = new Color(70, 130, 180, 255);
        BodyType = "average";
        ClothingLayers = new Dictionary<string, ClothingLayer>();
        EquippedArmor = null;
    }
}

/// <summary>
/// Represents a single layer of clothing
/// </summary>
public class ClothingLayer
{
    /// <summary>
    /// Clothing type (e.g., "shirt", "pants", "hat")
    /// </summary>
    public string Type { get; set; }
    
    /// <summary>
    /// Style identifier (e.g., "tunic", "robe", "vest")
    /// </summary>
    public string Style { get; set; }
    
    /// <summary>
    /// Primary color (RGBA)
    /// </summary>
    public Color PrimaryColor { get; set; }
    
    /// <summary>
    /// Secondary color for accents (RGBA)
    /// </summary>
    public Color SecondaryColor { get; set; }
    
    /// <summary>
    /// Render order (higher numbers render on top)
    /// </summary>
    public int RenderOrder { get; set; }
    
    /// <summary>
    /// Whether this layer is visible (false when armor is equipped)
    /// </summary>
    public bool IsVisible { get; set; }
    
    public ClothingLayer(string type, string style, Color primaryColor, Color secondaryColor, int renderOrder = 0)
    {
        Type = type;
        Style = style;
        PrimaryColor = primaryColor;
        SecondaryColor = secondaryColor;
        RenderOrder = renderOrder;
        IsVisible = true;
    }
}

/// <summary>
/// Simple color structure for RGBA values
/// </summary>
public struct Color
{
    public byte R { get; set; }
    public byte G { get; set; }
    public byte B { get; set; }
    public byte A { get; set; }
    
    public Color(byte r, byte g, byte b, byte a = 255)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }
    
    public override string ToString() => $"RGBA({R}, {G}, {B}, {A})";
}
