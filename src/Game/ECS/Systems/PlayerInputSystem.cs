using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.Engine;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// Player input system - handles player movement based on keyboard input
/// Supports both SDL2 and Windows virtual key codes for cross-platform compatibility
/// </summary>
public class PlayerInputSystem : ISystem
{
    // SDL2 key codes (lowercase ASCII for letters, special codes for arrows)
    private const int SDL_KEY_W = 119;  // 'w'
    private const int SDL_KEY_A = 97;   // 'a'
    private const int SDL_KEY_S = 115;  // 's'
    private const int SDL_KEY_D = 100;  // 'd'
    private const int SDL_KEY_UP = 1073741906;     // SDL_SCANCODE_UP
    private const int SDL_KEY_DOWN = 1073741905;   // SDL_SCANCODE_DOWN
    private const int SDL_KEY_LEFT = 1073741904;   // SDL_SCANCODE_LEFT
    private const int SDL_KEY_RIGHT = 1073741903;  // SDL_SCANCODE_RIGHT
    
    // Windows virtual key codes (uppercase ASCII for letters)
    private const int VK_W = 0x57;  // 87
    private const int VK_A = 0x41;  // 65
    private const int VK_S = 0x53;  // 83
    private const int VK_D = 0x44;  // 68
    private const int VK_UP = 0x26;    // 38
    private const int VK_DOWN = 0x28;  // 40
    private const int VK_LEFT = 0x25;  // 37
    private const int VK_RIGHT = 0x27; // 39
    
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
            var respawn = world.GetComponent<RespawnComponent>(entity);
            
            // Can't move while dead
            if (respawn != null && respawn.IsDead)
            {
                if (velocity != null)
                {
                    velocity.VX = 0;
                    velocity.VY = 0;
                }
                continue;
            }
            
            if (player != null && velocity != null)
            {
                float vx = 0;
                float vy = 0;
                
                // Horizontal movement - West (left)
                if (EngineInterop.Input_IsKeyDown(SDL_KEY_A) || EngineInterop.Input_IsKeyDown(VK_A) || 
                    EngineInterop.Input_IsKeyDown(SDL_KEY_LEFT) || EngineInterop.Input_IsKeyDown(VK_LEFT))
                {
                    vx -= player.Speed;
                }
                // Horizontal movement - East (right)
                if (EngineInterop.Input_IsKeyDown(SDL_KEY_D) || EngineInterop.Input_IsKeyDown(VK_D) || 
                    EngineInterop.Input_IsKeyDown(SDL_KEY_RIGHT) || EngineInterop.Input_IsKeyDown(VK_RIGHT))
                {
                    vx += player.Speed;
                }
                
                // Vertical movement - North (up)
                if (EngineInterop.Input_IsKeyDown(SDL_KEY_W) || EngineInterop.Input_IsKeyDown(VK_W) || 
                    EngineInterop.Input_IsKeyDown(SDL_KEY_UP) || EngineInterop.Input_IsKeyDown(VK_UP))
                {
                    vy -= player.Speed;
                }
                // Vertical movement - South (down)
                if (EngineInterop.Input_IsKeyDown(SDL_KEY_S) || EngineInterop.Input_IsKeyDown(VK_S) || 
                    EngineInterop.Input_IsKeyDown(SDL_KEY_DOWN) || EngineInterop.Input_IsKeyDown(VK_DOWN))
                {
                    vy += player.Speed;
                }
                
                velocity.VX = vx;
                velocity.VY = vy;
            }
        }
    }
}
