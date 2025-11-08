namespace ChroniclesOfADrifter;

/// <summary>
/// Game-wide constants for Chronicles of a Drifter.
/// These values are used throughout the game for consistency and scale reference.
/// </summary>
public static class GameConstants
{
    #region Player Character Dimensions
    
    /// <summary>
    /// The height of the player character in blocks.
    /// This is approximately 2.5 blocks tall, which serves as the scale reference 
    /// for all game elements, terrain features, and procedurally generated content.
    /// 
    /// Use this constant when generating or placing objects to ensure proper scale.
    /// For example:
    /// - Doors should be at least 3 blocks tall (to accommodate the player + headroom)
    /// - Trees can be 4-8 blocks tall for variety
    /// - Cave passages should be at least 3 blocks high
    /// </summary>
    public const float PlayerHeightInBlocks = 2.5f;
    
    /// <summary>
    /// The width of the player character in blocks.
    /// This represents the collision box width and is used for pathfinding and doorway sizing.
    /// </summary>
    public const float PlayerWidthInBlocks = 0.8f;
    
    /// <summary>
    /// The default collision box height for the player in pixels (at 1:1 scale).
    /// This should be slightly smaller than the sprite height for better game feel.
    /// Based on PlayerHeightInBlocks * BlockSize
    /// </summary>
    public const float PlayerCollisionHeight = 80f; // 2.5 blocks * 32 pixels/block
    
    /// <summary>
    /// The default collision box width for the player in pixels (at 1:1 scale).
    /// This should be narrower than the visual sprite for smoother navigation.
    /// Based on PlayerWidthInBlocks * BlockSize
    /// </summary>
    public const float PlayerCollisionWidth = 26f; // ~0.8 blocks * 32 pixels/block
    
    #endregion
    
    #region World/Block Dimensions
    
    /// <summary>
    /// The size of a single block in pixels (at 1:1 rendering scale).
    /// All world measurements are based on this block size.
    /// </summary>
    public const int BlockSize = 32;
    
    /// <summary>
    /// Chunk width in blocks.
    /// Matches the terrain generation system's chunk size.
    /// </summary>
    public const int ChunkWidth = 32;
    
    /// <summary>
    /// Chunk height in blocks.
    /// Matches the terrain generation system's chunk size.
    /// </summary>
    public const int ChunkHeight = 30;
    
    #endregion
    
    #region Scale Reference Guidelines
    
    /// <summary>
    /// Recommended minimum door height in blocks.
    /// Should accommodate player + headroom for comfortable passage.
    /// </summary>
    public const float MinDoorHeight = 3.0f;
    
    /// <summary>
    /// Recommended minimum cave/tunnel height in blocks.
    /// Should allow player to move through comfortably.
    /// </summary>
    public const float MinTunnelHeight = 3.0f;
    
    /// <summary>
    /// Recommended tree height range in blocks.
    /// Trees should be noticeably taller than the player for visual variety.
    /// </summary>
    public const float MinTreeHeight = 4.0f;
    public const float MaxTreeHeight = 8.0f;
    
    /// <summary>
    /// Recommended structure ceiling height in blocks.
    /// Buildings and indoor areas should feel spacious.
    /// </summary>
    public const float StructureCeilingHeight = 4.0f;
    
    #endregion
    
    #region Movement & Physics
    
    /// <summary>
    /// Default player movement speed in pixels per second.
    /// Can be overridden per-entity via PlayerComponent.
    /// </summary>
    public const float DefaultPlayerSpeed = 100.0f;
    
    /// <summary>
    /// Player jump height in blocks (if jumping is implemented).
    /// This helps determine vertical clearance needed for platforming.
    /// </summary>
    public const float PlayerJumpHeight = 1.5f;
    
    #endregion
}
