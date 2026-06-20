#pragma warning disable IDE0130
namespace JOSYN.Backend.JobRegistry;
#pragma warning restore IDE0130

internal sealed class ArgumentRecordEntity
{
    public string JobName { get; set; } = string.Empty;
    public string Name    { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;

    public JobRegistrationEntity? Registration { get; set; }
}
