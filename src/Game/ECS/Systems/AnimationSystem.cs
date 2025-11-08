using ChroniclesOfADrifter.ECS.Components;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// Animation system - handles frame progression and animation state management
/// </summary>
public class AnimationSystem : ISystem
{
    public void Initialize(World world)
    {
        // No initialization needed
    }
    
    public void Update(World world, float deltaTime)
    {
        // Update all entities with animation components
        foreach (var entity in world.GetEntitiesWithComponent<AnimationComponent>())
        {
            var animation = world.GetComponent<AnimationComponent>(entity);
            var animatedSprite = world.GetComponent<AnimatedSpriteComponent>(entity);
            
            if (animation == null || animatedSprite == null)
                continue;
            
            // Skip if animation is finished and not looping
            if (animation.IsFinished && !animation.Loop)
                continue;
            
            // Check if the current animation exists
            if (!animatedSprite.Animations.ContainsKey(animation.CurrentAnimation))
                continue;
            
            var animDef = animatedSprite.Animations[animation.CurrentAnimation];
            
            // Update frame timer
            animation.FrameTimer += deltaTime * animation.PlaybackSpeed;
            
            // Check if we should advance to the next frame
            if (animation.FrameTimer >= animation.FrameDuration)
            {
                animation.FrameTimer -= animation.FrameDuration;
                animation.CurrentFrame++;
                
                // Handle frame wraparound
                if (animation.CurrentFrame >= animDef.FrameCount)
                {
                    if (animation.Loop)
                    {
                        animation.CurrentFrame = 0;
                    }
                    else
                    {
                        animation.CurrentFrame = animDef.FrameCount - 1;
                        animation.IsFinished = true;
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Play a specific animation on an entity
    /// </summary>
    public static void PlayAnimation(World world, Entity entity, string animationName, bool loop = true, float playbackSpeed = 1.0f)
    {
        var animation = world.GetComponent<AnimationComponent>(entity);
        var animatedSprite = world.GetComponent<AnimatedSpriteComponent>(entity);
        
        if (animation == null || animatedSprite == null)
            return;
        
        // Only reset if playing a different animation
        if (animation.CurrentAnimation != animationName)
        {
            animation.CurrentAnimation = animationName;
            animation.CurrentFrame = 0;
            animation.FrameTimer = 0f;
            animation.IsFinished = false;
        }
        
        animation.Loop = loop;
        animation.PlaybackSpeed = playbackSpeed;
    }
    
    /// <summary>
    /// Get the current frame index for rendering
    /// </summary>
    public static int GetCurrentFrameIndex(AnimationComponent animation, AnimatedSpriteComponent sprite)
    {
        if (!sprite.Animations.ContainsKey(animation.CurrentAnimation))
            return 0;
        
        var animDef = sprite.Animations[animation.CurrentAnimation];
        
        if (animation.CurrentFrame < 0 || animation.CurrentFrame >= animDef.FrameCount)
            return 0;
        
        return animDef.FrameIndices[animation.CurrentFrame];
    }
}
