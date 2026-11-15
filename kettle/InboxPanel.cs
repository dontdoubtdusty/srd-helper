using System.Numerics;
using ImGuiNET;

// The New threads list: exports found in the watched folders, best drama first. Shown on the start screen,
// and in a popup from the top bar's "new" button while a thread is open.
public static class InboxPanel
{
    // The list rows: title, then subreddit, meter and hot spots; open on click, dismiss on the right.
    public static void DrawList(Desk d)
    {
        foreach (Inbox.Item it in d.Inbox.Items.ToList())
        {
            float lh = ImGui.GetTextLineHeight();
            float rowH = lh * 2f + 6f;
            Vector2 p = ImGui.GetCursorScreenPos();
            float w = ImGui.GetContentRegionAvail().X;
            float dismissW = ImGui.CalcTextSize("dismiss").X + ImGui.GetStyle().FramePadding.X * 2f;
            if (ImGui.Selectable("##in" + it.Path, false, ImGuiSelectableFlags.AllowOverlap, new Vector2(w - dismissW - 8f, rowH))) { d.OpenFromInbox(it); return; }
            if (ImGui.IsItemHovered()) ImGui.SetTooltip(it.Path + "\nOpening moves it into Kettle's threads folder.");
            ImDrawListPtr dl = ImGui.GetWindowDrawList();
            dl.PushClipRect(p, new Vector2(p.X + w - dismissW - 12f, p.Y + rowH), true);
            dl.AddText(new Vector2(p.X + 4f, p.Y + 1f), Theme.U(Theme.Tx), it.Info.Title.Length > 0 ? it.Info.Title : Path.GetFileName(it.Path));
            var parts = new List<string>();
            if (it.Info.Sub.Length > 0) parts.Add(it.Info.Sub);
            if (it.Info.Pct != null) parts.Add("drama " + Math.Round(it.Info.Pct.Value, MidpointRounding.AwayFromZero) + "%" + (it.Info.Plus ? "+" : "") + (it.Info.Tier.Length > 0 ? " (" + it.Info.Tier + ")" : ""));
            else parts.Add("no meter reading");
            if (it.HotSpots > 0) parts.Add(it.HotSpots + (it.HotSpots == 1 ? " hot spot" : " hot spots"));
            dl.AddText(new Vector2(p.X + 4f, p.Y + lh + 3f), Theme.U(Theme.Mut), string.Join("  ·  ", parts));
            dl.PopClipRect();
            ImGui.SameLine(w - dismissW + ImGui.GetCursorStartPos().X);
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + (rowH - ImGui.GetFrameHeight()) / 2f);
            if (Ui.Link("dismiss##dm" + it.Path)) { d.DismissFromInbox(it); return; }
            if (ImGui.IsItemHovered()) ImGui.SetTooltip("Hide this file from the list. It stays where it is.");
        }
    }

    // The top bar popup with the same list.
    public static void DrawPopup(Desk d, float screenW)
    {
        if (d.ShowInbox) { ImGui.OpenPopup("New threads"); d.ShowInbox = false; }
        ImGui.SetNextWindowPos(new Vector2(screenW - 20f, 50f), ImGuiCond.Appearing, new Vector2(1f, 0f));
        ImGui.SetNextWindowSize(new Vector2(Math.Min(520f * Math.Max(1f, Fonts.Scale), screenW - 40f), 0f));
        if (!ImGui.BeginPopup("New threads")) return;
        Ui.Heading("New threads");
        if (d.Inbox.Items.Count == 0) ImGui.TextColored(Theme.Mut, "Nothing new.");
        DrawList(d);
        ImGui.EndPopup();
    }
}
