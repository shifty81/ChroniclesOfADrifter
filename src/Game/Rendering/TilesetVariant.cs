using ChroniclesOfADrifter.ECS.Components;

namespace ChroniclesOfADrifter.Rendering;

/// <summary>
/// Represents different tileset variant types
/// </summary>
public enum TilesetVariantType
{
    Default,
    Spring,
    Summer,
    Fall,
    Winter,
    Dawn,
    Day,
    Dusk,
    Night,
    Rainy,
    Snowy,
    Foggy
}

/// <summary>
/// Manages tileset variants for seasonal and time-based visual changes
/// </summary>
public class TilesetVariantManager
{
    private TileCustomizationManager customizationManager;
    private TilesetVariantType currentVariant;
    private Dictionary<TilesetVariantType, Dictionary<TileType, float[]>> variantColorMods;
    
    public TilesetVariantManager(TileCustomizationManager customizationManager)
    {
        this.customizationManager = customizationManager;
        this.currentVariant = TilesetVariantType.Default;
        this.variantColorMods = new Dictionary<TilesetVariantType, Dictionary<TileType, float[]>>();
        
        InitializeVariants();
    }
    
    /// <summary>
    /// Initialize color modifications for each variant
    /// </summary>
    private void InitializeVariants()
    {
        // Spring - vibrant greens, more flowers
        variantColorMods[TilesetVariantType.Spring] = new()
        {
            [TileType.Grass] = new float[] { 0.20f, 0.75f, 0.20f },
            [TileType.TreeOak] = new float[] { 0.15f, 0.55f, 0.15f },
            [TileType.TreePine] = new float[] { 0.15f, 0.55f, 0.15f },
        };
        
        // Summer - warm, bright colors
        variantColorMods[TilesetVariantType.Summer] = new()
        {
            [TileType.Grass] = new float[] { 0.15f, 0.70f, 0.15f },
            [TileType.Sand] = new float[] { 1.0f, 0.92f, 0.60f },
            [TileType.Water] = new float[] { 0.25f, 0.75f, 0.95f },
        };
        
        // Fall - orange and brown tones
        variantColorMods[TilesetVariantType.Fall] = new()
        {
            [TileType.Grass] = new float[] { 0.50f, 0.45f, 0.20f },
            [TileType.TreeOak] = new float[] { 0.75f, 0.45f, 0.15f },
            [TileType.TreePine] = new float[] { 0.65f, 0.45f, 0.20f },
            [TileType.Bush] = new float[] { 0.60f, 0.40f, 0.15f },
        };
        
        // Winter - cold, desaturated colors
        variantColorMods[TilesetVariantType.Winter] = new()
        {
            [TileType.Grass] = new float[] { 0.75f, 0.80f, 0.85f },
            [TileType.Dirt] = new float[] { 0.60f, 0.60f, 0.65f },
            [TileType.Water] = new float[] { 0.40f, 0.50f, 0.60f },
            [TileType.TreeOak] = new float[] { 0.30f, 0.35f, 0.40f },
            [TileType.TreePine] = new float[] { 0.25f, 0.40f, 0.35f },
        };
        
        // Dawn - cool morning light
        variantColorMods[TilesetVariantType.Dawn] = new()
        {
            [TileType.Grass] = new float[] { 0.18f, 0.70f, 0.75f },
            [TileType.Sand] = new float[] { 0.95f, 0.85f, 0.75f },
            [TileType.Water] = new float[] { 0.50f, 0.65f, 0.85f },
        };
        
        // Day - standard colors (no modification)
        variantColorMods[TilesetVariantType.Day] = new();
        
        // Dusk - warm evening light
        variantColorMods[TilesetVariantType.Dusk] = new()
        {
            [TileType.Grass] = new float[] { 0.35f, 0.55f, 0.30f },
            [TileType.Sand] = new float[] { 0.90f, 0.70f, 0.45f },
            [TileType.Water] = new float[] { 0.35f, 0.50f, 0.70f },
            [TileType.Stone] = new float[] { 0.55f, 0.45f, 0.40f },
        };
        
        // Night - dark, desaturated colors
        variantColorMods[TilesetVariantType.Night] = new()
        {
            [TileType.Grass] = new float[] { 0.08f, 0.20f, 0.18f },
            [TileType.Dirt] = new float[] { 0.25f, 0.22f, 0.18f },
            [TileType.Sand] = new float[] { 0.30f, 0.28f, 0.25f },
            [TileType.Water] = new float[] { 0.10f, 0.15f, 0.25f },
            [TileType.Stone] = new float[] { 0.20f, 0.20f, 0.22f },
        };
        
        // Rainy - darker, saturated colors
        variantColorMods[TilesetVariantType.Rainy] = new()
        {
            [TileType.Grass] = new float[] { 0.10f, 0.50f, 0.10f },
            [TileType.Dirt] = new float[] { 0.40f, 0.35f, 0.20f },
            [TileType.Sand] = new float[] { 0.75f, 0.70f, 0.45f },
            [TileType.Stone] = new float[] { 0.35f, 0.35f, 0.35f },
        };
        
        // Snowy - white overlay effect
        variantColorMods[TilesetVariantType.Snowy] = new()
        {
            [TileType.Grass] = new float[] { 0.80f, 0.85f, 0.90f },
            [TileType.Dirt] = new float[] { 0.70f, 0.72f, 0.75f },
            [TileType.TreeOak] = new float[] { 0.40f, 0.45f, 0.50f },
            [TileType.TreePine] = new float[] { 0.35f, 0.45f, 0.45f },
            [TileType.Bush] = new float[] { 0.50f, 0.55f, 0.60f },
        };
        
        // Foggy - desaturated, misty colors
        variantColorMods[TilesetVariantType.Foggy] = new()
        {
            [TileType.Grass] = new float[] { 0.45f, 0.60f, 0.55f },
            [TileType.Dirt] = new float[] { 0.55f, 0.52f, 0.48f },
            [TileType.Water] = new float[] { 0.50f, 0.60f, 0.65f },
            [TileType.TreeOak] = new float[] { 0.35f, 0.45f, 0.40f },
        };
    }
    
    /// <summary>
    /// Apply a tileset variant
    /// </summary>
    public void ApplyVariant(TilesetVariantType variant)
    {
        if (!variantColorMods.ContainsKey(variant))
        {
            Console.WriteLine($"[TilesetVariant] Variant {variant} not found");
            return;
        }
        
        var colorMods = variantColorMods[variant];
        foreach (var kvp in colorMods)
        {
            var color = kvp.Value;
            customizationManager.SetColorOverride(kvp.Key, color[0], color[1], color[2]);
        }
        
        currentVariant = variant;
        Console.WriteLine($"[TilesetVariant] Applied variant: {variant}");
    }
    
    /// <summary>
    /// Get the current active variant
    /// </summary>
    public TilesetVariantType GetCurrentVariant()
    {
        return currentVariant;
    }
    
    /// <summary>
    /// Apply variant based on season
    /// </summary>
    public void ApplySeasonalVariant(int dayOfYear)
    {
        // Simple seasonal calculation (assuming 360 day year)
        TilesetVariantType season = (dayOfYear % 360) switch
        {
            >= 0 and < 90 => TilesetVariantType.Spring,
            >= 90 and < 180 => TilesetVariantType.Summer,
            >= 180 and < 270 => TilesetVariantType.Fall,
            _ => TilesetVariantType.Winter
        };
        
        ApplyVariant(season);
    }
    
    /// <summary>
    /// Apply variant based on time of day
    /// </summary>
    public void ApplyTimeVariant(float timeOfDay)
    {
        // timeOfDay is 0.0 to 24.0
        TilesetVariantType timeVariant = timeOfDay switch
        {
            >= 5.0f and < 7.0f => TilesetVariantType.Dawn,
            >= 7.0f and < 17.0f => TilesetVariantType.Day,
            >= 17.0f and < 19.0f => TilesetVariantType.Dusk,
            _ => TilesetVariantType.Night
        };
        
        ApplyVariant(timeVariant);
    }
    
    /// <summary>
    /// Apply variant based on weather
    /// </summary>
    public void ApplyWeatherVariant(string weatherType)
    {
        TilesetVariantType weatherVariant = weatherType.ToLower() switch
        {
            "rain" or "storm" => TilesetVariantType.Rainy,
            "snow" => TilesetVariantType.Snowy,
            "fog" => TilesetVariantType.Foggy,
            _ => TilesetVariantType.Day
        };
        
        ApplyVariant(weatherVariant);
    }
    
    /// <summary>
    /// Reset to default variant
    /// </summary>
    public void ResetToDefault()
    {
        customizationManager.ClearAllColorOverrides();
        currentVariant = TilesetVariantType.Default;
        Console.WriteLine("[TilesetVariant] Reset to default variant");
    }
}
