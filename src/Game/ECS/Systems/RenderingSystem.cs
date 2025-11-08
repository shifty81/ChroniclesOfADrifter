using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.Engine;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// Rendering system - draws all sprites
/// </summary>
public class RenderingSystem : ISystem
{
    public void Initialize(World world)
    {
        // No initialization needed
    }
    
    public void Update(World world, float deltaTime)
    {
        // Clear the screen
        EngineInterop.Renderer_Clear(0.1f, 0.1f, 0.15f, 1.0f);
        
        // Render all entities with sprite and position
        foreach (var entity in world.GetEntitiesWithComponent<SpriteComponent>())
        {
            var sprite = world.GetComponent<SpriteComponent>(entity);
            var position = world.GetComponent<PositionComponent>(entity);
            
            if (sprite != null && position != null)
            {
                EngineInterop.Renderer_DrawSprite(
                    sprite.TextureId,
                    position.X,
                    position.Y,
                    sprite.Width,
                    sprite.Height,
                    sprite.Rotation
                );
            }
        }
        
        // Present the frame
        EngineInterop.Renderer_Present();
    }
}
