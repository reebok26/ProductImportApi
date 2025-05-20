public class CsvSettings
{
    public string Delimiter { get; set; } = ",";
    public bool IgnoreBadData { get; set; } = false;
    public bool HasHeaderRecord { get; set; } = true;
}
