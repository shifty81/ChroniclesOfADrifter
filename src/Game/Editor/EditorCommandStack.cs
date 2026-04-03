namespace ChroniclesOfADrifter.Editor;

/// <summary>
/// Undo / redo stack for all editor mutations.
/// Only <see cref="Execute"/> should be used to apply commands; never mutate
/// the world directly from editor code.
/// </summary>
public class EditorCommandStack
{
    private readonly Stack<IEditorCommand> _undoStack = new();
    private readonly Stack<IEditorCommand> _redoStack = new();

    /// <summary>
    /// Maximum number of steps stored in the undo history (0 = unlimited).
    /// </summary>
    public int MaxHistory { get; set; } = 200;

    /// <summary>Executes <paramref name="command"/> and pushes it onto the undo stack.</summary>
    public void Execute(IEditorCommand command)
    {
        command.Execute();
        _undoStack.Push(command);

        // Trim history if a limit is set.
        if (MaxHistory > 0 && _undoStack.Count > MaxHistory)
        {
            // Stack doesn't expose a direct "trim bottom" API, so rebuild.
            var items = _undoStack.ToArray();  // top-first
            _undoStack.Clear();
            for (int i = MaxHistory - 1; i >= 0; i--)
                _undoStack.Push(items[i]);
        }

        // Any new command invalidates the redo branch.
        _redoStack.Clear();
    }

    /// <summary>Undoes the most recent command, if any.</summary>
    public bool Undo()
    {
        if (_undoStack.Count == 0) return false;
        var cmd = _undoStack.Pop();
        cmd.Undo();
        _redoStack.Push(cmd);
        return true;
    }

    /// <summary>Re-executes the most recently undone command, if any.</summary>
    public bool Redo()
    {
        if (_redoStack.Count == 0) return false;
        var cmd = _redoStack.Pop();
        cmd.Execute();
        _undoStack.Push(cmd);
        return true;
    }

    /// <summary>Clears both undo and redo history.</summary>
    public void Clear()
    {
        _undoStack.Clear();
        _redoStack.Clear();
    }

    public int UndoCount => _undoStack.Count;
    public int RedoCount => _redoStack.Count;
}
