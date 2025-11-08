using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.Scripting;
using NLua;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// Script system - executes Lua scripts for entities
/// </summary>
public class ScriptSystem : ISystem
{
    private ScriptEngine? _scriptEngine;
    private readonly Dictionary<int, LuaFunction?> _updateFunctions = new();
    private readonly HashSet<int> _loadedScripts = new();
    
    public void Initialize(World world)
    {
        Console.WriteLine("[ScriptSystem] Initializing script system...");
        _scriptEngine = new ScriptEngine(world);
    }
    
    public void Update(World world, float deltaTime)
    {
        if (_scriptEngine == null) return;
        
        // Load scripts for any new entities
        foreach (var entity in world.GetEntitiesWithComponent<ScriptComponent>())
        {
            if (!_loadedScripts.Contains(entity.Id))
            {
                LoadEntityScript(world, entity);
                _loadedScripts.Add(entity.Id);
            }
        }
        
        // Find player position for AI
        PositionComponent? playerPosition = null;
        foreach (var playerEntity in world.GetEntitiesWithComponent<PlayerComponent>())
        {
            playerPosition = world.GetComponent<PositionComponent>(playerEntity);
            break;
        }
        
        // Update all scripted entities
        foreach (var entity in world.GetEntitiesWithComponent<ScriptComponent>())
        {
            if (_updateFunctions.TryGetValue(entity.Id, out var updateFunc) && updateFunc != null)
            {
                try
                {
                    var position = world.GetComponent<PositionComponent>(entity);
                    var velocity = world.GetComponent<VelocityComponent>(entity);
                    
                    // Calculate distance to player
                    float? playerDistance = null;
                    if (playerPosition != null && position != null)
                    {
                        float dx = playerPosition.X - position.X;
                        float dy = playerPosition.Y - position.Y;
                        playerDistance = MathF.Sqrt(dx * dx + dy * dy);
                    }
                    
                    updateFunc.Call(entity.Id, deltaTime, position, velocity, playerPosition, playerDistance);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ScriptSystem] Error updating entity {entity.Id}: {ex.Message}");
                }
            }
        }
    }
    
    private void LoadEntityScript(World world, Entity entity)
    {
        if (_scriptEngine == null) return;
        
        var scriptComp = world.GetComponent<ScriptComponent>(entity);
        if (scriptComp == null) return;
        
        Console.WriteLine($"[ScriptSystem] Loading script for entity {entity.Id}: {scriptComp.ScriptPath}");
        
        // Load the script
        var result = _scriptEngine.LoadScript(scriptComp.ScriptPath);
        Console.WriteLine($"[ScriptSystem] Script result type: {result?.GetType()?.Name ?? "null"}");
        
        // DoFile returns an array of return values
        LuaTable? scriptTable = null;
        if (result is object[] results && results.Length > 0 && results[0] is LuaTable table)
        {
            scriptTable = table;
        }
        
        if (scriptTable != null)
        {
            Console.WriteLine($"[ScriptSystem] Script loaded successfully for entity {entity.Id}");
            scriptComp.ScriptData = scriptTable;
            
            // Get the OnUpdate function if it exists
            if (scriptTable["OnUpdate"] is LuaFunction updateFunc)
            {
                _updateFunctions[entity.Id] = updateFunc;
                Console.WriteLine($"[ScriptSystem] Found OnUpdate function for entity {entity.Id}");
            }
            
            // Call OnSpawn if it exists
            if (scriptTable["OnSpawn"] is LuaFunction spawnFunc)
            {
                try
                {
                    var position = world.GetComponent<PositionComponent>(entity);
                    spawnFunc.Call(entity.Id, position);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ScriptSystem] Error spawning entity {entity.Id}: {ex.Message}");
                }
            }
        }
        else
        {
            Console.WriteLine($"[ScriptSystem] Failed to load script for entity {entity.Id}");
        }
    }
}
