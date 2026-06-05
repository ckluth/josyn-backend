namespace JOSYN.Backend.ConfigStore;

internal sealed class ConfigStoreEntity
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string Value { get; set; } = string.Empty;
}
