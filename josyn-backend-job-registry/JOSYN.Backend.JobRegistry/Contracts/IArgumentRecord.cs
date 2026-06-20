#pragma warning disable IDE0130
namespace JOSYN.Backend.JobRegistry;
#pragma warning restore IDE0130

public interface IArgumentRecord
{
    string JobName  { get; }
    string Name     { get; }
    string Content  { get; }
}
