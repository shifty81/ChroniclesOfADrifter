namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// Crop growth stage
/// </summary>
public enum CropStage
{
    Seed,
    Sprout,
    Growing,
    Mature,
    Harvestable,
    Dead
}

/// <summary>
/// Seasons affecting crop growth
/// </summary>
public enum Season
{
    Spring,     // Best growing season
    Summer,     // Good for heat-loving crops
    Autumn,     // Harvest season, slower growth
    Winter      // Most crops cannot grow
}

/// <summary>
/// Fertilizer types that affect crop growth
/// </summary>
public enum FertilizerType
{
    None,
    BasicFertilizer,    // 25% faster growth
    QualityFertilizer,  // 50% faster growth, +1 yield
    SuperFertilizer     // 75% faster growth, +2 yield
}

/// <summary>
/// Crop type with growth properties
/// </summary>
public class CropType
{
    public string Name { get; set; }
    public TileType SeedItem { get; set; }
    public TileType HarvestItem { get; set; }
    public int GrowthDays { get; set; }
    public int MinYield { get; set; }
    public int MaxYield { get; set; }
    public int SellPrice { get; set; }
    public bool RequiresWater { get; set; }
    public Season PreferredSeason { get; set; }
    public bool GrowsInWinter { get; set; }
    
    public CropType(string name, TileType seedItem, TileType harvestItem, int growthDays, int minYield, int maxYield, int sellPrice, Season preferredSeason = Season.Spring, bool growsInWinter = false)
    {
        Name = name;
        SeedItem = seedItem;
        HarvestItem = harvestItem;
        GrowthDays = growthDays;
        MinYield = minYield;
        MaxYield = maxYield;
        SellPrice = sellPrice;
        RequiresWater = true;
        PreferredSeason = preferredSeason;
        GrowsInWinter = growsInWinter;
    }
    
    /// <summary>
    /// Get growth speed multiplier based on current season
    /// </summary>
    public float GetSeasonMultiplier(Season currentSeason)
    {
        if (currentSeason == Season.Winter && !GrowsInWinter)
            return 0f; // No growth in winter for most crops
            
        if (currentSeason == PreferredSeason)
            return 1.5f; // 50% faster in preferred season
            
        return currentSeason switch
        {
            Season.Spring => 1.2f,
            Season.Summer => 1.0f,
            Season.Autumn => 0.8f,
            Season.Winter => 0.5f,
            _ => 1.0f
        };
    }
}

/// <summary>
/// Individual crop instance
/// </summary>
public class Crop
{
    public CropType Type { get; set; }
    public CropStage Stage { get; set; }
    public int DaysGrowing { get; set; }
    public bool IsWatered { get; set; }
    public DateTime PlantedDate { get; set; }
    public float GrowthProgress { get; set; }
    
    public Crop(CropType type)
    {
        Type = type;
        Stage = CropStage.Seed;
        DaysGrowing = 0;
        IsWatered = false;
        PlantedDate = DateTime.Now;
        GrowthProgress = 0f;
    }
    
    /// <summary>
    /// Water the crop
    /// </summary>
    public void Water()
    {
        IsWatered = true;
    }
    
    /// <summary>
    /// Advance crop growth by one day with season and fertilizer modifiers
    /// </summary>
    public void AdvanceDay(Season currentSeason = Season.Spring, FertilizerType fertilizer = FertilizerType.None)
    {
        if (Type.RequiresWater && !IsWatered)
        {
            // No growth without water
            IsWatered = false;
            return;
        }
        
        // Calculate growth rate with modifiers
        float seasonMultiplier = Type.GetSeasonMultiplier(currentSeason);
        float fertilizerMultiplier = fertilizer switch
        {
            FertilizerType.BasicFertilizer => 1.25f,
            FertilizerType.QualityFertilizer => 1.5f,
            FertilizerType.SuperFertilizer => 1.75f,
            _ => 1.0f
        };
        
        float growthRate = seasonMultiplier * fertilizerMultiplier;
        
        if (growthRate > 0)
        {
            GrowthProgress += growthRate;
            DaysGrowing = (int)GrowthProgress;
        }
        
        IsWatered = false; // Reset for next day
        
        // Update stage based on growth progress
        float progress = (float)DaysGrowing / Type.GrowthDays;
        
        if (progress >= 1.0f)
            Stage = CropStage.Harvestable;
        else if (progress >= 0.75f)
            Stage = CropStage.Mature;
        else if (progress >= 0.5f)
            Stage = CropStage.Growing;
        else if (progress >= 0.25f)
            Stage = CropStage.Sprout;
        else
            Stage = CropStage.Seed;
    }
    
    /// <summary>
    /// Check if crop is ready to harvest
    /// </summary>
    public bool IsHarvestable()
    {
        return Stage == CropStage.Harvestable;
    }
    
    /// <summary>
    /// Harvest the crop and get yield (with optional fertilizer bonus)
    /// </summary>
    public int Harvest(FertilizerType fertilizer = FertilizerType.None)
    {
        if (!IsHarvestable())
            return 0;
        
        int yieldBonus = fertilizer switch
        {
            FertilizerType.QualityFertilizer => 1,
            FertilizerType.SuperFertilizer => 2,
            _ => 0
        };
        
        return Random.Shared.Next(Type.MinYield, Type.MaxYield + 1) + yieldBonus;
    }
}

/// <summary>
/// Component representing a farm plot where crops can be planted
/// </summary>
public class FarmPlotComponent : IComponent
{
    public int PlotX { get; set; }
    public int PlotY { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public Crop? CurrentCrop { get; set; }
    public bool IsTilled { get; set; }
    public bool IsWatered { get; set; }
    public FertilizerType Fertilizer { get; set; }
    
    public FarmPlotComponent(int x, int y, int width = 1, int height = 1)
    {
        PlotX = x;
        PlotY = y;
        Width = width;
        Height = height;
        IsTilled = false;
        IsWatered = false;
        CurrentCrop = null;
        Fertilizer = FertilizerType.None;
    }
    
    /// <summary>
    /// Till the plot to prepare for planting
    /// </summary>
    public void Till()
    {
        IsTilled = true;
    }
    
    /// <summary>
    /// Apply fertilizer to the plot
    /// </summary>
    public bool ApplyFertilizer(FertilizerType type)
    {
        if (!IsTilled) return false;
        Fertilizer = type;
        return true;
    }
    
    /// <summary>
    /// Plant a crop in this plot
    /// </summary>
    public bool PlantCrop(CropType cropType)
    {
        if (!IsTilled || CurrentCrop != null)
            return false;
            
        CurrentCrop = new Crop(cropType);
        return true;
    }
    
    /// <summary>
    /// Water the plot and its crop
    /// </summary>
    public void Water()
    {
        IsWatered = true;
        if (CurrentCrop != null)
        {
            CurrentCrop.Water();
        }
    }
    
    /// <summary>
    /// Advance time by one day with season effects
    /// </summary>
    public void AdvanceDay(Season currentSeason = Season.Spring)
    {
        if (CurrentCrop != null)
        {
            CurrentCrop.AdvanceDay(currentSeason, Fertilizer);
        }
        IsWatered = false; // Reset daily water status
    }
    
    /// <summary>
    /// Harvest the crop if ready
    /// </summary>
    public (bool success, TileType? item, int quantity) HarvestCrop()
    {
        if (CurrentCrop == null || !CurrentCrop.IsHarvestable())
            return (false, null, 0);
            
        var harvestItem = CurrentCrop.Type.HarvestItem;
        var quantity = CurrentCrop.Harvest(Fertilizer);
        CurrentCrop = null; // Clear the plot after harvest
        Fertilizer = FertilizerType.None; // Fertilizer consumed with harvest
        
        return (true, harvestItem, quantity);
    }
}
