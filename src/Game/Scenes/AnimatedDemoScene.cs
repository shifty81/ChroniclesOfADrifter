using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.ECS.Systems;

namespace ChroniclesOfADrifter.Scenes;

/// <summary>
/// Animated demo scene - showcases the sprite animation system
/// </summary>
public class AnimatedDemoScene : Scene
{
    private Entity _player;
    
    public override void OnLoad()
    {
        Console.WriteLine("[AnimatedDemo] Loading animated demo scene...");
        
        // Add systems - note the AnimationSystem is added before rendering
        World.AddSystem(new AnimationSystem());
        World.AddSystem(new ScriptSystem());
        World.AddSystem(new PlayerInputSystem());
        World.AddSystem(new MovementSystem());
        World.AddSystem(new CombatSystem());
        World.AddSystem(new CharacterRenderingSystem());
        World.AddSystem(new RenderingSystem());
        
        // Create animated player entity in the center
        _player = World.CreateEntity();
        World.AddComponent(_player, new PlayerComponent { Speed = 150.0f });
        World.AddComponent(_player, new PositionComponent(960, 540)); // Center of 1920x1080
        World.AddComponent(_player, new VelocityComponent());
        World.AddComponent(_player, new HealthComponent(100));
        World.AddComponent(_player, new CombatComponent(damage: 15f, range: 100f, cooldown: 0.3f));
        
        // Add character appearance with customization
        var appearance = new CharacterAppearanceComponent
        {
            SkinTone = "medium",
            HairStyle = "short",
            HairColor = new Color(100, 70, 40, 255),
            EyeColor = new Color(70, 130, 180, 255),
            BodyType = "average"
        };
        
        // Add clothing layers
        appearance.ClothingLayers["shirt"] = new ClothingLayer(
            "shirt", 
            "tunic", 
            new Color(34, 139, 34, 255),    // Forest green
            new Color(85, 107, 47, 255),    // Dark olive
            renderOrder: 1
        );
        
        appearance.ClothingLayers["pants"] = new ClothingLayer(
            "pants", 
            "trousers", 
            new Color(70, 50, 30, 255),     // Brown
            new Color(50, 35, 20, 255),     // Dark brown
            renderOrder: 0
        );
        
        World.AddComponent(_player, appearance);
        
        // Add animated sprite component
        var animatedSprite = new AnimatedSpriteComponent(0, 64, 64)
        {
            Scale = 1.5f // Render at 1.5x size
        };
        
        // Define idle animation (4 frames)
        animatedSprite.Animations["idle"] = new AnimationDefinition(
            frameIndices: new[] { 0, 1, 2, 3 },
            framesPerRow: 8,
            frameWidth: 64,
            frameHeight: 64
        );
        
        // Define walk animation (6 frames)
        animatedSprite.Animations["walk"] = new AnimationDefinition(
            frameIndices: new[] { 8, 9, 10, 11, 12, 13 },
            framesPerRow: 8,
            frameWidth: 64,
            frameHeight: 64
        );
        
        // Define attack animation (4 frames, non-looping)
        animatedSprite.Animations["attack"] = new AnimationDefinition(
            frameIndices: new[] { 16, 17, 18, 19 },
            framesPerRow: 8,
            frameWidth: 64,
            frameHeight: 64
        );
        
        World.AddComponent(_player, animatedSprite);
        
        // Add animation component (starts with idle)
        var animation = new AnimationComponent("idle", frameDuration: 0.15f, loop: true);
        World.AddComponent(_player, animation);
        
        // Also add a basic sprite component as fallback for console rendering
        World.AddComponent(_player, new SpriteComponent(0, 64, 64));
        
        // Create animated goblin enemies
        CreateAnimatedGoblin(World, 600, 300);
        CreateAnimatedGoblin(World, 1200, 300);
        CreateAnimatedGoblin(World, 600, 700);
        CreateAnimatedGoblin(World, 1200, 700);
        CreateAnimatedGoblin(World, 960, 200);
        
        Console.WriteLine("[AnimatedDemo] Demo scene loaded!");
        Console.WriteLine("[AnimatedDemo] Animated characters with customizable clothing!");
        Console.WriteLine("[AnimatedDemo] Watch the smooth animations as you move!");
    }
    
    private void CreateAnimatedGoblin(World world, float x, float y)
    {
        var goblin = world.CreateEntity();
        world.AddComponent(goblin, new PositionComponent(x, y));
        world.AddComponent(goblin, new VelocityComponent());
        world.AddComponent(goblin, new HealthComponent(30));
        world.AddComponent(goblin, new CombatComponent(damage: 5f, range: 75f, cooldown: 1.0f));
        world.AddComponent(goblin, new ScriptComponent("scripts/lua/enemies/goblin_ai.lua"));
        
        // Add animated sprite for goblins
        var animatedSprite = new AnimatedSpriteComponent(1, 48, 48)
        {
            Scale = 1.0f
        };
        
        // Goblin idle animation
        animatedSprite.Animations["idle"] = new AnimationDefinition(
            frameIndices: new[] { 0, 1, 2, 3 },
            framesPerRow: 8,
            frameWidth: 48,
            frameHeight: 48
        );
        
        // Goblin walk animation
        animatedSprite.Animations["walk"] = new AnimationDefinition(
            frameIndices: new[] { 8, 9, 10, 11 },
            framesPerRow: 8,
            frameWidth: 48,
            frameHeight: 48
        );
        
        world.AddComponent(goblin, animatedSprite);
        
        var animation = new AnimationComponent("idle", frameDuration: 0.2f, loop: true);
        world.AddComponent(goblin, animation);
        
        // Fallback sprite for console rendering
        world.AddComponent(goblin, new SpriteComponent(1, 48, 48));
    }
    
    public override void Update(float deltaTime)
    {
        // Update player animation based on velocity
        UpdatePlayerAnimation();
        
        // Update goblin animations based on their movement
        UpdateGoblinAnimations();
        
        // Call base update to process all systems
        base.Update(deltaTime);
    }
    
    private void UpdatePlayerAnimation()
    {
        var velocity = World.GetComponent<VelocityComponent>(_player);
        var combat = World.GetComponent<CombatComponent>(_player);
        
        if (velocity == null)
            return;
        
        // Check if attacking
        if (combat != null && combat.TimeSinceLastAttack < 0.3f)
        {
            // Play attack animation (non-looping)
            AnimationSystem.PlayAnimation(World, _player, "attack", loop: false, playbackSpeed: 1.5f);
        }
        // Check if moving
        else if (Math.Abs(velocity.VX) > 0.1f || Math.Abs(velocity.VY) > 0.1f)
        {
            // Play walk animation
            AnimationSystem.PlayAnimation(World, _player, "walk", loop: true);
        }
        else
        {
            // Play idle animation
            AnimationSystem.PlayAnimation(World, _player, "idle", loop: true);
        }
    }
    
    private void UpdateGoblinAnimations()
    {
        // Update animations for all goblins based on their velocity
        foreach (var entity in World.GetEntitiesWithComponent<ScriptComponent>())
        {
            var velocity = World.GetComponent<VelocityComponent>(entity);
            var health = World.GetComponent<HealthComponent>(entity);
            
            if (velocity == null || health == null)
                continue;
            
            // Skip dead goblins
            if (health.CurrentHealth <= 0)
                continue;
            
            // Check if moving
            if (Math.Abs(velocity.VX) > 0.1f || Math.Abs(velocity.VY) > 0.1f)
            {
                AnimationSystem.PlayAnimation(World, entity, "walk", loop: true);
            }
            else
            {
                AnimationSystem.PlayAnimation(World, entity, "idle", loop: true);
            }
        }
    }
    
    public override void OnUnload()
    {
        Console.WriteLine("[AnimatedDemo] Unloading animated demo scene...");
    }
}
