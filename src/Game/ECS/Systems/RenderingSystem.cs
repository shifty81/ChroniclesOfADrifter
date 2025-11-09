using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.Engine;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// Rendering system - draws all sprites with Y-sorting for 3/4 perspective depth
/// </summary>
public class RenderingSystem : ISystem
{
    private struct RenderData
    {
        public Entity Entity;
        public float Y;
        public float Height;
        public bool IsAnimated;
    }
    
    public void Initialize(World world)
    {
        // No initialization needed
    }
    
    public void Update(World world, float deltaTime)
    {
        // Clear the screen
        EngineInterop.Renderer_Clear(0.1f, 0.1f, 0.15f, 1.0f);
        
        // Collect all renderable entities with their Y positions for sorting
        var renderList = new List<RenderData>();
        
        // Collect animated sprites
        foreach (var entity in world.GetEntitiesWithComponent<AnimatedSpriteComponent>())
        {
            var position = world.GetComponent<PositionComponent>(entity);
            var animatedSprite = world.GetComponent<AnimatedSpriteComponent>(entity);
            
            if (position != null && animatedSprite != null)
            {
                renderList.Add(new RenderData
                {
                    Entity = entity,
                    Y = position.Y,
                    Height = animatedSprite.Height * animatedSprite.Scale,
                    IsAnimated = true
                });
            }
        }
        
        // Collect regular sprites (that don't have animated sprites)
        foreach (var entity in world.GetEntitiesWithComponent<SpriteComponent>())
        {
            if (world.HasComponent<AnimatedSpriteComponent>(entity))
                continue;
            
            var position = world.GetComponent<PositionComponent>(entity);
            var sprite = world.GetComponent<SpriteComponent>(entity);
            
            if (position != null && sprite != null)
            {
                renderList.Add(new RenderData
                {
                    Entity = entity,
                    Y = position.Y,
                    Height = sprite.Height,
                    IsAnimated = false
                });
            }
        }
        
        // Sort by Y position (and bottom edge for proper depth)
        // In 3/4 perspective, objects further "back" (lower Y) should render first
        renderList.Sort((a, b) =>
        {
            // Calculate bottom edge (Y + Height) for accurate depth sorting
            float bottomA = a.Y + a.Height;
            float bottomB = b.Y + b.Height;
            return bottomA.CompareTo(bottomB);
        });
        
        // Render sorted entities (back-to-front for proper occlusion)
        foreach (var data in renderList)
        {
            if (data.IsAnimated)
            {
                RenderAnimatedSprite(world, data.Entity);
            }
            else
            {
                RenderSprite(world, data.Entity);
            }
        }
        
        // Render UI on top of everything else
        UISystem.RenderUI(world);
        
        // Present the frame
        EngineInterop.Renderer_Present();
    }
    
    private void RenderAnimatedSprite(World world, Entity entity)
    {
        var animatedSprite = world.GetComponent<AnimatedSpriteComponent>(entity);
        var position = world.GetComponent<PositionComponent>(entity);
        var animation = world.GetComponent<AnimationComponent>(entity);
        
        if (animatedSprite != null && position != null && animation != null)
        {
            // Get the current frame index from the animation
            int frameIndex = AnimationSystem.GetCurrentFrameIndex(animation, animatedSprite);
            
            // Render using the basic sprite method
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
    
    private void RenderSprite(World world, Entity entity)
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
}
