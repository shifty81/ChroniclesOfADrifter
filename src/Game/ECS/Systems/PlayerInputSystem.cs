using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.Engine;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// Player input system - handles player movement based on keyboard input
/// </summary>
public class PlayerInputSystem : ISystem
{
    // Key codes matching standard keyboard layout
    private const int KEY_W = 87;
    private const int KEY_A = 65;
    private const int KEY_S = 83;
    private const int KEY_D = 68;
    private const int KEY_UP = 265;
    private const int KEY_DOWN = 264;
    private const int KEY_LEFT = 263;
    private const int KEY_RIGHT = 262;
    
    public void Initialize(World world)
    {
        // No initialization needed
    }
    
    public void Update(World world, float deltaTime)
    {
        // Process input for all player entities
        foreach (var entity in world.GetEntitiesWithComponent<PlayerComponent>())
        {
            var player = world.GetComponent<PlayerComponent>(entity);
            var velocity = world.GetComponent<VelocityComponent>(entity);
            
            if (player != null && velocity != null)
            {
                float vx = 0;
                float vy = 0;
                
                // Horizontal movement
                if (EngineInterop.Input_IsKeyDown(KEY_A) || EngineInterop.Input_IsKeyDown(KEY_LEFT))
                {
                    vx -= player.Speed;
                }
                if (EngineInterop.Input_IsKeyDown(KEY_D) || EngineInterop.Input_IsKeyDown(KEY_RIGHT))
                {
                    vx += player.Speed;
                }
                
                // Vertical movement
                if (EngineInterop.Input_IsKeyDown(KEY_W) || EngineInterop.Input_IsKeyDown(KEY_UP))
                {
                    vy -= player.Speed;
                }
                if (EngineInterop.Input_IsKeyDown(KEY_S) || EngineInterop.Input_IsKeyDown(KEY_DOWN))
                {
                    vy += player.Speed;
                }
                
                velocity.VX = vx;
                velocity.VY = vy;
            }
        }
    }
}
