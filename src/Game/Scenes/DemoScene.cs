using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.ECS.Systems;

namespace ChroniclesOfADrifter.Scenes;

/// <summary>
/// Demo scene showcasing the ECS system with a player entity
/// </summary>
public class DemoScene : Scene
{
    public override void OnLoad()
    {
        Console.WriteLine("[DemoScene] Loading demo scene...");
        
        // Add systems
        World.AddSystem(new PlayerInputSystem());
        World.AddSystem(new MovementSystem());
        World.AddSystem(new RenderingSystem());
        
        // Create player entity
        var player = World.CreateEntity();
        World.AddComponent(player, new PlayerComponent { Speed = 100.0f });
        World.AddComponent(player, new PositionComponent(960, 540)); // Center of 1920x1080
        World.AddComponent(player, new VelocityComponent());
        World.AddComponent(player, new SpriteComponent(0, 32, 32)); // 32x32 sprite
        World.AddComponent(player, new HealthComponent(100));
        
        Console.WriteLine("[DemoScene] Demo scene loaded!");
        Console.WriteLine("[DemoScene] Controls: WASD or Arrow Keys to move");
    }
    
    public override void OnUnload()
    {
        Console.WriteLine("[DemoScene] Unloading demo scene...");
    }
}
