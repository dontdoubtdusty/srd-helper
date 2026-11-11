using System.Numerics;
using ImGuiNET;

// Small drawing helpers shared by the panels: badges, wrapped text, tab labels and flowing buttons.
public static class Ui
{
    private static readonly Dictionary<(int, int), float> heights = new();

    // Forget measured text heights (they depend on the font size, which can change in Options).
    public static void ClearCache() => heights.Clear();

    // A rounded label drawn as one layout item of the current row height. Returns nothing; use SameLine after it.
    public static void Pill(string text, Vector4 fg, Vector4 bg, Vector4 border)
    {
        ImGui.PushFont(Fonts.Label);
        Vector2 size = ImGui.CalcTextSize(text);
        float rowH = ImGui.GetFrameHeight();
        float h = size.Y + 4f;
        Vector2 p = ImGui.GetCursorScreenPos();
        Vector2 box = new Vector2(size.X + 14f, rowH);
        ImGui.Dummy(box);
        // Off-screen pills are not drawn: rounded rects are vertex-heavy, and a big tree drawn in full passes
        // ImGui's 65535-vertex limit for one window, which crashes the app.
        if (!ImGui.IsRectVisible(p, p + box)) { ImGui.PopFont(); return; }
        // Drawn straight into the draw list, so follow the style alpha by hand (faded rows push a lower one).
        float alpha = ImGui.GetStyle().Alpha;
        fg.W *= alpha; bg.W *= alpha; border.W *= alpha;
        ImDrawListPtr dl = ImGui.GetWindowDrawList();
        Vector2 a = new Vector2(p.X, p.Y + (rowH - h) / 2f);
        Vector2 b = new Vector2(p.X + box.X, a.Y + h);
        dl.AddRectFilled(a, b, Theme.U(bg), h / 2f);
        dl.AddRect(a, b, Theme.U(border), h / 2f);
        dl.AddText(new Vector2(p.X + 7f, a.Y + 2f), Theme.U(fg), text);
        ImGui.PopFont();
    }

    // The username badge: its own color per person, the same everywhere.
    public static void Uname(string name)
    {
        float h = Theme.HueOf(name);
        // Light text on a dark tint for dark schemes, dark text on a light tint for light ones.
        if (Theme.Dark) Pill(name, Theme.Hsl(h, 0.85f, 0.82f), Theme.Hsl(h, 0.38f, 0.24f), Theme.Hsl(h, 0.40f, 0.44f));
        else Pill(name, Theme.Hsl(h, 0.70f, 0.26f), Theme.Hsl(h, 0.60f, 0.90f), Theme.Hsl(h, 0.45f, 0.68f));
    }

    public static void OpChip() => Pill("OP", Theme.Op, Theme.WithAlpha(Theme.Op, 0.16f), Theme.Op);

    // A plain outlined chip in one color.
    public static void Chip(string text, Vector4 color) => Pill(text, color, new Vector4(0, 0, 0, 0), Theme.WithAlpha(color, 0.6f));

    // Score as small text, red when negative.
    public static void Score(int? score)
    {
        ImGui.PushFont(Fonts.Label);
        ImGui.AlignTextToFramePadding();
        bool neg = score.HasValue && score < 0;
        ImGui.TextColored(neg ? Theme.Hot : Theme.Mut, score.HasValue ? score.Value.ToString() : "hidden");
        ImGui.PopFont();
    }

    // Text wrapped at a width, clipped to maxLines (0 = no limit). Drawn straight into the draw list, so a clipped
    // comment does not leave empty scroll space. Returns true when the text was cut short.
    public static bool Body(string text, float wrap, int maxLines, Vector4 color)
    {
        float lineH = ImGui.GetTextLineHeight();
        var key = (text.GetHashCode(), (int)wrap);
        if (!heights.TryGetValue(key, out float full))
        {
            full = ImGui.CalcTextSize(text, false, wrap).Y;
            if (heights.Count > 20000) heights.Clear();
            heights[key] = full;
        }
        bool clamp = maxLines > 0 && full > lineH * maxLines + 2f;
        float h = clamp ? lineH * maxLines : full;
        Vector2 p = ImGui.GetCursorScreenPos();
        // Skipped entirely when off screen, for the same vertex limit as Pill.
        if (ImGui.IsRectVisible(p, new Vector2(p.X + wrap, p.Y + h)))
        {
            ImDrawListPtr dl = ImGui.GetWindowDrawList();
            dl.PushClipRect(p, new Vector2(p.X + wrap + 2f, p.Y + h + 1f), true);
            dl.AddText(ImGui.GetFont(), ImGui.GetFontSize(), p, Theme.U(color), text, wrap);
            dl.PopClipRect();
        }
        ImGui.Dummy(new Vector2(wrap, h));
        return clamp;
    }

    // A text tab with an underline when active; returns true when clicked.
    public static bool TabLabel(string id, string text, bool active)
    {
        ImGui.PushFont(Fonts.Label);
        Vector2 size = ImGui.CalcTextSize(text);
        Vector2 p = ImGui.GetCursorScreenPos();
        bool clicked = ImGui.InvisibleButton(id, new Vector2(size.X, size.Y + 8f));
        bool hover = ImGui.IsItemHovered();
        ImDrawListPtr dl = ImGui.GetWindowDrawList();
        Vector4 col = active ? Theme.Tx : (hover ? Theme.Tx : Theme.Mut);
        dl.AddText(new Vector2(p.X, p.Y + 2f), Theme.U(col), text);
        if (active) dl.AddRectFilled(new Vector2(p.X, p.Y + size.Y + 5f), new Vector2(p.X + size.X, p.Y + size.Y + 7f), Theme.U(Theme.Acc));
        ImGui.PopFont();
        return clicked;
    }

    // A button that wraps onto a new line when it would pass rightEdge (screen x). Call with first = true for the first one.
    public static bool Flow(string label, float rightEdge, ref bool first, bool primary = false)
    {
        float w = ImGui.CalcTextSize(label).X + ImGui.GetStyle().FramePadding.X * 2f;
        if (!first)
        {
            float x = ImGui.GetItemRectMax().X + ImGui.GetStyle().ItemSpacing.X;
            if (x + w <= rightEdge) ImGui.SameLine();
        }
        first = false;
        if (primary) { ImGui.PushStyleColor(ImGuiCol.Text, Theme.Acc); ImGui.PushStyleColor(ImGuiCol.Border, Theme.Acc); }
        bool clicked = ImGui.Button(label);
        if (primary) ImGui.PopStyleColor(2);
        return clicked;
    }

    // A section label in small capitals.
    public static void Heading(string text)
    {
        ImGui.PushFont(Fonts.Label);
        ImGui.TextColored(Theme.Mut, text.ToUpperInvariant());
        ImGui.PopFont();
    }

    // A text-only small button (no frame), for "show more".
    public static bool Link(string label)
    {
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0, 0, 0, 0));
        ImGui.PushStyleColor(ImGuiCol.Text, Theme.Mut);
        ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(0, 0, 0, 0));
        bool clicked = ImGui.SmallButton(label);
        ImGui.PopStyleColor(3);
        return clicked;
    }

    public static Vector4 TierColor(string tier) => tier switch
    {
        "hot" => Theme.Hot,
        "good" => Theme.Ok,
        "unknown" => Theme.Warn,
        _ => Theme.Mut,
    };

    public static string ScoreText(int? s) => s.HasValue ? s.Value.ToString() : "hidden";
}
