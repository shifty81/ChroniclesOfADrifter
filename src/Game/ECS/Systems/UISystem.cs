using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.Engine;
using ChroniclesOfADrifter.UI;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// System that manages UI elements and handles input
/// </summary>
public class UISystem : ISystem
{
    private UIElement? _hoveredElement = null;
    private float _previousMouseX = 0;
    private float _previousMouseY = 0;
    
    public void Initialize(World world)
    {
        // No initialization needed
    }
    
    public void Update(World world, float deltaTime)
    {
        // Get mouse position
        float mouseX = 0, mouseY = 0;
        EngineInterop.Input_GetMousePosition(out mouseX, out mouseY);
        
        // Check for mouse button press
        bool mousePressed = EngineInterop.Input_IsMouseButtonPressed(0); // Left mouse button
        
        // Update all UI components
        foreach (var entity in world.GetEntitiesWithComponent<UIComponent>())
        {
            var uiComponent = world.GetComponent<UIComponent>(entity);
            if (uiComponent == null || !uiComponent.IsVisible) continue;
            
            // Update all elements
            foreach (var element in uiComponent.Elements)
            {
                element.Update(deltaTime);
                
                // Handle mouse interaction
                if (element.IsVisible && element.IsEnabled)
                {
                    bool contains = element.Contains(mouseX, mouseY);
                    
                    // Check if mouse is hovering
                    if (contains && _hoveredElement != element)
                    {
                        // Mouse entered
                        _hoveredElement?.OnMouseExit();
                        _hoveredElement = element;
                        element.OnMouseEnter();
                    }
                    else if (!contains && _hoveredElement == element)
                    {
                        // Mouse exited
                        element.OnMouseExit();
                        _hoveredElement = null;
                    }
                    
                    // Check for click
                    if (contains && mousePressed)
                    {
                        element.OnClick(mouseX, mouseY);
                    }
                }
            }
        }
        
        _previousMouseX = mouseX;
        _previousMouseY = mouseY;
    }
    
    /// <summary>
    /// Render all UI elements (should be called after all other rendering)
    /// </summary>
    public static void RenderUI(World world)
    {
        foreach (var entity in world.GetEntitiesWithComponent<UIComponent>())
        {
            var uiComponent = world.GetComponent<UIComponent>(entity);
            if (uiComponent == null || !uiComponent.IsVisible) continue;
            
            foreach (var element in uiComponent.Elements)
            {
                element.Render();
            }
        }
    }
}
