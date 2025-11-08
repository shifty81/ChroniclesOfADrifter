using ChroniclesOfADrifter.Engine;
using ChroniclesOfADrifter.Scenes;
using ChroniclesOfADrifter.Rendering;

namespace ChroniclesOfADrifter;

/// <summary>
/// Main entry point for the Terrain Demo
/// </summary>
class TerrainDemoProgram
{
    private const int KEY_Q = 81;
    private const int KEY_ESC = 27;
    
    public static void Run()
    {
        // Initialize console
        TerrainConsoleRenderer.InitializeConsole();
        
        Console.WriteLine("===========================================");
        Console.WriteLine("  Chronicles of a Drifter - Terrain Demo");
        Console.WriteLine("  2D Procedural Terrain Generation");
        Console.WriteLine("===========================================\n");
        
        // Initialize engine
        Console.WriteLine("[TerrainDemo] Initializing engine...");
        bool success = EngineInterop.Engine_Initialize(1920, 1080, "Chronicles of a Drifter - Terrain Demo");
        
        if (!success)
        {
            Console.WriteLine("[TerrainDemo] ERROR: Failed to initialize engine!");
            Console.WriteLine($"[TerrainDemo] Error: {EngineInterop.Engine_GetErrorMessage()}");
            return;
        }
        
        Console.WriteLine("[TerrainDemo] Engine initialized successfully\n");
        
        // Load terrain demo scene
        var scene = new TerrainDemoScene();
        scene.OnLoad();
        
        Console.WriteLine("\n[TerrainDemo] Starting game loop...");
        Console.WriteLine("[TerrainDemo] Press Q or ESC to exit\n");
        
        Thread.Sleep(2000); // Give user time to read initial messages
        
        // Create terrain console renderer
        var renderer = new TerrainConsoleRenderer();
        
        // Main game loop
        var lastTime = DateTime.Now;
        float fps = 60.0f;
        
        while (EngineInterop.Engine_IsRunning())
        {
            // Check for quit key
            if (EngineInterop.Input_IsKeyPressed(KEY_Q) || EngineInterop.Input_IsKeyPressed(KEY_ESC))
            {
                Console.WriteLine("\n[TerrainDemo] Quit key pressed...");
                break;
            }
            
            EngineInterop.Engine_BeginFrame();
            
            float deltaTime = EngineInterop.Engine_GetDeltaTime();
            
            // Update the scene
            scene.Update(deltaTime);
            
            // Render the terrain and game state to console
            var chunkManager = scene.GetChunkManager();
            if (chunkManager != null && scene.World != null)
            {
                renderer.Render(scene.World, chunkManager, fps);
            }
            
            EngineInterop.Engine_EndFrame();
            
            // Calculate FPS
            var currentTime = DateTime.Now;
            var elapsed = (currentTime - lastTime).TotalSeconds;
            if (elapsed > 0)
            {
                fps = (float)(1.0 / elapsed);
            }
            lastTime = currentTime;
            
            Thread.Sleep(16); // Target ~60 FPS
        }
        
        // Unload scene
        scene.OnUnload();
        
        // Shutdown
        Console.Clear();
        Console.CursorVisible = true;
        Console.WriteLine("\n[TerrainDemo] Shutting down...");
        EngineInterop.Engine_Shutdown();
        Console.WriteLine("[TerrainDemo] Goodbye!");
    }
}
