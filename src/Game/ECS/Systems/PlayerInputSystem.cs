using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.Engine;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// Player input system - handles player movement based on keyboard input
/// </summary>
public class PlayerInputSystem : ISystem
{
    // SDL2 key codes (lowercase ASCII for letters, special codes for arrows)
    private const int KEY_W = 119;  // 'w'
    private const int KEY_A = 97;   // 'a'
    private const int KEY_S = 115;  // 's'
    private const int KEY_D = 100;  // 'd'
    private const int KEY_UP = 1073741906;     // SDL_SCANCODE_UP
    private const int KEY_DOWN = 1073741905;   // SDL_SCANCODE_DOWN
    private const int KEY_LEFT = 1073741904;   // SDL_SCANCODE_LEFT
    private const int KEY_RIGHT = 1073741903;  // SDL_SCANCODE_RIGHT
    
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
