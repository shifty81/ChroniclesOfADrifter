using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.ECS.Systems;
using ChroniclesOfADrifter.Engine;

namespace ChroniclesOfADrifter.Scenes;

/// <summary>
/// Visual demo scene showing graphical rendering with SDL2
/// Demonstrates Zelda: A Link to the Past style tile-based rendering
/// </summary>
public class VisualDemoScene : Scene
{
    private Entity _playerEntity;
    private Entity _cameraEntity;
    
    public override void OnLoad()
    {
        Console.WriteLine("[VisualDemo] Loading visual demo scene...");
        
        // Add systems
        World.AddSystem(new PlayerInputSystem());
        World.AddSystem(new CameraInputSystem());
        World.AddSystem(new MovementSystem());
        World.AddSystem(new CameraSystem());
        World.AddSystem(new VisualRenderingSystem()); // Custom rendering system for SDL2
        
        // Create camera entity
        _cameraEntity = World.CreateEntity();
        var camera = new CameraComponent(1280, 720)
        {
            Zoom = 1.0f,
            FollowSpeed = 5.0f
        };
        World.AddComponent(_cameraEntity, camera);
        World.AddComponent(_cameraEntity, new PositionComponent(640, 360));
        
        // Create player entity (yellow square representing Link)
        _playerEntity = World.CreateEntity();
        World.AddComponent(_playerEntity, new PositionComponent(640, 360));
        World.AddComponent(_playerEntity, new VelocityComponent());
        World.AddComponent(_playerEntity, new SpriteComponent(0, 32, 32));
        World.AddComponent(_playerEntity, new PlayerComponent { Speed = 200.0f });
        World.AddComponent(_playerEntity, new HealthComponent(100));
        
        // Set camera to follow player
        CameraSystem.SetFollowTarget(World, _cameraEntity, _playerEntity, followSpeed: 5.0f);
        
        // Set camera bounds (larger world for exploration - Zelda-style overworld)
        CameraSystem.SetBounds(World, _cameraEntity,
            minX: 0, maxX: 2560,
            minY: 0, maxY: 1440);
        
        Console.WriteLine("[VisualDemo] Visual demo scene loaded!");
        Console.WriteLine("[VisualDemo] A graphical window should appear with Zelda-style rendering!");
        Console.WriteLine("[VisualDemo] Use WASD or Arrow keys to move");
        Console.WriteLine("[VisualDemo] Use +/- keys to zoom in/out");
        Console.WriteLine("[VisualDemo] Press Q or ESC to quit");
    }
    
    public override void Update(float deltaTime)
    {
        World.Update(deltaTime);
    }
    
    public override void OnUnload()
    {
        Console.WriteLine("[VisualDemo] Unloading visual demo scene...");
    }
}
