namespace OmegaTube.Ui
{
    internal sealed class PlaybackRecovery
    {
        private const long StallDelayMs = 1000;
        private long lastPositionMs = -1;
        private long lastProgressMs;

        public bool Visible { get; private set; }

        public bool Update(bool retrying, bool playbackExpected, long positionMs, long nowMs)
        {
            if (!playbackExpected || positionMs < 0)
            {
                Reset();
                return false;
            }
            if (positionMs != lastPositionMs || nowMs < lastProgressMs)
            {
                lastPositionMs = positionMs;
                lastProgressMs = nowMs;
                Visible = false;
            }
            // A download retry is invisible while buffered playback keeps moving. Once shown,
            // keep the overlay until playback resumes, not merely until downloading resumes.
            Visible = (retrying || Visible) && nowMs - lastProgressMs >= StallDelayMs;
            return Visible;
        }

        public void Reset()
        {
            lastPositionMs = -1;
            lastProgressMs = 0;
            Visible = false;
        }
    }
}
