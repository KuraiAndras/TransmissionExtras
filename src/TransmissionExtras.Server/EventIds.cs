namespace TransmissionExtras.Server;

public static class EventIds
{
    private const int ProjectId = 001_000_000;

    public static class Program
    {
        private const int ClassId = ProjectId + 000_000;

        public const int SettingsValidationFailed = ClassId + 0;
        public const int HealthCheckFailed = ClassId + 1;
    }

    public static class RemoveTorrentsJob
    {
        private const int ClassId = ProjectId + 001_000;

        public const int RemovingTorrentsJobFailed = ClassId + 0;
        public const int RemovingTorrentsFailed = ClassId + 1;
        public const int DryRun = ClassId + 2;
        public const int RemovedTorrent = ClassId + 3;
    }

    public static class VerifyTorrentsJob
    {
        private const int ClassId = ProjectId + 002_000;

        public const int VerifyTorrentsJobFailed = ClassId + 0;
        public const int VerifyingTorrentsFailed = ClassId + 1;
        public const int DryRun = ClassId + 2;
        public const int VerifyingTorrent = ClassId + 3;
    }
}