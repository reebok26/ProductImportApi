namespace ProductImportApi.Models.Csv
{
    public class ProductRaw
    {
        public string SKU { get; set; }
        public string name { get; set; }
        public string EAN { get; set; }
        public string producer_name { get; set; }
        public string category { get; set; }
        public string is_wire { get; set; }
        public string shipping { get; set; }
        public string available { get; set; }
        public string default_image { get; set; }
    }
}