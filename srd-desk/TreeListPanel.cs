using System.Numerics;
using ImGuiNET;

// Left column: one card per comment tree. Selecting a card shows that tree in the middle.
// With a scored file (version 2) the list starts on the trees that hold a hot spot, best first; "All" shows every tree
// in thread order (a whole thread can have hundreds).
public static class TreeListPanel
{
    private const float CardH = 74f;

    public static void Draw(Desk d, Vector2 pos, Vector2 size)
    {
        ImGui.SetNextWindowPos(pos);
        ImGui.SetNextWindowSize(size);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, Theme.Pn);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(8, 8));
        ImGui.Begin("list", Panels.Fixed ^ ImGuiWindowFlags.NoScrollbar ^ ImGuiWindowFlags.NoScrollWithMouse);
        ImDrawListPtr dl = ImGui.GetWindowDrawList();
        dl.AddLine(new Vector2(pos.X + size.X - 1, pos.Y), new Vector2(pos.X + size.X - 1, pos.Y + size.Y), Theme.U(Theme.Ln));

        ThreadData? data = d.Data;
        if (data != null)
        {
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 4);
            Ui.Heading("Trees");
            bool hasHot = data.HotRows.Count > 0;
            if (hasHot)
            {
                // Filter: the trees with hot spots, or all of them.
                int hotTrees = data.Trees.Count(t => t.HotCount > 0);
                ImGui.SameLine(0, 10);
                if (Ui.TabLabel("thot", "HOT " + hotTrees, d.HotTreesOnly)) d.HotTreesOnly = true;
                ImGui.SameLine(0, 10);
                if (Ui.TabLabel("tall", "ALL " + data.Trees.Count, !d.HotTreesOnly)) d.HotTreesOnly = false;
            }
            IEnumerable<TreeInfo> list = hasHot && d.HotTreesOnly
                ? data.Trees.Where(t => t.HotCount > 0).OrderBy(t => t.BestHot)
                : data.Trees;
            int maxCount = Math.Max(1, data.Trees.Max(t => t.Count));
            // Cards outside the visible area are skipped (only their space is kept), so long lists stay quick.
            float top = ImGui.GetScrollY(), bottom = top + ImGui.GetWindowHeight();
            foreach (TreeInfo t in list)
            {
                float y = ImGui.GetCursorPosY();
                if (y + CardH < top || y > bottom) { ImGui.Dummy(new Vector2(0, CardH)); ImGui.Dummy(new Vector2(0, 2f)); continue; }
                DrawCard(d, data, t, maxCount, dl);
            }
        }

        ImGui.End();
        ImGui.PopStyleVar();
        ImGui.PopStyleColor();
    }

    private static void DrawCard(Desk d, ThreadData data, TreeInfo t, int maxCount, ImDrawListPtr dl)
    {
        Node root = data.Roots[t.Index];
        float w = ImGui.GetContentRegionAvail().X;
        Vector2 p = ImGui.GetCursorScreenPos();
        if (ImGui.InvisibleButton("tree" + t.Index, new Vector2(w, CardH))) d.SelectTree(t.Index);
        bool hover = ImGui.IsItemHovered();
        bool on = t.Index == d.SelectedTree;
        Vector2 q = new Vector2(p.X + w, p.Y + CardH);

        if (on)
        {
            dl.AddRectFilled(p, q, Theme.U(Theme.Bg), 8f);
            dl.AddRect(p, q, Theme.U(Theme.Ln), 8f);
            dl.AddRectFilled(p, new Vector2(p.X + 3f, q.Y), Theme.U(Theme.Acc), 8f, ImDrawFlags.RoundCornersLeft);
        }
        else if (hover) dl.AddRectFilled(p, q, Theme.U(Theme.Pn2), 8f);

        // Top line: who started it, and the score on the right.
        string who = root.Author + (root.Op ? " (OP)" : "");
        dl.AddText(ImGui.GetFont(), ImGui.GetFontSize(), new Vector2(p.X + 12f, p.Y + 8f), Theme.U(Theme.Tx), who);
        string score = Ui.ScoreText(root.Score);
        ImGui.PushFont(Fonts.Label);
        Vector2 ss = ImGui.CalcTextSize(score);
        dl.AddText(new Vector2(q.X - 12f - ss.X, p.Y + 10f), Theme.U(root.Score < 0 ? Theme.Hot : Theme.Mut), score);

        // Second line: the best hot spot (when there is one), then size, people, chains.
        float x = p.X + 12f;
        dl.PushClipRect(new Vector2(p.X + 12f, p.Y), new Vector2(q.X - 8f, q.Y), true);
        if (t.BestHot > 0)
        {
            string hot = "HOT #" + t.BestHot;
            Vector2 hs = ImGui.CalcTextSize(hot);
            Vector4 col = t.BestHot <= 3 ? Theme.Hot : Theme.Warn;
            dl.AddRectFilled(new Vector2(x - 2f, p.Y + 31f), new Vector2(x + hs.X + 6f, p.Y + 33f + hs.Y), Theme.U(Theme.WithAlpha(col, 0.22f)), 4f);
            dl.AddText(new Vector2(x + 2f, p.Y + 32f), Theme.U(col), hot);
            x += hs.X + 14f;
        }
        string meta = (t.BestHot > 0 ? "" : "Tree " + (t.Index + 1) + "  ") + t.Count + " comments  " + t.People + " people" +
            (t.Chains > 0 ? "  " + t.Chains + (t.Chains == 1 ? " chain" : " chains") : "");
        dl.AddText(new Vector2(x, p.Y + 32f), Theme.U(Theme.Mut), meta);
        dl.PopClipRect();
        ImGui.PopFont();

        // Size bar: how big this tree is compared with the biggest one.
        float barW = w - 24f;
        Vector2 b0 = new Vector2(p.X + 12f, p.Y + CardH - 14f);
        dl.AddRectFilled(b0, new Vector2(b0.X + barW, b0.Y + 4f), Theme.U(Theme.Ln), 2f);
        dl.AddRectFilled(b0, new Vector2(b0.X + barW * t.Count / maxCount, b0.Y + 4f), Theme.U(t.BestHot > 0 && t.BestHot <= 3 ? Theme.Hot : Theme.Acc), 2f);
        ImGui.Dummy(new Vector2(0, 2f));
    }
}
