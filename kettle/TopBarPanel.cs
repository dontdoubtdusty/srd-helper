using System.Numerics;
using ImGuiNET;

// Shared window flags for the five fixed panels.
public static class Panels
{
    public const ImGuiWindowFlags Fixed = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse |
        ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
}

// The strip along the top: app name, the meter result, the thread title, the status line and the open buttons.
public static class TopBarPanel
{
    public static void Draw(Desk d, float w, float h)
    {
        ImGui.SetNextWindowPos(Vector2.Zero);
        ImGui.SetNextWindowSize(new Vector2(w, h));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(14, 5));
        ImGui.PushStyleColor(ImGuiCol.WindowBg, Theme.Pn);
        ImGui.Begin("top", Panels.Fixed);

        // Mascot, then the app name.
        Assets.DrawMascot(26f);
        if (Assets.MascotWidth(26f) > 0f) ImGui.SameLine(0, 8);
        ImGui.PushFont(Fonts.Label);
        ImGui.AlignTextToFramePadding();
        ImGui.TextColored(Theme.Acc, "KETTLE");
        ImGui.PopFont();
        ImGui.SameLine(0, 14);

        ThreadData? data = d.Data;
        if (data?.Meter != null)
        {
            string pct = data.Meter.Pct.HasValue ? Math.Round(data.Meter.Pct.Value) + "% " : "";
            Ui.Chip(pct + data.Meter.Text, Ui.TierColor(data.Meter.Tier));
            if (ImGui.IsItemHovered() && data.Meter.Why.Length > 0) ImGui.SetTooltip(data.Meter.Why + (data.Meter.Plus ? "\nNot every comment was loaded when this was exported." : ""));
            ImGui.SameLine(0, 14);
        }

        // Buttons on the right, then the status line before them; the title takes what is left.
        float pad = ImGui.GetStyle().FramePadding.X * 2f;
        float openW = ImGui.CalcTextSize("Open file...").X + pad;
        float origW = ImGui.CalcTextSize("Original thread").X + pad;
        string postedLabel = d.Draft.Posted ? "Posted" : "Mark posted";
        float postedW = ImGui.CalcTextSize(postedLabel).X + pad;
        float guideW = ImGui.CalcTextSize("Guide").X + pad;
        float optW = ImGui.CalcTextSize("Options").X + pad;
        // "N new" only while a thread is open; the start screen lists them itself.
        string newLabel = d.Inbox.Items.Count + " new";
        bool showNew = data != null && d.Inbox.Items.Count > 0;
        float newW = showNew ? ImGui.CalcTextSize(newLabel).X + pad + 8f : 0f;
        float homeW = ImGui.CalcTextSize("Home").X + pad;
        float rightX = w - 14f - openW - (data != null ? homeW + origW + postedW + 24f : 0f) - guideW - optW - 16f - newW;
        float statusW = 230f;
        float titleW = Math.Max(60f, rightX - statusW - ImGui.GetCursorPosX() - 10f);
        ImGui.AlignTextToFramePadding();
        if (data != null)
        {
            ImGui.TextUnformatted(Fit(data.Title, titleW));
            if (ImGui.IsItemHovered()) ImGui.SetTooltip(data.Title);
            ImGui.SameLine();
            ImGui.SetCursorPosX(ImGui.GetCursorPosX());
        }
        ImGui.SameLine(rightX - statusW);
        ImGui.AlignTextToFramePadding();
        ImGui.PushFont(Fonts.Label);
        ImGui.TextColored(Theme.Acc, Fit(d.Status, statusW - 10f));
        ImGui.PopFont();
        ImGui.SameLine(rightX);
        if (showNew)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, Theme.WithAlpha(Theme.Hot, 0.35f));
            if (ImGui.Button(newLabel)) d.ShowInbox = true;
            ImGui.PopStyleColor();
            if (ImGui.IsItemHovered()) ImGui.SetTooltip("New thread exports in your watched folders");
            ImGui.SameLine();
        }
        // Back to the start screen (recent files, new threads); the draft stays saved.
        if (data != null)
        {
            if (ImGui.Button("Home")) d.CloseThread();
            if (ImGui.IsItemHovered()) ImGui.SetTooltip("Close this thread and go back to the start screen (the draft is kept)");
            ImGui.SameLine();
        }
        if (ImGui.Button("Open file...")) d.WantOpenDialog = true;
        if (data != null)
        {
            // Marks the whole thread as one that became a real SRD post (green while set; click again to take it off).
            ImGui.SameLine();
            bool posted = d.Draft.Posted;
            if (posted) ImGui.PushStyleColor(ImGuiCol.Button, Theme.WithAlpha(Theme.Ok, 0.45f));
            if (ImGui.Button(postedLabel)) d.TogglePosted();
            if (posted) ImGui.PopStyleColor();
            if (ImGui.IsItemHovered()) ImGui.SetTooltip(posted
                ? "This thread became a real SRD post. Click to take the mark off."
                : "Mark this thread as one that became a real SRD post.\nmeasure.js weighs posted threads more than plain drama labels.");
            ImGui.SameLine();
            // Left click opens the thread on Reddit; right click copies its link.
            if (ImGui.Button("Original thread")) d.OpenUrl(data.ThreadUrl);
            if (ImGui.IsItemClicked(ImGuiMouseButton.Right)) { ImGui.SetClipboardText(data.ThreadUrl); d.SetStatus("Thread link copied"); }
            if (ImGui.IsItemHovered()) ImGui.SetTooltip("Open the thread on Reddit (right click: copy the link)");
        }
        ImGui.SameLine();
        if (ImGui.Button("Guide")) d.ShowGuide = !d.ShowGuide;
        if (ImGui.IsItemHovered()) ImGui.SetTooltip("How to use Kettle (F1)");
        ImGui.SameLine();
        if (ImGui.Button("Options")) d.OpenOptions = true;

        ImGui.End();
        ImGui.PopStyleColor();
        ImGui.PopStyleVar();
    }

    // Shorten text with "..." so it fits a pixel width.
    private static string Fit(string text, float width)
    {
        if (string.IsNullOrEmpty(text) || ImGui.CalcTextSize(text).X <= width) return text;
        int lo = 0, hi = text.Length;
        while (lo < hi)
        {
            int mid = (lo + hi + 1) / 2;
            if (ImGui.CalcTextSize(text[..mid] + "...").X <= width) lo = mid; else hi = mid - 1;
        }
        return text[..lo] + "...";
    }
}
