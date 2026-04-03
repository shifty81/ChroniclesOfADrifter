using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.Terrain;
using ChroniclesOfADrifter.WorldManagement;

namespace ChroniclesOfADrifter.Editor;

/// <summary>
/// Reversible command that sets a single tile in a chunk.
/// </summary>
public class SetTileCommand : IEditorCommand
{
    private readonly ChunkManager _chunkManager;
    private readonly int _tileX;
    private readonly int _tileY;
    private readonly TileType _newType;
    private TileType _previousType;

    public SetTileCommand(ChunkManager chunkManager, int tileX, int tileY, TileType newType)
    {
        _chunkManager = chunkManager;
        _tileX        = tileX;
        _tileY        = tileY;
        _newType      = newType;
    }

    public void Execute()
    {
        _previousType = ReadTile();
        WriteTile(_newType);
    }

    public void Undo()
    {
        WriteTile(_previousType);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private TileType ReadTile()
    {
        int chunkX = GridCoordUtility.TileToChunkX(_tileX);
        int localX = GridCoordUtility.TileToLocalX(_tileX);
        var chunk  = _chunkManager.GetChunk(chunkX);
        return chunk != null && GridCoordUtility.IsValidLocal(localX, _tileY)
            ? chunk.GetTile(localX, _tileY)
            : TileType.Air;
    }

    private void WriteTile(TileType type)
    {
        int chunkX = GridCoordUtility.TileToChunkX(_tileX);
        int localX = GridCoordUtility.TileToLocalX(_tileX);
        var chunk  = _chunkManager.GetChunk(chunkX);
        if (chunk != null && GridCoordUtility.IsValidLocal(localX, _tileY))
        {
            chunk.SetTile(localX, _tileY, type);
        }
    }
}
