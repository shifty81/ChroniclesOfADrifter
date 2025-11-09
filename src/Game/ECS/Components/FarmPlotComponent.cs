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
    
    public CropType(string name, TileType seedItem, TileType harvestItem, int growthDays, int minYield, int maxYield, int sellPrice)
    {
        Name = name;
        SeedItem = seedItem;
        HarvestItem = harvestItem;
        GrowthDays = growthDays;
        MinYield = minYield;
        MaxYield = maxYield;
        SellPrice = sellPrice;
        RequiresWater = true;
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
    
    public Crop(CropType type)
    {
        Type = type;
        Stage = CropStage.Seed;
        DaysGrowing = 0;
        IsWatered = false;
        PlantedDate = DateTime.Now;
    }
    
    /// <summary>
    /// Water the crop
    /// </summary>
    public void Water()
    {
        IsWatered = true;
    }
    
    /// <summary>
    /// Advance crop growth by one day
    /// </summary>
    public void AdvanceDay()
    {
        if (Type.RequiresWater && !IsWatered)
        {
            // No growth without water
            IsWatered = false;
            return;
        }
        
        DaysGrowing++;
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
    /// Harvest the crop and get yield
    /// </summary>
    public int Harvest()
    {
        if (!IsHarvestable())
            return 0;
            
        return Random.Shared.Next(Type.MinYield, Type.MaxYield + 1);
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
    
    public FarmPlotComponent(int x, int y, int width = 1, int height = 1)
    {
        PlotX = x;
        PlotY = y;
        Width = width;
        Height = height;
        IsTilled = false;
        IsWatered = false;
        CurrentCrop = null;
    }
    
    /// <summary>
    /// Till the plot to prepare for planting
    /// </summary>
    public void Till()
    {
        IsTilled = true;
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
    /// Advance time by one day
    /// </summary>
    public void AdvanceDay()
    {
        if (CurrentCrop != null)
        {
            CurrentCrop.AdvanceDay();
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
        var quantity = CurrentCrop.Harvest();
        CurrentCrop = null; // Clear the plot after harvest
        
        return (true, harvestItem, quantity);
    }
}
