using System.ComponentModel.DataAnnotations;

using Microsoft.Extensions.Options;

namespace TransmissionExtras;

public sealed class TransmissionOptions
{
    public const string Section = "Transmission";

    [Required, Url]
    public required string Url { get; set; }

    public string? User { get; set; }

    public string? Password { get; set; }

    public TimeSpan RetryTimeout { get; set; } = TimeSpan.FromMinutes(1);
}

[OptionsValidator]
public partial class ValidateTransmissionOptions : IValidateOptions<TransmissionOptions> { }
