using System.Numerics;
using ImGuiNET;

// Bottom strip: one table per tab. Hot spots and chains (with an Add chain button each), the category winners
// (Crowd Magnet, Duels, OP) and the heated comments, each with a jump button.
public static class ConsolePanel
{
    public static void Draw(Desk d, Vector2 pos, Vector2 size)
    {
        ImGui.SetNextWindowPos(pos);
        ImGui.SetNextWindowSize(size);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, Theme.Pn);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
        ImGui.Begin("console", Panels.Fixed);
        ImGui.GetWindowDrawList().AddLine(pos, new Vector2(pos.X + size.X, pos.Y), Theme.U(Theme.Ln));

        ThreadData? data = d.Data;
        if (data != null)
        {
            // Tab strip.
            ImGui.SetCursorPos(new Vector2(14, 8));
            var tabs = new List<(string key, string label)>();
            if (data.Scored) tabs.Add(("hot", "Hot spots " + data.HotRows.Count));
            tabs.AddRange(new List<(string key, string label)>
            {
                ("chains", "Chains " + data.ChainRows.Count),
                ("crowd", "Crowd Magnet " + data.Strip.Crowd.Count),
                ("duels", "Duels " + data.Strip.Duels.Count),
            });
            if (!string.IsNullOrEmpty(data.Op)) tabs.Add(("op", "OP " + data.Strip.Op.Count));
            tabs.Add(("heated", "Heated " + data.Strip.Heated));
            for (int i = 0; i < tabs.Count; i++)
            {
                if (i > 0) ImGui.SameLine(0, 20);
                if (Ui.TabLabel("ct" + tabs[i].key, tabs[i].label.ToUpperInvariant(), d.ConsoleTab == tabs[i].key)) d.ConsoleTab = tabs[i].key;
            }
            ImGui.SetCursorPos(new Vector2(0, 34));
            ImGui.GetWindowDrawList().AddLine(new Vector2(pos.X, pos.Y + 34), new Vector2(pos.X + size.X, pos.Y + 34), Theme.U(Theme.Ln));

            ImGui.BeginChild("cbody", Vector2.Zero, ImGuiChildFlags.None, ImGuiWindowFlags.None);
            ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vector2(14, 5));
            if (d.ConsoleTab == "hot") DrawHot(d, data);
            else if (d.ConsoleTab == "chains") DrawChains(d, data);
            else if (d.ConsoleTab == "heated") DrawHeated(d, data);
            else DrawStrip(d, d.ConsoleTab == "crowd" ? data.Strip.Crowd : d.ConsoleTab == "duels" ? data.Strip.Duels : data.Strip.Op);
            ImGui.PopStyleVar();
            ImGui.EndChild();
        }

        ImGui.End();
        ImGui.PopStyleVar();
        ImGui.PopStyleColor();
    }

    private const ImGuiTableFlags TableFlags = ImGuiTableFlags.BordersInnerH | ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.ScrollY | ImGuiTableFlags.RowBg | ImGuiTableFlags.PadOuterX;

    private static void DrawChains(Desk d, ThreadData data)
    {
        if (data.ChainRows.Count == 0) { ImGui.Dummy(new Vector2(0, 6)); ImGui.TextColored(Theme.Mut, "  None found."); return; }
        if (!ImGui.BeginTable("chains", 7, TableFlags, Vector2.Zero)) return;
        Header("#", "Head", "Score", "Levels", "Comments", "People", "");
        for (int i = 0; i < data.ChainRows.Count; i++)
        {
            ChainRow r = data.ChainRows[i];
            ImGui.TableNextRow();
            ImGui.TableNextColumn(); ImGui.PushStyleColor(ImGuiCol.Text, Theme.Hot); ImGui.TextUnformatted((i + 1).ToString()); ImGui.PopStyleColor();
            ImGui.TableNextColumn(); Ui.Uname(r.Head.Author);
            ImGui.TableNextColumn(); Ui.Score(r.Head.Score);
            ImGui.TableNextColumn(); ImGui.TextUnformatted(r.Levels.ToString());
            ImGui.TableNextColumn(); ImGui.TextUnformatted(r.Comments.ToString());
            ImGui.TableNextColumn(); ImGui.TextUnformatted(r.People.ToString());
            ImGui.TableNextColumn();
            if (ImGui.SmallButton("jump##j" + r.Head.Id)) d.Jump(r.Head.Id);
            ImGui.SameLine();
            if (ImGui.SmallButton("Add chain##a" + r.Head.Id)) d.AddChainFrom(r.Head);
        }
        ImGui.EndTable();
    }

    // The hot spots, best first: where the drama score says the fights are, with what the score is made of.
    private static void DrawHot(Desk d, ThreadData data)
    {
        if (data.HotRows.Count == 0) { ImGui.Dummy(new Vector2(0, 6)); ImGui.TextColored(Theme.Mut, "  None found."); return; }
        if (!ImGui.BeginTable("hot", 6, TableFlags, Vector2.Zero)) return;
        // Why stretches and clips, so the buttons on the right always stay visible.
        foreach (string c in new[] { "#", "Head", "Score", "Drama" }) ImGui.TableSetupColumn(c, ImGuiTableColumnFlags.WidthFixed, 0f);
        ImGui.TableSetupColumn("Why", ImGuiTableColumnFlags.WidthStretch, 1f);
        ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed, 0f);
        ImGui.TableSetupScrollFreeze(0, 1);
        ImGui.TableHeadersRow();
        HashSet<string> used = d.Draft.UsedLinks();
        foreach (Node n in data.HotRows)
        {
            // A skipped row is drawn faded in place, buttons included, so the order never changes.
            bool skipped = d.SpotSkipped(n);
            bool byTree = skipped && !d.Draft.IsSkipped(n.Link);
            ImGui.TableNextRow();
            if (skipped) ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.4f);
            ImGui.TableNextColumn(); ImGui.PushStyleColor(ImGuiCol.Text, n.Hot <= 3 ? Theme.Hot : Theme.Warn); ImGui.TextUnformatted(n.Hot.ToString()); ImGui.PopStyleColor();
            ImGui.TableNextColumn(); Ui.Uname(n.Author);
            ImGui.TableNextColumn(); Ui.Score(n.Score);
            ImGui.TableNextColumn(); ImGui.TextUnformatted(Math.Round(n.D, MidpointRounding.AwayFromZero).ToString());
            ImGui.TableNextColumn(); ImGui.TextColored(Theme.Mut, ThreadData.Why(n));
            ImGui.TableNextColumn();
            if (ImGui.SmallButton("jump##hj" + n.Id)) d.Jump(n.Id);
            ImGui.SameLine();
            if (used.Contains(n.Link)) ImGui.TextColored(Theme.Ok, "in draft");
            else if (ImGui.SmallButton("Add chain##ha" + n.Id)) d.AddChainFrom(n);
            ImGui.SameLine();
            // Skipping a whole tree covers its spots; those are taken back from the tree, not here.
            if (byTree) ImGui.TextColored(Theme.Mut, "tree skipped");
            else if (ImGui.SmallButton((skipped ? "unskip" : "skip") + "##hs" + n.Id)) d.Draft.ToggleSkip(n.Link);
            if (skipped) ImGui.PopStyleVar();
        }
        ImGui.EndTable();
    }

    private static void DrawHeated(Desk d, ThreadData data)
    {
        var rows = data.Everything.Where(n => n.Heat > 0).ToList();
        if (rows.Count == 0) { ImGui.Dummy(new Vector2(0, 6)); ImGui.TextColored(Theme.Mut, "  None found."); return; }
        if (!ImGui.BeginTable("heated", 5, TableFlags, Vector2.Zero)) return;
        Header("Who", "Heat", "Words", "Comment", "");
        foreach (Node n in rows)
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn(); Ui.Uname(n.Author);
            ImGui.TableNextColumn(); ImGui.TextUnformatted("x" + n.Heat);
            ImGui.TableNextColumn(); ImGui.TextUnformatted(string.Join(", ", n.Words));
            ImGui.TableNextColumn(); ImGui.TextUnformatted(Snip(n.Text, 90));
            ImGui.TableNextColumn();
            if (ImGui.SmallButton("jump##h" + n.Id)) d.Jump(n.Id);
        }
        ImGui.EndTable();
    }

    private static void DrawStrip(Desk d, List<StripItem> items)
    {
        if (items.Count == 0) { ImGui.Dummy(new Vector2(0, 6)); ImGui.TextColored(Theme.Mut, "  None found."); return; }
        if (!ImGui.BeginTable("strip", 2, TableFlags, Vector2.Zero)) return;
        foreach (StripItem it in items)
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn(); ImGui.TextUnformatted(it.T);
            ImGui.TableNextColumn();
            if (ImGui.SmallButton("jump##s" + it.Id)) d.Jump(it.Id);
        }
        ImGui.EndTable();
    }

    private static void Header(params string[] names)
    {
        foreach (string n in names) ImGui.TableSetupColumn(n, ImGuiTableColumnFlags.WidthFixed, 0f);
        ImGui.TableSetupScrollFreeze(0, 1);
        ImGui.TableHeadersRow();
    }

    private static string Snip(string text, int max)
    {
        string flat = System.Text.RegularExpressions.Regex.Replace(text ?? "", @"\s+", " ").Trim();
        return flat.Length <= max ? flat : flat[..max] + "...";
    }
}
