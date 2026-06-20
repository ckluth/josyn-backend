#pragma warning disable IDE0130
namespace JOSYN.Backend.JobRegistry;
#pragma warning restore IDE0130

internal sealed class JobRegistrationEntity
{
    public int    Id                { get; set; }
    public string Name              { get; set; } = string.Empty;
    public string TechnicalUserName { get; set; } = string.Empty;

    public ICollection<ArgumentRecordEntity> ArgumentRecords { get; set; } = [];
}
