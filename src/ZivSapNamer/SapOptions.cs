using System.ComponentModel.DataAnnotations;

namespace ZivSapNamer;

public sealed class SapOptions
{
    [Required]
    public string ApplicationServerHost { get; set; } = "";

    [Required, RegularExpression(@"^\d{2}$")]
    public string SystemNumber { get; set; } = "";

    [Required, RegularExpression(@"^\d{3}$")]
    public string Client { get; set; } = "";

    [Required]
    public string User { get; set; } = "";

    [Required]
    public string Password { get; set; } = "";

    [Required, RegularExpression(@"^[A-Za-z]{2}$")]
    public string Language { get; set; } = "EN";
}
