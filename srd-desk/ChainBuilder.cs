using System.Text.RegularExpressions;

// One comment of a chain, as the markdown builder needs it.
public record ChainItem(int Depth, string Text, string Url, bool Linked);

// Builds the post markdown for one chain. This is a straight port of buildChain in the userscript, so the
// text you copy is identical to what the old focus view produced.
public static class ChainBuilder
{
    // Items are in thread order. Nesting level is each comment's depth relative to the shallowest one.
    public static string Build(List<ChainItem> items)
    {
        if (items.Count == 0) return "";
        int minDepth = items.Min(i => i.Depth);
        var lines = new List<string>();
        int prevLevel = 0;
        for (int idx = 0; idx < items.Count; idx++)
        {
            ChainItem it = items[idx];
            int level = it.Depth - minDepth + 1;
            string prefix = new string('>', level) + " ";
            List<string> paras = Regex.Split(it.Text, @"\n\n+")
                .Select(p => Regex.Replace(p, @"\s+", " ").Trim())
                .Where(p => p.Length > 0).ToList();
            if (idx > 0) lines.Add(new string('>', Math.Min(prevLevel, level)));
            if (it.Linked)
            {
                // A linked quote is one line: the comment text is the link text.
                lines.Add(prefix + "[" + string.Join(" ", paras) + "](" + it.Url + ")");
            }
            else
            {
                for (int i = 0; i < paras.Count; i++)
                {
                    if (i > 0) lines.Add(new string('>', level));
                    lines.Add(prefix + paras[i]);
                }
            }
            prevLevel = level;
        }
        return string.Join("\n", lines);
    }
}
