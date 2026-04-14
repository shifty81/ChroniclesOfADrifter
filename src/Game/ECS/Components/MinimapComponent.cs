namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// Represents a tile entry in the minimap (explored or visible)
/// </summary>
public class MinimapTile
{
    public int WorldX { get; set; }
    public int WorldY { get; set; }
    public MinimapTileType TileType { get; set; }
    public bool IsVisible { get; set; }

    public MinimapTile(int x, int y, MinimapTileType type, bool visible = false)
    {
        WorldX = x;
        WorldY = y;
        TileType = type;
        IsVisible = visible;
    }
}

/// <summary>
/// Broad category of terrain shown on the minimap
/// </summary>
public enum MinimapTileType
{
    Unknown,
    Grass,
    Water,
    Mountain,
    Desert,
    Snow,
    Forest,
    Dungeon,
    Village,
    Player,
    Enemy,
    Boss,
    Chest
}

/// <summary>
/// Component that stores minimap data for an entity (typically the player/world entity)
/// </summary>
public class MinimapComponent : IComponent
{
    /// <summary>Explored cells keyed by (chunkX, chunkY)</summary>
    public HashSet<(int, int)> ExploredChunks { get; private set; }

    /// <summary>POI markers visible on the minimap</summary>
    public List<MinimapTile> POIMarkers { get; private set; }

    /// <summary>Current visible radius in world units (chunks)</summary>
    public int VisibilityRadius { get; set; }

    /// <summary>Display size in pixels for the HUD minimap</summary>
    public int DisplaySize { get; set; }

    public float PlayerWorldX { get; set; }
    public float PlayerWorldY { get; set; }

    public bool IsVisible { get; set; }

    public MinimapComponent(int visibilityRadius = 5, int displaySize = 128)
    {
        ExploredChunks = new HashSet<(int, int)>();
        POIMarkers = new List<MinimapTile>();
        VisibilityRadius = visibilityRadius;
        DisplaySize = displaySize;
        IsVisible = true;
    }

    /// <summary>
    /// Mark a chunk as explored and return true if it was newly discovered
    /// </summary>
    public bool ExploreChunk(int chunkX, int chunkY)
    {
        return ExploredChunks.Add((chunkX, chunkY));
    }

    public bool IsChunkExplored(int chunkX, int chunkY)
    {
        return ExploredChunks.Contains((chunkX, chunkY));
    }

    public void AddPOI(int worldX, int worldY, MinimapTileType type)
    {
        POIMarkers.Add(new MinimapTile(worldX, worldY, type));
    }

    public void RemovePOI(int worldX, int worldY)
    {
        POIMarkers.RemoveAll(p => p.WorldX == worldX && p.WorldY == worldY);
    }

    public void UpdatePlayerPosition(float x, float y)
    {
        PlayerWorldX = x;
        PlayerWorldY = y;
    }
}
