using System.Diagnostics;
using System.Numerics;
using ImGuiNET;

// The Options window: color scheme, text size, the longest chain that is added, and where drafts are kept.
// Changes apply at once and are saved to settings.json.
public static class OptionsPanel
{
    public static void Draw(Desk d, float screenW, float screenH)
    {
        if (d.OpenOptions) { ImGui.OpenPopup("Options"); d.OpenOptions = false; }
        ImGui.SetNextWindowPos(new Vector2(screenW / 2f, screenH / 2f), ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
        ImGui.SetNextWindowSize(new Vector2(Math.Min(560f * Math.Max(1f, Fonts.Scale), screenW - 40f), 0f));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(22, 18));
        bool open = ImGui.BeginPopupModal("Options", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoSavedSettings);
        ImGui.PopStyleVar();
        if (!open) return;

        AppSettings s = d.Settings;

        // Color scheme: one swatch button per scheme.
        Ui.Heading("Colors");
        foreach (Palette p in Theme.All)
        {
            bool on = s.Theme == p.Name;
            if (DrawSwatch(p, on)) { s.Theme = p.Name; s.Save(); }
            ImGui.SameLine();
        }
        ImGui.NewLine();

        // Text size: four steps. The fonts are rebuilt at the new size.
        ImGui.Dummy(new Vector2(0, 6));
        Ui.Heading("Text size");
        foreach (var (name, _) in Fonts.Sizes)
        {
            if (ImGui.RadioButton(name, s.TextSize == name)) { s.TextSize = name; s.Save(); }
            ImGui.SameLine(0, 14);
        }
        ImGui.NewLine();

        // Chain length limit.
        ImGui.Dummy(new Vector2(0, 6));
        Ui.Heading("Chains");
        bool useFile = s.ChainLimit <= 0;
        if (ImGui.Checkbox("Use the limit stored in each thread file (normally 6)", ref useFile))
        {
            s.ChainLimit = useFile ? 0 : (d.Data?.MaxChain ?? 6);
            s.Save();
        }
        if (!useFile)
        {
            int limit = s.ChainLimit;
            ImGui.SetNextItemWidth(260);
            if (ImGui.SliderInt("comments at most", ref limit, 2, 15)) { s.ChainLimit = limit; s.Save(); }
        }
        ImGui.PushStyleColor(ImGuiCol.Text, Theme.Mut);
        ImGui.PushTextWrapPos(ImGui.GetWindowWidth() - 24f);
        ImGui.TextUnformatted("The longest chain that + chain and Add all chains build from a head comment. The Chains table in the console still shows the levels the userscript found.");
        ImGui.PopTextWrapPos();
        ImGui.PopStyleColor();

        // Where drafts are kept.
        ImGui.Dummy(new Vector2(0, 6));
        Ui.Heading("Saved data");
        ImGui.PushTextWrapPos(ImGui.GetWindowWidth() - 24f);
        string roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string shown = Draft.DataFolder.StartsWith(roaming, StringComparison.OrdinalIgnoreCase) ? "%AppData%" + Draft.DataFolder[roaming.Length..] : Draft.DataFolder;
        ImGui.TextUnformatted(shown);
        ImGui.PopTextWrapPos();
        ImGui.PushStyleColor(ImGuiCol.Text, Theme.Mut);
        ImGui.PushTextWrapPos(ImGui.GetWindowWidth() - 24f);
        ImGui.TextUnformatted("Drafts are in the drafts folder (one file per thread); settings, header presets and the template are in settings.json.");
        ImGui.PopTextWrapPos();
        ImGui.PopStyleColor();
        if (ImGui.Button("Open folder")) d.OpenFolder(Draft.DataFolder);

        ImGui.Dummy(new Vector2(0, 8));
        ImGui.Separator();
        ImGui.Dummy(new Vector2(0, 4));
        if (ImGui.Button("Close", new Vector2(110, 0))) ImGui.CloseCurrentPopup();
        ImGui.SameLine();
        if (ImGui.Button("Open the guide", new Vector2(150, 0))) { ImGui.CloseCurrentPopup(); d.ShowGuide = true; }
        ImGui.EndPopup();
    }

    // A small preview tile of a scheme (its panel color, an accent bar and its name). Returns true when clicked.
    private static bool DrawSwatch(Palette p, bool selected)
    {
        Vector2 size = new Vector2(112, 62);
        Vector2 a = ImGui.GetCursorScreenPos();
        bool clicked = ImGui.InvisibleButton("sw" + p.Name, size);
        bool hover = ImGui.IsItemHovered();
        ImDrawListPtr dl = ImGui.GetWindowDrawList();
        Vector2 b = new Vector2(a.X + size.X, a.Y + size.Y);
        dl.AddRectFilled(a, b, Theme.U(Theme.Hex(p.Bg)), 6f);
        dl.AddRectFilled(new Vector2(a.X, a.Y), new Vector2(a.X + 30, b.Y), Theme.U(Theme.Hex(p.Pn)), 6f, ImDrawFlags.RoundCornersLeft);
        dl.AddRectFilled(new Vector2(a.X + 40, a.Y + 12), new Vector2(a.X + 100, a.Y + 18), Theme.U(Theme.Hex(p.Acc)), 3f);
        dl.AddRectFilled(new Vector2(a.X + 40, a.Y + 24), new Vector2(a.X + 90, a.Y + 29), Theme.U(Theme.Hex(p.Tx)), 2f);
        dl.AddRectFilled(new Vector2(a.X + 40, a.Y + 33), new Vector2(a.X + 80, a.Y + 38), Theme.U(Theme.Hex(p.Mut)), 2f);
        dl.AddText(new Vector2(a.X + 40, b.Y - 20), Theme.U(Theme.Hex(p.Tx)), p.Name);
        dl.AddRect(a, b, Theme.U(selected ? Theme.Acc : hover ? Theme.Mut : Theme.Ln), 6f, ImDrawFlags.None, selected ? 2.5f : 1f);
        return clicked;
    }
}
