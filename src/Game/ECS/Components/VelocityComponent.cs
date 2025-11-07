namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// Velocity component - represents 2D velocity
/// </summary>
public class VelocityComponent : IComponent
{
    public float VX { get; set; }
    public float VY { get; set; }
    
    public VelocityComponent(float vx = 0, float vy = 0)
    {
        VX = vx;
        VY = vy;
    }
}
