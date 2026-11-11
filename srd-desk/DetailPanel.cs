using System.Numerics;
using ImGuiNET;

// Middle column: the selected tree as nested comments. Each comment has a fold arrow, a username badge, its
// score and tags, the text, and (on hover) pick, link, "+ chain" and a link to reddit.
public static class DetailPanel
{
    private const float BodyIndent = 26f;
    private const float KidIndent = 16f;
    private const float MaxBodyWidth = 640f;
    private const int ClampLines = 8;

    public static void Draw(Desk d, Vector2 pos, Vector2 size)
    {
        ImGui.SetNextWindowPos(pos);
        ImGui.SetNextWindowSize(size);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(22, 0));
        ImGui.Begin("detail", Panels.Fixed);

        ThreadData? data = d.Data;
        if (data == null) { DrawWelcome(d); }
        else
        {
            Node root = data.Roots[d.SelectedTree];
            DrawHeader(d, data, root);
            ImGui.BeginChild("scroll", Vector2.Zero, ImGuiChildFlags.None, ImGuiWindowFlags.None);
            ImGui.Dummy(new Vector2(0, 4));
            DrawNode(d, data, root);
            // Room below the last comment so a jump can always bring a comment to the top of the pane.
            ImGui.Dummy(new Vector2(0, Math.Max(60f, ImGui.GetWindowHeight() * 0.7f)));
            ImGui.EndChild();
        }

        ImGui.End();
        ImGui.PopStyleVar();
    }

    // Tree name, size chips and the fold buttons, fixed above the scrolling comments.
    private static void DrawHeader(Desk d, ThreadData data, Node root)
    {
        ImGui.Dummy(new Vector2(0, 2));
        ImGui.AlignTextToFramePadding();
        TreeInfo t = data.Trees[d.SelectedTree];
        ImGui.TextUnformatted("Tree " + (d.SelectedTree + 1));
        ImGui.SameLine(0, 10);
        Ui.Chip(t.Count + " comments", Theme.Mut);
        ImGui.SameLine(0, 6);
        Ui.Chip(t.People + " people", Theme.Mut);
        if (t.HotCount > 0) { ImGui.SameLine(0, 6); Ui.Chip(t.HotCount + (t.HotCount == 1 ? " hot spot" : " hot spots"), Theme.Hot); }

        float unfoldW = ImGui.CalcTextSize("Unfold all").X + ImGui.GetStyle().FramePadding.X * 2f;
        float foldW = ImGui.CalcTextSize("Fold replies").X + ImGui.GetStyle().FramePadding.X * 2f;
        ImGui.SameLine(ImGui.GetWindowWidth() - 22f - unfoldW - foldW - 8f);
        if (ImGui.Button("Fold replies")) foreach (Node k in root.Kids) d.Folded.Add(k.Id);
        ImGui.SameLine();
        if (ImGui.Button("Unfold all")) d.Folded.Clear();
        ImGui.Separator();
    }

    // The empty state: how to open a thread, plus the recent files.
    private static void DrawWelcome(Desk d)
    {
        ImGui.Dummy(new Vector2(0, 40));
        Assets.DrawMascot(96f);
        ImGui.Dummy(new Vector2(0, 8));
        ImGui.PushFont(Fonts.Label);
        ImGui.TextColored(Theme.Acc, "SRD DESK");
        ImGui.PopFont();
        ImGui.Dummy(new Vector2(0, 6));
        ImGui.TextWrapped("Drop a thread file (.srd.json) on this window, or press Open file. Export one from Reddit with the SRD Helper panel (old.reddit.com), button \"Export for app\".");
        ImGui.Dummy(new Vector2(0, 14));
        if (ImGui.Button("Open file...")) d.WantOpenDialog = true;
        if (d.Settings.Recent.Count > 0)
        {
            ImGui.Dummy(new Vector2(0, 18));
            Ui.Heading("Recent");
            foreach (string f in d.Settings.Recent.ToList())
            {
                if (!File.Exists(f)) continue;
                if (ImGui.Selectable(Path.GetFileName(f) + "##" + f)) d.Open(f);
                if (ImGui.IsItemHovered()) ImGui.SetTooltip(f);
            }
        }
    }

    // One comment, its replies below it indented, with a guide line down the replies.
    private static void DrawNode(Desk d, ThreadData data, Node n)
    {
        ImDrawListPtr dl = ImGui.GetWindowDrawList();
        if (d.ScrollTo == n.Id) { ImGui.SetScrollHereY(0f); d.ScrollTo = -1; }

        Vector2 top = ImGui.GetCursorScreenPos();
        float avail = ImGui.GetContentRegionAvail().X;
        float rowH = ImGui.GetFrameHeight();
        bool folded = d.Folded.Contains(n.Id);
        // AllowWhenBlockedByActiveItem: while a control on the row is pressed it is the active item, and without this flag the row
        // would stop counting as hovered, the controls would vanish before the mouse is released, and the click would be lost.
        bool rowHover = ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows | ImGuiHoveredFlags.AllowWhenBlockedByActiveItem) && ImGui.IsMouseHoveringRect(top, new Vector2(top.X + avail, top.Y + rowH));

        if (d.FlashId == n.Id && d.FlashLeft > 0)
            dl.AddRectFilled(top, new Vector2(top.X + avail, top.Y + rowH), Theme.U(Theme.WithAlpha(Theme.Acc, 0.35f * Math.Min(1f, d.FlashLeft))), 4f);

        // Header row: fold arrow, username, OP, score, tags.
        if (ImGui.InvisibleButton("fold" + n.Id, new Vector2(20, rowH))) { if (!d.Folded.Remove(n.Id)) d.Folded.Add(n.Id); }
        DrawArrow(dl, top, rowH, folded);
        ImGui.SameLine(0, 2);
        Ui.Uname(n.Author);
        if (n.Op) { ImGui.SameLine(0, 6); Ui.OpChip(); }
        ImGui.SameLine(0, 8);
        Ui.Score(n.Score);
        foreach (Tag t in n.Tags)
        {
            ImGui.SameLine(0, 6);
            if (t.K == "crowd") Ui.Chip(t.T, Theme.Acc);
            else if (t.K == "duel") Ui.Chip(t.T, Theme.Duel);
            else Ui.Chip(t.T, Theme.WithAlpha(Theme.Duel, 0.8f));
        }
        if (n.Hot > 0) { ImGui.SameLine(0, 6); Ui.Chip("HOT #" + n.Hot + "  drama " + Math.Round(n.D, MidpointRounding.AwayFromZero), n.Hot <= 3 ? Theme.Hot : Theme.Warn); }
        if (n.Heat > 0) { ImGui.SameLine(0, 6); Ui.Chip("heated x" + n.Heat, Theme.Hot); }
        if (d.Draft.UsedLinks().Contains(n.Link)) { ImGui.SameLine(0, 6); Ui.Chip("used", Theme.Ok); }
        if (folded && n.Kids.Count > 0) { ImGui.SameLine(0, 8); ImGui.AlignTextToFramePadding(); ImGui.TextColored(Theme.Mut, "(" + (ThreadData.Count(n) - 1) + " replies hidden)"); }
        if (rowHover || d.Picked.Contains(n.Id) || d.Linked.Contains(n.Id)) DrawControls(d, n, top, avail, rowH);

        if (folded) { ImGui.Dummy(new Vector2(0, 2)); return; }

        // Body.
        ImGui.Indent(BodyIndent);
        float wrap = Math.Min(ImGui.GetContentRegionAvail().X - 8f, MaxBodyWidth);
        if (wrap < 120f) wrap = 120f;
        bool clamped = Ui.Body(string.IsNullOrEmpty(n.Text) ? "[no text]" : n.Text, wrap, d.Expanded.Contains(n.Id) ? 0 : ClampLines, Theme.Tx);
        if (clamped) { if (Ui.Link("show more##m" + n.Id)) d.Expanded.Add(n.Id); }
        else if (d.Expanded.Contains(n.Id) && Ui.Link("show less##l" + n.Id)) d.Expanded.Remove(n.Id);
        ImGui.Unindent(BodyIndent);
        ImGui.Dummy(new Vector2(0, 4));

        // The edge marks the head of a hot spot (red for the top three, amber below) or, in older files, of a chain.
        if (n.Hot > 0 || n.Chain)
        {
            Vector4 edge = n.Hot > 3 ? Theme.Warn : Theme.Hot;
            dl.AddRectFilled(new Vector2(top.X, top.Y), new Vector2(top.X + (n.Hot > 0 ? 4f : 3f), ImGui.GetCursorScreenPos().Y - 4f), Theme.U(edge), 1.5f);
        }

        // Replies.
        if (n.Kids.Count > 0)
        {
            float y0 = ImGui.GetCursorScreenPos().Y;
            ImGui.Indent(KidIndent);
            foreach (Node k in n.Kids) DrawNode(d, data, k);
            ImGui.Unindent(KidIndent);
            float y1 = ImGui.GetCursorScreenPos().Y;
            dl.AddLine(new Vector2(top.X + 8f, y0), new Vector2(top.X + 8f, y1 - 4f), Theme.U(Theme.Ln));
        }
    }

    // A small triangle: pointing right when folded, down when open.
    private static void DrawArrow(ImDrawListPtr dl, Vector2 top, float rowH, bool folded)
    {
        Vector2 c = new Vector2(top.X + 9f, top.Y + rowH / 2f);
        uint col = Theme.U(Theme.Mut);
        if (folded) dl.AddTriangleFilled(new Vector2(c.X - 3f, c.Y - 5f), new Vector2(c.X - 3f, c.Y + 5f), new Vector2(c.X + 4f, c.Y), col);
        else dl.AddTriangleFilled(new Vector2(c.X - 5f, c.Y - 3f), new Vector2(c.X + 5f, c.Y - 3f), new Vector2(c.X, c.Y + 4f), col);
    }

    // Pick, link, "+ chain" and the reddit link, right-aligned on the header row.
    private static void DrawControls(Desk d, Node n, Vector2 top, float avail, float rowH)
    {
        var style = ImGui.GetStyle();
        float checkW(string s) => ImGui.GetFrameHeight() + style.ItemInnerSpacing.X + ImGui.CalcTextSize(s).X;
        float buttonW(string s) => ImGui.CalcTextSize(s).X + style.FramePadding.X * 2f;
        float total = checkW("pick") + checkW("link") + buttonW("+ chain") + buttonW("reddit") + style.ItemSpacing.X * 3f;
        ImGui.SameLine();
        ImGui.SetCursorScreenPos(new Vector2(top.X + avail - total, top.Y));

        bool pick = d.Picked.Contains(n.Id);
        if (ImGui.Checkbox("pick##p" + n.Id, ref pick)) { if (pick) d.Picked.Add(n.Id); else { d.Picked.Remove(n.Id); d.Linked.Remove(n.Id); } }
        ImGui.SameLine();
        bool link = d.Linked.Contains(n.Id);
        if (ImGui.Checkbox("link##k" + n.Id, ref link))
        {
            if (link) { d.Linked.Add(n.Id); d.Picked.Add(n.Id); } else d.Linked.Remove(n.Id);
        }
        ImGui.SameLine();
        if (ImGui.Button("+ chain##c" + n.Id)) d.AddChainFrom(n);
        ImGui.SameLine();
        if (ImGui.Button("reddit##r" + n.Id)) d.OpenUrl(n.Link);
    }
}
