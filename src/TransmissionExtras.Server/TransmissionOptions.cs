using System.ComponentModel.DataAnnotations;

using Microsoft.Extensions.Options;

namespace TransmissionExtras.Server;

public sealed class TransmissionOptions
{
    public const string Section = "Transmission";

    [Required, Url]
    public required string Url { get; set; }

    public string? User { get; set; }

    public string? Password { get; set; }
}

[OptionsValidator]
public partial class ValidateTransmissionOptions : IValidateOptions<TransmissionOptions> { }
