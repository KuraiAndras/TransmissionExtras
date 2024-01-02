using System.ComponentModel.DataAnnotations;

using Microsoft.Extensions.Options;

namespace TransmissionExtras.Server.TorrentRemoval;

public sealed class RemoveTorrentsOptions
{
    public const string Section = "RemoveTorrents";

    public bool DryRun { get; set; } = true;
    public bool DeleteData { get; set; } = false;

    public TimeSpan CheckInterval { get; set; } = TimeSpan.FromHours(1);

    [Required]
    public required TimeSpan? RemoveAfter { get; set; }
}

[OptionsValidator]
public partial class ValidateRemoveTorrentsOptions : IValidateOptions<RemoveTorrentsOptions> { }
