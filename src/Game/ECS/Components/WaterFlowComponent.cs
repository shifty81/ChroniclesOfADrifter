namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// Component that represents water flow direction and speed
/// Attached to water tiles to simulate water flow
/// </summary>
public class WaterFlowComponent : IComponent
{
    /// <summary>
    /// Flow direction in X axis (-1 = left, 0 = no flow, 1 = right)
    /// </summary>
    public float FlowX { get; set; }
    
    /// <summary>
    /// Flow direction in Y axis (-1 = up, 0 = no flow, 1 = down)
    /// </summary>
    public float FlowY { get; set; }
    
    /// <summary>
    /// Flow speed/strength (0.0 to 1.0)
    /// </summary>
    public float FlowStrength { get; set; }
    
    /// <summary>
    /// Type of water body this flow belongs to
    /// </summary>
    public WaterBodyType BodyType { get; set; }
    
    /// <summary>
    /// Pressure level - higher values push entities more
    /// </summary>
    public float Pressure { get; set; }
    
    public WaterFlowComponent(float flowX = 0, float flowY = 0, float flowStrength = 0.5f, 
                              WaterBodyType bodyType = WaterBodyType.Lake)
    {
        FlowX = flowX;
        FlowY = flowY;
        FlowStrength = flowStrength;
        BodyType = bodyType;
        Pressure = flowStrength;
    }
}

/// <summary>
/// Types of water bodies with different flow characteristics
/// </summary>
public enum WaterBodyType
{
    /// <summary>
    /// Still water with minimal flow
    /// </summary>
    Lake,
    
    /// <summary>
    /// Flowing water with directional current
    /// </summary>
    River,
    
    /// <summary>
    /// Large water body with tidal flow and waves
    /// </summary>
    Ocean,
    
    /// <summary>
    /// Underground water pocket
    /// </summary>
    UndergroundWater
}
