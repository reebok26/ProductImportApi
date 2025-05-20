using CsvHelper.Configuration;

namespace ProductImportApi.Models.Csv
{
    public sealed class ProductRawMap : ClassMap<ProductRaw>
    {
        public ProductRawMap()
        {
            Map(m => m.SKU).Name("SKU");
            Map(m => m.name).Name("name");
            Map(m => m.EAN).Name("EAN");
            Map(m => m.producer_name).Name("producer_name");
            Map(m => m.category).Name("category");
            Map(m => m.is_wire).Name("is_wire");
            Map(m => m.shipping).Name("shipping");
            Map(m => m.available).Name("available");
            Map(m => m.default_image).Name("default_image");
        }
    }
}