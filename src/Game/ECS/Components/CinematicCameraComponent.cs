namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// Component for cinematic camera movements (cutscenes, dramatic moments)
/// </summary>
public class CinematicCameraComponent : IComponent
{
    /// <summary>
    /// Whether a cinematic sequence is currently playing
    /// </summary>
    public bool IsPlaying { get; set; } = false;
    
    /// <summary>
    /// Current cinematic sequence being played
    /// </summary>
    public CinematicSequence? CurrentSequence { get; set; } = null;
    
    /// <summary>
    /// Current step index in the sequence
    /// </summary>
    public int CurrentStepIndex { get; set; } = 0;
    
    /// <summary>
    /// Time elapsed in current step (seconds)
    /// </summary>
    public float StepElapsedTime { get; set; } = 0f;
    
    /// <summary>
    /// Starting position for current step (for interpolation)
    /// </summary>
    public float StartX { get; set; } = 0f;
    public float StartY { get; set; } = 0f;
    public float StartZoom { get; set; } = 1.0f;
    
    /// <summary>
    /// Callback to invoke when sequence completes
    /// </summary>
    public Action? OnSequenceComplete { get; set; } = null;
}

/// <summary>
/// Represents a complete cinematic sequence with multiple steps
/// </summary>
public class CinematicSequence
{
    public string Name { get; set; } = "Unnamed";
    public List<CinematicStep> Steps { get; set; } = new();
    
    public CinematicSequence(string name)
    {
        Name = name;
    }
}

/// <summary>
/// A single step in a cinematic sequence (move camera to a position)
/// </summary>
public class CinematicStep
{
    /// <summary>
    /// Target position for this step
    /// </summary>
    public float TargetX { get; set; }
    public float TargetY { get; set; }
    
    /// <summary>
    /// Target zoom level for this step
    /// </summary>
    public float TargetZoom { get; set; } = 1.0f;
    
    /// <summary>
    /// Duration of this step in seconds
    /// </summary>
    public float Duration { get; set; } = 2.0f;
    
    /// <summary>
    /// Easing function to use for this step
    /// </summary>
    public EasingType Easing { get; set; } = EasingType.Linear;
    
    /// <summary>
    /// Optional: Wait time after reaching target before moving to next step
    /// </summary>
    public float HoldDuration { get; set; } = 0f;
    
    /// <summary>
    /// Optional: Screen shake to trigger when this step starts
    /// </summary>
    public ScreenShakeConfig? ScreenShake { get; set; } = null;
    
    public CinematicStep(float targetX, float targetY, float duration, EasingType easing = EasingType.Linear)
    {
        TargetX = targetX;
        TargetY = targetY;
        Duration = duration;
        Easing = easing;
    }
}

/// <summary>
/// Configuration for optional screen shake in a cinematic step
/// </summary>
public class ScreenShakeConfig
{
    public float Intensity { get; set; }
    public float Duration { get; set; }
    public float Frequency { get; set; } = 25f;
    
    public ScreenShakeConfig(float intensity, float duration)
    {
        Intensity = intensity;
        Duration = duration;
    }
}

/// <summary>
/// Easing functions for smooth cinematic movements
/// </summary>
public enum EasingType
{
    Linear,          // Constant speed
    EaseIn,          // Start slow, end fast
    EaseOut,         // Start fast, end slow
    EaseInOut,       // Start slow, speed up, slow down
    EaseInQuad,      // Quadratic ease in
    EaseOutQuad,     // Quadratic ease out
    EaseInOutQuad,   // Quadratic ease in-out
    EaseInCubic,     // Cubic ease in (more dramatic)
    EaseOutCubic,    // Cubic ease out (more dramatic)
    EaseInOutCubic,  // Cubic ease in-out (more dramatic)
}
