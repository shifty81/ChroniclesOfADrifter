using ChroniclesOfADrifter.Engine;

namespace ChroniclesOfADrifter.UI;

/// <summary>
/// Tooltip UI element that displays contextual information when hovering over items.
/// Automatically positions itself near the mouse cursor and adjusts to stay on screen.
/// </summary>
public class UITooltip : UIElement
{
    /// <summary>
    /// Title text displayed at the top of the tooltip
    /// </summary>
    public string Title { get; set; } = "";

    /// <summary>
    /// Description text displayed below the title
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// Additional stat lines (e.g., "Attack: +5", "Defense: +2")
    /// </summary>
    public List<string> StatLines { get; private set; } = new();

    /// <summary>
    /// Background color
    /// </summary>
    public float BackgroundR { get; set; } = 0.1f;
    public float BackgroundG { get; set; } = 0.1f;
    public float BackgroundB { get; set; } = 0.15f;
    public float BackgroundA { get; set; } = 0.95f;

    /// <summary>
    /// Border color
    /// </summary>
    public float BorderR { get; set; } = 0.5f;
    public float BorderG { get; set; } = 0.5f;
    public float BorderB { get; set; } = 0.6f;
    public float BorderA { get; set; } = 1f;

    /// <summary>
    /// Title text color
    /// </summary>
    public float TitleR { get; set; } = 1f;
    public float TitleG { get; set; } = 0.9f;
    public float TitleB { get; set; } = 0.4f;
    public float TitleA { get; set; } = 1f;

    /// <summary>
    /// Description text color
    /// </summary>
    public float DescR { get; set; } = 0.8f;
    public float DescG { get; set; } = 0.8f;
    public float DescB { get; set; } = 0.8f;
    public float DescA { get; set; } = 1f;

    /// <summary>
    /// Stat text color
    /// </summary>
    public float StatR { get; set; } = 0.4f;
    public float StatG { get; set; } = 0.9f;
    public float StatB { get; set; } = 0.4f;
    public float StatA { get; set; } = 1f;

    /// <summary>
    /// Padding inside the tooltip
    /// </summary>
    public float Padding { get; set; } = 6f;

    /// <summary>
    /// Border thickness
    /// </summary>
    public float BorderThickness { get; set; } = 1f;

    /// <summary>
    /// Character scale for text rendering
    /// </summary>
    public float TextScale { get; set; } = 6f;

    /// <summary>
    /// Offset from the mouse cursor position
    /// </summary>
    public float CursorOffsetX { get; set; } = 12f;
    public float CursorOffsetY { get; set; } = 12f;

    /// <summary>
    /// Screen bounds for clamping tooltip position
    /// </summary>
    public float ScreenWidth { get; set; } = 800f;
    public float ScreenHeight { get; set; } = 600f;

    /// <summary>
    /// Whether to show the tooltip
    /// </summary>
    public bool IsShowing { get; set; } = false;

    /// <summary>
    /// Current mouse position for positioning
    /// </summary>
    private float _mouseX;
    private float _mouseY;

    public UITooltip()
    {
        IsVisible = false;
    }

    /// <summary>
    /// Show the tooltip at the given mouse position
    /// </summary>
    public void Show(float mouseX, float mouseY, string title, string description = "", List<string>? stats = null)
    {
        Title = title;
        Description = description;
        StatLines = stats ?? new List<string>();
        _mouseX = mouseX;
        _mouseY = mouseY;
        IsShowing = true;
        IsVisible = true;

        RecalculateSize();
    }

    /// <summary>
    /// Hide the tooltip
    /// </summary>
    public void Hide()
    {
        IsShowing = false;
        IsVisible = false;
    }

    /// <summary>
    /// Update mouse position for the tooltip
    /// </summary>
    public void UpdatePosition(float mouseX, float mouseY)
    {
        _mouseX = mouseX;
        _mouseY = mouseY;
    }

    /// <summary>
    /// Recalculate tooltip size based on content
    /// </summary>
    private void RecalculateSize()
    {
        float lineHeight = TextScale * 1.5f;
        int lineCount = 0;
        float maxWidth = 0;

        // Title
        if (!string.IsNullOrEmpty(Title))
        {
            float titleWidth = UIText.MeasureWidth(Title, TextScale);
            if (titleWidth > maxWidth) maxWidth = titleWidth;
            lineCount++;
        }

        // Description
        if (!string.IsNullOrEmpty(Description))
        {
            lineCount++; // separator space
            float descWidth = UIText.MeasureWidth(Description, TextScale);
            if (descWidth > maxWidth) maxWidth = descWidth;
            lineCount++;
        }

        // Stats
        if (StatLines.Count > 0)
        {
            lineCount++; // separator space
            foreach (var stat in StatLines)
            {
                float statWidth = UIText.MeasureWidth(stat, TextScale);
                if (statWidth > maxWidth) maxWidth = statWidth;
                lineCount++;
            }
        }

        Width = maxWidth + Padding * 2;
        Height = lineCount * lineHeight + Padding * 2;
    }

    protected override void OnRender()
    {
        if (!IsShowing || string.IsNullOrEmpty(Title)) return;

        // Calculate position (offset from mouse, clamped to screen)
        float tooltipX = _mouseX + CursorOffsetX;
        float tooltipY = _mouseY + CursorOffsetY;

        // Clamp to screen bounds
        if (tooltipX + Width > ScreenWidth)
            tooltipX = _mouseX - Width - CursorOffsetX;
        if (tooltipY + Height > ScreenHeight)
            tooltipY = _mouseY - Height - CursorOffsetY;
        if (tooltipX < 0) tooltipX = 0;
        if (tooltipY < 0) tooltipY = 0;

        // Draw background
        EngineInterop.Renderer_DrawRect(
            tooltipX, tooltipY, Width, Height,
            BackgroundR, BackgroundG, BackgroundB, BackgroundA
        );

        // Draw border (4 edges)
        float bt = BorderThickness;
        // Top
        EngineInterop.Renderer_DrawRect(tooltipX, tooltipY, Width, bt, BorderR, BorderG, BorderB, BorderA);
        // Bottom
        EngineInterop.Renderer_DrawRect(tooltipX, tooltipY + Height - bt, Width, bt, BorderR, BorderG, BorderB, BorderA);
        // Left
        EngineInterop.Renderer_DrawRect(tooltipX, tooltipY, bt, Height, BorderR, BorderG, BorderB, BorderA);
        // Right
        EngineInterop.Renderer_DrawRect(tooltipX + Width - bt, tooltipY, bt, Height, BorderR, BorderG, BorderB, BorderA);

        // Render content using UIText instances
        float lineHeight = TextScale * 1.5f;
        float contentX = tooltipX + Padding;
        float contentY = tooltipY + Padding;

        // Render title
        if (!string.IsNullOrEmpty(Title))
        {
            RenderTextLine(contentX, contentY, Title, TitleR, TitleG, TitleB, TitleA);
            contentY += lineHeight;
        }

        // Render description
        if (!string.IsNullOrEmpty(Description))
        {
            contentY += lineHeight * 0.3f; // small gap
            RenderTextLine(contentX, contentY, Description, DescR, DescG, DescB, DescA);
            contentY += lineHeight;
        }

        // Render stats
        if (StatLines.Count > 0)
        {
            contentY += lineHeight * 0.3f; // small gap
            // Draw separator line
            EngineInterop.Renderer_DrawRect(
                contentX, contentY - lineHeight * 0.1f,
                Width - Padding * 2, 1f,
                BorderR, BorderG, BorderB, BorderA * 0.5f
            );

            foreach (var stat in StatLines)
            {
                RenderTextLine(contentX, contentY, stat, StatR, StatG, StatB, StatA);
                contentY += lineHeight;
            }
        }
    }

    /// <summary>
    /// Render a line of text using pixel font
    /// </summary>
    private void RenderTextLine(float x, float y, string text, float r, float g, float b, float a)
    {
        var textElement = new UIText(text, 0, 0, TextScale);
        textElement.TextR = r;
        textElement.TextG = g;
        textElement.TextB = b;
        textElement.TextA = a;
        // Temporarily set absolute position by overriding
        textElement.X = x;
        textElement.Y = y;
        textElement.Parent = null;
        textElement.Render();
    }
}
