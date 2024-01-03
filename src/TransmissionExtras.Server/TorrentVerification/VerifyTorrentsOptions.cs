namespace TransmissionExtras.Server.TorrentVerification;

public sealed class VerifyTorrentsOptions
{
    public const string Section = "VerifyTorrents";

    public bool DryRun { get; set; } = true;

    public TimeSpan CheckInterval { get; set; } = TimeSpan.FromHours(1);
}
