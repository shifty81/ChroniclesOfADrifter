namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// Animation component - manages sprite animation state and frame progression
/// </summary>
public class AnimationComponent : IComponent
{
    /// <summary>
    /// Current animation state (e.g., "idle", "walk", "attack")
    /// </summary>
    public string CurrentAnimation { get; set; }
    
    /// <summary>
    /// Current frame index in the animation
    /// </summary>
    public int CurrentFrame { get; set; }
    
    /// <summary>
    /// Time accumulated for frame progression
    /// </summary>
    public float FrameTimer { get; set; }
    
    /// <summary>
    /// Frame duration in seconds
    /// </summary>
    public float FrameDuration { get; set; }
    
    /// <summary>
    /// Whether the animation should loop
    /// </summary>
    public bool Loop { get; set; }
    
    /// <summary>
    /// Whether the animation has finished playing (for non-looping animations)
    /// </summary>
    public bool IsFinished { get; set; }
    
    /// <summary>
    /// Animation playback speed multiplier
    /// </summary>
    public float PlaybackSpeed { get; set; }
    
    public AnimationComponent(string initialAnimation = "idle", float frameDuration = 0.1f, bool loop = true)
    {
        CurrentAnimation = initialAnimation;
        CurrentFrame = 0;
        FrameTimer = 0f;
        FrameDuration = frameDuration;
        Loop = loop;
        IsFinished = false;
        PlaybackSpeed = 1.0f;
    }
}
