using ChroniclesOfADrifter.Engine;
using ChroniclesOfADrifter.Scenes;
using ChroniclesOfADrifter.Rendering;

namespace ChroniclesOfADrifter;

/// <summary>
/// Main entry point for Chronicles of a Drifter
/// </summary>
class Program
{
    private const int KEY_Q = 113;  // 'q' in SDL2
    private const int KEY_ESC = 27;
    
    /// <summary>
    /// Initialize settings and apply renderer configuration before engine starts
    /// </summary>
    private static void InitializeSettings()
    {
        var settings = SettingsManager.LoadSettings();
        SettingsManager.ApplyRendererSettings(settings);
    }
    
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
        
        // Check for water test mode
        if (args.Length > 0 && args[0].ToLower() == "water-test")
        {
            Tests.WaterGenerationTest.Run();
            return;
        }
        
        // Check for collision test mode
        if (args.Length > 0 && args[0].ToLower() == "collision-test")
        {
            Tests.CollisionSystemTest.Run();
            return;
        }
        
        // Check for creature spawn test mode
        if (args.Length > 0 && args[0].ToLower() == "creature-test")
        {
            Tests.CreatureSpawnTest.Run();
            return;
        }
        
        // Check for world creature manager test mode
        if (args.Length > 0 && args[0].ToLower() == "world-creature-test")
        {
            Tests.WorldCreatureManagerTest.Run();
            return;
        }
        
        // Check for structure generation test mode
        if (args.Length > 0 && args[0].ToLower() == "structure-test")
        {
            Tests.StructureGenerationTest.Run();
            return;
        }
        
        // Check for crafting system test mode
        if (args.Length > 0 && args[0].ToLower() == "crafting-test")
        {
            Tests.CraftingSystemTest.RunTests();
            return;
        }
        
        // Check for swimming system test mode
        if (args.Length > 0 && args[0].ToLower() == "swimming-test")
        {
            Tests.SwimmingSystemTest.Run();
            return;
        }
        
        // Check for weather system test mode
        if (args.Length > 0 && args[0].ToLower() == "weather-test")
        {
            Tests.WeatherSystemTest.Run();
            return;
        }
        
        // Check for async chunk generation test mode
        if (args.Length > 0 && args[0].ToLower() == "async-test")
        {
            Tests.AsyncChunkGenerationTest.Run();
            return;
        }
        
        // Check for time system test mode
        if (args.Length > 0 && args[0].ToLower() == "time-test")
        {
            Tests.TimeSystemTest.Run();
            return;
        }
        
        // Check for hybrid gameplay test mode
        if (args.Length > 0 && args[0].ToLower() == "hybrid-test")
        {
            Tests.HybridGameplayTest.Run();
            return;
        }
        
        // Check for cinematic camera test mode
        if (args.Length > 0 && args[0].ToLower() == "cinematic-test")
        {
            Tests.CinematicCameraTest.RunTests();
            return;
        }
        
        // Check for settings system test mode
        if (args.Length > 0 && args[0].ToLower() == "settings-test")
        {
            Tests.SettingsSystemTest.Run();
            return;
        }
        
        // Check for save/load system test mode
        if (args.Length > 0 && args[0].ToLower() == "save-test")
        {
            Tests.SaveLoadTest.RunTest();
            return;
        }
        
        // Check for experience/leveling system test mode
        if (args.Length > 0 && args[0].ToLower() == "xp-test")
        {
            Tests.ExperienceLevelingTest.Run();
            return;
        }
        
        // Check if terrain demo was requested via command line argument
        if (args.Length > 0 && args[0].ToLower() == "terrain")
        {
            TerrainDemoProgram.Run();
            return;
        }
        
        // Check if hybrid demo was requested
        if (args.Length > 0 && args[0].ToLower() == "hybrid")
        {
            RunHybridDemo();
            return;
        }
        
        // Check if map editor was requested via command line argument
        if (args.Length > 0 && args[0].ToLower() == "editor")
        {
            RunMapEditor();
            return;
        }
        
        // Check if visual demo was requested via command line argument
        if (args.Length > 0 && args[0].ToLower() == "visual")
        {
            RunVisualDemo();
            return;
        }
        
        // Check if mining demo was requested via command line argument
        if (args.Length > 0 && args[0].ToLower() == "mining")
        {
            RunMiningDemo();
            return;
        }
        
        // Check if collision demo was requested via command line argument
        if (args.Length > 0 && args[0].ToLower() == "collision")
        {
            RunCollisionDemo();
            return;
        }
        
        // Check if creature spawn demo was requested via command line argument
        if (args.Length > 0 && args[0].ToLower() == "creatures")
        {
            RunCreatureSpawnDemo();
            return;
        }
        
        // Check if crafting demo was requested via command line argument
        if (args.Length > 0 && args[0].ToLower() == "crafting")
        {
            RunCraftingDemo();
            return;
        }
        
        // Check if cinematic camera demo was requested via command line argument
        if (args.Length > 0 && args[0].ToLower() == "cinematic")
        {
            RunCinematicDemo();
            return;
        }
        
        // Check if complete game loop demo was requested via command line argument
        if (args.Length > 0 && args[0].ToLower() == "complete")
        {
            RunCompleteGameLoopDemo();
            return;
        }
        
        // Initialize console
        ConsoleRenderer.InitializeConsole();
        
        // Load and apply settings (especially renderer backend)
        InitializeSettings();
        var settings = SettingsManager.LoadSettings();
        
        Console.WriteLine("===========================================");
        Console.WriteLine("  Chronicles of a Drifter - Playable Demo");
        Console.WriteLine("  C++/.NET 9/Lua Custom Voxel Game Engine");
        Console.WriteLine($"  Platform: {(OperatingSystem.IsWindows() ? "Windows (DirectX 11 default)" : "Linux/Unix (SDL2 default)")}");
        Console.WriteLine("===========================================\n");
        Console.WriteLine("  Rendering Backend:");
        Console.WriteLine($"    Active: {settings.Renderer.Backend}");
        Console.WriteLine("    Available: dx11 (Windows), dx12 (Windows), sdl2 (cross-platform)");
        Console.WriteLine("    Note: Can be changed in settings menu (game restarts)");
        Console.WriteLine("\n  Available Commands:");
        Console.WriteLine("       Run with 'test' for terrain tests");
        Console.WriteLine("       Run with 'camera-test' for camera tests");
        Console.WriteLine("       Run with 'vegetation-test' for vegetation tests");
        Console.WriteLine("       Run with 'lighting-test' for lighting tests");
        Console.WriteLine("       Run with 'water-test' for water generation tests");
        Console.WriteLine("       Run with 'collision-test' for collision detection tests");
        Console.WriteLine("       Run with 'creature-test' for creature spawn tests");
        Console.WriteLine("       Run with 'crafting-test' for crafting system tests");
        Console.WriteLine("       Run with 'swimming-test' for swimming mechanics tests");
        Console.WriteLine("       Run with 'hybrid-test' for hybrid gameplay tests");
        Console.WriteLine("       Run with 'cinematic-test' for cinematic camera tests");
        Console.WriteLine("       Run with 'settings-test' for settings system tests");
        Console.WriteLine("       Run with 'terrain' for terrain demo");
        Console.WriteLine("       Run with 'visual' for GRAPHICAL visual demo");
        Console.WriteLine("       Run with 'mining' for mining/digging demo");
        Console.WriteLine("       Run with 'collision' for collision detection demo");
        Console.WriteLine("       Run with 'creatures' for creature spawning demo");
        Console.WriteLine("       Run with 'crafting' for crafting system demo");
        Console.WriteLine("       Run with 'cinematic' for cinematic camera demo");
        Console.WriteLine("       Run with 'hybrid' for hybrid gameplay demo");
        Console.WriteLine("       Run with 'complete' for COMPLETE GAME LOOP demo");
        Console.WriteLine("       Run with 'editor' for MAP EDITOR with tileset support");
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
        
        // Load and apply settings
        InitializeSettings();
        
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
    
    static void RunCollisionDemo()
    {
        // Initialize console
        ConsoleRenderer.InitializeConsole();
        
        // Load and apply settings
        InitializeSettings();
        
        Console.WriteLine("===========================================");
        Console.WriteLine("  Chronicles of a Drifter - Collision Demo");
        Console.WriteLine("  C++/.NET 9/Lua Custom Voxel Game Engine");
        Console.WriteLine("===========================================\n");
        
        // Initialize engine
        Console.WriteLine("[Game] Initializing engine...");
        bool success = EngineInterop.Engine_Initialize(1920, 1080, "Chronicles of a Drifter - Collision Demo");
        
        if (!success)
        {
            Console.WriteLine("[Game] ERROR: Failed to initialize engine!");
            Console.WriteLine($"[Game] Error: {EngineInterop.Engine_GetErrorMessage()}");
            return;
        }
        
        Console.WriteLine("[Game] Engine initialized successfully\n");
        
        // Load collision demo scene
        var scene = new CollisionDemoScene();
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
    
    static void RunVisualDemo()
    {
        // Load and apply settings
        InitializeSettings();
        
        Console.WriteLine("===========================================");
        Console.WriteLine("  Chronicles of a Drifter - VISUAL Demo");
        Console.WriteLine("  Graphical Rendering (DirectX 11 Default)");
        Console.WriteLine("===========================================\n");
        
        // Check which renderer backend will be used
        string renderer = Environment.GetEnvironmentVariable("CHRONICLES_RENDERER") ?? "dx11";
        Console.WriteLine($"[Game] Renderer Backend: {renderer.ToUpper()}");
        
        // Initialize engine with window
        Console.WriteLine("[Game] Initializing engine...");
        bool success = EngineInterop.Engine_Initialize(1280, 720, "Chronicles of a Drifter - Visual Demo");
        
        if (!success)
        {
            Console.WriteLine("[Game] ERROR: Failed to initialize engine!");
            Console.WriteLine($"[Game] Error: {EngineInterop.Engine_GetErrorMessage()}");
            return;
        }
        
        Console.WriteLine("[Game] Engine initialized successfully\n");
        
        // Load visual demo scene
        var scene = new VisualDemoScene();
        scene.OnLoad();
        
        Console.WriteLine("\n[Game] Starting visual game loop...");
        Console.WriteLine("[Game] A graphical window should appear!");
        Console.WriteLine("[Game] Press Q or ESC to exit\n");
        
        // Main game loop
        int frameCount = 0;
        var lastTime = DateTime.Now;
        
        while (EngineInterop.Engine_IsRunning())
        {
            EngineInterop.Engine_BeginFrame();
            
            float deltaTime = EngineInterop.Engine_GetDeltaTime();
            
            // Update the scene
            scene.Update(deltaTime);
            
            EngineInterop.Engine_EndFrame();
            
            frameCount++;
            
            // Print FPS every 60 frames
            if (frameCount % 60 == 0)
            {
                var currentTime = DateTime.Now;
                var elapsed = (currentTime - lastTime).TotalSeconds;
                if (elapsed > 0)
                {
                    float fps = 60.0f / (float)elapsed;
                    Console.WriteLine($"[Game] FPS: {fps:F1}");
                }
                lastTime = currentTime;
            }
        }
        
        // Unload scene
        scene.OnUnload();
        
        // Shutdown
        Console.WriteLine("\n[Game] Shutting down...");
        EngineInterop.Engine_Shutdown();
        Console.WriteLine("[Game] Goodbye!");
    }
    
    static void RunCreatureSpawnDemo()
    {
        // Initialize console
        ConsoleRenderer.InitializeConsole();
        
        // Load and apply settings
        InitializeSettings();
        
        Console.WriteLine("===========================================");
        Console.WriteLine("  Chronicles of a Drifter - Creature Spawn Demo");
        Console.WriteLine("  C++/.NET 9/Lua Custom Voxel Game Engine");
        Console.WriteLine("===========================================\n");
        
        // Initialize engine
        Console.WriteLine("[Game] Initializing engine...");
        bool success = EngineInterop.Engine_Initialize(1920, 1080, "Chronicles of a Drifter - Creature Spawn Demo");
        
        if (!success)
        {
            Console.WriteLine("[Game] ERROR: Failed to initialize engine!");
            Console.WriteLine($"[Game] Error: {EngineInterop.Engine_GetErrorMessage()}");
            return;
        }
        
        Console.WriteLine("[Game] Engine initialized successfully\n");
        
        // Load creature spawn demo scene
        var scene = new CreatureSpawnDemoScene();
        scene.OnLoad();
        
        Console.WriteLine("\n[Game] Starting game loop...");
        Console.WriteLine("[Game] Press Q or ESC to exit\n");
        
        Thread.Sleep(2000);
        
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
            
            // Update the scene
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
    
    /// <summary>
    /// Run the crafting demo scene
    /// </summary>
    static void RunCraftingDemo()
    {
        Console.WriteLine("\n[Game] Starting crafting demo...\n");
        
        // Create and initialize the crafting demo scene
        var scene = new CraftingDemoScene();
        scene.OnLoad();
        
        // The demo is self-contained and completes immediately
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
        
        // Cleanup
        scene.OnUnload();
        Console.WriteLine("[Game] Crafting demo complete!");
    }
    
    /// <summary>
    /// Run the cinematic camera demo scene
    /// </summary>
    static void RunCinematicDemo()
    {
        Console.WriteLine("\n[Game] Starting cinematic camera demo...\n");
        
        var scene = new CinematicCameraDemoScene();
        scene.OnLoad();
        
        // Run demo for about 35 seconds (enough for all demos)
        float totalTime = 0f;
        float deltaTime = 0.016f; // ~60 FPS
        
        while (totalTime < 35f)
        {
            scene.Update(deltaTime);
            totalTime += deltaTime;
            System.Threading.Thread.Sleep(16); // ~60 FPS
        }
        
        scene.OnUnload();
        Console.WriteLine("\n[Game] Cinematic camera demo complete!");
    }
    
    /// <summary>
    /// Run the hybrid gameplay demo scene
    /// </summary>
    static void RunHybridDemo()
    {
        Console.WriteLine("\n[Game] Starting hybrid gameplay demo...\n");
        
        // Create and initialize the hybrid gameplay demo scene
        var scene = new HybridGameplayDemoScene();
        scene.OnLoad();
        
        // The demo is self-contained and completes immediately
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
        
        // Cleanup
        scene.OnUnload();
        Console.WriteLine("[Game] Hybrid gameplay demo complete!");
    }
    
    /// <summary>
    /// Run the map editor scene
    /// </summary>
    static void RunMapEditor()
    {
        // Load and apply settings
        InitializeSettings();
        
        Console.WriteLine("===========================================");
        Console.WriteLine("  Chronicles of a Drifter - MAP EDITOR");
        Console.WriteLine("  Real-Time Scene Editing");
        Console.WriteLine("===========================================\n");
        
        // Check which renderer backend will be used
        string renderer = Environment.GetEnvironmentVariable("CHRONICLES_RENDERER") ?? "dx11";
        Console.WriteLine($"[Game] Renderer Backend: {renderer.ToUpper()}");
        
        // Initialize engine with window
        Console.WriteLine("[Game] Initializing engine...");
        bool success = EngineInterop.Engine_Initialize(1280, 720, "Chronicles of a Drifter - Map Editor");
        
        if (!success)
        {
            Console.WriteLine("[Game] ERROR: Failed to initialize engine!");
            Console.WriteLine($"[Game] Error: {EngineInterop.Engine_GetErrorMessage()}");
            return;
        }
        
        Console.WriteLine("[Game] Engine initialized successfully\n");
        
        // Load map editor scene
        var scene = new MapEditorScene();
        scene.OnLoad();
        
        Console.WriteLine("\n[Game] Starting map editor...");
        Console.WriteLine("[Game] A graphical window should appear!");
        Console.WriteLine("[Game] Press Q or ESC to exit\n");
        
        // Main game loop
        int frameCount = 0;
        var lastTime = DateTime.Now;
        
        while (EngineInterop.Engine_IsRunning())
        {
            EngineInterop.Engine_BeginFrame();
            
            float deltaTime = EngineInterop.Engine_GetDeltaTime();
            
            // Update the scene
            scene.Update(deltaTime);
            
            EngineInterop.Engine_EndFrame();
            
            frameCount++;
            
            // Print FPS every 60 frames
            if (frameCount % 60 == 0)
            {
                var currentTime = DateTime.Now;
                var elapsed = (currentTime - lastTime).TotalSeconds;
                if (elapsed > 0)
                {
                    float fps = 60.0f / (float)elapsed;
                    Console.WriteLine($"[Game] FPS: {fps:F1}");
                }
                lastTime = currentTime;
            }
        }
        
        // Unload scene
        scene.OnUnload();
        
        // Shutdown
        Console.WriteLine("\n[Game] Shutting down...");
        EngineInterop.Engine_Shutdown();
        Console.WriteLine("[Game] Goodbye!");
    }
    
    /// <summary>
    /// Run the complete game loop demo scene showcasing all integrated systems
    /// </summary>
    static void RunCompleteGameLoopDemo()
    {
        // Initialize console
        ConsoleRenderer.InitializeConsole();
        
        // Load and apply settings
        InitializeSettings();
        
        Console.WriteLine("===========================================");
        Console.WriteLine("  Chronicles of a Drifter");
        Console.WriteLine("  COMPLETE GAME LOOP DEMONSTRATION");
        Console.WriteLine("  All Systems Integrated");
        Console.WriteLine("===========================================\n");
        
        // Initialize engine
        Console.WriteLine("[Game] Initializing engine...");
        bool success = EngineInterop.Engine_Initialize(1920, 1080, "Chronicles of a Drifter - Complete Game Loop");
        
        if (!success)
        {
            Console.WriteLine("[Game] ERROR: Failed to initialize engine!");
            Console.WriteLine($"[Game] Error: {EngineInterop.Engine_GetErrorMessage()}");
            return;
        }
        
        Console.WriteLine("[Game] Engine initialized successfully\n");
        
        // Load complete game loop scene
        var scene = new CompleteGameLoopScene();
        scene.OnLoad();
        
        Console.WriteLine("\n[Game] Starting complete game loop...");
        Console.WriteLine("[Game] Press Q or ESC to exit\n");
        
        Thread.Sleep(2000); // Give user time to read messages
        
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
            
            // Update the scene (which updates all systems)
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
}
