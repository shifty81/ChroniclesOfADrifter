using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.ECS.Systems;

namespace ChroniclesOfADrifter.Scenes;

/// <summary>
/// Demo scene showcasing cinematic camera movements
/// </summary>
public class CinematicCameraDemoScene : Scene
{
    private Entity _camera = default;
    private Entity _player = default;
    private int _currentDemoIndex = 0;
    private float _timeSinceLastDemo = 0f;
    private const float DelayBetweenDemos = 2.0f;
    
    public override void OnLoad()
    {
        Console.WriteLine("\n===========================================");
        Console.WriteLine("  Cinematic Camera System Demo");
        Console.WriteLine("===========================================\n");
        
        // Register required systems
        World.AddSystem(new CinematicCameraSystem());
        World.AddSystem(new ScreenShakeSystem());
        World.AddSystem(new CameraSystem());
        
        // Create camera with cinematic component
        _camera = World.CreateEntity();
        World.AddComponent(_camera, new CameraComponent(800, 600)
        {
            X = 400,
            Y = 300,
            Zoom = 1.0f,
            IsActive = true
        });
        World.AddComponent(_camera, new CinematicCameraComponent());
        World.AddComponent(_camera, new ScreenShakeComponent());
        
        // Create a player entity (visual reference)
        _player = World.CreateEntity();
        World.AddComponent(_player, new PositionComponent { X = 400, Y = 300 });
        
        // Create some reference points in the world
        CreateReferencePoints();
        
        Console.WriteLine("[Setup] Camera and reference points created");
        Console.WriteLine("[Info] Watch as different cinematic sequences play...\n");
        
        // Start first demo after a short delay
        _timeSinceLastDemo = DelayBetweenDemos - 0.5f;
    }
    
    private void CreateReferencePoints()
    {
        // Create some entities to serve as visual reference points
        var points = new[]
        {
            (100f, 100f, "Point A"),
            (700f, 100f, "Point B"),
            (700f, 500f, "Point C"),
            (100f, 500f, "Point D"),
            (400f, 300f, "Center")
        };
        
        foreach (var (x, y, name) in points)
        {
            var entity = World.CreateEntity();
            World.AddComponent(entity, new PositionComponent { X = x, Y = y });
            // In a real game, these would have sprite components
        }
    }
    
    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);
        
        var cinematic = World.GetComponent<CinematicCameraComponent>(_camera);
        var camera = World.GetComponent<CameraComponent>(_camera);
        
        if (cinematic == null || camera == null)
            return;
        
        // Wait for current sequence to finish before starting next demo
        if (!cinematic.IsPlaying)
        {
            _timeSinceLastDemo += deltaTime;
            
            if (_timeSinceLastDemo >= DelayBetweenDemos)
            {
                StartNextDemo(camera);
                _timeSinceLastDemo = 0f;
            }
        }
        else
        {
            // Display current camera state during cinematic
            if (cinematic.StepElapsedTime % 0.5f < deltaTime)
            {
                var step = cinematic.CurrentSequence?.Steps[cinematic.CurrentStepIndex];
                if (step != null)
                {
                    Console.WriteLine($"  Step {cinematic.CurrentStepIndex + 1}: " +
                                    $"Pos=({camera.X:F1}, {camera.Y:F1}), " +
                                    $"Zoom={camera.Zoom:F2}, " +
                                    $"Target=({step.TargetX:F1}, {step.TargetY:F1})");
                }
            }
        }
    }
    
    private void StartNextDemo(CameraComponent camera)
    {
        CinematicSequence? sequence = null;
        string demoName = "";
        
        switch (_currentDemoIndex)
        {
            case 0:
                demoName = "Simple Pan (Linear)";
                sequence = CinematicCameraSystem.CreatePanSequence(
                    "Pan to Point A", 100, 100, 2.0f, EasingType.Linear);
                break;
                
            case 1:
                demoName = "Smooth Pan (Ease In-Out)";
                sequence = CinematicCameraSystem.CreatePanSequence(
                    "Pan to Point B", 700, 100, 2.0f, EasingType.EaseInOut);
                break;
                
            case 2:
                demoName = "Zoom In (Ease Out)";
                sequence = CinematicCameraSystem.CreateZoomSequence(
                    "Zoom Center", 400, 300, 2.0f, 2.0f, EasingType.EaseOut);
                break;
                
            case 3:
                demoName = "Zoom Out (Ease In)";
                sequence = CinematicCameraSystem.CreateZoomSequence(
                    "Zoom Out", 400, 300, 0.5f, 2.0f, EasingType.EaseIn);
                break;
                
            case 4:
                demoName = "Dramatic Reveal";
                sequence = CinematicCameraSystem.CreateRevealSequence(
                    "Reveal", 700, 500, 100, 100, 2.5f, 1.0f, 4.0f);
                break;
                
            case 5:
                demoName = "Patrol Path (Waypoints)";
                var waypoints = new List<(float, float)>
                {
                    (100, 100),
                    (700, 100),
                    (700, 500),
                    (100, 500),
                    (400, 300)
                };
                sequence = CinematicCameraSystem.CreatePatrolSequence(
                    "Patrol", waypoints, 1.5f, 0.3f);
                break;
                
            case 6:
                demoName = "Shake Effect";
                sequence = CinematicCameraSystem.CreateShakeSequence(
                    "Earthquake", 400, 300, 25f, 1.5f);
                break;
                
            case 7:
                demoName = "Complex Sequence (Custom)";
                sequence = CreateComplexSequence();
                break;
                
            default:
                // Reset to beginning
                _currentDemoIndex = -1;
                camera.X = 400;
                camera.Y = 300;
                camera.Zoom = 1.0f;
                Console.WriteLine("\n===========================================");
                Console.WriteLine("  Demo Complete - Restarting...");
                Console.WriteLine("===========================================\n");
                return;
        }
        
        if (sequence != null)
        {
            Console.WriteLine($"\n[Demo {_currentDemoIndex + 1}] {demoName}");
            Console.WriteLine(new string('-', 50));
            
            CinematicCameraSystem.PlaySequence(World, _camera, sequence, () =>
            {
                Console.WriteLine($"  ✓ Sequence '{sequence.Name}' completed!\n");
            });
        }
        
        _currentDemoIndex++;
    }
    
    private CinematicSequence CreateComplexSequence()
    {
        var sequence = new CinematicSequence("Epic Battle Intro");
        
        // Start zoomed in on player
        sequence.Steps.Add(new CinematicStep(400, 300, 1.0f, EasingType.EaseOut)
        {
            TargetZoom = 2.0f,
            HoldDuration = 0.5f
        });
        
        // Quick pan to enemy location with shake
        sequence.Steps.Add(new CinematicStep(600, 200, 0.8f, EasingType.EaseInOut)
        {
            TargetZoom = 2.0f,
            HoldDuration = 0.5f,
            ScreenShake = new ScreenShakeConfig(10f, 0.3f)
        });
        
        // Zoom out to show battlefield
        sequence.Steps.Add(new CinematicStep(400, 300, 2.0f, EasingType.EaseInOutCubic)
        {
            TargetZoom = 0.8f,
            HoldDuration = 1.0f
        });
        
        // Big dramatic shake at the end
        sequence.Steps.Add(new CinematicStep(400, 300, 0.01f, EasingType.Linear)
        {
            TargetZoom = 0.8f,
            HoldDuration = 1.0f,
            ScreenShake = new ScreenShakeConfig(30f, 1.0f)
        });
        
        return sequence;
    }
    
    public override void OnUnload()
    {
        Console.WriteLine("\n===========================================");
        Console.WriteLine("  Cinematic Camera Demo - Cleanup");
        Console.WriteLine("===========================================");
    }
}
