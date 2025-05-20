using CsvHelper.Configuration;

namespace ProductImportApi.Models.Csv
{
    public sealed class PriceRawMap : ClassMap<PriceRaw>
    {
        public PriceRawMap()
        {
            Map(m => m.Sku).Index(1);
            Map(m => m.NetPrice).Index(2);
        }
    }
}