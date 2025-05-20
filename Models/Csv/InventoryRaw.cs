namespace ProductImportApi.Models.Csv;

public class InventoryRaw
{
    public string SKU { get; set; }
    public string Unit { get; set; }
    public string Qty { get; set; }
    public string ManufacturerName { get; set; }
    public string ManufacturerRefNum { get; set; }
    public string Shipping { get; set; }
    public string ShippingCost { get; set; }
}
