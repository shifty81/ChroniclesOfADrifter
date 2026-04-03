using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.ECS.Systems;
using ChroniclesOfADrifter.Editor;
using ChroniclesOfADrifter.Engine;
using ChroniclesOfADrifter.Terrain;
using ChroniclesOfADrifter.Rendering;
using ChroniclesOfADrifter.WorldManagement;

namespace ChroniclesOfADrifter.Scenes;

/// <summary>
/// The single authoritative scene for Chronicles of a Drifter.
///
/// Architecture rules enforced here:
///   - One world authority (this scene owns the ChunkManager).
///   - One grid system (<see cref="GridCoordUtility"/>).
///   - All tile edits go through <see cref="EditorCommandStack"/>.
///   - No direct world mutation outside commands.
///   - The editor is the source of truth for content.
///
/// Startup order:
///   1. World load / terrain generation.
///   2. <see cref="EditorController"/> initialization.
///   3. Input routed to <see cref="EditorToolContext"/> each frame.
/// </summary>
public class DevWorldScene : Scene
{
    // ── Key codes ────────────────────────────────────────────────────────────
    private const int KEY_W     = 119;
    private const int KEY_A     = 97;
    private const int KEY_S     = 115;
    private const int KEY_D     = 100;
    private const int KEY_UP    = 1073741906;
    private const int KEY_DOWN  = 1073741905;
    private const int KEY_LEFT  = 1073741904;
    private const int KEY_RIGHT = 1073741903;
    private const int KEY_F1    = 1073741882;

    // ── Core subsystems ──────────────────────────────────────────────────────
    private Entity          _cameraEntity;
    private ChunkManager?   _chunkManager;
    private TerrainGenerator? _terrainGenerator;
    private EditorController? _editorController;

    private float _cameraMoveSpeed = 400.0f;
    private bool  _editorVisible   = true;

    // ── Scene lifecycle ──────────────────────────────────────────────────────

    public override void OnLoad()
    {
        Console.WriteLine();
        Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║          CHRONICLES OF A DRIFTER — DEV WORLD            ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        // 1. Terrain / chunk system.
        _terrainGenerator = new TerrainGenerator(seed: 42);
        _chunkManager     = new ChunkManager();
        _chunkManager.SetTerrainGenerator(_terrainGenerator);

        // Expose chunkManager as a shared resource (read-only by convention).
        World.SetSharedResource("ChunkManager", _chunkManager);

        // 2. ECS systems.
        World.AddSystem(new CameraInputSystem());
        World.AddSystem(new CameraSystem());
        World.AddSystem(new TerrainRenderingSystem());

        // 3. Camera entity.
        _cameraEntity = World.CreateEntity();
        World.AddComponent(_cameraEntity, new CameraComponent(1280, 720) { Zoom = 1.0f });
        World.AddComponent(_cameraEntity, new PositionComponent(640, 360));

        // 4. Pre-generate chunks around the spawn point.
        _chunkManager.UpdateChunks(640);
        Console.WriteLine($"[DevWorld] Loaded {_chunkManager.GetLoadedChunkCount()} chunks.");

        // 5. Editor controller — wired after the world is ready.
        _editorController = new EditorController(
            world:        World,
            cameraEntity: _cameraEntity,
            chunkManager: _chunkManager,
            saveDirectory: "assets/worlds");

        Console.WriteLine("[DevWorld] Ready.  F1 = toggle editor HUD");
    }

    public override void OnUnload()
    {
        Console.WriteLine("[DevWorld] Unloading...");
        _chunkManager?.Dispose();
    }

    // ── Per-frame update ─────────────────────────────────────────────────────

    public override void Update(float deltaTime)
    {
        HandleCameraMovement(deltaTime);
        UpdateChunks();

        // Route input to the editor tool context.
        if (_editorVisible)
            _editorController?.Update(deltaTime);

        // Toggle editor overlay.
        if (EngineInterop.Input_IsKeyPressed(KEY_F1))
        {
            _editorVisible = !_editorVisible;
            Console.WriteLine($"[DevWorld] Editor: {(_editorVisible ? "ON" : "OFF")}");
        }

        // ECS world tick (systems).
        World.Update(deltaTime);

        // Status line.
        Console.Write($"\r[DevWorld] Cam:{GetCameraPos()}  Tile:{GetSelectedTileName()}" +
                      $"  Erase:{GetEraseMode()}  Undo:{GetUndoCount()}  Editor:{_editorVisible}     ");
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private void HandleCameraMovement(float deltaTime)
    {
        var pos = World.GetComponent<PositionComponent>(_cameraEntity);
        if (pos == null) return;

        float d = _cameraMoveSpeed * deltaTime;

        if (EngineInterop.Input_IsKeyDown(KEY_W) || EngineInterop.Input_IsKeyDown(KEY_UP))    pos.Y -= d;
        if (EngineInterop.Input_IsKeyDown(KEY_S) || EngineInterop.Input_IsKeyDown(KEY_DOWN))  pos.Y += d;
        if (EngineInterop.Input_IsKeyDown(KEY_A) || EngineInterop.Input_IsKeyDown(KEY_LEFT))  pos.X -= d;
        if (EngineInterop.Input_IsKeyDown(KEY_D) || EngineInterop.Input_IsKeyDown(KEY_RIGHT)) pos.X += d;
    }

    private void UpdateChunks()
    {
        if (_chunkManager == null) return;
        var pos = World.GetComponent<PositionComponent>(_cameraEntity);
        if (pos != null) _chunkManager.UpdateChunks(pos.X);
    }

    private string GetCameraPos()
    {
        var pos = World.GetComponent<PositionComponent>(_cameraEntity);
        return pos != null ? $"({pos.X:F0},{pos.Y:F0})" : "(?,?)";
    }

    private string GetSelectedTileName()
        => _editorController?.PaintTool.SelectedTile.ToString() ?? "—";

    private string GetEraseMode()
        => _editorController?.PaintTool.EraseMode.ToString() ?? "—";

    private string GetUndoCount()
        => _editorController?.CommandStack.UndoCount.ToString() ?? "0";
}
