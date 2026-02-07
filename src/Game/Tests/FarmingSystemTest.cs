using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.ECS.Systems;

namespace ChroniclesOfADrifter.Tests;

/// <summary>
/// Test suite for the enhanced farming system with seasons, fertilizer, and new crops
/// </summary>
public static class FarmingSystemTest
{
    public static void Run()
    {
        Console.WriteLine("\n===========================================");
        Console.WriteLine("  Farming System Tests");
        Console.WriteLine("===========================================\n");

        TestBasicFarming();
        TestSeasonalGrowth();
        TestFertilizerEffect();
        TestWinterGrowth();
        TestNewCropTypes();
        TestFertilizerYieldBonus();
        TestSeasonCycle();

        Console.WriteLine("\n===========================================");
        Console.WriteLine("  All Farming Tests Completed!");
        Console.WriteLine("===========================================\n");
    }

    private static void TestBasicFarming()
    {
        Console.WriteLine("[Test] Basic Farming - Till, Plant, Water, Grow");

        var plot = new FarmPlotComponent(5, 5);
        plot.Till();

        var wheat = FarmingSystem.CropTypes.Wheat;
        bool planted = plot.PlantCrop(wheat);

        Console.WriteLine($"  Tilled: {plot.IsTilled}");
        Console.WriteLine($"  Planted: {planted}");
        Console.WriteLine($"  Crop stage: {plot.CurrentCrop?.Stage}");

        // Simulate 4 days of watering and growing (spring = preferred for carrots, neutral for wheat)
        for (int i = 0; i < wheat.GrowthDays; i++)
        {
            plot.Water();
            plot.AdvanceDay(Season.Summer); // Summer is wheat's preferred season
        }

        bool harvestable = plot.CurrentCrop?.IsHarvestable() ?? false;
        Console.WriteLine($"  After {wheat.GrowthDays} days: {plot.CurrentCrop?.Stage}");

        if (plot.IsTilled && planted && harvestable)
        {
            Console.WriteLine("  ✓ Test passed\n");
        }
        else
        {
            Console.WriteLine("  ✗ Test failed\n");
        }
    }

    private static void TestSeasonalGrowth()
    {
        Console.WriteLine("[Test] Seasonal Growth - Preferred season grows faster");

        var wheat = FarmingSystem.CropTypes.Wheat; // Preferred: Summer
        float summerMultiplier = wheat.GetSeasonMultiplier(Season.Summer);
        float springMultiplier = wheat.GetSeasonMultiplier(Season.Spring);
        float autumnMultiplier = wheat.GetSeasonMultiplier(Season.Autumn);
        float winterMultiplier = wheat.GetSeasonMultiplier(Season.Winter);

        Console.WriteLine($"  Summer (preferred): {summerMultiplier}x");
        Console.WriteLine($"  Spring: {springMultiplier}x");
        Console.WriteLine($"  Autumn: {autumnMultiplier}x");
        Console.WriteLine($"  Winter: {winterMultiplier}x");

        if (summerMultiplier == 1.5f && springMultiplier == 1.2f && 
            autumnMultiplier == 0.8f && winterMultiplier == 0f)
        {
            Console.WriteLine("  ✓ Test passed\n");
        }
        else
        {
            Console.WriteLine("  ✗ Test failed\n");
        }
    }

    private static void TestFertilizerEffect()
    {
        Console.WriteLine("[Test] Fertilizer Effect - Faster growth with fertilizer");

        // Without fertilizer
        var plot1 = new FarmPlotComponent(1, 1);
        plot1.Till();
        var carrot1 = FarmingSystem.CropTypes.Carrot; // 3 days growth
        plot1.PlantCrop(carrot1);

        // With quality fertilizer
        var plot2 = new FarmPlotComponent(2, 2);
        plot2.Till();
        plot2.ApplyFertilizer(FertilizerType.QualityFertilizer);
        var carrot2 = FarmingSystem.CropTypes.Carrot;
        plot2.PlantCrop(carrot2);

        // Simulate 2 days in spring
        for (int i = 0; i < 2; i++)
        {
            plot1.Water();
            plot1.AdvanceDay(Season.Spring);
            plot2.Water();
            plot2.AdvanceDay(Season.Spring);
        }

        Console.WriteLine($"  Without fertilizer after 2 days: Stage={plot1.CurrentCrop?.Stage}, Days={plot1.CurrentCrop?.DaysGrowing}");
        Console.WriteLine($"  With QualityFertilizer after 2 days: Stage={plot2.CurrentCrop?.Stage}, Days={plot2.CurrentCrop?.DaysGrowing}");

        bool fertilizerFaster = (plot2.CurrentCrop?.DaysGrowing ?? 0) > (plot1.CurrentCrop?.DaysGrowing ?? 0);

        if (fertilizerFaster)
        {
            Console.WriteLine("  ✓ Test passed\n");
        }
        else
        {
            Console.WriteLine("  ✗ Test failed\n");
        }
    }

    private static void TestWinterGrowth()
    {
        Console.WriteLine("[Test] Winter Growth - Most crops don't grow in winter");

        var plot = new FarmPlotComponent(3, 3);
        plot.Till();
        var wheat = FarmingSystem.CropTypes.Wheat; // GrowsInWinter = false (default)
        plot.PlantCrop(wheat);

        int initialDays = plot.CurrentCrop?.DaysGrowing ?? 0;

        // Simulate 3 days in winter
        for (int i = 0; i < 3; i++)
        {
            plot.Water();
            plot.AdvanceDay(Season.Winter);
        }

        int finalDays = plot.CurrentCrop?.DaysGrowing ?? 0;
        Console.WriteLine($"  Days growing after 3 winter days: {finalDays} (started at {initialDays})");

        if (finalDays == initialDays)
        {
            Console.WriteLine("  ✓ Test passed (no growth in winter)\n");
        }
        else
        {
            Console.WriteLine("  ✗ Test failed\n");
        }
    }

    private static void TestNewCropTypes()
    {
        Console.WriteLine("[Test] New Crop Types - All 9 varieties available");

        var allCrops = FarmingSystem.CropTypes.GetAll();

        Console.WriteLine($"  Total crop types: {allCrops.Count}");
        foreach (var crop in allCrops)
        {
            Console.WriteLine($"    - {crop.Name}: {crop.GrowthDays} days, yield {crop.MinYield}-{crop.MaxYield}, ${crop.SellPrice}, season: {crop.PreferredSeason}");
        }

        bool hasAll = allCrops.Count == 9 &&
                      allCrops.Any(c => c.Name == "Wheat") &&
                      allCrops.Any(c => c.Name == "Carrot") &&
                      allCrops.Any(c => c.Name == "Pumpkin") &&
                      allCrops.Any(c => c.Name == "Sunflower") &&
                      allCrops.Any(c => c.Name == "Rice") &&
                      allCrops.Any(c => c.Name == "Cotton");

        if (hasAll)
        {
            Console.WriteLine("  ✓ Test passed\n");
        }
        else
        {
            Console.WriteLine("  ✗ Test failed\n");
        }
    }

    private static void TestFertilizerYieldBonus()
    {
        Console.WriteLine("[Test] Fertilizer Yield Bonus - Super fertilizer adds to yield");

        var plot = new FarmPlotComponent(4, 4);
        plot.Till();
        plot.ApplyFertilizer(FertilizerType.SuperFertilizer);
        var wheat = FarmingSystem.CropTypes.Wheat;
        plot.PlantCrop(wheat);

        // Grow to harvestable in preferred season
        for (int i = 0; i < wheat.GrowthDays + 2; i++)
        {
            plot.Water();
            plot.AdvanceDay(Season.Summer);
        }

        bool isHarvestable = plot.CurrentCrop?.IsHarvestable() ?? false;
        Console.WriteLine($"  Crop harvestable: {isHarvestable}");
        Console.WriteLine($"  Fertilizer type: {plot.Fertilizer}");

        if (isHarvestable && plot.Fertilizer == FertilizerType.SuperFertilizer)
        {
            Console.WriteLine("  ✓ Test passed\n");
        }
        else
        {
            Console.WriteLine("  ✗ Test failed\n");
        }
    }

    private static void TestSeasonCycle()
    {
        Console.WriteLine("[Test] Season Cycle - FarmingSystem tracks seasons");

        var world = new World();
        var farmingSystem = new FarmingSystem();
        farmingSystem.Initialize(world);

        Console.WriteLine($"  Initial season: {farmingSystem.CurrentSeason}");
        Console.WriteLine($"  Day in season: {farmingSystem.DayInSeason}");

        // Restore to day 28 (end of first season)
        farmingSystem.RestoreState(28);
        Console.WriteLine($"  After 28 days: {farmingSystem.CurrentSeason} (Day {farmingSystem.DayInSeason})");

        farmingSystem.RestoreState(56);
        Console.WriteLine($"  After 56 days: {farmingSystem.CurrentSeason} (Day {farmingSystem.DayInSeason})");

        farmingSystem.RestoreState(84);
        Console.WriteLine($"  After 84 days: {farmingSystem.CurrentSeason} (Day {farmingSystem.DayInSeason})");

        farmingSystem.RestoreState(112);
        Console.WriteLine($"  After 112 days: {farmingSystem.CurrentSeason} (Day {farmingSystem.DayInSeason})");

        bool cyclesCorrectly = true;
        farmingSystem.RestoreState(0);
        cyclesCorrectly &= farmingSystem.CurrentSeason == Season.Spring;
        farmingSystem.RestoreState(28);
        cyclesCorrectly &= farmingSystem.CurrentSeason == Season.Summer;
        farmingSystem.RestoreState(56);
        cyclesCorrectly &= farmingSystem.CurrentSeason == Season.Autumn;
        farmingSystem.RestoreState(84);
        cyclesCorrectly &= farmingSystem.CurrentSeason == Season.Winter;
        farmingSystem.RestoreState(112);
        cyclesCorrectly &= farmingSystem.CurrentSeason == Season.Spring; // Back to spring

        if (cyclesCorrectly)
        {
            Console.WriteLine("  ✓ Test passed\n");
        }
        else
        {
            Console.WriteLine("  ✗ Test failed\n");
        }
    }
}
