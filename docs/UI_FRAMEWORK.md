# UI Framework

## Overview

The Chronicles of a Drifter UI framework provides a flexible, component-based system for creating game interfaces including inventory, crafting menus, and other interactive elements.

## Architecture

The UI framework follows the ECS (Entity Component System) pattern used throughout the game:

### Core Components

- **UIElement** - Base class for all UI elements with positioning, hierarchy, and input handling
- **UIComponent** - ECS component that holds UI elements for an entity
- **UISystem** - ECS system that manages UI lifecycle, input handling, and rendering

### UI Element Types

#### UIPanel
A rectangular container with customizable background and border.

```csharp
var panel = new UIPanel
{
    X = 100, Y = 100,
    Width = 400, Height = 300,
    BackgroundR = 0.2f, BackgroundG = 0.2f, BackgroundB = 0.2f, BackgroundA = 0.9f,
    BorderR = 0.5f, BorderG = 0.5f, BorderB = 0.5f, BorderA = 1.0f,
    BorderThickness = 3f
};
```

#### UIButton
A clickable button with hover and pressed states.

```csharp
var button = new UIButton
{
    X = 10, Y = 10,
    Width = 100, Height = 40,
    Text = "Click Me",
    OnClickAction = () => Console.WriteLine("Button clicked!")
};
```

#### InventoryUI
A specialized panel that displays inventory contents in a grid layout.

```csharp
var inventoryUI = new InventoryUI(100, 100);
inventoryUI.SetInventory(playerInventory);
```

#### CraftingUI
A specialized panel for crafting items from recipes.

```csharp
var craftingUI = new CraftingUI(500, 100, 400, 450);
craftingUI.SetCraftingData(craftingSystem, playerInventory);
```

## Input Handling

The UI framework integrates with the game engine's input system:

### Keyboard Input
- Keys are processed through `EngineInterop.Input_IsKeyPressed(keyCode)`
- Common keys:
  - 73 = 'I' (Inventory)
  - 67 = 'C' (Crafting)
  - 27 = ESC (Close UI)

### Mouse Input
- Mouse position tracked via `EngineInterop.Input_GetMousePosition(ref x, ref y)`
- Mouse buttons tracked via `EngineInterop.Input_IsMouseButtonPressed(button)`
  - Button 0 = Left mouse button
  - Button 1 = Right mouse button
  - Button 2 = Middle mouse button

### Input Events
UI elements respond to:
- `OnClick(mouseX, mouseY)` - When clicked
- `OnMouseEnter()` - When mouse hovers over
- `OnMouseExit()` - When mouse leaves

## Usage Example

### Creating a UI Entity

```csharp
// Create UI entity
var uiEntity = world.CreateEntity();
var uiComponent = new UIComponent();

// Create inventory UI
var inventoryUI = new InventoryUI(100, 100);
inventoryUI.SetInventory(playerInventory);
inventoryUI.IsVisible = false;  // Start hidden
uiComponent.AddElement(inventoryUI);

// Create crafting UI
var craftingUI = new CraftingUI(570, 100, 400, 450);
craftingUI.SetCraftingData(craftingSystem, playerInventory);
craftingUI.IsVisible = false;  // Start hidden
uiComponent.AddElement(craftingUI);

// Add component to entity
world.AddComponent(uiEntity, uiComponent);
```

### Toggling UI Visibility

```csharp
// In your scene's OnUpdate method
if (EngineInterop.Input_IsKeyPressed(73)) // 'I' key
{
    inventoryUI.IsVisible = !inventoryUI.IsVisible;
}

if (EngineInterop.Input_IsKeyPressed(67)) // 'C' key
{
    craftingUI.IsVisible = !craftingUI.IsVisible;
}
```

## System Integration

### Registering the UI System

```csharp
// In your scene's OnLoad method
world.RegisterSystem(new UISystem());
```

### Rendering

UI elements are automatically rendered after all other game objects in the `RenderingSystem`. This ensures UI is always on top.

## Customization

### Creating Custom UI Elements

Extend `UIElement` to create custom UI widgets:

```csharp
public class MyCustomElement : UIElement
{
    protected override void OnRender()
    {
        float absX = GetAbsoluteX();
        float absY = GetAbsoluteY();
        
        // Draw your custom element using EngineInterop.Renderer_DrawRect()
        EngineInterop.Renderer_DrawRect(absX, absY, Width, Height, 
                                       0.5f, 0.5f, 0.5f, 1.0f);
    }
    
    public override void OnClick(float mouseX, float mouseY)
    {
        // Handle click event
        Console.WriteLine("Custom element clicked!");
    }
}
```

## Limitations

### Current Limitations

1. **Text Rendering**: The engine doesn't currently support text rendering. UI elements use color-coding and visual indicators instead.
   - Item types are represented by colors
   - Quantities are indicated by small markers
   - Button text is stored but not displayed

2. **Sprite-Based Icons**: Items are currently represented by colored rectangles. Future updates will add sprite-based icons.

3. **No Drag-and-Drop**: While the structure supports it, drag-and-drop is not yet implemented.

### Future Enhancements

- Text rendering support
- Sprite-based item icons
- Drag-and-drop for inventory management
- Tooltips with detailed item information
- Animated UI transitions
- UI themes and styling system
- Layout managers (flow, grid, anchor)
- Scroll containers for long lists

## Performance Considerations

- UI elements are only updated and rendered when visible
- Input checking is optimized to only process visible and enabled elements
- Mouse hover tracking prevents redundant event firing
- UI rendering is done after culling invisible elements

## Troubleshooting

### Input Not Working

If keyboard or mouse input doesn't work:

1. **Check Renderer**: Input handling requires DirectX 11/12 or SDL2 renderer
2. **Windows Only**: DirectX renderers require Windows with proper message handling
3. **Element State**: Ensure UI elements are `IsVisible = true` and `IsEnabled = true`
4. **Input Codes**: Verify you're using correct key codes (Windows VK codes)

### UI Not Displaying

If UI doesn't appear:

1. **Visibility**: Check `IsVisible` property on both parent and child elements
2. **Position**: Verify X/Y coordinates are within screen bounds
3. **Rendering Order**: Ensure UISystem is registered and rendering
4. **Component**: Verify UIComponent is added to an entity

## See Also

- [Crafting System Documentation](MINING_BUILDING_SYSTEM.md#crafting-system)
- [Inventory System Documentation](MINING_BUILDING_SYSTEM.md#inventory-system)
- [Engine Architecture](ARCHITECTURE.md)
