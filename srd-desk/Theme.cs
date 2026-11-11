using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;

// One color scheme. The accent and the chip colors are chosen per scheme so they stay readable on its background.
public record Palette(string Name, bool Dark, string Bg, string Pn, string Pn2, string Ln, string Tx, string Mut, string Acc,
    string Hot, string Ok, string Gold, string Duel);

// The color schemes (from the mockups) and the style that is applied to every ImGui widget. Dusk is the default.
public static class Theme
{
    public static readonly Palette[] All =
    {
        new("Dusk",  true,  "2d3640", "343f4a", "3d4a57", "4a5866", "e4e9ee", "a3b0bd", "7cc4e8", "ff9d8d", "82dcb2", "f0cd7a", "c3a8f5"),
        new("Mist",  false, "f3f5f8", "e9edf2", "dfe5ec", "ccd4de", "1f2933", "5a6878", "2f6fdb", "c2412f", "1f8a5f", "8f5f00", "7a4fd1"),
        new("Paper", false, "f6f1e7", "efe8da", "e6dec9", "d8cdb4", "2e2a24", "6b6150", "1f6f8b", "b5432f", "2f7d57", "8f6410", "7a4fb8"),
        new("Sage",  false, "e9eee6", "dfe6db", "d3dccd", "bfcbb8", "22301f", "4f5f49", "26728a", "b3442f", "4d7c1f", "85620d", "6f4fb0"),
    };

    public static Vector4 Bg, Pn, Pn2, Ln, Tx, Mut, Acc, Op, Ok, Duel, Hot, Warn;
    public static bool Dark = true;
    public static string Name = "Dusk";

    static Theme() { Use("Dusk"); }

    // Switch to a scheme by name (Dusk when the name is unknown). Call Apply afterwards to restyle the widgets.
    public static void Use(string name)
    {
        Palette p = All.FirstOrDefault(x => x.Name == name) ?? All[0];
        Name = p.Name; Dark = p.Dark;
        Bg = Hex(p.Bg); Pn = Hex(p.Pn); Pn2 = Hex(p.Pn2); Ln = Hex(p.Ln); Tx = Hex(p.Tx); Mut = Hex(p.Mut); Acc = Hex(p.Acc);
        Hot = Hex(p.Hot); Ok = Hex(p.Ok); Duel = Hex(p.Duel); Op = Hex(p.Gold); Warn = Hex(p.Gold);
    }

    public static Vector4 Hex(string rgb)
    {
        int v = Convert.ToInt32(rgb, 16);
        return new Vector4(((v >> 16) & 255) / 255f, ((v >> 8) & 255) / 255f, (v & 255) / 255f, 1f);
    }

    public static Vector4 WithAlpha(Vector4 c, float a) => new Vector4(c.X, c.Y, c.Z, a);

    // Blend a toward b (0 = a, 1 = b); used for hover and pressed shades.
    public static Vector4 Mix(Vector4 a, Vector4 b, float t) => new Vector4(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t, a.Z + (b.Z - a.Z) * t, 1f);

    public static uint U(Vector4 c) => ImGui.ColorConvertFloat4ToU32(c);

    // Hue (0-360), saturation and lightness (0-1) to a color.
    public static Vector4 Hsl(float h, float s, float l)
    {
        float c = (1f - MathF.Abs(2f * l - 1f)) * s;
        float x = c * (1f - MathF.Abs(h / 60f % 2f - 1f));
        float m = l - c / 2f;
        (float r, float g, float b) = h switch
        {
            < 60 => (c, x, 0f),
            < 120 => (x, c, 0f),
            < 180 => (0f, c, x),
            < 240 => (0f, x, c),
            < 300 => (x, 0f, c),
            _ => (c, 0f, x),
        };
        return new Vector4(r + m, g + m, b + m, 1f);
    }

    // A stable hue per username, so each person keeps the same color everywhere.
    public static float HueOf(string name)
    {
        int h = 0;
        foreach (char ch in name) h = (h * 31 + ch) % 360;
        return h;
    }

    // Style colors and shapes for every ImGui widget, from the current scheme.
    public static void Apply()
    {
        ImGuiStylePtr s = ImGui.GetStyle();
        s.WindowRounding = 0f;
        s.ChildRounding = 6f;
        s.FrameRounding = 4f;
        s.GrabRounding = 4f;
        s.ScrollbarRounding = 6f;
        s.WindowBorderSize = 0f;
        s.ChildBorderSize = 0f;
        s.FrameBorderSize = 1f;
        s.WindowPadding = new Vector2(12, 10);
        s.FramePadding = new Vector2(8, 4);
        s.ItemSpacing = new Vector2(8, 6);
        s.ScrollbarSize = 11f;
        Vector4 hover = Mix(Pn2, Tx, 0.08f), pressed = Mix(Pn2, Tx, 0.16f);
        var c = s.Colors;
        c[(int)ImGuiCol.Text] = Tx;
        c[(int)ImGuiCol.TextDisabled] = Mut;
        c[(int)ImGuiCol.WindowBg] = Bg;
        c[(int)ImGuiCol.ChildBg] = new Vector4(0, 0, 0, 0);
        c[(int)ImGuiCol.PopupBg] = Pn;
        c[(int)ImGuiCol.Border] = Ln;
        c[(int)ImGuiCol.FrameBg] = Pn2;
        c[(int)ImGuiCol.FrameBgHovered] = hover;
        c[(int)ImGuiCol.FrameBgActive] = hover;
        c[(int)ImGuiCol.Button] = Pn2;
        c[(int)ImGuiCol.ButtonHovered] = hover;
        c[(int)ImGuiCol.ButtonActive] = pressed;
        c[(int)ImGuiCol.Header] = Pn2;
        c[(int)ImGuiCol.HeaderHovered] = hover;
        c[(int)ImGuiCol.HeaderActive] = pressed;
        c[(int)ImGuiCol.CheckMark] = Acc;
        c[(int)ImGuiCol.SliderGrab] = Acc;
        c[(int)ImGuiCol.SliderGrabActive] = Acc;
        c[(int)ImGuiCol.ScrollbarBg] = new Vector4(0, 0, 0, 0);
        c[(int)ImGuiCol.ScrollbarGrab] = Ln;
        c[(int)ImGuiCol.ScrollbarGrabHovered] = Mut;
        c[(int)ImGuiCol.ScrollbarGrabActive] = Mut;
        c[(int)ImGuiCol.Separator] = Ln;
        c[(int)ImGuiCol.TableHeaderBg] = Pn;
        c[(int)ImGuiCol.TableBorderStrong] = Ln;
        c[(int)ImGuiCol.TableBorderLight] = Mix(Pn, Ln, 0.6f);
        c[(int)ImGuiCol.TableRowBg] = new Vector4(0, 0, 0, 0);
        c[(int)ImGuiCol.TableRowBgAlt] = Dark ? new Vector4(1, 1, 1, 0.03f) : new Vector4(0, 0, 0, 0.03f);
        c[(int)ImGuiCol.TextSelectedBg] = WithAlpha(Acc, 0.35f);
        c[(int)ImGuiCol.TitleBg] = Pn;
        c[(int)ImGuiCol.TitleBgActive] = Pn2;
        c[(int)ImGuiCol.TitleBgCollapsed] = Pn;
        c[(int)ImGuiCol.ModalWindowDimBg] = new Vector4(0, 0, 0, 0.45f);
    }
}

// The two fonts: Quicksand for reading comments, Chakra Petch for labels, badges and tables.
public static class Fonts
{
    public static ImFontPtr Body;
    public static ImFontPtr Label;
    private static GCHandle ranges;

    // The text size choices in the Options menu, as a multiplier of the base sizes (19 and 15.5 px).
    public static readonly (string Name, float Scale)[] Sizes = { ("Small", 0.9f), ("Default", 1f), ("Large", 1.15f), ("Extra large", 1.3f) };
    public static float Scale = 1f;

    public static float ScaleOf(string name) => Sizes.FirstOrDefault(s => s.Name == name).Scale is float f and > 0 ? f : 1f;

    // Call between rlImGui.BeginInitImGui and EndInitImGui (or before rlImGui.ReloadFonts when the size changes).
    // The first font added becomes the default.
    // The glyph ranges add curly quotes, dashes, ellipses, accents and Cyrillic, which Reddit text is full of.
    public static void Load(float scale)
    {
        Scale = scale;
        ushort[] r = { 0x0020, 0x00FF, 0x0100, 0x017F, 0x0400, 0x04FF, 0x2000, 0x206F, 0x20A0, 0x20CF, 0x2190, 0x21FF, 0x2200, 0x22FF, 0 };
        ranges = GCHandle.Alloc(r, GCHandleType.Pinned);
        IntPtr p = ranges.AddrOfPinnedObject();
        ImGuiIOPtr io = ImGui.GetIO();
        Body = io.Fonts.AddFontFromFileTTF(Assets.PathTo("fonts/Quicksand-SemiBold.ttf"), 19f * scale, default, p);
        Label = io.Fonts.AddFontFromFileTTF(Assets.PathTo("fonts/ChakraPetch-Medium.ttf"), 15.5f * scale, default, p);
    }

    // Call after the font atlas is built.
    public static void Release() { if (ranges.IsAllocated) ranges.Free(); }

    // Rebuild the fonts at a new size while the app runs.
    public static void Rebuild(float scale)
    {
        ImGui.GetIO().Fonts.Clear();
        Ui.ClearCache();
        Load(scale);
        rlImGui.ReloadFonts();
        Release();
    }
}

public static class Assets
{
    public static string PathTo(string relative) => Path.Combine(AppContext.BaseDirectory, "resources", relative);

    // The mascot picture (Poppy, 256 px wide). Loaded once the window exists; the window icon uses the same file.
    private static Texture2D mascot;
    private static bool mascotLoaded;
    public static void LoadMascot()
    {
        string file = PathTo("mascot.png");
        if (!File.Exists(file)) return;
        Raylib_cs.Image img = Raylib.LoadImage(file);
        Raylib.SetWindowIcon(img);
        Raylib.UnloadImage(img);
        mascot = Raylib.LoadTexture(file);
        // The picture is much larger than where it is drawn (26 px in the top bar), so build smaller copies
        // (mipmaps) and blend between them; otherwise the shrunken outline looks jagged.
        Raylib.GenTextureMipmaps(ref mascot);
        Raylib.SetTextureFilter(mascot, TextureFilter.Trilinear);
        mascotLoaded = true;
    }

    // Draws the mascot at the given height, keeping its proportions. Draws nothing if the file is missing.
    public static void DrawMascot(float height)
    {
        if (!mascotLoaded) return;
        rlImGui.ImageSize(mascot, new Vector2(height * mascot.Width / mascot.Height, height));
    }

    public static float MascotWidth(float height) => mascotLoaded ? height * mascot.Width / mascot.Height : 0f;
}
