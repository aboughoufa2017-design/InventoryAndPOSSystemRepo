namespace Warehouse.Api.Hosting;

public sealed class CorsOptions
{
    public const string SectionName = "Cors";

    /// <summary>Allowed browser origins in non-Development environments.</summary>
    public string[] Origins { get; set; } = [];
}
