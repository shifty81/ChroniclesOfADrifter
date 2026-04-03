using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.Terrain;
using ChroniclesOfADrifter.WorldManagement;

namespace ChroniclesOfADrifter.Editor;

/// <summary>
/// Paints (or erases) a single voxel/tile at a given world position.
/// Routes every change through <see cref="EditorCommandStack"/> so that
/// all mutations are undoable.
/// </summary>
public class VoxelPaintTool
{
    private readonly ChunkManager       _chunkManager;
    private readonly EditorCommandStack _commandStack;

    public TileType  SelectedTile { get; set; } = TileType.Grass;
    public bool      EraseMode    { get; set; } = false;

    public VoxelPaintTool(ChunkManager chunkManager, EditorCommandStack commandStack)
    {
        _chunkManager = chunkManager;
        _commandStack = commandStack;
    }

    /// <summary>
    /// Paint the tile at the given world-pixel coordinates using the current
    /// <see cref="SelectedTile"/> (or <see cref="TileType.Air"/> when
    /// <see cref="EraseMode"/> is true).
    /// </summary>
    public void Paint(float worldX, float worldY)
    {
        int tileX = GridCoordUtility.WorldToTileX(worldX);
        int tileY = GridCoordUtility.WorldToTileY(worldY);
        var type  = EraseMode ? TileType.Air : SelectedTile;
        _commandStack.Execute(new SetTileCommand(_chunkManager, tileX, tileY, type));
    }

    /// <summary>
    /// Paint the tile at explicit tile coordinates.
    /// </summary>
    public void PaintTile(int tileX, int tileY)
    {
        var type = EraseMode ? TileType.Air : SelectedTile;
        _commandStack.Execute(new SetTileCommand(_chunkManager, tileX, tileY, type));
    }
}
