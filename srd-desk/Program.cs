using System.Runtime.InteropServices;

public class Program
{
    // STAThread is needed by the Windows Open dialog.
    [STAThread]
    static void Main(string[] args)
    {
        try
        {
            // --selftest file: write the chain markdown for a fixed sample and exit (used to compare with the userscript).
            int st = Array.IndexOf(args, "--selftest");
            if (st >= 0 && st + 1 < args.Length) { SelfTest.Run(args[st + 1]); return; }

            var app = new DeskApp();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--shot" && i + 1 < args.Length) app.ShotPath = args[++i];
                else if (args[i] == "--script" && i + 1 < args.Length) app.Script = args[++i];
                else if (!args[i].StartsWith("--")) app.OpenPath = args[i];
            }
            app.Run();
        }
        catch (Exception e)
        {
            ReportCrash(e);
        }
    }

    // There is no console window, so a crash would otherwise close the app without a word.
    // This saves the error to crash.txt next to the app and shows it in a Windows message box.
    private static void ReportCrash(Exception e)
    {
        string file = Path.Combine(AppContext.BaseDirectory, "crash.txt");
        string report = $"{DeskApp.Version} crashed at {DateTime.Now}\n\n{e}";
        try { File.WriteAllText(file, report); }
        catch (Exception) { file = "(could not be saved)"; }

        const uint ErrorIcon = 0x10;
        MessageBoxW(IntPtr.Zero, $"{e.Message}\n\nThe details were saved to:\n{file}", "SRD Desk crashed", ErrorIcon);
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int MessageBoxW(IntPtr owner, string text, string caption, uint type);
}

// A fixed chain run through ChainBuilder, written to a file so it can be compared with the userscript's buildChain.
public static class SelfTest
{
    public static void Run(string outFile)
    {
        var items = new List<ChainItem>
        {
            new(2, "You are making this about yourself.\n\nA seating chart is not an insult.", "https://www.reddit.com/r/x/comments/abc/c/1", true),
            new(3, "It is not the chart.   I was put at the\nkids table.", "https://www.reddit.com/r/x/comments/abc/c/2", false),
            new(4, "Paying does not buy you a seat.\n\nThat is called a gift.", "https://www.reddit.com/r/x/comments/abc/c/3", false),
            new(3, "Show the message.", "https://www.reddit.com/r/x/comments/abc/c/4", true),
        };
        File.WriteAllText(outFile, ChainBuilder.Build(items));
    }
}
