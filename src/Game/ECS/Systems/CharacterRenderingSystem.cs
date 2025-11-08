using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.Engine;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// Character rendering system - handles rendering of customizable characters with clothing layers
/// </summary>
public class CharacterRenderingSystem : ISystem
{
    public void Initialize(World world)
    {
        // No initialization needed
    }
    
    public void Update(World world, float deltaTime)
    {
        // Render all characters with appearance components
        foreach (var entity in world.GetEntitiesWithComponent<CharacterAppearanceComponent>())
        {
            var appearance = world.GetComponent<CharacterAppearanceComponent>(entity);
            var position = world.GetComponent<PositionComponent>(entity);
            var animatedSprite = world.GetComponent<AnimatedSpriteComponent>(entity);
            var animation = world.GetComponent<AnimationComponent>(entity);
            
            if (appearance == null || position == null)
                continue;
            
            // Render base character body
            RenderCharacterBase(entity, world, position, animatedSprite, animation);
            
            // Render clothing layers if no armor is equipped
            if (string.IsNullOrEmpty(appearance.EquippedArmor))
            {
                RenderClothingLayers(appearance, position, animatedSprite, animation);
            }
            else
            {
                // Render armor instead of clothing
                RenderArmor(appearance, position, animatedSprite, animation);
            }
        }
    }
    
    private void RenderCharacterBase(Entity entity, World world, PositionComponent position, 
                                     AnimatedSpriteComponent? animatedSprite, AnimationComponent? animation)
    {
        // If entity has animated sprite, use animation system
        if (animatedSprite != null && animation != null)
        {
            int frameIndex = AnimationSystem.GetCurrentFrameIndex(animation, animatedSprite);
            
            // For now, use the basic sprite rendering
            // In a full implementation, this would render the specific frame from the sprite sheet
            EngineInterop.Renderer_DrawSprite(
                animatedSprite.TextureId,
                position.X,
                position.Y,
                animatedSprite.Width * animatedSprite.Scale,
                animatedSprite.Height * animatedSprite.Scale,
                animatedSprite.Rotation
            );
        }
        else
        {
            // Fall back to basic sprite if no animation
            var sprite = world.GetComponent<SpriteComponent>(entity);
            if (sprite != null)
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
    
    private void RenderClothingLayers(CharacterAppearanceComponent appearance, PositionComponent position,
                                     AnimatedSpriteComponent? animatedSprite, AnimationComponent? animation)
    {
        // Sort clothing layers by render order
        var sortedLayers = appearance.ClothingLayers.Values
            .Where(layer => layer.IsVisible)
            .OrderBy(layer => layer.RenderOrder)
            .ToList();
        
        foreach (var layer in sortedLayers)
        {
            // In a full implementation, each layer would have its own sprite/texture
            // For now, we'll just track that the layers exist
            // The actual rendering would call EngineInterop.Renderer_DrawSprite for each layer
        }
    }
    
    private void RenderArmor(CharacterAppearanceComponent appearance, PositionComponent position,
                            AnimatedSpriteComponent? animatedSprite, AnimationComponent? animation)
    {
        // Render the equipped armor sprite
        // This would override the clothing layers
    }
    
    /// <summary>
    /// Update clothing visibility based on armor equipment
    /// </summary>
    public static void UpdateClothingVisibility(CharacterAppearanceComponent appearance)
    {
        bool hasArmor = !string.IsNullOrEmpty(appearance.EquippedArmor);
        
        foreach (var layer in appearance.ClothingLayers.Values)
        {
            layer.IsVisible = !hasArmor;
        }
    }
}
