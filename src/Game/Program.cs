using ChroniclesOfADrifter.Engine;
using ChroniclesOfADrifter.Scenes;

namespace ChroniclesOfADrifter;

/// <summary>
/// Main entry point for Chronicles of a Drifter
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("===========================================");
        Console.WriteLine("  Chronicles of a Drifter - ECS Demo");
        Console.WriteLine("  C++/.NET 9/Lua Custom Voxel Game Engine");
        Console.WriteLine("===========================================\n");
        
        // Initialize engine
        Console.WriteLine("[Game] Initializing engine...");
        bool success = EngineInterop.Engine_Initialize(1920, 1080, "Chronicles of a Drifter");
        
        if (!success)
        {
            Console.WriteLine("[Game] ERROR: Failed to initialize engine!");
            Console.WriteLine($"[Game] Error: {EngineInterop.Engine_GetErrorMessage()}");
            return;
        }
        
        Console.WriteLine("[Game] Engine initialized successfully\n");
        
        // Load demo scene
        var scene = new DemoScene();
        scene.OnLoad();
        
        Console.WriteLine("\n[Game] Starting game loop...");
        Console.WriteLine("[Game] Press Ctrl+C to exit\n");
        
        // Main game loop
        int frameCount = 0;
        while (EngineInterop.Engine_IsRunning() && frameCount < 300) // Run for ~5 seconds
        {
            EngineInterop.Engine_BeginFrame();
            
            float deltaTime = EngineInterop.Engine_GetDeltaTime();
            
            // Update the scene (which updates the ECS world)
            scene.Update(deltaTime);
            
            // Log progress periodically
            if (frameCount % 60 == 0)
            {
                float totalTime = EngineInterop.Engine_GetTotalTime();
                Console.WriteLine($"[Game] Frame: {frameCount}, DeltaTime: {deltaTime:F4}s, TotalTime: {totalTime:F2}s");
            }
            
            EngineInterop.Engine_EndFrame();
            
            frameCount++;
            Thread.Sleep(16); // Simulate ~60 FPS
        }
        
        // Unload scene
        scene.OnUnload();
        
        // Shutdown
        Console.WriteLine("\n[Game] Shutting down...");
        EngineInterop.Engine_Shutdown();
        Console.WriteLine("[Game] Goodbye!");
    }
}
