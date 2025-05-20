using CsvHelper.Configuration;

namespace ProductImportApi.Models.Csv;

public sealed class InventoryRawMap : ClassMap<InventoryRaw>
{
    public InventoryRawMap()
    {
        Map(m => m.sku).Name("sku");
        Map(m => m.unit).Name("unit");
        Map(m => m.qty).Name("qty");
        Map(m => m.shipping_cost).Name("shipping_cost");
    }
}
