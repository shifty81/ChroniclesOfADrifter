# Phase 5: Achievements, Minimap, Dialogue Trees & Day/Night Visual Cycle

**Status:** ✅ COMPLETE  
**Date:** April 14, 2026

---

## Overview

Phase 5 adds four major quality-of-life and immersion systems to Chronicles of a Drifter, building on the polished foundation of Phases 1–4.

---

## 1. Achievement System

### Components
- **`AchievementComponent`** — Tracks per-player achievement progress, stat counters (enemies defeated, crops harvested, etc.), XP earned from achievements, and the recently-unlocked queue.
- **`AchievementDefinition`** — Immutable metadata: id, name, description, category, required count, XP reward.
- **`AchievementProgress`** — Mutable progress record: current count, unlock state, timestamp.

### System: `AchievementSystem`
- Maintains a catalogue of **23 achievements** across **8 categories**:
  - Combat (5), Exploration (3), Crafting (3), Farming (3), Social (2), Survival (2), Collection (2), Progression (3)
- Static helper methods allow other systems to fire events:
  - `OnEnemyDefeated`, `OnBossDefeated`, `OnChunkExplored`
  - `OnCraftCompleted`, `OnCropHarvested`, `OnNPCSpokenTo`
  - `OnPlayerDied`, `OnItemCollected`, `OnLevelReached`
- Achievement unlock notifications printed to console with XP rewards.

### Test: `achievement-test`
- Tests unlock at correct thresholds for all 8 categories.
- Tests multi-unlock and XP accumulation.

---

## 2. Minimap System

### Components
- **`MinimapComponent`** — Stores the set of explored chunks (by chunk coordinate), POI markers (boss arenas, villages, chests, enemies), player world position, display size, and visibility flag.
- **`MinimapTile`** / **`MinimapTileType`** — POI data with tile type enum (Grass, Water, Mountain, Desert, Snow, Forest, Dungeon, Village, Player, Enemy, Boss, Chest).

### System: `MinimapSystem`
- Each frame, determines the player's current chunk and reveals all chunks within `VisibilityRadius`.
- Tracks cumulative exploration (chunk set grows monotonically).
- Static helpers: `AddPOI`, `RemovePOI`, `ToggleVisibility`.
- `RenderASCII(minimap, radius)` generates a text-mode minimap for console/debug:
  - `@` = player, `.` = explored, `!` = POI, ` ` = unexplored.

### Test: `minimap-test`
- Initialization, chunk exploration, POI add/remove, ASCII render, visibility toggle, player position tracking.

---

## 3. Branching NPC Dialogue Trees

### Components
- **`DialogueLine`** — Speaker + text + list of `DialogueChoice` options + `EndsConversation` flag.
- **`DialogueChoice`** — Player choice text, next node ID, optional condition (`Func<bool>`), optional action (`Action`).
- **`DialogueNode`** — Ordered list of lines within a tree node.
- **`DialogueTree`** — Dictionary of nodes by ID; `StartNodeId` for entry point.
- **`DialogueTreeComponent`** — Active state on an NPC entity: current node/line, awaiting-choice flag, conversation history, `StartDialogue`/`AdvanceLine`/`SelectChoice`/`EndDialogue` API.

### System: `DialogueSystem`
- `StartConversation(world, npc, tree)` — Begins a conversation.
- `GetCurrentLine`, `AdvanceLine`, `SelectChoice`, `EndConversation` — Drive the dialogue state machine.
- `IsInConversation`, `IsAwaitingChoice` — Query helpers for UI.
- **Factory methods** for common patterns:
  - `BuildSimpleGreeting(name, lines)` — Linear dialogue, no branches.
  - `BuildMerchantDialogue(name)` — Buy/sell/farewell branches.
  - `BuildQuestDialogue(name, title, desc, onAccept?)` — Accept/decline branches with optional callback.

### Test: `dialogue-test`
- Linear dialogue flow, branching path selection, merchant factory, quest factory with callback, conversation history, invalid choice handling, force-end conversation.

---

## 4. Day/Night Visual Cycle

### Components
- **`AmbientColor`** — RGBA float struct with `Lerp` helper.
- **`DayNightPreset`** — Named time-of-day preset: start hour, ambient color, brightness, fog density.
- **`DayNightVisualComponent`** — Current interpolated ambient color, brightness, fog density, phase name, and a list of 8 default presets:

| Hour | Phase     | Brightness | Fog  |
|------|-----------|-----------|------|
| 00   | Midnight  | 0.10      | 0.20 |
| 05   | Dawn      | 0.55      | 0.10 |
| 07   | Morning   | 0.90      | 0.00 |
| 12   | Noon      | 1.00      | 0.00 |
| 15   | Afternoon | 0.90      | 0.00 |
| 17   | Dusk      | 0.55      | 0.08 |
| 19   | Evening   | 0.20      | 0.15 |
| 21   | Night     | 0.10      | 0.20 |

### System: `DayNightVisualSystem`
- Each frame, reads current hour from `TimeSystem` (falls back to wall clock for demos).
- Calls `GetBlendFactors(hour)` to find the surrounding presets and blend factor `t`.
- Linearly interpolates ambient color, brightness, and fog density and writes them back to the component.
- `DescribeState(dnv)` returns a formatted string for console/HUD display.
- `CreateDayNightEntity(world)` factory convenience method.

### Test: `daynight-test`
- Component initialization, blend factor correctness at key hours, system update, phase naming, brightness/fog range validation, full 24-hour cycle summary.

---

## Integration

All 4 systems are registered in `Program.cs` with dedicated test modes:

```
dotnet run -- achievement-test   # Achievement system tests
dotnet run -- minimap-test       # Minimap system tests
dotnet run -- dialogue-test      # Dialogue tree tests
dotnet run -- daynight-test      # Day/night visual cycle tests
```

All tests pass with 0 errors.

---

## Files Added

### Components (4 new)
- `src/Game/ECS/Components/AchievementComponent.cs`
- `src/Game/ECS/Components/MinimapComponent.cs`
- `src/Game/ECS/Components/DialogueTreeComponent.cs`
- `src/Game/ECS/Components/DayNightVisualComponent.cs`

### Systems (4 new)
- `src/Game/ECS/Systems/AchievementSystem.cs`
- `src/Game/ECS/Systems/MinimapSystem.cs`
- `src/Game/ECS/Systems/DialogueSystem.cs`
- `src/Game/ECS/Systems/DayNightVisualSystem.cs`

### Tests (4 new)
- `src/Game/Tests/AchievementSystemTest.cs`
- `src/Game/Tests/MinimapSystemTest.cs`
- `src/Game/Tests/DialogueSystemTest.cs`
- `src/Game/Tests/DayNightVisualTest.cs`
