using ChroniclesOfADrifter.Engine;

namespace ChroniclesOfADrifter.UI;

/// <summary>
/// UI element that renders text using colored rectangles as pixel-style characters.
/// Simulates text rendering when the engine doesn't support font rendering directly.
/// Each character is rendered as a small pattern of colored blocks.
/// </summary>
public class UIText : UIElement
{
    /// <summary>
    /// Text content to display
    /// </summary>
    public string Text { get; set; } = "";

    /// <summary>
    /// Text color
    /// </summary>
    public float TextR { get; set; } = 1f;
    public float TextG { get; set; } = 1f;
    public float TextB { get; set; } = 1f;
    public float TextA { get; set; } = 1f;

    /// <summary>
    /// Character scale (pixel size of each character cell)
    /// </summary>
    public float CharScale { get; set; } = 8f;

    /// <summary>
    /// Spacing between characters
    /// </summary>
    public float CharSpacing { get; set; } = 1f;

    /// <summary>
    /// Line spacing multiplier
    /// </summary>
    public float LineSpacing { get; set; } = 1.5f;

    /// <summary>
    /// Whether to render a background behind the text
    /// </summary>
    public bool ShowBackground { get; set; } = false;

    /// <summary>
    /// Background color
    /// </summary>
    public float BackgroundR { get; set; } = 0f;
    public float BackgroundG { get; set; } = 0f;
    public float BackgroundB { get; set; } = 0f;
    public float BackgroundA { get; set; } = 0.5f;

    /// <summary>
    /// Background padding in pixels
    /// </summary>
    public float BackgroundPadding { get; set; } = 2f;

    // 3x5 pixel font definitions for basic ASCII characters
    // Each character is 3 pixels wide and 5 pixels tall
    // Stored as 5 rows of 3 bits each (packed into a ushort)
    private static readonly Dictionary<char, ushort[]> _charPatterns = new()
    {
        ['A'] = new ushort[] { 0b010, 0b101, 0b111, 0b101, 0b101 },
        ['B'] = new ushort[] { 0b110, 0b101, 0b110, 0b101, 0b110 },
        ['C'] = new ushort[] { 0b011, 0b100, 0b100, 0b100, 0b011 },
        ['D'] = new ushort[] { 0b110, 0b101, 0b101, 0b101, 0b110 },
        ['E'] = new ushort[] { 0b111, 0b100, 0b110, 0b100, 0b111 },
        ['F'] = new ushort[] { 0b111, 0b100, 0b110, 0b100, 0b100 },
        ['G'] = new ushort[] { 0b011, 0b100, 0b101, 0b101, 0b011 },
        ['H'] = new ushort[] { 0b101, 0b101, 0b111, 0b101, 0b101 },
        ['I'] = new ushort[] { 0b111, 0b010, 0b010, 0b010, 0b111 },
        ['J'] = new ushort[] { 0b001, 0b001, 0b001, 0b101, 0b010 },
        ['K'] = new ushort[] { 0b101, 0b110, 0b100, 0b110, 0b101 },
        ['L'] = new ushort[] { 0b100, 0b100, 0b100, 0b100, 0b111 },
        ['M'] = new ushort[] { 0b101, 0b111, 0b111, 0b101, 0b101 },
        ['N'] = new ushort[] { 0b101, 0b111, 0b111, 0b111, 0b101 },
        ['O'] = new ushort[] { 0b010, 0b101, 0b101, 0b101, 0b010 },
        ['P'] = new ushort[] { 0b110, 0b101, 0b110, 0b100, 0b100 },
        ['Q'] = new ushort[] { 0b010, 0b101, 0b101, 0b111, 0b011 },
        ['R'] = new ushort[] { 0b110, 0b101, 0b110, 0b110, 0b101 },
        ['S'] = new ushort[] { 0b011, 0b100, 0b010, 0b001, 0b110 },
        ['T'] = new ushort[] { 0b111, 0b010, 0b010, 0b010, 0b010 },
        ['U'] = new ushort[] { 0b101, 0b101, 0b101, 0b101, 0b010 },
        ['V'] = new ushort[] { 0b101, 0b101, 0b101, 0b101, 0b010 },
        ['W'] = new ushort[] { 0b101, 0b101, 0b111, 0b111, 0b101 },
        ['X'] = new ushort[] { 0b101, 0b101, 0b010, 0b101, 0b101 },
        ['Y'] = new ushort[] { 0b101, 0b101, 0b010, 0b010, 0b010 },
        ['Z'] = new ushort[] { 0b111, 0b001, 0b010, 0b100, 0b111 },
        ['0'] = new ushort[] { 0b010, 0b101, 0b101, 0b101, 0b010 },
        ['1'] = new ushort[] { 0b010, 0b110, 0b010, 0b010, 0b111 },
        ['2'] = new ushort[] { 0b110, 0b001, 0b010, 0b100, 0b111 },
        ['3'] = new ushort[] { 0b110, 0b001, 0b010, 0b001, 0b110 },
        ['4'] = new ushort[] { 0b101, 0b101, 0b111, 0b001, 0b001 },
        ['5'] = new ushort[] { 0b111, 0b100, 0b110, 0b001, 0b110 },
        ['6'] = new ushort[] { 0b011, 0b100, 0b111, 0b101, 0b010 },
        ['7'] = new ushort[] { 0b111, 0b001, 0b010, 0b010, 0b010 },
        ['8'] = new ushort[] { 0b010, 0b101, 0b010, 0b101, 0b010 },
        ['9'] = new ushort[] { 0b010, 0b101, 0b111, 0b001, 0b110 },
        ['+'] = new ushort[] { 0b000, 0b010, 0b111, 0b010, 0b000 },
        ['-'] = new ushort[] { 0b000, 0b000, 0b111, 0b000, 0b000 },
        [':'] = new ushort[] { 0b000, 0b010, 0b000, 0b010, 0b000 },
        ['/'] = new ushort[] { 0b001, 0b001, 0b010, 0b100, 0b100 },
        ['!'] = new ushort[] { 0b010, 0b010, 0b010, 0b000, 0b010 },
        ['?'] = new ushort[] { 0b110, 0b001, 0b010, 0b000, 0b010 },
        ['.'] = new ushort[] { 0b000, 0b000, 0b000, 0b000, 0b010 },
        [','] = new ushort[] { 0b000, 0b000, 0b000, 0b010, 0b100 },
        ['('] = new ushort[] { 0b010, 0b100, 0b100, 0b100, 0b010 },
        [')'] = new ushort[] { 0b010, 0b001, 0b001, 0b001, 0b010 },
        ['%'] = new ushort[] { 0b101, 0b001, 0b010, 0b100, 0b101 },
        ['='] = new ushort[] { 0b000, 0b111, 0b000, 0b111, 0b000 },
        ['<'] = new ushort[] { 0b001, 0b010, 0b100, 0b010, 0b001 },
        ['>'] = new ushort[] { 0b100, 0b010, 0b001, 0b010, 0b100 },
    };

    public UIText()
    {
    }

    public UIText(string text, float x = 0, float y = 0, float scale = 8f)
    {
        Text = text;
        X = x;
        Y = y;
        CharScale = scale;
        Width = MeasureWidth(text, scale);
        Height = MeasureHeight(text, scale);
    }

    /// <summary>
    /// Measure the width of a text string in pixels
    /// </summary>
    public static float MeasureWidth(string text, float scale)
    {
        if (string.IsNullOrEmpty(text)) return 0;

        float pixelSize = scale / 5f;
        float charWidth = 3f * pixelSize;
        float spacing = pixelSize;

        // Find longest line
        float maxWidth = 0;
        float currentWidth = 0;
        foreach (char c in text)
        {
            if (c == '\n')
            {
                if (currentWidth > maxWidth) maxWidth = currentWidth;
                currentWidth = 0;
            }
            else
            {
                currentWidth += charWidth + spacing;
            }
        }
        if (currentWidth > maxWidth) maxWidth = currentWidth;
        return maxWidth;
    }

    /// <summary>
    /// Measure the height of a text string in pixels
    /// </summary>
    public static float MeasureHeight(string text, float scale)
    {
        if (string.IsNullOrEmpty(text)) return scale;

        int lines = 1;
        foreach (char c in text)
        {
            if (c == '\n') lines++;
        }
        return lines * scale * 1.5f;
    }

    protected override void OnRender()
    {
        if (string.IsNullOrEmpty(Text)) return;

        float absX = GetAbsoluteX();
        float absY = GetAbsoluteY();

        // Draw background if enabled
        if (ShowBackground)
        {
            float textWidth = MeasureWidth(Text, CharScale);
            float textHeight = MeasureHeight(Text, CharScale);
            EngineInterop.Renderer_DrawRect(
                absX - BackgroundPadding,
                absY - BackgroundPadding,
                textWidth + BackgroundPadding * 2,
                textHeight + BackgroundPadding * 2,
                BackgroundR, BackgroundG, BackgroundB, BackgroundA
            );
        }

        float pixelSize = CharScale / 5f;
        float charWidth = 3f * pixelSize;
        float spacing = pixelSize * CharSpacing;

        float cursorX = absX;
        float cursorY = absY;

        foreach (char c in Text)
        {
            if (c == '\n')
            {
                cursorX = absX;
                cursorY += CharScale * LineSpacing;
                continue;
            }

            if (c == ' ')
            {
                cursorX += charWidth + spacing;
                continue;
            }

            char upper = char.ToUpper(c);
            if (_charPatterns.TryGetValue(upper, out var pattern))
            {
                RenderCharacter(cursorX, cursorY, pattern, pixelSize);
            }

            cursorX += charWidth + spacing;
        }
    }

    /// <summary>
    /// Render a single character using the pixel font pattern
    /// </summary>
    private void RenderCharacter(float x, float y, ushort[] pattern, float pixelSize)
    {
        for (int row = 0; row < 5; row++)
        {
            ushort rowData = pattern[row];
            for (int col = 0; col < 3; col++)
            {
                // Check if pixel is set (bit 2-col)
                if ((rowData & (1 << (2 - col))) != 0)
                {
                    EngineInterop.Renderer_DrawRect(
                        x + col * pixelSize,
                        y + row * pixelSize,
                        pixelSize,
                        pixelSize,
                        TextR, TextG, TextB, TextA
                    );
                }
            }
        }
    }

    /// <summary>
    /// Set text color
    /// </summary>
    public void SetColor(float r, float g, float b, float a = 1f)
    {
        TextR = r;
        TextG = g;
        TextB = b;
        TextA = a;
    }
}
