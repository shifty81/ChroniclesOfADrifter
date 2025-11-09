using System;
using System.Collections.Generic;
using System.IO;
using Python.Runtime;

namespace ChroniclesOfADrifter.Scripting
{
    /// <summary>
    /// Python scripting engine for Chronicles of a Drifter
    /// Provides Python scripting support alongside Lua
    /// </summary>
    public class PythonScriptEngine : IDisposable
    {
        private bool _isInitialized = false;
        private readonly Dictionary<string, PyObject> _loadedScripts = new();
        private readonly Dictionary<string, dynamic> _scriptInstances = new();

        /// <summary>
        /// Initialize the Python scripting engine
        /// </summary>
        public bool Initialize()
        {
            if (_isInitialized)
                return true;

            try
            {
                // Initialize Python.NET
                Runtime.PythonDLL = GetPythonDll();
                PythonEngine.Initialize();
                PythonEngine.BeginAllowThreads();
                
                // Add scripts directory to Python path
                using (Py.GIL())
                {
                    dynamic sys = Py.Import("sys");
                    string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts", "python");
                    if (Directory.Exists(scriptPath))
                    {
                        sys.path.append(scriptPath);
                        Console.WriteLine($"[PythonEngine] Added to Python path: {scriptPath}");
                    }
                }

                _isInitialized = true;
                Console.WriteLine("[PythonEngine] Python scripting engine initialized");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PythonEngine] Failed to initialize: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get the Python DLL path based on the platform
        /// </summary>
        private string GetPythonDll()
        {
            // Try common Python installation locations on Windows
            var pythonVersions = new[] { "312", "311", "310", "39", "38" };
            
            foreach (var version in pythonVersions)
            {
                // Try system Python
                var systemPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), 
                    "..", "Python" + version, "python" + version + ".dll");
                if (File.Exists(systemPath))
                    return systemPath;

                // Try user Python
                var userPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Programs", "Python", "Python" + version, "python" + version + ".dll");
                if (File.Exists(userPath))
                    return userPath;
            }

            // Return default and let Python.NET try to find it
            return "python312.dll";
        }

        /// <summary>
        /// Load a Python script from file
        /// </summary>
        public bool LoadScript(string scriptName, string scriptPath)
        {
            if (!_isInitialized)
            {
                Console.WriteLine("[PythonEngine] Engine not initialized");
                return false;
            }

            try
            {
                using (Py.GIL())
                {
                    string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts", "python", scriptPath);
                    
                    if (!File.Exists(fullPath))
                    {
                        Console.WriteLine($"[PythonEngine] Script not found: {fullPath}");
                        return false;
                    }

                    // Import the module
                    string moduleName = Path.GetFileNameWithoutExtension(scriptPath).Replace("/", ".").Replace("\\", ".");
                    dynamic module = Py.Import(moduleName);
                    
                    _loadedScripts[scriptName] = module;
                    Console.WriteLine($"[PythonEngine] Loaded script: {scriptName} from {scriptPath}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PythonEngine] Failed to load script {scriptName}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Create an instance of a Python class from a loaded script
        /// </summary>
        public bool CreateInstance(string scriptName, string instanceName, string factoryFunction = "create_ai")
        {
            if (!_loadedScripts.ContainsKey(scriptName))
            {
                Console.WriteLine($"[PythonEngine] Script not loaded: {scriptName}");
                return false;
            }

            try
            {
                using (Py.GIL())
                {
                    dynamic module = _loadedScripts[scriptName];
                    dynamic factory = module.GetAttr(factoryFunction);
                    dynamic instance = factory();
                    
                    _scriptInstances[instanceName] = instance;
                    Console.WriteLine($"[PythonEngine] Created instance: {instanceName} from {scriptName}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PythonEngine] Failed to create instance {instanceName}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Call a method on a Python script instance
        /// </summary>
        public dynamic? CallMethod(string instanceName, string methodName, params object[] args)
        {
            if (!_scriptInstances.ContainsKey(instanceName))
            {
                Console.WriteLine($"[PythonEngine] Instance not found: {instanceName}");
                return null;
            }

            try
            {
                using (Py.GIL())
                {
                    dynamic instance = _scriptInstances[instanceName];
                    dynamic method = instance.GetAttr(methodName);
                    
                    // Convert C# args to Python objects
                    var pyArgs = new PyObject[args.Length];
                    for (int i = 0; i < args.Length; i++)
                    {
                        pyArgs[i] = args[i].ToPython();
                    }
                    
                    dynamic result = method.Invoke(pyArgs);
                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PythonEngine] Failed to call {methodName} on {instanceName}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Unload a script
        /// </summary>
        public void UnloadScript(string scriptName)
        {
            if (_loadedScripts.ContainsKey(scriptName))
            {
                _loadedScripts[scriptName].Dispose();
                _loadedScripts.Remove(scriptName);
                Console.WriteLine($"[PythonEngine] Unloaded script: {scriptName}");
            }
        }

        /// <summary>
        /// Remove a script instance
        /// </summary>
        public void RemoveInstance(string instanceName)
        {
            if (_scriptInstances.ContainsKey(instanceName))
            {
                _scriptInstances.Remove(instanceName);
                Console.WriteLine($"[PythonEngine] Removed instance: {instanceName}");
            }
        }

        /// <summary>
        /// Shutdown the Python engine
        /// </summary>
        public void Dispose()
        {
            if (!_isInitialized)
                return;

            try
            {
                // Clean up instances
                _scriptInstances.Clear();
                
                // Clean up loaded scripts
                foreach (var script in _loadedScripts.Values)
                {
                    script?.Dispose();
                }
                _loadedScripts.Clear();

                // Shutdown Python
                PythonEngine.Shutdown();
                _isInitialized = false;
                Console.WriteLine("[PythonEngine] Python scripting engine shutdown");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PythonEngine] Error during shutdown: {ex.Message}");
            }
        }
    }
}
