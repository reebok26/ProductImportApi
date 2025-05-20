using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using System.Net.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using System;
using ProductImportApi.Models.Csv;

namespace ProductImportApi.Services
{
    public class ProductService
    {
        private readonly IDbConnection _db;
        private readonly HttpClient _httpClient;
        private readonly CsvSettings _csvSettings;

        public ProductService(IConfiguration config, CsvSettings csvSettings)
        {
            _db = new SqlConnection(config.GetConnectionString("Default"));
            _httpClient = new HttpClient();
            _csvSettings = csvSettings;
        }

        public async Task ImportProductsFromCsvAsync()
        {
            var dataDir = Path.Combine(AppContext.BaseDirectory, "Data");
            Directory.CreateDirectory(dataDir);

            var productsPath = Path.Combine(dataDir, "Products.csv");
            var inventoryPath = Path.Combine(dataDir, "Inventory.csv");
            var pricesPath = Path.Combine(dataDir, "Prices.csv");

            await DownloadFile("https://rekturacjazadanie.blob.core.windows.net/zadanie/Products.csv", productsPath);
            await DownloadFile("https://rekturacjazadanie.blob.core.windows.net/zadanie/Inventory.csv", inventoryPath);
            await DownloadFile("https://rekturacjazadanie.blob.core.windows.net/zadanie/Prices.csv", pricesPath);

            var inventoryMap = CreateUniqueDictionary(ReadCsvFromFile<InventoryRaw>(inventoryPath, ","), i => i.sku, "Inventory");
            var priceMap = CreateUniqueDictionary(ReadCsvFromFile<PriceRaw>(pricesPath, ","), p => p.Sku, "Prices");

            using var reader = new StreamReader(productsPath);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null,
                Delimiter = ";",
            });

            await foreach (var raw in csv.GetRecordsAsync<ProductRaw>())
            {
                var cleanedShipping = new string(raw.shipping?.Where(char.IsDigit).ToArray());
                int.TryParse(cleanedShipping, out var ship);

                if (raw.is_wire != "0" || raw.available != "1" || ship > 24)
                    continue;

                var product = new Product
                {
                    SKU = raw.SKU,
                    Name = raw.name,
                    EAN = raw.EAN,
                    Manufacturer = raw.producer_name,
                    Category = ExtractLastCategory(raw.category),
                    ImageUrl = raw.default_image
                };

                if (inventoryMap.TryGetValue(product.SKU, out var inventory))
                {
                    product.Stock = int.TryParse(inventory.qty, out var stock) ? stock : 0;
                    product.DeliveryCost = decimal.TryParse(inventory.shipping_cost, out var cost) ? cost : 0;
                    product.LogisticUnit = inventory.unit;
                }

                if (priceMap.TryGetValue(product.SKU, out var price))
                {
                    product.NetPurchasePrice = decimal.TryParse(price.NetPrice, out var net) ? net : 0;
                }

                var sql = @"MERGE INTO Products AS target
                    USING (VALUES (@SKU)) AS source(SKU)
                    ON target.SKU = source.SKU
                    WHEN MATCHED THEN
                        UPDATE SET
                            Name = @Name,
                            EAN = @EAN,
                            Manufacturer = @Manufacturer,
                            Category = @Category,
                            ImageUrl = @ImageUrl,
                            NetPurchasePrice = @NetPurchasePrice,
                            DeliveryCost = @DeliveryCost,
                            LogisticUnit = @LogisticUnit,
                            Stock = @Stock
                    WHEN NOT MATCHED THEN
                        INSERT (SKU, Name, EAN, Manufacturer, Category, ImageUrl, NetPurchasePrice, DeliveryCost, LogisticUnit, Stock)
                        VALUES (@SKU, @Name, @EAN, @Manufacturer, @Category, @ImageUrl, @NetPurchasePrice, @DeliveryCost, @LogisticUnit, @Stock);";

                await _db.ExecuteAsync(sql, product);
            }
        }

        private static string ExtractLastCategory(string? rawCategory)
        {
            if (string.IsNullOrWhiteSpace(rawCategory))
                return string.Empty;

            var separators = new[] { '|', '/' };
            var parts = rawCategory.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            return parts.Last().Trim();
        }

        private Dictionary<string, T> CreateUniqueDictionary<T>(IEnumerable<T> records, Func<T, string> keySelector, string sourceName)
        {
            var grouped = records.GroupBy(keySelector);
            var duplicates = grouped.Where(g => g.Count() > 1).ToList();

            if (duplicates.Any())
            {
                Console.WriteLine($"[WARN] Duplicate keys found in {sourceName}:");
                foreach (var dup in duplicates)
                {
                    Console.WriteLine($" - {dup.Key}");
                }
            }

            return grouped.ToDictionary(g => g.Key, g => g.First());
        }


        private async Task DownloadFile(string url, string path)
        {
            var data = await _httpClient.GetByteArrayAsync(url);
            await File.WriteAllBytesAsync(path, data);
        }

        private IEnumerable<T> ReadCsvFromFile<T>(string path, string? delimiterOverride = null)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = delimiterOverride ?? _csvSettings.Delimiter,
                HasHeaderRecord = _csvSettings.HasHeaderRecord,
                HeaderValidated = null,
                MissingFieldFound = null,
                BadDataFound = _csvSettings.IgnoreBadData
                    ? null
                    : args => throw new Exception($"Bad CSV data at row {args.Context.Parser.RawRow}: {args.RawRecord}")
            };

            using var reader = new StreamReader(path);
            using var csv = new CsvReader(reader, config);

            var mapRegistry = new Dictionary<Type, Action>
            {
                [typeof(PriceRaw)] = () => csv.Context.RegisterClassMap<PriceRawMap>(),
                [typeof(InventoryRaw)] = () => csv.Context.RegisterClassMap<InventoryRawMap>(),
                [typeof(ProductRaw)] = () => csv.Context.RegisterClassMap<ProductRawMap>()
            };

            if (mapRegistry.TryGetValue(typeof(T), out var registerMap))
            {
                registerMap();
            }

            return csv.GetRecords<T>().ToList();
        }



        public async Task<ProductDto> GetProductBySkuAsync(string sku)
        {
            var sql = "SELECT * FROM Products WHERE SKU = @Sku";
            var product = await _db.QuerySingleOrDefaultAsync<Product>(sql, new { Sku = sku });
            if (product == null) return null;

            return new ProductDto
            {
                SKU = product.SKU,
                Name = product.Name,
                EAN = product.EAN,
                Manufacturer = product.Manufacturer,
                Category = product.Category,
                ImageUrl = product.ImageUrl,
                Stock = product.Stock,
                LogisticUnit = product.LogisticUnit,
                NetPurchasePrice = product.NetPurchasePrice,
                DeliveryCost = product.DeliveryCost
            };
        }
    }
}