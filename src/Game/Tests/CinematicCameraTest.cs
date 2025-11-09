using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.ECS.Systems;

namespace ChroniclesOfADrifter.Tests;

/// <summary>
/// Tests for the cinematic camera system
/// </summary>
public class CinematicCameraTest
{
    public static void RunTests()
    {
        Console.WriteLine("\n===========================================");
        Console.WriteLine("  Cinematic Camera System Tests");
        Console.WriteLine("===========================================\n");
        
        TestBasicSequence();
        TestEasingFunctions();
        TestMultiStepSequence();
        TestZoomSequence();
        TestSequenceCompletion();
        TestSequenceHelpers();
        
        Console.WriteLine("\n===========================================");
        Console.WriteLine("  All Cinematic Camera Tests Passed! ✓");
        Console.WriteLine("===========================================\n");
    }
    
    private static void TestBasicSequence()
    {
        Console.WriteLine("[Test 1] Basic sequence playback...");
        
        var world = new World();
        var cinematicSystem = new CinematicCameraSystem();
        cinematicSystem.Initialize(world);
        
        // Create camera with cinematic component
        var camera = world.CreateEntity();
        world.AddComponent(camera, new CameraComponent(800, 600)
        {
            X = 0,
            Y = 0,
            Zoom = 1.0f
        });
        world.AddComponent(camera, new CinematicCameraComponent());
        
        // Create a simple sequence
        var sequence = new CinematicSequence("Test");
        sequence.Steps.Add(new CinematicStep(100, 100, 1.0f, EasingType.Linear));
        
        // Play sequence
        CinematicCameraSystem.PlaySequence(world, camera, sequence);
        
        var cinematic = world.GetComponent<CinematicCameraComponent>(camera);
        
        Assert(cinematic != null, "Cinematic component should exist");
        Assert(cinematic!.IsPlaying, "Sequence should be playing");
        Assert(cinematic.CurrentSequence == sequence, "Sequence should be set");
        Assert(cinematic.CurrentStepIndex == 0, "Should start at step 0");
        
        Console.WriteLine("✓ Basic sequence started successfully\n");
    }
    
    private static void TestEasingFunctions()
    {
        Console.WriteLine("[Test 2] Easing function interpolation...");
        
        var world = new World();
        var cinematicSystem = new CinematicCameraSystem();
        cinematicSystem.Initialize(world);
        
        var camera = world.CreateEntity();
        var cameraComp = new CameraComponent(800, 600) { X = 0, Y = 0 };
        world.AddComponent(camera, cameraComp);
        world.AddComponent(camera, new CinematicCameraComponent());
        
        // Test linear easing
        var sequence = new CinematicSequence("Linear Test");
        sequence.Steps.Add(new CinematicStep(100, 0, 1.0f, EasingType.Linear));
        
        CinematicCameraSystem.PlaySequence(world, camera, sequence);
        
        // Simulate halfway through (0.5 seconds)
        cinematicSystem.Update(world, 0.5f);
        
        // With linear easing, at 50% time we should be at ~50% position
        float expectedX = 50f; // Halfway between 0 and 100
        float tolerance = 5f;
        
        Assert(MathF.Abs(cameraComp.X - expectedX) < tolerance, 
               $"Linear easing: Expected ~{expectedX}, got {cameraComp.X}");
        
        Console.WriteLine($"  Linear: Pos={cameraComp.X:F1} (expected ~50) ✓");
        
        // Test ease-in (should be slower at start)
        var camera2 = world.CreateEntity();
        var cameraComp2 = new CameraComponent(800, 600) { X = 0, Y = 0 };
        world.AddComponent(camera2, cameraComp2);
        world.AddComponent(camera2, new CinematicCameraComponent());
        
        var sequence2 = new CinematicSequence("EaseIn Test");
        sequence2.Steps.Add(new CinematicStep(100, 0, 1.0f, EasingType.EaseIn));
        
        CinematicCameraSystem.PlaySequence(world, camera2, sequence2);
        cinematicSystem.Update(world, 0.5f);
        
        // With ease-in, at 50% time we should be less than 50% of the way there
        Assert(cameraComp2.X < 50f, 
               $"EaseIn should be slower at start: got {cameraComp2.X:F1}");
        
        Console.WriteLine($"  EaseIn: Pos={cameraComp2.X:F1} (expected <50) ✓");
        
        Console.WriteLine("✓ Easing functions work correctly\n");
    }
    
    private static void TestMultiStepSequence()
    {
        Console.WriteLine("[Test 3] Multi-step sequence progression...");
        
        var world = new World();
        var cinematicSystem = new CinematicCameraSystem();
        cinematicSystem.Initialize(world);
        
        var camera = world.CreateEntity();
        world.AddComponent(camera, new CameraComponent(800, 600) { X = 0, Y = 0 });
        world.AddComponent(camera, new CinematicCameraComponent());
        
        // Create sequence with 3 steps
        var sequence = new CinematicSequence("Multi-Step");
        sequence.Steps.Add(new CinematicStep(100, 0, 0.5f, EasingType.Linear));
        sequence.Steps.Add(new CinematicStep(200, 0, 0.5f, EasingType.Linear));
        sequence.Steps.Add(new CinematicStep(300, 0, 0.5f, EasingType.Linear));
        
        CinematicCameraSystem.PlaySequence(world, camera, sequence);
        
        var cinematic = world.GetComponent<CinematicCameraComponent>(camera);
        Assert(cinematic != null, "Cinematic component should exist");
        
        // Update past first step
        cinematicSystem.Update(world, 0.6f);
        Assert(cinematic!.CurrentStepIndex == 1, 
               $"Should be on step 1, got {cinematic.CurrentStepIndex}");
        
        // Update past second step
        cinematicSystem.Update(world, 0.6f);
        Assert(cinematic.CurrentStepIndex == 2, 
               $"Should be on step 2, got {cinematic.CurrentStepIndex}");
        
        // Update past third step (need to go past the duration to complete)
        cinematicSystem.Update(world, 0.6f);
        Assert(cinematic.CurrentStepIndex == 3, 
               $"Should be on step 3 (past all steps), got {cinematic.CurrentStepIndex}");
        
        // One more update to trigger completion
        cinematicSystem.Update(world, 0.01f);
        Assert(!cinematic.IsPlaying, "Sequence should be complete");
        
        Console.WriteLine("✓ Multi-step sequence progresses correctly\n");
    }
    
    private static void TestZoomSequence()
    {
        Console.WriteLine("[Test 4] Zoom interpolation...");
        
        var world = new World();
        var cinematicSystem = new CinematicCameraSystem();
        cinematicSystem.Initialize(world);
        
        var camera = world.CreateEntity();
        var cameraComp = new CameraComponent(800, 600) 
        { 
            X = 0, 
            Y = 0, 
            Zoom = 1.0f 
        };
        world.AddComponent(camera, cameraComp);
        world.AddComponent(camera, new CinematicCameraComponent());
        
        // Create zoom sequence
        var sequence = new CinematicSequence("Zoom Test");
        var step = new CinematicStep(0, 0, 1.0f, EasingType.Linear)
        {
            TargetZoom = 2.0f
        };
        sequence.Steps.Add(step);
        
        CinematicCameraSystem.PlaySequence(world, camera, sequence);
        
        // Update halfway
        cinematicSystem.Update(world, 0.5f);
        
        // Should be at ~1.5 zoom (halfway between 1.0 and 2.0)
        float expectedZoom = 1.5f;
        float tolerance = 0.1f;
        
        Assert(MathF.Abs(cameraComp.Zoom - expectedZoom) < tolerance,
               $"Zoom should be ~{expectedZoom}, got {cameraComp.Zoom}");
        
        Console.WriteLine($"  Zoom at 50%: {cameraComp.Zoom:F2} (expected ~1.50) ✓");
        Console.WriteLine("✓ Zoom interpolation works correctly\n");
    }
    
    private static void TestSequenceCompletion()
    {
        Console.WriteLine("[Test 5] Sequence completion callback...");
        
        var world = new World();
        var cinematicSystem = new CinematicCameraSystem();
        cinematicSystem.Initialize(world);
        
        var camera = world.CreateEntity();
        world.AddComponent(camera, new CameraComponent(800, 600));
        world.AddComponent(camera, new CinematicCameraComponent());
        
        bool callbackInvoked = false;
        
        var sequence = new CinematicSequence("Callback Test");
        sequence.Steps.Add(new CinematicStep(100, 100, 0.1f, EasingType.Linear));
        
        CinematicCameraSystem.PlaySequence(world, camera, sequence, () =>
        {
            callbackInvoked = true;
        });
        
        // Update past completion - need enough time to complete the step
        cinematicSystem.Update(world, 0.15f);
        
        // One more update to trigger the completion callback
        cinematicSystem.Update(world, 0.01f);
        
        Assert(callbackInvoked, "Completion callback should be invoked");
        
        var cinematic = world.GetComponent<CinematicCameraComponent>(camera);
        Assert(cinematic != null && !cinematic.IsPlaying, 
               "Sequence should be stopped after completion");
        
        Console.WriteLine("✓ Completion callback works correctly\n");
    }
    
    private static void TestSequenceHelpers()
    {
        Console.WriteLine("[Test 6] Sequence helper functions...");
        
        // Test CreatePanSequence
        var panSeq = CinematicCameraSystem.CreatePanSequence("Pan", 100, 200, 2.0f);
        Assert(panSeq.Steps.Count == 1, "Pan sequence should have 1 step");
        Assert(panSeq.Steps[0].TargetX == 100, "Pan target X should be correct");
        Assert(panSeq.Steps[0].TargetY == 200, "Pan target Y should be correct");
        Console.WriteLine("  ✓ CreatePanSequence");
        
        // Test CreateZoomSequence
        var zoomSeq = CinematicCameraSystem.CreateZoomSequence("Zoom", 50, 60, 1.5f, 1.0f);
        Assert(zoomSeq.Steps.Count == 1, "Zoom sequence should have 1 step");
        Assert(zoomSeq.Steps[0].TargetZoom == 1.5f, "Zoom target should be correct");
        Console.WriteLine("  ✓ CreateZoomSequence");
        
        // Test CreateRevealSequence
        var revealSeq = CinematicCameraSystem.CreateRevealSequence("Reveal", 0, 0, 100, 100);
        Assert(revealSeq.Steps.Count == 3, "Reveal sequence should have 3 steps");
        Console.WriteLine("  ✓ CreateRevealSequence");
        
        // Test CreatePatrolSequence
        var waypoints = new List<(float, float)> { (0, 0), (100, 0), (100, 100) };
        var patrolSeq = CinematicCameraSystem.CreatePatrolSequence("Patrol", waypoints);
        Assert(patrolSeq.Steps.Count == 3, "Patrol sequence should have 3 steps");
        Console.WriteLine("  ✓ CreatePatrolSequence");
        
        // Test CreateShakeSequence
        var shakeSeq = CinematicCameraSystem.CreateShakeSequence("Shake", 50, 50);
        Assert(shakeSeq.Steps.Count == 1, "Shake sequence should have 1 step");
        Assert(shakeSeq.Steps[0].ScreenShake != null, "Shake config should be set");
        Console.WriteLine("  ✓ CreateShakeSequence");
        
        Console.WriteLine("✓ All helper functions work correctly\n");
    }
    
    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new Exception($"Assertion failed: {message}");
        }
    }
}
