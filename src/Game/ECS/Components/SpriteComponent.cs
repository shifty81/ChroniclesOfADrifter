namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// Sprite component - represents visual appearance
/// </summary>
public class SpriteComponent : IComponent
{
    public int TextureId { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    public float Rotation { get; set; }
    
    public SpriteComponent(int textureId, float width, float height, float rotation = 0)
    {
        TextureId = textureId;
        Width = width;
        Height = height;
        Rotation = rotation;
    }
}
