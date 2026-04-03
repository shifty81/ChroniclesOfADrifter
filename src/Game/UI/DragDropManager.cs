using ChroniclesOfADrifter.Engine;

namespace ChroniclesOfADrifter.UI;

/// <summary>
/// Manages drag-and-drop operations for inventory items.
/// Tracks the currently dragged item, source/destination slots,
/// and renders the dragged item following the mouse cursor.
/// </summary>
public class DragDropManager
{
    /// <summary>
    /// Whether an item is currently being dragged
    /// </summary>
    public bool IsDragging { get; private set; }

    /// <summary>
    /// The source slot index of the dragged item
    /// </summary>
    public int SourceSlot { get; private set; } = -1;

    /// <summary>
    /// Display name of the dragged item
    /// </summary>
    public string DraggedItemName { get; private set; } = "";

    /// <summary>
    /// Quantity of the dragged item
    /// </summary>
    public int DraggedQuantity { get; private set; }

    /// <summary>
    /// Color of the dragged item for rendering
    /// </summary>
    public float ItemR { get; private set; }
    public float ItemG { get; private set; }
    public float ItemB { get; private set; }

    /// <summary>
    /// Current mouse position
    /// </summary>
    public float MouseX { get; private set; }
    public float MouseY { get; private set; }

    /// <summary>
    /// Size of the dragged item icon
    /// </summary>
    public float DragIconSize { get; set; } = 28f;

    /// <summary>
    /// Callback invoked when an item is dropped on a target slot
    /// </summary>
    public Action<int, int>? OnItemDropped { get; set; }

    /// <summary>
    /// Callback invoked when a drag is cancelled (dropped outside inventory)
    /// </summary>
    public Action<int>? OnDragCancelled { get; set; }

    /// <summary>
    /// Start dragging an item from a slot
    /// </summary>
    public void BeginDrag(int slotIndex, string itemName, int quantity, float r, float g, float b, float mouseX, float mouseY)
    {
        if (IsDragging) return;

        IsDragging = true;
        SourceSlot = slotIndex;
        DraggedItemName = itemName;
        DraggedQuantity = quantity;
        ItemR = r;
        ItemG = g;
        ItemB = b;
        MouseX = mouseX;
        MouseY = mouseY;

        Console.WriteLine($"[DragDrop] Started dragging '{itemName}' x{quantity} from slot {slotIndex}");
    }

    /// <summary>
    /// Update the mouse position during a drag
    /// </summary>
    public void UpdateDrag(float mouseX, float mouseY)
    {
        if (!IsDragging) return;
        MouseX = mouseX;
        MouseY = mouseY;
    }

    /// <summary>
    /// End the drag operation on a target slot
    /// </summary>
    public void EndDrag(int targetSlot)
    {
        if (!IsDragging) return;

        if (targetSlot >= 0 && targetSlot != SourceSlot)
        {
            Console.WriteLine($"[DragDrop] Dropped '{DraggedItemName}' from slot {SourceSlot} to slot {targetSlot}");
            OnItemDropped?.Invoke(SourceSlot, targetSlot);
        }
        else if (targetSlot < 0)
        {
            Console.WriteLine($"[DragDrop] Drag cancelled for '{DraggedItemName}' from slot {SourceSlot}");
            OnDragCancelled?.Invoke(SourceSlot);
        }

        CancelDrag();
    }

    /// <summary>
    /// Cancel the current drag operation without performing any action
    /// </summary>
    public void CancelDrag()
    {
        IsDragging = false;
        SourceSlot = -1;
        DraggedItemName = "";
        DraggedQuantity = 0;
    }

    /// <summary>
    /// Render the dragged item icon following the mouse cursor
    /// </summary>
    public void Render()
    {
        if (!IsDragging) return;

        float iconX = MouseX - DragIconSize * 0.5f;
        float iconY = MouseY - DragIconSize * 0.5f;

        // Draw shadow
        EngineInterop.Renderer_DrawRect(
            iconX + 2f, iconY + 2f,
            DragIconSize, DragIconSize,
            0f, 0f, 0f, 0.4f
        );

        // Draw item icon
        EngineInterop.Renderer_DrawRect(
            iconX, iconY,
            DragIconSize, DragIconSize,
            ItemR, ItemG, ItemB, 0.9f
        );

        // Draw border
        float bt = 1f;
        EngineInterop.Renderer_DrawRect(iconX, iconY, DragIconSize, bt, 1f, 1f, 1f, 0.6f);
        EngineInterop.Renderer_DrawRect(iconX, iconY + DragIconSize - bt, DragIconSize, bt, 1f, 1f, 1f, 0.6f);
        EngineInterop.Renderer_DrawRect(iconX, iconY, bt, DragIconSize, 1f, 1f, 1f, 0.6f);
        EngineInterop.Renderer_DrawRect(iconX + DragIconSize - bt, iconY, bt, DragIconSize, 1f, 1f, 1f, 0.6f);

        // Draw quantity indicator if > 1
        if (DraggedQuantity > 1)
        {
            float indicatorSize = 6f;
            EngineInterop.Renderer_DrawRect(
                iconX + DragIconSize - indicatorSize - 1f,
                iconY + DragIconSize - indicatorSize - 1f,
                indicatorSize, indicatorSize,
                1f, 1f, 1f, 0.8f
            );
        }
    }
}
