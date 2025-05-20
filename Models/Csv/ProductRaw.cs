namespace ProductImportApi.Models.Csv
{
    public class ProductRaw
    {
        public string SKU { get; set; }
        public string Name { get; set; }
        public string EAN { get; set; }
        public string ProducerName { get; set; }
        public string Category { get; set; }
        public string DefaultImage { get; set; }
    }
}