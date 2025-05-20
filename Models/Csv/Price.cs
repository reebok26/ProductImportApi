using CsvHelper.Configuration;

namespace ProductImportApi.Models.Csv
{
    public class Price
    {
        public string SKU { get; set; }
        public decimal? NetPrice { get; set; }
    }
}