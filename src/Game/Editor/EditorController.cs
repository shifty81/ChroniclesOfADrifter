using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.Engine;
using ChroniclesOfADrifter.Terrain;
using ChroniclesOfADrifter.WorldManagement;

namespace ChroniclesOfADrifter.Editor;

/// <summary>
/// Orchestrates all editor subsystems:
///   - boots after the world is loaded
///   - owns the <see cref="EditorCommandStack"/> (undo/redo)
///   - owns the <see cref="EditorToolContext"/> (active tool)
///   - owns the <see cref="VoxelPaintTool"/>
///   - handles save/load via <see cref="WorldFileService"/>
///   - receives input every frame through <see cref="Update"/>
/// </summary>
public class EditorController
{
    // ── Key codes ────────────────────────────────────────────────────────────
    private const int KEY_Z             = 122;   // Z → undo
    private const int KEY_Y             = 121;   // Y → redo
    private const int KEY_S             = 115;   // S → save
    private const int KEY_L             = 108;   // L → load
    private const int KEY_E             = 101;   // E → toggle erase mode
    private const int KEY_SPACE         = 32;    // Space → paint at camera pos
    private const int KEY_LEFT_BRACKET  = 91;    // [ → prev tile
    private const int KEY_RIGHT_BRACKET = 93;    // ] → next tile

    // ── State ────────────────────────────────────────────────────────────────
    private readonly ChunkManager       _chunkManager;
    private readonly WorldFileService   _fileService;
    private readonly EditorCommandStack _commandStack;
    private readonly EditorToolContext  _toolContext;
    private readonly VoxelPaintTool     _paintTool;
    private readonly ECS.World          _world;
    private readonly Entity             _cameraEntity;

    private readonly List<TileType> _paintableTiles;
    private int _tileIndex = 0;

    public bool IsEnabled { get; private set; } = true;

    public EditorController(
        ECS.World world,
        Entity    cameraEntity,
        ChunkManager chunkManager,
        string saveDirectory = "assets/worlds")
    {
        _world        = world;
        _cameraEntity = cameraEntity;
        _chunkManager = chunkManager;
        _fileService  = new WorldFileService(chunkManager);
        _commandStack = new EditorCommandStack();
        _toolContext  = new EditorToolContext();
        _paintTool    = new VoxelPaintTool(chunkManager, _commandStack);

        // Build the list of tiles the editor can paint.
        _paintableTiles = new List<TileType>
        {
            TileType.Grass, TileType.Dirt, TileType.Stone, TileType.Sand,
            TileType.Snow, TileType.Water, TileType.Wood, TileType.WoodPlank,
            TileType.Cobblestone, TileType.Brick, TileType.CoalOre,
            TileType.IronOre, TileType.GoldOre, TileType.Torch
        };

        // Activate the paint tool by default.
        _toolContext.SetActiveTool(new PaintToolAdapter(_paintTool, world, cameraEntity));

        _saveDirectory = saveDirectory;
        Directory.CreateDirectory(saveDirectory);

        Console.WriteLine("[EditorController] Initialized — undo/redo active, paint tool ready.");
        PrintHelp();
    }

    private readonly string _saveDirectory;

    // ── Per-frame entry point ────────────────────────────────────────────────

    public void Update(float deltaTime)
    {
        if (!IsEnabled) return;

        HandleTileSelection();
        HandleUndoRedo();
        HandleSaveLoad();
        HandleEraseToggle();

        // Forward to the active tool.
        _toolContext.Update(deltaTime);
    }

    // ── Input handlers ───────────────────────────────────────────────────────

    private void HandleTileSelection()
    {
        if (EngineInterop.Input_IsKeyPressed(KEY_LEFT_BRACKET))
        {
            _tileIndex = (_tileIndex - 1 + _paintableTiles.Count) % _paintableTiles.Count;
            _paintTool.SelectedTile = _paintableTiles[_tileIndex];
            Console.WriteLine($"[Editor] Selected tile: {_paintTool.SelectedTile}");
        }
        if (EngineInterop.Input_IsKeyPressed(KEY_RIGHT_BRACKET))
        {
            _tileIndex = (_tileIndex + 1) % _paintableTiles.Count;
            _paintTool.SelectedTile = _paintableTiles[_tileIndex];
            Console.WriteLine($"[Editor] Selected tile: {_paintTool.SelectedTile}");
        }
    }

    private void HandleUndoRedo()
    {
        if (EngineInterop.Input_IsKeyPressed(KEY_Z))
        {
            if (_commandStack.Undo())
                Console.WriteLine($"[Editor] Undo  (stack: {_commandStack.UndoCount})");
        }
        if (EngineInterop.Input_IsKeyPressed(KEY_Y))
        {
            if (_commandStack.Redo())
                Console.WriteLine($"[Editor] Redo  (stack: {_commandStack.UndoCount})");
        }
    }

    private void HandleSaveLoad()
    {
        if (EngineInterop.Input_IsKeyPressed(KEY_S))
        {
            string path = Path.Combine(_saveDirectory, $"world_{DateTime.Now:yyyyMMdd_HHmmss_fff}.json");
            _fileService.Save(path);
        }
        if (EngineInterop.Input_IsKeyPressed(KEY_L))
        {
            var files = Directory.GetFiles(_saveDirectory, "*.json");
            if (files.Length > 0)
            {
                string latest = files.OrderByDescending(File.GetLastWriteTime).First();
                _fileService.Load(latest);
            }
            else
            {
                Console.WriteLine("[Editor] No saved worlds found.");
            }
        }
    }

    private void HandleEraseToggle()
    {
        if (EngineInterop.Input_IsKeyPressed(KEY_E))
        {
            _paintTool.EraseMode = !_paintTool.EraseMode;
            Console.WriteLine($"[Editor] Erase mode: {_paintTool.EraseMode}");
        }
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public EditorCommandStack CommandStack => _commandStack;
    public EditorToolContext  ToolContext   => _toolContext;
    public VoxelPaintTool     PaintTool     => _paintTool;

    // ── Help ──────────────────────────────────────────────────────────────────

    private static void PrintHelp()
    {
        Console.WriteLine("─── Editor Controls ─────────────────────────────────────");
        Console.WriteLine("  Space       Paint tile at camera position");
        Console.WriteLine("  [ / ]       Previous / next tile");
        Console.WriteLine("  E           Toggle erase mode");
        Console.WriteLine("  Z           Undo");
        Console.WriteLine("  Y           Redo");
        Console.WriteLine("  S           Save world");
        Console.WriteLine("  L           Load latest world");
        Console.WriteLine("─────────────────────────────────────────────────────────");
    }

    // ── Inner adapter ─────────────────────────────────────────────────────────

    /// <summary>
    /// Wraps <see cref="VoxelPaintTool"/> as an <see cref="IEditorTool"/> so it
    /// can be managed by <see cref="EditorToolContext"/>.
    /// </summary>
    private sealed class PaintToolAdapter : IEditorTool
    {
        private const int KEY_SPACE = 32;

        private readonly VoxelPaintTool _tool;
        private readonly ECS.World      _world;
        private readonly Entity         _camera;

        public PaintToolAdapter(VoxelPaintTool tool, ECS.World world, Entity camera)
        {
            _tool   = tool;
            _world  = world;
            _camera = camera;
        }

        public void OnUpdate(float deltaTime)
        {
            if (!EngineInterop.Input_IsKeyDown(KEY_SPACE)) return;

            var pos = _world.GetComponent<PositionComponent>(_camera);
            if (pos == null) return;

            _tool.Paint(pos.X, pos.Y);
        }
    }
}
