using ChroniclesOfADrifter.Engine;
using ChroniclesOfADrifter.Scenes;
using ChroniclesOfADrifter.Rendering;

namespace ChroniclesOfADrifter;

/// <summary>
/// Main entry point for Chronicles of a Drifter
/// </summary>
class Program
{
    private const int KEY_Q = 81;
    private const int KEY_ESC = 27;
    
    static void Main(string[] args)
    {
        // Check for test mode
        if (args.Length > 0 && args[0].ToLower() == "test")
        {
            Tests.TerrainGenerationTest.Run();
            return;
        }
        
        // Check for camera test mode
        if (args.Length > 0 && args[0].ToLower() == "camera-test")
        {
            Tests.CameraFeaturesTest.Run();
            return;
        }
        
        // Check for vegetation test mode
        if (args.Length > 0 && args[0].ToLower() == "vegetation-test")
        {
            Tests.VegetationGenerationTest.Run();
            return;
        }
        
        // Check for lighting test mode
        if (args.Length > 0 && args[0].ToLower() == "lighting-test")
        {
            Tests.LightingTest.Run();
            return;
        }
        
        // Check if terrain demo was requested via command line argument
        if (args.Length > 0 && args[0].ToLower() == "terrain")
        {
            TerrainDemoProgram.Run();
            return;
        }
        
        // Check if mining demo was requested via command line argument
        if (args.Length > 0 && args[0].ToLower() == "mining")
        {
            RunMiningDemo();
            return;
        }
        
        // Initialize console
        ConsoleRenderer.InitializeConsole();
        
        Console.WriteLine("===========================================");
        Console.WriteLine("  Chronicles of a Drifter - Playable Demo");
        Console.WriteLine("  C++/.NET 9/Lua Custom Voxel Game Engine");
        Console.WriteLine("===========================================\n");
        Console.WriteLine("  Tip: Run with 'test' for terrain tests");
        Console.WriteLine("       Run with 'camera-test' for camera tests");
        Console.WriteLine("       Run with 'vegetation-test' for vegetation tests");
        Console.WriteLine("       Run with 'lighting-test' for lighting tests");
        Console.WriteLine("       Run with 'terrain' for terrain demo");
        Console.WriteLine("       Run with 'mining' for mining/digging demo");
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
        
        // Load playable demo scene
        var scene = new PlayableDemoScene();
        scene.OnLoad();
        
        Console.WriteLine("\n[Game] Starting game loop...");
        Console.WriteLine("[Game] Press Q or ESC to exit\n");
        
        Thread.Sleep(2000); // Give user time to read initial messages
        
        // Create console renderer
        var renderer = new ConsoleRenderer();
        
        // Main game loop
        int frameCount = 0;
        var lastTime = DateTime.Now;
        float fps = 60.0f;
        
        while (EngineInterop.Engine_IsRunning())
        {
            // Check for quit key
            if (EngineInterop.Input_IsKeyPressed(KEY_Q) || EngineInterop.Input_IsKeyPressed(KEY_ESC))
            {
                Console.WriteLine("\n[Game] Quit key pressed...");
                break;
            }
            
            EngineInterop.Engine_BeginFrame();
            
            float deltaTime = EngineInterop.Engine_GetDeltaTime();
            
            // Update the scene (which updates the ECS world)
            scene.Update(deltaTime);
            
            // Render the game state to console
            renderer.Render(scene.World, fps);
            
            EngineInterop.Engine_EndFrame();
            
            frameCount++;
            
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
        Console.WriteLine("\n[Game] Shutting down...");
        EngineInterop.Engine_Shutdown();
        Console.WriteLine("[Game] Goodbye!");
    }
    
    static void RunMiningDemo()
    {
        // Initialize console
        ConsoleRenderer.InitializeConsole();
        
        Console.WriteLine("===========================================");
        Console.WriteLine("  Chronicles of a Drifter - Mining Demo");
        Console.WriteLine("  C++/.NET 9/Lua Custom Voxel Game Engine");
        Console.WriteLine("===========================================\n");
        
        // Initialize engine
        Console.WriteLine("[Game] Initializing engine...");
        bool success = EngineInterop.Engine_Initialize(1920, 1080, "Chronicles of a Drifter - Mining Demo");
        
        if (!success)
        {
            Console.WriteLine("[Game] ERROR: Failed to initialize engine!");
            Console.WriteLine($"[Game] Error: {EngineInterop.Engine_GetErrorMessage()}");
            return;
        }
        
        Console.WriteLine("[Game] Engine initialized successfully\n");
        
        // Load mining demo scene
        var scene = new MiningDemoScene();
        scene.OnLoad();
        
        Console.WriteLine("\n[Game] Starting game loop...");
        Console.WriteLine("[Game] Press Q or ESC to exit\n");
        
        Thread.Sleep(2000);
        
        // Main game loop
        int frameCount = 0;
        var lastTime = DateTime.Now;
        float fps = 60.0f;
        
        while (EngineInterop.Engine_IsRunning())
        {
            // Check for quit key
            if (EngineInterop.Input_IsKeyPressed(KEY_Q) || EngineInterop.Input_IsKeyPressed(KEY_ESC))
            {
                Console.WriteLine("\n[Game] Quit key pressed...");
                break;
            }
            
            EngineInterop.Engine_BeginFrame();
            
            float deltaTime = EngineInterop.Engine_GetDeltaTime();
            
            // Update the scene
            scene.Update(deltaTime);
            
            EngineInterop.Engine_EndFrame();
            
            frameCount++;
            
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
        Console.WriteLine("\n[Game] Shutting down...");
        EngineInterop.Engine_Shutdown();
        Console.WriteLine("[Game] Goodbye!");
    }
}
