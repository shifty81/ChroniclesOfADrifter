namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// Animated sprite component - defines sprite sheet layout and animation frames
/// </summary>
public class AnimatedSpriteComponent : IComponent
{
    /// <summary>
    /// Texture ID for the sprite sheet
    /// </summary>
    public int TextureId { get; set; }
    
    /// <summary>
    /// Width of the sprite in pixels
    /// </summary>
    public float Width { get; set; }
    
    /// <summary>
    /// Height of the sprite in pixels
    /// </summary>
    public float Height { get; set; }
    
    /// <summary>
    /// Rotation angle in degrees
    /// </summary>
    public float Rotation { get; set; }
    
    /// <summary>
    /// Dictionary of animation definitions (animation name -> frame data)
    /// </summary>
    public Dictionary<string, AnimationDefinition> Animations { get; set; }
    
    /// <summary>
    /// Scale factor for high-resolution sprites
    /// </summary>
    public float Scale { get; set; }
    
    /// <summary>
    /// Whether to flip the sprite horizontally
    /// </summary>
    public bool FlipHorizontal { get; set; }
    
    /// <summary>
    /// Whether to flip the sprite vertically
    /// </summary>
    public bool FlipVertical { get; set; }
    
    public AnimatedSpriteComponent(int textureId, float width, float height)
    {
        TextureId = textureId;
        Width = width;
        Height = height;
        Rotation = 0f;
        Animations = new Dictionary<string, AnimationDefinition>();
        Scale = 1.0f;
        FlipHorizontal = false;
        FlipVertical = false;
    }
}

/// <summary>
/// Defines a single animation sequence
/// </summary>
public class AnimationDefinition
{
    /// <summary>
    /// List of frame indices in the sprite sheet
    /// </summary>
    public int[] FrameIndices { get; set; }
    
    /// <summary>
    /// Number of frames per row in the sprite sheet
    /// </summary>
    public int FramesPerRow { get; set; }
    
    /// <summary>
    /// Total frame count in animation
    /// </summary>
    public int FrameCount => FrameIndices?.Length ?? 0;
    
    /// <summary>
    /// Frame width in the sprite sheet
    /// </summary>
    public int FrameWidth { get; set; }
    
    /// <summary>
    /// Frame height in the sprite sheet
    /// </summary>
    public int FrameHeight { get; set; }
    
    public AnimationDefinition(int[] frameIndices, int framesPerRow, int frameWidth, int frameHeight)
    {
        FrameIndices = frameIndices;
        FramesPerRow = framesPerRow;
        FrameWidth = frameWidth;
        FrameHeight = frameHeight;
    }
}
