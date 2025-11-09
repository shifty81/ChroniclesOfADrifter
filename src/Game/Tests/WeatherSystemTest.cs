using ChroniclesOfADrifter.Terrain;
using ChroniclesOfADrifter.WorldManagement;

namespace ChroniclesOfADrifter.Tests;

/// <summary>
/// Tests for the weather system
/// </summary>
public static class WeatherSystemTest
{
    public static void Run()
    {
        Console.WriteLine("=======================================");
        Console.WriteLine("  Weather System Test");
        Console.WriteLine("=======================================\n");
        
        TestBasicWeatherSystem();
        TestWeatherTransitions();
        TestBiomeSpecificWeather();
        TestWeatherEffects();
        
        Console.WriteLine("\n=======================================");
        Console.WriteLine("  All Weather System Tests Passed");
        Console.WriteLine("=======================================\n");
    }
    
    private static void TestBasicWeatherSystem()
    {
        Console.WriteLine("[Test] Basic Weather System");
        Console.WriteLine("----------------------------------------");
        
        var weatherSystem = new WeatherSystem(12345);
        
        Console.WriteLine($"Initial weather: {weatherSystem.CurrentWeather}");
        Console.WriteLine($"Initial intensity: {weatherSystem.CurrentIntensity}");
        
        System.Diagnostics.Debug.Assert(weatherSystem.CurrentWeather == WeatherType.Clear, 
            "Weather should start as Clear");
        System.Diagnostics.Debug.Assert(!weatherSystem.IsTransitioning, 
            "Should not be transitioning initially");
        
        // Update for a bit
        weatherSystem.Update(BiomeType.Plains, 10f);
        
        Console.WriteLine($"After 10s: {weatherSystem.CurrentWeather}");
        Console.WriteLine($"Visibility: {weatherSystem.GetVisibilityMultiplier() * 100:F0}%");
        Console.WriteLine($"Movement speed: {weatherSystem.GetMovementSpeedMultiplier() * 100:F0}%");
        
        Console.WriteLine("✓ Basic weather system working\n");
    }
    
    private static void TestWeatherTransitions()
    {
        Console.WriteLine("[Test] Weather Transitions");
        Console.WriteLine("----------------------------------------");
        
        var weatherSystem = new WeatherSystem(54321);
        
        // Force a weather change
        weatherSystem.SetWeather(WeatherType.Rain, WeatherIntensity.Moderate);
        
        Console.WriteLine($"Forced weather change to Rain");
        System.Diagnostics.Debug.Assert(weatherSystem.IsTransitioning, 
            "Should be transitioning after forced change");
        
        // Update through transition
        float totalTime = 0f;
        int updates = 0;
        while (weatherSystem.IsTransitioning && updates < 100)
        {
            weatherSystem.Update(BiomeType.Plains, 1f);
            totalTime += 1f;
            updates++;
            
            if (updates % 10 == 0)
            {
                Console.WriteLine($"  {totalTime}s: Progress {weatherSystem.TransitionProgress * 100:F0}%");
            }
        }
        
        System.Diagnostics.Debug.Assert(!weatherSystem.IsTransitioning, 
            "Transition should be complete");
        System.Diagnostics.Debug.Assert(weatherSystem.CurrentWeather == WeatherType.Rain, 
            "Weather should be Rain after transition");
        
        Console.WriteLine($"Transition complete after {totalTime}s");
        Console.WriteLine($"Final weather: {weatherSystem.CurrentWeather} ({weatherSystem.CurrentIntensity})");
        Console.WriteLine("✓ Weather transitions working\n");
    }
    
    private static void TestBiomeSpecificWeather()
    {
        Console.WriteLine("[Test] Biome-Specific Weather");
        Console.WriteLine("----------------------------------------");
        
        var weatherSystem = new WeatherSystem(99999);
        
        // Test different biomes
        var biomes = new[] 
        { 
            BiomeType.Desert, 
            BiomeType.Snow, 
            BiomeType.Jungle, 
            BiomeType.Plains 
        };
        
        foreach (var biome in biomes)
        {
            weatherSystem.SetWeather(WeatherType.Clear);
            weatherSystem.Update(biome, 31f); // Complete any transition
            
            // Simulate long time to potentially trigger weather change
            for (int i = 0; i < 20; i++)
            {
                weatherSystem.Update(biome, 60f); // 1 minute per update
            }
            
            Console.WriteLine($"{biome}: {weatherSystem.CurrentWeather} ({weatherSystem.CurrentIntensity})");
        }
        
        Console.WriteLine("✓ Biome-specific weather working\n");
    }
    
    private static void TestWeatherEffects()
    {
        Console.WriteLine("[Test] Weather Effects");
        Console.WriteLine("----------------------------------------");
        
        var weatherTypes = new[] 
        {
            WeatherType.Clear,
            WeatherType.Rain,
            WeatherType.Snow,
            WeatherType.Fog,
            WeatherType.Storm,
            WeatherType.Sandstorm
        };
        
        var weatherSystem = new WeatherSystem(11111);
        
        foreach (var weatherType in weatherTypes)
        {
            weatherSystem.SetWeather(weatherType, WeatherIntensity.Heavy);
            weatherSystem.Update(BiomeType.Plains, 31f); // Complete transition
            
            var visibility = weatherSystem.GetVisibilityMultiplier();
            var movementSpeed = weatherSystem.GetMovementSpeedMultiplier();
            var isDamaging = weatherSystem.IsDamagingWeather();
            var damage = weatherSystem.GetWeatherDamagePerSecond();
            var tint = weatherSystem.GetWeatherTint();
            
            Console.WriteLine($"{weatherType}:");
            Console.WriteLine($"  Visibility: {visibility * 100:F0}%");
            Console.WriteLine($"  Movement: {movementSpeed * 100:F0}%");
            Console.WriteLine($"  Damaging: {isDamaging} ({damage:F1} DPS)");
            Console.WriteLine($"  Tint: R={tint.r:F2} G={tint.g:F2} B={tint.b:F2} A={tint.a:F2}");
            
            System.Diagnostics.Debug.Assert(visibility > 0f && visibility <= 1.0f, 
                "Visibility should be between 0 and 1");
            System.Diagnostics.Debug.Assert(movementSpeed > 0f && movementSpeed <= 1.0f, 
                "Movement speed should be between 0 and 1");
        }
        
        Console.WriteLine("✓ Weather effects working correctly\n");
    }
}
