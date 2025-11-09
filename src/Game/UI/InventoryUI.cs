using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.UI;

namespace ChroniclesOfADrifter.UI;

/// <summary>
/// UI for displaying and managing inventory
/// </summary>
public class InventoryUI : UIPanel
{
    private const int GRID_COLS = 8;
    private const int GRID_ROWS = 5;
    private const float SLOT_SIZE = 50f;
    private const float SLOT_PADDING = 5f;
    private const float HEADER_HEIGHT = 40f;
    
    private InventoryComponent? _inventory;
    private List<UIInventorySlot> _slots = new();
    
    public InventoryUI(float x, float y)
    {
        X = x;
        Y = y;
        Width = GRID_COLS * (SLOT_SIZE + SLOT_PADDING) + SLOT_PADDING * 2;
        Height = GRID_ROWS * (SLOT_SIZE + SLOT_PADDING) + SLOT_PADDING * 2 + HEADER_HEIGHT;
        
        // Dark semi-transparent background
        BackgroundR = 0.1f;
        BackgroundG = 0.1f;
        BackgroundB = 0.1f;
        BackgroundA = 0.95f;
        
        // Lighter border
        BorderR = 0.5f;
        BorderG = 0.5f;
        BorderB = 0.5f;
        BorderA = 1.0f;
        BorderThickness = 3f;
        
        // Create inventory slots
        for (int row = 0; row < GRID_ROWS; row++)
        {
            for (int col = 0; col < GRID_COLS; col++)
            {
                int slotIndex = row * GRID_COLS + col;
                var slot = new UIInventorySlot(slotIndex)
                {
                    X = SLOT_PADDING * 2 + col * (SLOT_SIZE + SLOT_PADDING),
                    Y = HEADER_HEIGHT + SLOT_PADDING * 2 + row * (SLOT_SIZE + SLOT_PADDING),
                    Width = SLOT_SIZE,
                    Height = SLOT_SIZE
                };
                _slots.Add(slot);
                AddChild(slot);
            }
        }
    }
    
    /// <summary>
    /// Set the inventory to display
    /// </summary>
    public void SetInventory(InventoryComponent? inventory)
    {
        _inventory = inventory;
        UpdateSlots();
    }
    
    /// <summary>
    /// Update slot contents based on inventory
    /// </summary>
    public void UpdateSlots()
    {
        if (_inventory == null)
        {
            // Clear all slots
            foreach (var slot in _slots)
            {
                slot.SetItem(TileType.Air, 0);
            }
            return;
        }
        
        // Get all items from inventory
        var items = _inventory.GetAllItems();
        int slotIndex = 0;
        
        // Fill slots with items
        foreach (var kvp in items)
        {
            if (slotIndex < _slots.Count)
            {
                _slots[slotIndex].SetItem(kvp.Key, kvp.Value);
                slotIndex++;
            }
        }
        
        // Clear remaining slots
        for (int i = slotIndex; i < _slots.Count; i++)
        {
            _slots[i].SetItem(TileType.Air, 0);
        }
    }
    
    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);
        
        // Periodically update slots to reflect inventory changes
        // In a real implementation, you'd want to subscribe to inventory change events
        UpdateSlots();
    }
}

/// <summary>
/// Represents a single inventory slot
/// </summary>
public class UIInventorySlot : UIElement
{
    private int _slotIndex;
    private TileType _itemType = TileType.Air;
    private int _quantity = 0;
    private bool _isHovered = false;
    
    public UIInventorySlot(int slotIndex)
    {
        _slotIndex = slotIndex;
    }
    
    public void SetItem(TileType itemType, int quantity)
    {
        _itemType = itemType;
        _quantity = quantity;
    }
    
    public TileType GetItemType() => _itemType;
    public int GetQuantity() => _quantity;
    
    public override void OnMouseEnter()
    {
        _isHovered = true;
    }
    
    public override void OnMouseExit()
    {
        _isHovered = false;
    }
    
    public override void OnClick(float mouseX, float mouseY)
    {
        // TODO: Implement item selection/use logic
        Console.WriteLine($"Clicked inventory slot {_slotIndex}: {_itemType} x{_quantity}");
    }
    
    protected override void OnRender()
    {
        float absX = GetAbsoluteX();
        float absY = GetAbsoluteY();
        
        // Slot background
        float bgR = _isHovered ? 0.3f : 0.2f;
        float bgG = _isHovered ? 0.3f : 0.2f;
        float bgB = _isHovered ? 0.35f : 0.25f;
        
        ChroniclesOfADrifter.Engine.EngineInterop.Renderer_DrawRect(
            absX, absY, Width, Height, bgR, bgG, bgB, 1.0f);
        
        // Slot border
        float borderSize = _isHovered ? 2f : 1f;
        ChroniclesOfADrifter.Engine.EngineInterop.Renderer_DrawRect(
            absX, absY, Width, borderSize, 0.4f, 0.4f, 0.4f, 1.0f);
        ChroniclesOfADrifter.Engine.EngineInterop.Renderer_DrawRect(
            absX + Width - borderSize, absY, borderSize, Height, 0.4f, 0.4f, 0.4f, 1.0f);
        ChroniclesOfADrifter.Engine.EngineInterop.Renderer_DrawRect(
            absX, absY + Height - borderSize, Width, borderSize, 0.4f, 0.4f, 0.4f, 1.0f);
        ChroniclesOfADrifter.Engine.EngineInterop.Renderer_DrawRect(
            absX, absY, borderSize, Height, 0.4f, 0.4f, 0.4f, 1.0f);
        
        // Draw item representation
        if (_itemType != TileType.Air && _quantity > 0)
        {
            // Use color coding to represent different item types
            float itemR = 0.5f, itemG = 0.5f, itemB = 0.5f;
            
            // Assign different colors based on item type
            switch (_itemType)
            {
                case TileType.Wood:
                    itemR = 0.6f; itemG = 0.4f; itemB = 0.2f; // Brown
                    break;
                case TileType.Stone:
                    itemR = 0.5f; itemG = 0.5f; itemB = 0.5f; // Gray
                    break;
                case TileType.Dirt:
                    itemR = 0.4f; itemG = 0.3f; itemB = 0.2f; // Dark brown
                    break;
                case TileType.Iron:
                    itemR = 0.7f; itemG = 0.7f; itemB = 0.7f; // Light gray
                    break;
                case TileType.Gold:
                    itemR = 1.0f; itemG = 0.84f; itemB = 0.0f; // Gold
                    break;
                case TileType.Coal:
                    itemR = 0.2f; itemG = 0.2f; itemB = 0.2f; // Black
                    break;
                default:
                    itemR = 0.5f; itemG = 0.5f; itemB = 0.5f; // Default gray
                    break;
            }
            
            // Draw item icon (simplified as colored rectangle)
            float padding = 8f;
            ChroniclesOfADrifter.Engine.EngineInterop.Renderer_DrawRect(
                absX + padding, absY + padding, 
                Width - padding * 2, Height - padding * 2,
                itemR, itemG, itemB, 1.0f);
            
            // TODO: Draw quantity text when text rendering is available
            // For now, we'll indicate quantity with a small indicator if > 1
            if (_quantity > 1)
            {
                // Draw a small indicator in the bottom-right corner
                ChroniclesOfADrifter.Engine.EngineInterop.Renderer_DrawRect(
                    absX + Width - 12f, absY + Height - 12f, 
                    8f, 8f, 1.0f, 1.0f, 1.0f, 1.0f);
            }
        }
    }
}
