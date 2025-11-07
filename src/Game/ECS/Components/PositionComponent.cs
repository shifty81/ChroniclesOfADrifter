namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// Position component - represents 2D position
/// </summary>
public class PositionComponent : IComponent
{
    public float X { get; set; }
    public float Y { get; set; }
    
    public PositionComponent(float x = 0, float y = 0)
    {
        X = x;
        Y = y;
    }
}
