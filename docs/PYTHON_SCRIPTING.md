# Python Scripting Guide

Chronicles of a Drifter supports Python scripting alongside Lua for gameplay logic, AI behaviors, and dynamic content.

## Overview

Python scripting is integrated via **Python.NET (pythonnet)**, allowing seamless interop between C# game code and Python scripts. This provides an alternative to Lua for developers who prefer Python's syntax and ecosystem.

## Requirements

- **Python 3.8+** installed on Windows
- **Python.NET (Python.Runtime)** NuGet package (included in project)
- Python scripts located in `scripts/python/` directory

## Architecture

The Python scripting system consists of:

1. **PythonScriptEngine** - C# class managing Python runtime
2. **Python Scripts** - `.py` files in `scripts/python/` directory
3. **Script Instances** - Runtime instances of Python classes

## Python Script Structure

### Basic Script Template

```python
"""
My Custom Script
Chronicles of a Drifter
"""

class MyScript:
    """Script class with custom behavior"""
    
    def __init__(self):
        self.state = "idle"
        self.timer = 0.0
    
    def update(self, entity, delta_time):
        """
        Update the script
        
        Args:
            entity: The entity being controlled
            delta_time: Time elapsed since last update
        
        Returns:
            Dictionary with state updates
        """
        self.timer += delta_time
        
        return {
            'state': self.state,
            'value': self.timer
        }

# Factory function for creating script instances
def create_ai():
    """Create a new script instance"""
    return MyScript()
```

### Key Components

1. **Class Definition** - Main script logic in a Python class
2. **`__init__` Method** - Initialize script state
3. **`update` Method** - Called each frame with entity and delta time
4. **Factory Function** - Creates new instances (typically named `create_ai`)

## Example: Goblin AI Script

See `scripts/python/enemies/goblin_ai.py` for a complete example:

```python
class GoblinAI:
    """Simple Goblin AI behavior"""
    
    def __init__(self):
        self.state = "patrol"
        self.patrol_timer = 0.0
        self.chase_distance = 5.0
        self.attack_distance = 1.5
    
    def update(self, entity, player_pos, delta_time):
        # Calculate distance to player
        dx = player_pos[0] - entity.position[0]
        dy = player_pos[1] - entity.position[1]
        distance = (dx * dx + dy * dy) ** 0.5
        
        # State machine logic
        if self.state == "patrol":
            # Patrol behavior
            # ...
        elif self.state == "chase":
            # Chase behavior
            # ...
        elif self.state == "attack":
            # Attack behavior
            # ...
        
        return {
            'state': self.state,
            'velocity': (vx, vy),
            'should_attack': False
        }

def create_ai():
    return GoblinAI()
```

## Using Python Scripts in C#

### 1. Initialize the Python Engine

```csharp
using ChroniclesOfADrifter.Scripting;

var pythonEngine = new PythonScriptEngine();
if (!pythonEngine.Initialize())
{
    Console.WriteLine("Failed to initialize Python engine");
}
```

### 2. Load a Python Script

```csharp
// Load a script from the scripts/python directory
pythonEngine.LoadScript("goblin_ai", "enemies/goblin_ai.py");
```

### 3. Create Script Instance

```csharp
// Create an instance using the factory function
pythonEngine.CreateInstance("goblin_ai", "goblin_instance_1", "create_ai");
```

### 4. Call Script Methods

```csharp
// Prepare arguments
var entity = new { position = new[] { 10.0, 20.0 } };
var playerPos = new[] { 15.0, 25.0 };
var deltaTime = 0.016;

// Call the update method
dynamic result = pythonEngine.CallMethod(
    "goblin_instance_1", 
    "update", 
    entity, 
    playerPos, 
    deltaTime);

// Access returned values
if (result != null)
{
    string state = result["state"];
    var velocity = result["velocity"];
    bool shouldAttack = result["should_attack"];
}
```

### 5. Clean Up

```csharp
// Remove instance
pythonEngine.RemoveInstance("goblin_instance_1");

// Unload script
pythonEngine.UnloadScript("goblin_ai");

// Dispose engine when done
pythonEngine.Dispose();
```

## Python vs Lua

| Feature | Python | Lua |
|---------|--------|-----|
| **Syntax** | More verbose, readable | Compact, lightweight |
| **Performance** | Good via Python.NET | Excellent (native) |
| **Ecosystem** | Large (NumPy, etc.) | Gaming-focused |
| **Learning Curve** | Gentle for Python devs | Steeper for non-Lua devs |
| **Use Cases** | Complex AI, data processing | Fast gameplay scripts |

## Best Practices

### 1. Keep Scripts Focused

Each script should handle one specific behavior or system:
- ✅ `goblin_ai.py` - Goblin enemy AI
- ✅ `fire_sword.py` - Fire sword weapon effect
- ❌ `game_logic.py` - Too broad

### 2. Return Dictionaries for State

Always return a dictionary from `update` methods:

```python
return {
    'state': self.state,
    'velocity': (vx, vy),
    'should_attack': False,
    'custom_data': { ... }
}
```

### 3. Use Type Hints (Optional)

```python
from typing import Tuple, Dict, Any

def update(self, entity: Any, player_pos: Tuple[float, float], 
           delta_time: float) -> Dict[str, Any]:
    # ...
```

### 4. Handle Errors Gracefully

```python
def update(self, entity, player_pos, delta_time):
    try:
        # Your logic here
        pass
    except Exception as e:
        print(f"Error in script: {e}")
        return {'state': 'error'}
```

### 5. Test in Python REPL First

Before integrating, test your logic in Python:

```bash
python
>>> from enemies.goblin_ai import create_ai
>>> ai = create_ai()
>>> result = ai.update(entity, (10, 20), 0.016)
>>> print(result)
```

## Script Directory Structure

```
scripts/
└── python/
    ├── enemies/
    │   ├── goblin_ai.py
    │   ├── skeleton_ai.py
    │   └── boss_ai.py
    ├── weapons/
    │   ├── fire_sword.py
    │   └── ice_bow.py
    ├── quests/
    │   └── quest_handler.py
    └── utils/
        └── pathfinding.py
```

## Debugging Python Scripts

### 1. Print Statements

```python
def update(self, entity, player_pos, delta_time):
    print(f"Entity at: {entity.position}")
    print(f"Player at: {player_pos}")
    # ...
```

### 2. Python Debugger (pdb)

```python
import pdb

def update(self, entity, player_pos, delta_time):
    pdb.set_trace()  # Breakpoint
    # ...
```

### 3. Error Logging

Exceptions in Python scripts are caught and logged in the C# console.

## Performance Tips

1. **Minimize Cross-Language Calls** - Batch operations when possible
2. **Cache Calculations** - Store computed values in instance variables
3. **Use Simple Data Types** - Primitives and tuples convert fastest
4. **Avoid Heavy Libraries** - NumPy and Pandas may have overhead

## Advanced Topics

### Using Python Libraries

```python
import random
import math

class AdvancedAI:
    def update(self, entity, player_pos, delta_time):
        # Use standard library
        angle = math.atan2(dy, dx)
        random_value = random.uniform(0, 1)
        # ...
```

### Custom Python Modules

Create reusable modules in `scripts/python/utils/`:

```python
# utils/pathfinding.py
def find_path(start, goal, obstacles):
    # A* pathfinding
    return path
```

Use in other scripts:

```python
from utils.pathfinding import find_path

class SmartAI:
    def update(self, entity, player_pos, delta_time):
        path = find_path(entity.position, player_pos, obstacles)
        # Follow path
```

## Troubleshooting

### Python Engine Fails to Initialize

- Ensure Python 3.8+ is installed
- Check that `python3X.dll` is in system PATH
- Verify Python.Runtime NuGet package is installed

### Script Not Found

- Check script path relative to `scripts/python/`
- Use forward slashes: `enemies/goblin_ai.py`

### Module Import Errors

- Ensure `__init__.py` files exist in subdirectories (optional for Python 3.3+)
- Check Python path includes `scripts/python/`

### Performance Issues

- Profile with `cProfile` in Python
- Reduce frequency of script calls in C#
- Consider switching to Lua for high-frequency scripts

## See Also

- **[Lua Scripting Guide](LUA_SCRIPTING.md)** - Alternative scripting with Lua
- **[ECS Architecture](ARCHITECTURE.md)** - How scripts integrate with entities
- **[AI Systems](AI_SYSTEMS.md)** - AI behavior patterns

## Resources

- [Python.NET Documentation](https://github.com/pythonnet/pythonnet)
- [Python 3 Tutorial](https://docs.python.org/3/tutorial/)
- [C# to Python Interop Guide](https://github.com/pythonnet/pythonnet/wiki)
