using ChroniclesOfADrifter.Engine;

namespace ChroniclesOfADrifter;

/// <summary>
/// Main entry point for Chronicles of a Drifter
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("===========================================");
        Console.WriteLine("  Chronicles of a Drifter - Planning Phase");
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
        
        Console.WriteLine("[Game] Engine initialized successfully");
        Console.WriteLine("[Game] Press Ctrl+C to exit\n");
        
        // Main game loop
        int frameCount = 0;
        while (EngineInterop.Engine_IsRunning() && frameCount < 100) // Limit frames for planning phase
        {
            EngineInterop.Engine_BeginFrame();
            
            float deltaTime = EngineInterop.Engine_GetDeltaTime();
            float totalTime = EngineInterop.Engine_GetTotalTime();
            
            // Game logic would go here
            if (frameCount % 60 == 0)
            {
                Console.WriteLine($"[Game] Frame: {frameCount}, DeltaTime: {deltaTime:F4}s, TotalTime: {totalTime:F2}s");
            }
            
            EngineInterop.Engine_EndFrame();
            
            frameCount++;
            Thread.Sleep(16); // Simulate ~60 FPS
        }
        
        // Shutdown
        Console.WriteLine("\n[Game] Shutting down...");
        EngineInterop.Engine_Shutdown();
        Console.WriteLine("[Game] Goodbye!");
    }
}
