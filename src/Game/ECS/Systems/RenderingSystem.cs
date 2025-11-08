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
        
        // Render all entities with animated sprite and position first
        foreach (var entity in world.GetEntitiesWithComponent<AnimatedSpriteComponent>())
        {
            var animatedSprite = world.GetComponent<AnimatedSpriteComponent>(entity);
            var position = world.GetComponent<PositionComponent>(entity);
            var animation = world.GetComponent<AnimationComponent>(entity);
            
            if (animatedSprite != null && position != null && animation != null)
            {
                // Get the current frame index from the animation
                int frameIndex = AnimationSystem.GetCurrentFrameIndex(animation, animatedSprite);
                
                // For now, render using the basic sprite method
                // In a full implementation, this would calculate UV coordinates for the frame
                EngineInterop.Renderer_DrawSprite(
                    animatedSprite.TextureId,
                    position.X,
                    position.Y,
                    animatedSprite.Width * animatedSprite.Scale,
                    animatedSprite.Height * animatedSprite.Scale,
                    animatedSprite.Rotation
                );
            }
        }
        
        // Render all entities with regular sprite and position (that don't have animated sprites)
        foreach (var entity in world.GetEntitiesWithComponent<SpriteComponent>())
        {
            // Skip if entity has an AnimatedSpriteComponent (already rendered above)
            if (world.HasComponent<AnimatedSpriteComponent>(entity))
                continue;
            
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
