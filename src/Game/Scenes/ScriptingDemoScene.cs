using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.ECS.Systems;

namespace ChroniclesOfADrifter.Scenes;

/// <summary>
/// Scene demonstrating Lua scripting integration
/// </summary>
public class ScriptingDemoScene : Scene
{
    public override void OnLoad()
    {
        Console.WriteLine("[ScriptingDemo] Loading scripting demo scene...");
        
        // Add systems (ScriptSystem first to load scripts)
        World.AddSystem(new ScriptSystem());
        World.AddSystem(new PlayerInputSystem());
        World.AddSystem(new MovementSystem());
        World.AddSystem(new RenderingSystem());
        
        // Create player entity
        var player = World.CreateEntity();
        World.AddComponent(player, new PlayerComponent { Speed = 100.0f });
        World.AddComponent(player, new PositionComponent(960, 540));
        World.AddComponent(player, new VelocityComponent());
        World.AddComponent(player, new SpriteComponent(0, 32, 32));
        World.AddComponent(player, new HealthComponent(100));
        
        // Create goblin entity with Lua script
        var goblin = World.CreateEntity();
        World.AddComponent(goblin, new PositionComponent(800, 400));
        World.AddComponent(goblin, new SpriteComponent(1, 24, 24));
        World.AddComponent(goblin, new HealthComponent(50));
        World.AddComponent(goblin, new ScriptComponent("scripts/lua/enemies/goblin_example.lua"));
        
        Console.WriteLine("[ScriptingDemo] Demo scene loaded!");
        Console.WriteLine("[ScriptingDemo] Controls: WASD or Arrow Keys to move player");
        Console.WriteLine("[ScriptingDemo] Watch for Lua script output from goblin AI");
    }
    
    public override void OnUnload()
    {
        Console.WriteLine("[ScriptingDemo] Unloading scripting demo scene...");
    }
}
