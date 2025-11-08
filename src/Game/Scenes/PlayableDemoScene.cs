using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.ECS.Systems;

namespace ChroniclesOfADrifter.Scenes;

/// <summary>
/// Playable demo scene with combat and multiple enemies
/// </summary>
public class PlayableDemoScene : Scene
{
    public override void OnLoad()
    {
        Console.WriteLine("[PlayableDemo] Loading playable demo scene...");
        
        // Add systems
        World.AddSystem(new ScriptSystem());
        World.AddSystem(new PlayerInputSystem());
        World.AddSystem(new CameraInputSystem());
        World.AddSystem(new MovementSystem());
        World.AddSystem(new CameraSystem());
        World.AddSystem(new CombatSystem());
        World.AddSystem(new RenderingSystem());
        
        // Create player entity in the center
        var player = World.CreateEntity();
        World.AddComponent(player, new PlayerComponent { Speed = 150.0f });
        World.AddComponent(player, new PositionComponent(960, 540)); // Center of 1920x1080
        World.AddComponent(player, new VelocityComponent());
        World.AddComponent(player, new SpriteComponent(0, 32, 32));
        World.AddComponent(player, new HealthComponent(100));
        World.AddComponent(player, new CombatComponent(damage: 15f, range: 100f, cooldown: 0.3f));
        
        // Create camera entity that follows player
        var camera = World.CreateEntity();
        var cameraComponent = new CameraComponent(1920, 1080)
        {
            Zoom = 1.0f,
            FollowSpeed = 8.0f
        };
        World.AddComponent(camera, cameraComponent);
        World.AddComponent(camera, new PositionComponent(960, 540));
        CameraSystem.SetFollowTarget(World, camera, player, followSpeed: 8.0f);
        
        // Create multiple goblin enemies in different positions
        CreateGoblin(World, 600, 300);
        CreateGoblin(World, 1200, 300);
        CreateGoblin(World, 600, 700);
        CreateGoblin(World, 1200, 700);
        CreateGoblin(World, 960, 200);
        
        Console.WriteLine("[PlayableDemo] Demo scene loaded!");
        Console.WriteLine("[PlayableDemo] Fight the goblins! Use SPACE to attack when near enemies.");
        Console.WriteLine("[PlayableDemo] Use +/- keys to zoom in/out");
    }
    
    private void CreateGoblin(World world, float x, float y)
    {
        var goblin = world.CreateEntity();
        world.AddComponent(goblin, new PositionComponent(x, y));
        world.AddComponent(goblin, new VelocityComponent());
        world.AddComponent(goblin, new SpriteComponent(1, 24, 24));
        world.AddComponent(goblin, new HealthComponent(30));
        world.AddComponent(goblin, new CombatComponent(damage: 5f, range: 75f, cooldown: 1.0f));
        world.AddComponent(goblin, new ScriptComponent("scripts/lua/enemies/goblin_ai.lua"));
    }
    
    public override void OnUnload()
    {
        Console.WriteLine("[PlayableDemo] Unloading playable demo scene...");
    }
}
