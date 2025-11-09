using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// System that processes cinematic camera movements for cutscenes
/// </summary>
public class CinematicCameraSystem : ISystem
{
    public void Initialize(World world)
    {
        // No initialization needed
    }
    
    public void Update(World world, float deltaTime)
    {
        foreach (var entity in world.GetEntitiesWithComponent<CinematicCameraComponent>())
        {
            var cinematic = world.GetComponent<CinematicCameraComponent>(entity);
            var camera = world.GetComponent<CameraComponent>(entity);
            
            if (cinematic == null || camera == null || !cinematic.IsPlaying)
                continue;
            
            if (cinematic.CurrentSequence == null || cinematic.CurrentSequence.Steps.Count == 0)
            {
                StopSequence(cinematic);
                continue;
            }
            
            // Get current step
            if (cinematic.CurrentStepIndex >= cinematic.CurrentSequence.Steps.Count)
            {
                // Sequence complete
                CompleteSequence(world, cinematic);
                continue;
            }
            
            var step = cinematic.CurrentSequence.Steps[cinematic.CurrentStepIndex];
            
            // Check if this is the first frame of this step
            if (cinematic.StepElapsedTime == 0f)
            {
                // Store starting position for interpolation
                cinematic.StartX = camera.X;
                cinematic.StartY = camera.Y;
                cinematic.StartZoom = camera.Zoom;
                
                // Trigger screen shake if configured
                if (step.ScreenShake != null)
                {
                    var shake = world.GetComponent<ScreenShakeComponent>(entity);
                    if (shake != null)
                    {
                        shake.Trigger(step.ScreenShake.Intensity, 
                                    step.ScreenShake.Duration, 
                                    step.ScreenShake.Frequency);
                    }
                }
            }
            
            // Update elapsed time
            cinematic.StepElapsedTime += deltaTime;
            
            // Calculate progress through this step
            float totalStepTime = step.Duration + step.HoldDuration;
            
            if (cinematic.StepElapsedTime >= totalStepTime)
            {
                // Move to next step
                cinematic.CurrentStepIndex++;
                cinematic.StepElapsedTime = 0f;
            }
            else if (cinematic.StepElapsedTime < step.Duration)
            {
                // Still moving - interpolate position
                float t = cinematic.StepElapsedTime / step.Duration;
                float easedT = ApplyEasing(t, step.Easing);
                
                // Interpolate camera position
                camera.X = Lerp(cinematic.StartX, step.TargetX, easedT);
                camera.Y = Lerp(cinematic.StartY, step.TargetY, easedT);
                camera.Zoom = Lerp(cinematic.StartZoom, step.TargetZoom, easedT);
            }
            // else: holding at target position
        }
    }
    
    /// <summary>
    /// Apply easing function to progress value (0 to 1)
    /// </summary>
    private float ApplyEasing(float t, EasingType easing)
    {
        return easing switch
        {
            EasingType.Linear => t,
            EasingType.EaseIn => t * t,
            EasingType.EaseOut => 1f - (1f - t) * (1f - t),
            EasingType.EaseInOut => t < 0.5f 
                ? 2f * t * t 
                : 1f - MathF.Pow(-2f * t + 2f, 2f) / 2f,
            EasingType.EaseInQuad => t * t,
            EasingType.EaseOutQuad => 1f - (1f - t) * (1f - t),
            EasingType.EaseInOutQuad => t < 0.5f 
                ? 2f * t * t 
                : 1f - MathF.Pow(-2f * t + 2f, 2f) / 2f,
            EasingType.EaseInCubic => t * t * t,
            EasingType.EaseOutCubic => 1f - MathF.Pow(1f - t, 3f),
            EasingType.EaseInOutCubic => t < 0.5f 
                ? 4f * t * t * t 
                : 1f - MathF.Pow(-2f * t + 2f, 3f) / 2f,
            _ => t
        };
    }
    
    /// <summary>
    /// Linear interpolation
    /// </summary>
    private float Lerp(float a, float b, float t)
    {
        return a + (b - a) * MathF.Max(0, MathF.Min(1, t));
    }
    
    /// <summary>
    /// Stop the current sequence without completing it
    /// </summary>
    private void StopSequence(CinematicCameraComponent cinematic)
    {
        cinematic.IsPlaying = false;
        cinematic.CurrentSequence = null;
        cinematic.CurrentStepIndex = 0;
        cinematic.StepElapsedTime = 0f;
    }
    
    /// <summary>
    /// Complete the current sequence and invoke callback
    /// </summary>
    private void CompleteSequence(World world, CinematicCameraComponent cinematic)
    {
        var callback = cinematic.OnSequenceComplete;
        StopSequence(cinematic);
        callback?.Invoke();
    }
    
    /// <summary>
    /// Start playing a cinematic sequence
    /// </summary>
    public static void PlaySequence(World world, Entity cameraEntity, CinematicSequence sequence, Action? onComplete = null)
    {
        var cinematic = world.GetComponent<CinematicCameraComponent>(cameraEntity);
        var camera = world.GetComponent<CameraComponent>(cameraEntity);
        
        if (cinematic == null || camera == null)
            return;
        
        // Disable camera following during cinematic
        camera.FollowTarget = default;
        
        // Setup sequence
        cinematic.CurrentSequence = sequence;
        cinematic.CurrentStepIndex = 0;
        cinematic.StepElapsedTime = 0f;
        cinematic.IsPlaying = true;
        cinematic.OnSequenceComplete = onComplete;
    }
    
    /// <summary>
    /// Stop the currently playing sequence
    /// </summary>
    public static void StopSequence(World world, Entity cameraEntity)
    {
        var cinematic = world.GetComponent<CinematicCameraComponent>(cameraEntity);
        if (cinematic != null)
        {
            cinematic.IsPlaying = false;
            cinematic.CurrentSequence = null;
            cinematic.CurrentStepIndex = 0;
            cinematic.StepElapsedTime = 0f;
        }
    }
    
    /// <summary>
    /// Check if a cinematic sequence is currently playing
    /// </summary>
    public static bool IsPlaying(World world, Entity cameraEntity)
    {
        var cinematic = world.GetComponent<CinematicCameraComponent>(cameraEntity);
        return cinematic != null && cinematic.IsPlaying;
    }
    
    /// <summary>
    /// Create a simple pan sequence (move from current position to target)
    /// </summary>
    public static CinematicSequence CreatePanSequence(string name, float targetX, float targetY, 
                                                       float duration = 2.0f, EasingType easing = EasingType.EaseInOut)
    {
        var sequence = new CinematicSequence(name);
        sequence.Steps.Add(new CinematicStep(targetX, targetY, duration, easing));
        return sequence;
    }
    
    /// <summary>
    /// Create a zoom sequence (zoom to target level)
    /// </summary>
    public static CinematicSequence CreateZoomSequence(string name, float targetX, float targetY, 
                                                        float targetZoom, float duration = 2.0f, 
                                                        EasingType easing = EasingType.EaseInOut)
    {
        var sequence = new CinematicSequence(name);
        var step = new CinematicStep(targetX, targetY, duration, easing)
        {
            TargetZoom = targetZoom
        };
        sequence.Steps.Add(step);
        return sequence;
    }
    
    /// <summary>
    /// Create a dramatic reveal sequence (zoom out while panning)
    /// </summary>
    public static CinematicSequence CreateRevealSequence(string name, float startX, float startY, 
                                                          float endX, float endY, float startZoom = 2.0f, 
                                                          float endZoom = 1.0f, float duration = 3.0f)
    {
        var sequence = new CinematicSequence(name);
        
        // Zoom in and pan to start
        var step1 = new CinematicStep(startX, startY, duration * 0.3f, EasingType.EaseOut)
        {
            TargetZoom = startZoom
        };
        sequence.Steps.Add(step1);
        
        // Hold at start position
        var step2 = new CinematicStep(startX, startY, 0.01f, EasingType.Linear)
        {
            TargetZoom = startZoom,
            HoldDuration = 0.5f
        };
        sequence.Steps.Add(step2);
        
        // Zoom out and pan to end
        var step3 = new CinematicStep(endX, endY, duration * 0.7f, EasingType.EaseInOut)
        {
            TargetZoom = endZoom
        };
        sequence.Steps.Add(step3);
        
        return sequence;
    }
    
    /// <summary>
    /// Create a patrol sequence (camera moves through multiple waypoints)
    /// </summary>
    public static CinematicSequence CreatePatrolSequence(string name, List<(float x, float y)> waypoints, 
                                                          float durationPerWaypoint = 2.0f, 
                                                          float holdTime = 0.5f,
                                                          EasingType easing = EasingType.EaseInOut)
    {
        var sequence = new CinematicSequence(name);
        
        foreach (var (x, y) in waypoints)
        {
            var step = new CinematicStep(x, y, durationPerWaypoint, easing)
            {
                HoldDuration = holdTime
            };
            sequence.Steps.Add(step);
        }
        
        return sequence;
    }
    
    /// <summary>
    /// Create a dramatic shake sequence (camera shakes while at a position)
    /// </summary>
    public static CinematicSequence CreateShakeSequence(string name, float x, float y, 
                                                         float shakeIntensity = 20f, 
                                                         float duration = 1.0f)
    {
        var sequence = new CinematicSequence(name);
        var step = new CinematicStep(x, y, 0.01f, EasingType.Linear)
        {
            HoldDuration = duration,
            ScreenShake = new ScreenShakeConfig(shakeIntensity, duration)
        };
        sequence.Steps.Add(step);
        return sequence;
    }
}
