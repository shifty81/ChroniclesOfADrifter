using System.Collections.Generic;

namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// Component that represents an inventory for storing items and resources
/// </summary>
public class InventoryComponent : ChroniclesOfADrifter.ECS.IComponent
{
    /// <summary>
    /// Maximum number of unique item types that can be stored
    /// </summary>
    public int MaxSlots { get; set; } = 40;
    
    /// <summary>
    /// Dictionary mapping item types to quantities
    /// Key: TileType (representing the item/resource)
    /// Value: Quantity
    /// </summary>
    private Dictionary<TileType, int> items;
    
    public InventoryComponent(int maxSlots = 40)
    {
        MaxSlots = maxSlots;
        items = new Dictionary<TileType, int>();
    }
    
    /// <summary>
    /// Adds items to the inventory
    /// </summary>
    /// <returns>True if items were added successfully, false if inventory is full</returns>
    public bool AddItem(TileType itemType, int quantity = 1)
    {
        if (itemType == TileType.Air)
            return false;
            
        if (items.ContainsKey(itemType))
        {
            items[itemType] += quantity;
            return true;
        }
        else if (items.Count < MaxSlots)
        {
            items[itemType] = quantity;
            return true;
        }
        
        return false; // Inventory full
    }
    
    /// <summary>
    /// Removes items from the inventory
    /// </summary>
    /// <returns>True if items were removed, false if not enough items</returns>
    public bool RemoveItem(TileType itemType, int quantity = 1)
    {
        if (!items.ContainsKey(itemType) || items[itemType] < quantity)
            return false;
            
        items[itemType] -= quantity;
        if (items[itemType] <= 0)
        {
            items.Remove(itemType);
        }
        
        return true;
    }
    
    /// <summary>
    /// Gets the quantity of a specific item type
    /// </summary>
    public int GetItemCount(TileType itemType)
    {
        return items.ContainsKey(itemType) ? items[itemType] : 0;
    }
    
    /// <summary>
    /// Checks if the inventory has at least the specified quantity of an item
    /// </summary>
    public bool HasItem(TileType itemType, int quantity = 1)
    {
        return GetItemCount(itemType) >= quantity;
    }
    
    /// <summary>
    /// Gets all items in the inventory
    /// </summary>
    public IReadOnlyDictionary<TileType, int> GetAllItems()
    {
        return items;
    }
    
    /// <summary>
    /// Clears all items from the inventory
    /// </summary>
    public void Clear()
    {
        items.Clear();
    }
}
