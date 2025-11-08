namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// Script component - associates a Lua script with an entity
/// </summary>
public class ScriptComponent : IComponent
{
    public string ScriptPath { get; set; }
    public object? ScriptData { get; set; }
    
    public ScriptComponent(string scriptPath)
    {
        ScriptPath = scriptPath;
    }
}
