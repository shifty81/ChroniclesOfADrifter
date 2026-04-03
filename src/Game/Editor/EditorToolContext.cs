namespace ChroniclesOfADrifter.Editor;

/// <summary>
/// Abstract base for editor tools that receive input through
/// <see cref="EditorToolContext"/>.
/// </summary>
public interface IEditorTool
{
    /// <summary>Called every frame while this tool is active.</summary>
    void OnUpdate(float deltaTime);

    /// <summary>Called when the tool becomes the active tool.</summary>
    void OnActivate()   { }

    /// <summary>Called when another tool replaces this one.</summary>
    void OnDeactivate() { }
}

/// <summary>
/// Routes input to whichever <see cref="IEditorTool"/> is currently active.
/// Swap the active tool at any time with <see cref="SetActiveTool"/>.
/// </summary>
public class EditorToolContext
{
    private IEditorTool? _activeTool;

    public IEditorTool? ActiveTool => _activeTool;

    /// <summary>
    /// Sets <paramref name="tool"/> as the active editor tool, notifying
    /// both the outgoing and incoming tools.
    /// </summary>
    public void SetActiveTool(IEditorTool? tool)
    {
        _activeTool?.OnDeactivate();
        _activeTool = tool;
        _activeTool?.OnActivate();
    }

    /// <summary>Forwards the update tick to the currently active tool.</summary>
    public void Update(float deltaTime)
    {
        _activeTool?.OnUpdate(deltaTime);
    }
}
