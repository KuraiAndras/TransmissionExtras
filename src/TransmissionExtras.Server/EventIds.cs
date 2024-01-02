namespace TransmissionExtras.Server;

public static class EventIds
{
    private const int ProjectId = 001_000_000;

    public static class Program
    {
        private const int ClassId = ProjectId + 000_000;

        public const int SettingsValidationFailed = ClassId + 0;
    }

    public static class RemoveTorrentsJob
    {
        private const int ClassId = ProjectId + 001_000;

        public const int RemovingTorrentsJobFailed = ClassId + 0;
        public const int RemovingTorrentsFailed = ClassId + 1;
        public const int DryRun = ClassId + 2;
        public const int RemovedTorrent = ClassId + 3;
    }
}