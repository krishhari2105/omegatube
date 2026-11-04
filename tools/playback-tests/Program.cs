using System;
using OmegaTube.Ui;

internal static class Program
{
    private static int failures;
    private static int tests;

    private static void Check(string name,
        params (long Time, long Position, bool Retrying, bool PlaybackExpected, bool Visible)[] frames)
    {
        tests++;
        var recovery = new PlaybackRecovery();
        foreach (var frame in frames)
        {
            bool actual = recovery.Update(frame.Retrying, frame.PlaybackExpected, frame.Position, frame.Time);
            if (actual != frame.Visible || recovery.Visible != frame.Visible)
            {
                Console.Error.WriteLine($"FAIL {name} at {frame.Time} ms: expected visible={frame.Visible}, got {actual}");
                failures++;
                return;
            }
        }
        Console.WriteLine("PASS " + name);
    }

    private static int Main()
    {
        Check("Background retries stay silent throughout uninterrupted playback",
            (0, 10000, false, true, false),
            (250, 10250, true, true, false),
            (750, 10750, true, true, false),
            (1500, 11500, true, true, false),
            (2500, 12500, true, true, false),
            (3000, 13000, false, true, false));

        Check("Brief playback-clock gaps do not flash the spinner",
            (0, 10000, true, true, false),
            (250, 10000, true, true, false),
            (500, 10500, true, true, false));

        Check("A sustained stalled retry becomes visible",
            (0, 10000, false, true, false),
            (250, 10000, true, true, false),
            (750, 10000, true, true, false),
            (1000, 10000, true, true, true),
            (1250, 10000, true, true, true));

        Check("Playback progress immediately hides recovery even while retrying",
            (0, 10000, true, true, false),
            (1250, 10000, true, true, true),
            (1500, 10250, true, true, false));

        Check("Download recovery alone does not hide an already stalled overlay",
            (0, 10000, true, true, false),
            (1250, 10000, true, true, true),
            (1500, 10000, false, true, true),
            (1750, 10250, false, true, false));

        Check("Paused playback never shows recovery and resume gets a fresh baseline",
            (0, 10000, true, true, false),
            (500, 10000, true, false, false),
            (5000, 10000, true, false, false),
            (5250, 10000, true, true, false),
            (5500, 10250, true, true, false));

        Check("Seek or end-of-stream suppression clears a visible overlay",
            (0, 10000, true, true, false),
            (1250, 10000, true, true, true),
            (1500, 10000, true, false, false),
            (5000, 50000, true, true, false),
            (5250, 50250, true, true, false));

        Check("A backwards position jump resets stale stall timing",
            (0, 10000, true, true, false),
            (1250, 10000, true, true, true),
            (1500, 5000, true, true, false),
            (1750, 5250, true, true, false));

        Check("A stopped clock without a retry does not invent a reconnecting state",
            (0, 10000, false, true, false),
            (5000, 10000, false, true, false));

        Check("A retry that ends before a playback stall stays silent",
            (0, 10000, true, true, false),
            (500, 10000, true, true, false),
            (750, 10000, false, true, false),
            (1250, 10000, false, true, false));

        Check("Invalid positions do not create a recovery overlay",
            (0, -1, true, true, false),
            (5000, -1, true, true, false),
            (5250, 0, true, true, false));

        Check("A backwards time jump starts a new observation interval",
            (1000, 10000, true, true, false),
            (0, 10000, true, true, false),
            (250, 10000, true, true, false));

        tests++;
        var restarted = new PlaybackRecovery();
        restarted.Update(true, true, 10000, 0);
        restarted.Update(true, true, 10000, 1250);
        restarted.Reset();
        if (restarted.Visible || restarted.Update(true, true, 0, 5000))
        {
            Console.Error.WriteLine("FAIL Restart clears previous recovery state");
            failures++;
        }
        else
        {
            Console.WriteLine("PASS Restart clears previous recovery state");
        }

        Console.WriteLine($"{tests - failures}/{tests} tests passed");
        return failures == 0 ? 0 : 1;
    }
}
