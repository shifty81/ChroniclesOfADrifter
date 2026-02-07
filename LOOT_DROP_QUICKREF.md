# Loot Drop System - Quick Reference

## Adding Loot to an Enemy

```csharp
// Method 1: Use pre-defined loot table
World.AddComponent(enemy, LootDropComponent.CreateGoblinLoot());

// Method 2: Create custom loot table
var loot = new LootDropComponent
{
    MinGold = 5,
    MaxGold = 15,
    GoldDropChance = 80f,
    Rarity = LootRarity.Uncommon
};
loot.AddLoot(TileType.Diamond, 1, 2, 10f);  // 10% chance for 1-2 diamonds
loot.AddLoot(TileType.Iron, 2, 5, 50f);     // 50% chance for 2-5 iron
World.AddComponent(enemy, loot);
```

## Goblin Default Loot

| Item      | Quantity | Drop Chance |
|-----------|----------|-------------|
| Gold      | 1-3      | 70%         |
| Wood      | 1-2      | 40%         |
| Stone     | 1-2      | 20%         |
| Coal      | 1-2      | 15%         |
| Iron Ore  | 1        | 10%         |

## Key Constants

```csharp
PICKUP_RADIUS = 64f;              // 2 blocks (32 pixels per block)
DEFAULT_EXPIRATION_TIME = 60f;    // Items disappear after 60 seconds
```

## Rarity Tiers

```csharp
LootRarity.Common      // Most frequent drops
LootRarity.Uncommon    // Occasional drops
LootRarity.Rare        // Infrequent drops
LootRarity.Epic        // Very rare drops
LootRarity.Legendary   // Extremely rare drops
```

## Console Messages

```
[LootDropSystem] Enemy dropped: 3 Gold, 2x Wood
[LootDropSystem] Picked up: 2x Wood, +3 Gold
[LootDropSystem] Inventory full! Cannot pick up item.
```

## System Integration

```csharp
// In CompleteGameLoopScene.cs InitializeECSSystems():
lootDropSystem = new LootDropSystem(seed: 42069);
World.AddSystem(lootDropSystem);

deathSystem = new DeathSystem();
World.AddSystem(deathSystem);
deathSystem.SetLootDropSystem(lootDropSystem);
```

## Available Item Types (TileType)

Resources: `Wood`, `Stone`, `Coal`, `Iron`, `Gold`, `Cobblestone`, `Brick`
Ores: `IronOre`, `CopperOre`, `GoldOre`, `SilverOre`, `CoalOre`, `DiamondOre`
