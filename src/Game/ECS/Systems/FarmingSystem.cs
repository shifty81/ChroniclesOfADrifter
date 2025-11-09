using ChroniclesOfADrifter.ECS.Components;
using World = ChroniclesOfADrifter.ECS.World;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// System managing farming mechanics - planting, watering, harvesting
/// </summary>
public class FarmingSystem : ISystem
{
    private float dayTimer = 0f;
    private const float secondsPerDay = 600f; // 10 minutes = 1 in-game day
    
    public void Initialize(World world)
    {
        Console.WriteLine("[Farming] Farming system initialized");
    }
    
    public void Update(World world, float deltaTime)
    {
        dayTimer += deltaTime;
        
        // Advance day for all farm plots
        if (dayTimer >= secondsPerDay)
        {
            dayTimer -= secondsPerDay;
            AdvanceAllFarmPlots(world);
        }
    }
    
    /// <summary>
    /// Advance all farm plots by one day
    /// </summary>
    private void AdvanceAllFarmPlots(World world)
    {
        foreach (var entity in world.GetEntitiesWithComponent<FarmPlotComponent>())
        {
            var plot = world.GetComponent<FarmPlotComponent>(entity);
            if (plot != null)
            {
                plot.AdvanceDay();
                
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
                    return $"{crop.Type.Name} - {crop.Stage} ({progress:F0}% grown)";
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
    /// Create common crop types
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
            sellPrice: 10
        );
        
        public static CropType Corn => new CropType(
            "Corn",
            TileType.Grass,
            TileType.Grass,
            growthDays: 7,
            minYield: 1,
            maxYield: 2,
            sellPrice: 25
        );
        
        public static CropType Tomato => new CropType(
            "Tomato",
            TileType.Grass,
            TileType.Grass,
            growthDays: 5,
            minYield: 2,
            maxYield: 5,
            sellPrice: 15
        );
        
        public static CropType Potato => new CropType(
            "Potato",
            TileType.Grass,
            TileType.Grass,
            growthDays: 6,
            minYield: 3,
            maxYield: 6,
            sellPrice: 8
        );
    }
}
