using System.Collections.Generic;

namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// Rarity tier for loot drops
/// </summary>
public enum LootRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

/// <summary>
/// Defines a single loot item with quantity range and drop chance
/// </summary>
public class LootItem
{
    public TileType ItemType { get; set; }
    public int MinQuantity { get; set; }
    public int MaxQuantity { get; set; }
    public float DropChance { get; set; } // 0-100%
    
    public LootItem(TileType itemType, int minQuantity, int maxQuantity, float dropChance)
    {
        ItemType = itemType;
        MinQuantity = minQuantity;
        MaxQuantity = maxQuantity;
        DropChance = dropChance;
    }
}

/// <summary>
/// Component that defines what loot an entity drops when defeated
/// </summary>
public class LootDropComponent : IComponent
{
    public List<LootItem> PossibleLoot { get; private set; }
    public int MinGold { get; set; }
    public int MaxGold { get; set; }
    public float GoldDropChance { get; set; } // 0-100%
    public LootRarity Rarity { get; set; }
    
    public LootDropComponent()
    {
        PossibleLoot = new List<LootItem>();
        MinGold = 0;
        MaxGold = 0;
        GoldDropChance = 0f;
        Rarity = LootRarity.Common;
    }
    
    /// <summary>
    /// Adds a loot item to the possible drops
    /// </summary>
    public void AddLoot(TileType itemType, int minQuantity, int maxQuantity, float dropChance)
    {
        PossibleLoot.Add(new LootItem(itemType, minQuantity, maxQuantity, dropChance));
    }
    
    /// <summary>
    /// Creates a default goblin loot table
    /// </summary>
    public static LootDropComponent CreateGoblinLoot()
    {
        var loot = new LootDropComponent
        {
            MinGold = 1,
            MaxGold = 3,
            GoldDropChance = 70f,
            Rarity = LootRarity.Common
        };
        
        loot.AddLoot(TileType.Wood, 1, 2, 40f);
        loot.AddLoot(TileType.Stone, 1, 2, 20f);
        loot.AddLoot(TileType.IronOre, 1, 1, 10f);
        loot.AddLoot(TileType.Coal, 1, 2, 15f);
        
        return loot;
    }
}
