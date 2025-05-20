namespace ProductImportApi.Models.Csv;

public class Inventory
{
    public string SKU { get; set; }
    public int? Qty { get; set; }
    public decimal? ShippingCost { get; set; }
    public string Unit { get; set; }
}
