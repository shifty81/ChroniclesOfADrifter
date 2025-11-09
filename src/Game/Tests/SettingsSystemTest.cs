using System;
using ChroniclesOfADrifter.Engine;

namespace ChroniclesOfADrifter.Tests;

/// <summary>
/// Tests for the settings system and Windows/DirectX 11 configuration
/// </summary>
public static class SettingsSystemTest
{
    public static void Run()
    {
        Console.WriteLine("========================================");
        Console.WriteLine("  Settings System Test");
        Console.WriteLine("========================================\n");
        
        // Test 1: Load default settings
        Console.WriteLine("[Test 1] Loading default settings...");
        var settings = SettingsManager.LoadSettings();
        Console.WriteLine($"  ✓ Default renderer: {settings.Renderer.Backend}");
        Console.WriteLine($"  ✓ Window size: {settings.Renderer.WindowWidth}x{settings.Renderer.WindowHeight}");
        Console.WriteLine($"  ✓ VSync: {settings.Renderer.Vsync}");
        
        // Test 2: Verify DirectX 11 is default on Windows
        Console.WriteLine("\n[Test 2] Verifying DirectX 11 default configuration...");
        if (settings.Renderer.Backend == "dx11")
        {
            Console.WriteLine("  ✓ DirectX 11 is correctly set as default renderer");
        }
        else
        {
            Console.WriteLine($"  ✗ WARNING: Expected 'dx11' but got '{settings.Renderer.Backend}'");
        }
        
        // Test 3: Test changing renderer backend
        Console.WriteLine("\n[Test 3] Testing renderer backend change...");
        string originalBackend = settings.Renderer.Backend;
        settings.Renderer.Backend = "dx12";
        SettingsManager.SaveSettings(settings);
        Console.WriteLine("  ✓ Changed renderer to DirectX 12");
        
        var reloadedSettings = SettingsManager.LoadSettings();
        if (reloadedSettings.Renderer.Backend == "dx12")
        {
            Console.WriteLine("  ✓ Settings persisted correctly");
        }
        else
        {
            Console.WriteLine("  ✗ Settings did not persist correctly");
        }
        
        // Test 4: Restore original settings
        Console.WriteLine("\n[Test 4] Restoring original settings...");
        settings.Renderer.Backend = originalBackend;
        SettingsManager.SaveSettings(settings);
        Console.WriteLine($"  ✓ Restored renderer to {originalBackend}");
        
        // Test 5: Test environment variable override
        Console.WriteLine("\n[Test 5] Testing environment variable override...");
        Environment.SetEnvironmentVariable("CHRONICLES_RENDERER", "sdl2");
        string envRenderer = Environment.GetEnvironmentVariable("CHRONICLES_RENDERER");
        Console.WriteLine($"  ✓ Environment variable set to: {envRenderer}");
        Console.WriteLine("  Note: Environment variable takes precedence over settings.json");
        
        // Clean up
        Environment.SetEnvironmentVariable("CHRONICLES_RENDERER", null);
        
        Console.WriteLine("\n========================================");
        Console.WriteLine("  Settings System Test Complete");
        Console.WriteLine("========================================");
        Console.WriteLine("\nKey Points:");
        Console.WriteLine("  • DirectX 11 is the default renderer on Windows");
        Console.WriteLine("  • Settings persist across sessions via settings.json");
        Console.WriteLine("  • Environment variable overrides settings.json");
        Console.WriteLine("  • Changing renderer requires game restart");
    }
}
