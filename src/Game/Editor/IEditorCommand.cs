namespace ChroniclesOfADrifter.Editor;

/// <summary>
/// Represents a reversible editor action.
/// All world mutations must be expressed as IEditorCommands and routed
/// through <see cref="EditorCommandStack"/> — never mutated directly.
/// </summary>
public interface IEditorCommand
{
    void Execute();
    void Undo();
}
