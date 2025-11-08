using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.ECS.Systems;

namespace ChroniclesOfADrifter.Scenes;

/// <summary>
/// Demo scene showcasing the camera system with player following and zoom controls
/// </summary>
public class CameraDemoScene : Scene
{
    private Entity _playerEntity;
    private Entity _cameraEntity;
    
    public override void OnLoad()
    {
        // Add systems (order matters!)
        World.AddSystem(new PlayerInputSystem());
        World.AddSystem(new CameraInputSystem());
        World.AddSystem(new MovementSystem());
        World.AddSystem(new CameraSystem());
        World.AddSystem(new RenderingSystem());
        
        // Create camera entity
        _cameraEntity = World.CreateEntity();
        var camera = new CameraComponent(1920, 1080)
        {
            Zoom = 1.0f,
            FollowSpeed = 5.0f
        };
        World.AddComponent(_cameraEntity, camera);
        World.AddComponent(_cameraEntity, new PositionComponent(960, 540));
        
        // Create player entity
        _playerEntity = World.CreateEntity();
        World.AddComponent(_playerEntity, new PositionComponent(960, 540));
        World.AddComponent(_playerEntity, new VelocityComponent());
        World.AddComponent(_playerEntity, new SpriteComponent(0, 32, 32));
        World.AddComponent(_playerEntity, new PlayerComponent());
        World.AddComponent(_playerEntity, new HealthComponent(100));
        
        // Set camera to follow player
        CameraSystem.SetFollowTarget(World, _cameraEntity, _playerEntity, followSpeed: 5.0f);
        
        // Set camera bounds (optional - creates a larger world to explore)
        CameraSystem.SetBounds(World, _cameraEntity, 
            minX: 0, maxX: 3840,  // 2x viewport width
            minY: 0, maxY: 2160); // 2x viewport height
        
        // Create some enemy entities scattered around the world
        CreateEnemyAt(500, 300);
        CreateEnemyAt(1500, 300);
        CreateEnemyAt(500, 800);
        CreateEnemyAt(1500, 800);
        CreateEnemyAt(960, 200);
        CreateEnemyAt(960, 880);
        CreateEnemyAt(200, 540);
        CreateEnemyAt(1720, 540);
        
        // Create some additional enemies outside initial view
        CreateEnemyAt(2500, 1200);
        CreateEnemyAt(3200, 1800);
        CreateEnemyAt(300, 1500);
        
        Console.WriteLine("[CameraDemoScene] Scene loaded!");
        Console.WriteLine("[CameraDemoScene] - Camera follows player");
        Console.WriteLine("[CameraDemoScene] - Move around to see camera following");
        Console.WriteLine("[CameraDemoScene] - Use +/- keys to zoom in/out");
        Console.WriteLine("[CameraDemoScene] - Camera bounds: 3840x2160 world space");
    }
    
    private void CreateEnemyAt(float x, float y)
    {
        var enemy = World.CreateEntity();
        World.AddComponent(enemy, new PositionComponent(x, y));
        World.AddComponent(enemy, new SpriteComponent(1, 24, 24));
        World.AddComponent(enemy, new HealthComponent(50));
        World.AddComponent(enemy, new ScriptComponent("goblin_ai"));
    }
    
    public override void Update(float deltaTime)
    {
        World.Update(deltaTime);
    }
    
    public override void OnUnload()
    {
        Console.WriteLine("[CameraDemoScene] Scene unloaded");
    }
}
