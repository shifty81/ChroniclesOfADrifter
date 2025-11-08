using NLua;
using ChroniclesOfADrifter.ECS;

namespace ChroniclesOfADrifter.Scripting;

/// <summary>
/// Manages Lua script execution and provides API for scripts
/// </summary>
public class ScriptEngine : IDisposable
{
    private readonly Lua _lua;
    private readonly World _world;
    private bool _disposed;
    
    public ScriptEngine(World world)
    {
        _world = world;
        _lua = new Lua();
        _lua.State.Encoding = System.Text.Encoding.UTF8;
        
        RegisterEngineAPI();
    }
    
    /// <summary>
    /// Register engine functions that Lua scripts can call
    /// </summary>
    private void RegisterEngineAPI()
    {
        // Register logging functions
        _lua.RegisterFunction("print", this, GetType().GetMethod(nameof(LuaPrint)));
        _lua.RegisterFunction("log", this, GetType().GetMethod(nameof(LuaLog)));
        
        // Make world available to scripts
        _lua["world"] = _world;
    }
    
    /// <summary>
    /// Load and execute a Lua script file
    /// </summary>
    public object? LoadScript(string filePath)
    {
        try
        {
            // Get the directory where the executable is running
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string fullPath = Path.Combine(basePath, filePath);
            
            if (!File.Exists(fullPath))
            {
                Console.WriteLine($"[ScriptEngine] ERROR: Script file not found: {fullPath}");
                return null;
            }
            
            Console.WriteLine($"[ScriptEngine] Loading script: {fullPath}");
            return _lua.DoFile(fullPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ScriptEngine] ERROR loading script {filePath}: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Execute a Lua script string
    /// </summary>
    public object? ExecuteScript(string script)
    {
        try
        {
            return _lua.DoString(script);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ScriptEngine] ERROR executing script: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Get a Lua function
    /// </summary>
    public LuaFunction? GetFunction(string name)
    {
        return _lua[name] as LuaFunction;
    }
    
    /// <summary>
    /// Get a Lua table
    /// </summary>
    public LuaTable? GetTable(string name)
    {
        return _lua[name] as LuaTable;
    }
    
    /// <summary>
    /// Set a global variable in Lua
    /// </summary>
    public void SetGlobal(string name, object value)
    {
        _lua[name] = value;
    }
    
    /// <summary>
    /// Get a global variable from Lua
    /// </summary>
    public object? GetGlobal(string name)
    {
        return _lua[name];
    }
    
    // ===== Lua API Functions =====
    
    public void LuaPrint(string message)
    {
        Console.WriteLine($"[Lua] {message}");
    }
    
    public void LuaLog(string message)
    {
        Console.WriteLine($"[Lua] {message}");
    }
    
    public void Dispose()
    {
        if (!_disposed)
        {
            _lua?.Dispose();
            _disposed = true;
        }
    }
}
