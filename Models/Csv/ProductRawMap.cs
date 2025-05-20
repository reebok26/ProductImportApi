using CsvHelper.Configuration;

namespace ProductImportApi.Models.Csv
{
    
    public sealed class ProductRawMap : ClassMap<ProductRaw>
    {
        public ProductRawMap()
        {
            Map(m => m.SKU).Name("sku");
            Map(m => m.Name).Name("name");
            Map(m => m.EAN).Name("ean");
            Map(m => m.ProducerName).Name("producer_name");
            Map(m => m.Category).Name("category");
            Map(m => m.DefaultImage).Name("default_image");
        }
    }

}
