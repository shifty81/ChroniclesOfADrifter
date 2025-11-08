namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// Character creator component - manages available customization options
/// </summary>
public class CharacterCreatorComponent : IComponent
{
    /// <summary>
    /// Available skin tones
    /// </summary>
    public List<string> AvailableSkinTones { get; set; }
    
    /// <summary>
    /// Available hair styles
    /// </summary>
    public List<string> AvailableHairStyles { get; set; }
    
    /// <summary>
    /// Available body types
    /// </summary>
    public List<string> AvailableBodyTypes { get; set; }
    
    /// <summary>
    /// Available clothing styles by type
    /// </summary>
    public Dictionary<string, List<string>> AvailableClothingStyles { get; set; }
    
    /// <summary>
    /// Preset color palettes for quick selection
    /// </summary>
    public List<ColorPalette> ColorPalettes { get; set; }
    
    public CharacterCreatorComponent()
    {
        AvailableSkinTones = new List<string>
        {
            "pale", "light", "medium", "tan", "brown", "dark"
        };
        
        AvailableHairStyles = new List<string>
        {
            "short", "long", "ponytail", "bald", "curly", "braided", "spiky"
        };
        
        AvailableBodyTypes = new List<string>
        {
            "slim", "average", "athletic", "heavy"
        };
        
        AvailableClothingStyles = new Dictionary<string, List<string>>
        {
            { "shirt", new List<string> { "tunic", "vest", "robe", "armor_shirt", "peasant" } },
            { "pants", new List<string> { "trousers", "leggings", "shorts", "armor_pants" } },
            { "boots", new List<string> { "leather", "heavy", "light", "traveling" } },
            { "gloves", new List<string> { "fingerless", "leather", "cloth", "armored" } },
            { "hat", new List<string> { "hood", "cap", "helmet", "bandana", "none" } }
        };
        
        ColorPalettes = new List<ColorPalette>
        {
            new ColorPalette("Earth Tones", new Color(139, 90, 43), new Color(101, 67, 33)),
            new ColorPalette("Forest", new Color(34, 139, 34), new Color(85, 107, 47)),
            new ColorPalette("Ocean", new Color(70, 130, 180), new Color(30, 80, 120)),
            new ColorPalette("Crimson", new Color(178, 34, 34), new Color(139, 0, 0)),
            new ColorPalette("Royal", new Color(75, 0, 130), new Color(138, 43, 226)),
            new ColorPalette("Neutral", new Color(105, 105, 105), new Color(169, 169, 169)),
            new ColorPalette("Midnight", new Color(25, 25, 112), new Color(0, 0, 0)),
            new ColorPalette("Desert", new Color(210, 180, 140), new Color(244, 164, 96))
        };
    }
}

/// <summary>
/// Color palette for quick customization
/// </summary>
public class ColorPalette
{
    public string Name { get; set; }
    public Color PrimaryColor { get; set; }
    public Color SecondaryColor { get; set; }
    
    public ColorPalette(string name, Color primaryColor, Color secondaryColor)
    {
        Name = name;
        PrimaryColor = primaryColor;
        SecondaryColor = secondaryColor;
    }
}
