public class Product
{
    public string SKU { get; set; }
    public string Name { get; set; }
    public string EAN { get; set; }
    public string Manufacturer { get; set; }
    public string Category { get; set; }
    public string ImageUrl { get; set; }
    public decimal NetPurchasePrice { get; set; }
    public decimal DeliveryCost { get; set; }
    public string LogisticUnit { get; set; }
    public int Stock { get; set; }
}
