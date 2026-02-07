# Save/Load System Documentation

## Overview

The Save/Load system provides comprehensive game state persistence for Chronicles of a Drifter. It serializes all important game data to JSON files, allowing players to save their progress and resume later.

## Features

- **Full game state serialization**: Player data, world modifications, time/weather, NPCs, and quests
- **JSON format**: Human-readable save files for easy debugging
- **Quick save/load**: F5 and F9 keyboard shortcuts for rapid saving/loading
- **Multiple save slots**: Named save files with date/time tracking
- **Error handling**: Graceful handling of corrupted or missing save files

## Architecture

### Components

1. **SaveData.cs** (`src/Game/Serialization/SaveData.cs`)
   - Data Transfer Objects (DTOs) for serialization
   - Includes: `SaveData`, `PlayerData`, `WorldData`, `TimeData`, `WeatherData`, `NPCData`, `QuestData`, `ChunkData`

2. **SaveSystem.cs** (`src/Game/ECS/Systems/SaveSystem.cs`)
   - ECS System that manages save/load operations
   - Handles serialization/deserialization
   - Manages save file I/O

### Data Saved

#### Player State
- Position (X, Y coordinates)
- Health (Current and Max)
- Speed
- Gold/Currency
- Inventory (items and quantities)
- Equipped tool (type and material)
- Active quests (with progress)
- Completed quests

#### World State
- Modified chunks (only chunks that have been altered by player)
- Block modifications (placed/destroyed blocks)
- Vegetation changes
- World seed (for regeneration)

#### Time/Weather State
- Current game time (hour, minute)
- Day count
- Time scale
- Current weather type and intensity
- Weather duration and timer

#### NPC State
- NPC positions
- Current activities
- Shop inventories
- Shop prices

## Usage

### In Code

```csharp
// Create and initialize save system
var saveSystem = new SaveSystem();
world.AddSystem(saveSystem);

// Save game
bool success = saveSystem.SaveGame("my_save", gameTime: 123.5f);

// Load game
bool success = saveSystem.LoadGame("my_save", out float loadedTime);

// Quick save (F5)
bool success = saveSystem.QuickSave(gameTime);

// Quick load (F9)
bool success = saveSystem.QuickLoad(out float loadedTime);

// List all saves
var saves = saveSystem.ListSaves();
foreach (var (name, date) in saves)
{
    Console.WriteLine($"{name} - {date}");
}

// Delete a save
bool success = saveSystem.DeleteSave("my_save");
```

### Keyboard Shortcuts (in CompleteGameLoopScene)

- **F5**: Quick Save - Saves to "quicksave" slot
- **F9**: Quick Load - Loads from "quicksave" slot

### Save File Location

Save files are stored in: `./saves/` directory in the game root folder.

File format: `{saveName}.json`

Example: `saves/quicksave.json`, `saves/my_game.json`

## Implementation Details

### Serialization Strategy

1. **Components to DTOs**: Game components are converted to serializable DTOs
2. **JSON Serialization**: Uses `System.Text.Json` with indented formatting
3. **File I/O**: Saves written to disk as JSON files

### Chunk Optimization

Only modified chunks are saved (chunks where `IsModified == true`). This reduces save file size significantly, as most generated terrain doesn't need to be saved.

### Entity Matching

On load, entities are matched by:
- **Player**: First entity with `PlayerComponent`
- **NPCs**: Matched by name (since entity IDs may change)

### Reflection for Private Fields

Some game systems (TimeSystem, WeatherSystem) have private fields that need to be restored. The SaveSystem uses reflection to access these fields during load operations.

## Testing

Run the save/load test:

```bash
dotnet run --project src/Game/ChroniclesOfADrifter.csproj save-test
```

This test validates:
- Saving game state
- Loading game state
- Quick save/load functionality
- Save file listing
- Save file deletion

## Example Save File Structure

```json
{
  "SaveName": "quicksave",
  "SaveDate": "2026-02-07T14:05:28.800Z",
  "GameVersion": "1.0.0",
  "GameTime": 200,
  "Player": {
    "EntityId": 1,
    "X": 2000,
    "Y": 300,
    "Speed": 150,
    "MaxHealth": 100,
    "CurrentHealth": 75,
    "Gold": 250,
    "Inventory": {
      "Wood": 50,
      "Stone": 30,
      "IronOre": 10
    },
    "ToolType": 1,
    "ToolMaterial": 3,
    "ActiveQuests": [...],
    "CompletedQuests": [...]
  },
  "World": {
    "ModifiedChunks": [...]
  },
  "Time": {
    "CurrentTime": 36000,
    "TimeScale": 60,
    "DayCount": 0
  },
  "Weather": {
    "CurrentWeather": "Clear",
    "CurrentIntensity": "Light",
    "WeatherTimer": 0,
    "WeatherDuration": 300
  },
  "NPCs": [...]
}
```

## Error Handling

The SaveSystem handles various error conditions:

1. **Missing save file**: Returns `false` and logs error message
2. **Corrupted save file**: Catches deserialization errors, returns `false`
3. **I/O errors**: Catches file access exceptions, returns `false`
4. **Missing components**: Gracefully handles missing entities/components

All errors are logged to the console for debugging.

## Future Enhancements

Potential improvements for the save system:

1. **Compression**: Compress save files to reduce disk space
2. **Versioning**: Support loading saves from older game versions
3. **Auto-save**: Periodic automatic saves
4. **Cloud saves**: Integration with cloud storage services
5. **Backup saves**: Keep multiple backup copies
6. **Save validation**: Checksum validation to detect corruption
7. **Incremental saves**: Only save changed data since last save

## Integration with CompleteGameLoopScene

The `CompleteGameLoopScene` demonstrates full integration:

1. SaveSystem is initialized in `InitializeECSSystems()`
2. Keyboard input is handled in `HandleSaveLoadInput()` method
3. F5/F9 keys trigger quick save/load
4. Camera position is updated after loading to follow player
5. Game time is restored from save data

## Notes

- Save files are human-readable JSON for easy debugging
- The system is designed to be extensible - new data can be added easily
- Reflection is used minimally and only where necessary
- The save system works seamlessly with the ECS architecture
