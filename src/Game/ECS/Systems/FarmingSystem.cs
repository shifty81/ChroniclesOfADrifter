using ChroniclesOfADrifter.ECS.Components;
using World = ChroniclesOfADrifter.ECS.World;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// System managing farming mechanics - planting, watering, harvesting
/// with seasonal effects and fertilizer support
/// </summary>
public class FarmingSystem : ISystem
{
    private float dayTimer = 0f;
    private const float secondsPerDay = 600f; // 10 minutes = 1 in-game day
    private const int daysPerSeason = 28; // 28 in-game days per season
    
    private int totalDaysPassed = 0;
    
    /// <summary>
    /// Get the current season based on total days passed
    /// </summary>
    public Season CurrentSeason => (Season)((totalDaysPassed / daysPerSeason) % 4);
    
    /// <summary>
    /// Get the day within the current season (1-28)
    /// </summary>
    public int DayInSeason => (totalDaysPassed % daysPerSeason) + 1;
    
    /// <summary>
    /// Get the total number of days passed
    /// </summary>
    public int TotalDays => totalDaysPassed;
    
    public void Initialize(World world)
    {
        Console.WriteLine("[Farming] Farming system initialized with seasonal support");
    }
    
    public void Update(World world, float deltaTime)
    {
        dayTimer += deltaTime;
        
        // Advance day for all farm plots
        if (dayTimer >= secondsPerDay)
        {
            dayTimer -= secondsPerDay;
            totalDaysPassed++;
            
            var season = CurrentSeason;
            Console.WriteLine($"[Farming] Day {totalDaysPassed} ({season}, Day {DayInSeason}/{daysPerSeason})");
            
            AdvanceAllFarmPlots(world, season);
        }
    }
    
    /// <summary>
    /// Advance all farm plots by one day
    /// </summary>
    private void AdvanceAllFarmPlots(World world, Season season)
    {
        foreach (var entity in world.GetEntitiesWithComponent<FarmPlotComponent>())
        {
            var plot = world.GetComponent<FarmPlotComponent>(entity);
            if (plot != null)
            {
                plot.AdvanceDay(season);
                
                // Log crop status
                if (plot.CurrentCrop != null)
                {
                    Console.WriteLine($"[Farming] Crop at ({plot.PlotX}, {plot.PlotY}) is now {plot.CurrentCrop.Stage} (Day {plot.CurrentCrop.DaysGrowing}/{plot.CurrentCrop.Type.GrowthDays})");
                    
                    if (plot.CurrentCrop.IsHarvestable())
                    {
                        Console.WriteLine($"[Farming] Crop ready to harvest!");
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Set the total days passed (for save/load)
    /// </summary>
    public void RestoreState(int days)
    {
        totalDaysPassed = Math.Max(0, days);
    }
    
    /// <summary>
    /// Till a plot at a specific location
    /// </summary>
    public static bool TillPlot(World world, int x, int y)
    {
        // Check if plot already exists
        foreach (var entity in world.GetEntitiesWithComponent<FarmPlotComponent>())
        {
            var plot = world.GetComponent<FarmPlotComponent>(entity);
            if (plot != null && plot.PlotX == x && plot.PlotY == y)
            {
                plot.Till();
                Console.WriteLine($"[Farming] Tilled plot at ({x}, {y})");
                return true;
            }
        }
        
        // Create new plot
        var newPlotEntity = world.CreateEntity();
        var newPlot = new FarmPlotComponent(x, y);
        newPlot.Till();
        world.AddComponent(newPlotEntity, newPlot);
        
        Console.WriteLine($"[Farming] Created and tilled new plot at ({x}, {y})");
        return true;
    }
    
    /// <summary>
    /// Apply fertilizer to a plot
    /// </summary>
    public static bool FertilizePlot(World world, int x, int y, FertilizerType type)
    {
        foreach (var entity in world.GetEntitiesWithComponent<FarmPlotComponent>())
        {
            var plot = world.GetComponent<FarmPlotComponent>(entity);
            if (plot != null && plot.PlotX == x && plot.PlotY == y)
            {
                if (plot.ApplyFertilizer(type))
                {
                    Console.WriteLine($"[Farming] Applied {type} to plot at ({x}, {y})");
                    return true;
                }
                else
                {
                    Console.WriteLine("[Farming] Plot must be tilled first!");
                    return false;
                }
            }
        }
        
        Console.WriteLine("[Farming] No plot found at that location!");
        return false;
    }
    
    /// <summary>
    /// Plant a crop in a plot
    /// </summary>
    public static bool PlantCrop(World world, int x, int y, CropType cropType, Entity playerEntity)
    {
        // Find plot at location
        foreach (var entity in world.GetEntitiesWithComponent<FarmPlotComponent>())
        {
            var plot = world.GetComponent<FarmPlotComponent>(entity);
            if (plot != null && plot.PlotX == x && plot.PlotY == y)
            {
                // Check if player has seeds
                var inventory = world.GetComponent<InventoryComponent>(playerEntity);
                if (inventory != null && inventory.HasItem(cropType.SeedItem, 1))
                {
                    if (plot.PlantCrop(cropType))
                    {
                        inventory.RemoveItem(cropType.SeedItem, 1);
                        Console.WriteLine($"[Farming] Planted {cropType.Name} at ({x}, {y})");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("[Farming] Plot must be tilled and empty to plant!");
                        return false;
                    }
                }
                else
                {
                    Console.WriteLine("[Farming] You don't have the seeds!");
                    return false;
                }
            }
        }
        
        Console.WriteLine("[Farming] No plot found at that location!");
        return false;
    }
    
    /// <summary>
    /// Water a plot
    /// </summary>
    public static bool WaterPlot(World world, int x, int y)
    {
        foreach (var entity in world.GetEntitiesWithComponent<FarmPlotComponent>())
        {
            var plot = world.GetComponent<FarmPlotComponent>(entity);
            if (plot != null && plot.PlotX == x && plot.PlotY == y)
            {
                plot.Water();
                Console.WriteLine($"[Farming] Watered plot at ({x}, {y})");
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Harvest a crop from a plot
    /// </summary>
    public static bool HarvestCrop(World world, int x, int y, Entity playerEntity)
    {
        foreach (var entity in world.GetEntitiesWithComponent<FarmPlotComponent>())
        {
            var plot = world.GetComponent<FarmPlotComponent>(entity);
            if (plot != null && plot.PlotX == x && plot.PlotY == y)
            {
                var (success, item, quantity) = plot.HarvestCrop();
                
                if (success && item.HasValue)
                {
                    var inventory = world.GetComponent<InventoryComponent>(playerEntity);
                    if (inventory != null)
                    {
                        inventory.AddItem(item.Value, quantity);
                        Console.WriteLine($"[Farming] Harvested {quantity}x {item.Value}!");
                        
                        // Calculate sell value
                        if (plot.CurrentCrop != null)
                        {
                            int totalValue = quantity * plot.CurrentCrop.Type.SellPrice;
                            Console.WriteLine($"[Farming] Estimated value: {totalValue} gold");
                        }
                        
                        return true;
                    }
                }
                else
                {
                    Console.WriteLine("[Farming] Crop is not ready to harvest!");
                }
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Get crop information at a location
    /// </summary>
    public static string? GetCropInfo(World world, int x, int y)
    {
        foreach (var entity in world.GetEntitiesWithComponent<FarmPlotComponent>())
        {
            var plot = world.GetComponent<FarmPlotComponent>(entity);
            if (plot != null && plot.PlotX == x && plot.PlotY == y)
            {
                if (plot.CurrentCrop != null)
                {
                    var crop = plot.CurrentCrop;
                    float progress = (float)crop.DaysGrowing / crop.Type.GrowthDays * 100f;
                    string fertInfo = plot.Fertilizer != FertilizerType.None ? $" [{plot.Fertilizer}]" : "";
                    return $"{crop.Type.Name} - {crop.Stage} ({progress:F0}% grown){fertInfo}";
                }
                else if (plot.IsTilled)
                {
                    return "Empty (Tilled)";
                }
                else
                {
                    return "Untilled";
                }
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Create common crop types with seasonal preferences
    /// </summary>
    public static class CropTypes
    {
        public static CropType Wheat => new CropType(
            "Wheat",
            TileType.Grass, // Placeholder for seed
            TileType.Grass, // Placeholder for harvest
            growthDays: 4,
            minYield: 1,
            maxYield: 3,
            sellPrice: 10,
            preferredSeason: Season.Summer
        );
        
        public static CropType Corn => new CropType(
            "Corn",
            TileType.Grass,
            TileType.Grass,
            growthDays: 7,
            minYield: 1,
            maxYield: 2,
            sellPrice: 25,
            preferredSeason: Season.Summer
        );
        
        public static CropType Tomato => new CropType(
            "Tomato",
            TileType.Grass,
            TileType.Grass,
            growthDays: 5,
            minYield: 2,
            maxYield: 5,
            sellPrice: 15,
            preferredSeason: Season.Summer
        );
        
        public static CropType Potato => new CropType(
            "Potato",
            TileType.Grass,
            TileType.Grass,
            growthDays: 6,
            minYield: 3,
            maxYield: 6,
            sellPrice: 8,
            preferredSeason: Season.Autumn
        );
        
        public static CropType Carrot => new CropType(
            "Carrot",
            TileType.Grass,
            TileType.Grass,
            growthDays: 3,
            minYield: 2,
            maxYield: 4,
            sellPrice: 12,
            preferredSeason: Season.Spring
        );
        
        public static CropType Pumpkin => new CropType(
            "Pumpkin",
            TileType.Grass,
            TileType.Grass,
            growthDays: 10,
            minYield: 1,
            maxYield: 2,
            sellPrice: 50,
            preferredSeason: Season.Autumn
        );
        
        public static CropType Sunflower => new CropType(
            "Sunflower",
            TileType.Grass,
            TileType.Grass,
            growthDays: 8,
            minYield: 1,
            maxYield: 3,
            sellPrice: 30,
            preferredSeason: Season.Summer
        );
        
        public static CropType Rice => new CropType(
            "Rice",
            TileType.Grass,
            TileType.Grass,
            growthDays: 6,
            minYield: 2,
            maxYield: 5,
            sellPrice: 18,
            preferredSeason: Season.Spring
        );
        
        public static CropType Cotton => new CropType(
            "Cotton",
            TileType.Grass,
            TileType.Grass,
            growthDays: 7,
            minYield: 1,
            maxYield: 3,
            sellPrice: 22,
            preferredSeason: Season.Summer
        );
        
        /// <summary>
        /// Get all available crop types
        /// </summary>
        public static List<CropType> GetAll() => new List<CropType>
        {
            Wheat, Corn, Tomato, Potato, Carrot, Pumpkin, Sunflower, Rice, Cotton
        };
    }
}
