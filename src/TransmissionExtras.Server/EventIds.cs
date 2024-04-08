namespace TransmissionExtras.Server;

public static class EventIds
{
    private const int ProjectId = 001_000_000;

    public static class Program
    {
        private const int ClassId = ProjectId + 000_000;

        public const int SettingsValidationFailed = ClassId + 0;
        public const int RunningApplicationFailed = ClassId + 1;
    }

    public static class TorrentJob
    {
        private const int ClassId = ProjectId + 001_000;

        public const int StartingJob = ClassId + 0;
        public const int CancelledJob = ClassId + 1;
        public const int JobFailed = ClassId + 2;
    }

    public static class RemoveAfterSeedTimeTorrentJob
    {
        private const int ClassId = ProjectId + 002_000;

        public const int DryRun = ClassId + 0;
        public const int RemovedTorrent = ClassId + 1;
    }

    public static class RemoveAfterAddedTimeTorrentJob
    {
        private const int ClassId = ProjectId + 003_000;

        public const int DryRun = ClassId + 0;
        public const int RemovedTorrent = ClassId + 1;
    }

    public static class VerifyTorrentJob
    {
        private const int ClassId = ProjectId + 004_000;

        public const int DryRun = ClassId + 0;
        public const int VerifyingTorrent = ClassId + 1;
    }
}