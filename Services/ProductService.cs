using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ProductImportApi.Models;
using ProductImportApi.Models.Csv;
using ProductImportApi.Utils;

namespace ProductImportApi.Services
{
    public class ProductService
    {
        private readonly string _connectionString;
        private readonly IDbConnection _dbOverride;

        public ProductService(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("Default");
        }

        public ProductService(IConfiguration config, IDbConnection dbOverride)
        {
            _connectionString = config.GetConnectionString("Default");
            _dbOverride = dbOverride;
        }

        public async Task ImportDataFromCsvAsync()
        {
            var products = LoadAndPrepareProducts();
            Console.WriteLine($"[INFO] Loaded {products.Count()} products.");

            var inventory = LoadAndPrepareInventory();
            Console.WriteLine($"[INFO] Loaded {inventory.Count()} inventory items.");

            var (validPrices, rejectedPrices) = LoadAndPreparePrices();
            Console.WriteLine($"[INFO] Loaded {validPrices.Count} valid prices, {rejectedPrices.Count} rejected.");

            CsvImportHelper.LogRejectedPrices(rejectedPrices);

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            await TruncateTables(connection, transaction);
            Console.WriteLine("[INFO] Tables truncated.");

            await InsertAllData(connection, transaction, products, inventory, validPrices);
            Console.WriteLine("[INFO] Data inserted successfully.");

            transaction.Commit();
            Console.WriteLine("[INFO] Transaction committed.");
        }

        private IEnumerable<Product> LoadAndPrepareProducts()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Data", "Products.csv");
            var raw = CsvImportHelper.ReadCsv<ProductRaw>(path, ';');
            var unique = CsvImportHelper.CreateUniqueDictionary(raw, p => p.SKU, "Products");

            return unique.Values.Select(p => new Product
            {
                SKU = p.SKU,
                Name = p.Name,
                EAN = p.EAN,
                Manufacturer = p.ProducerName,
                Category = p.Category,
                ImageUrl = p.DefaultImage
            });
        }

        private IEnumerable<Inventory> LoadAndPrepareInventory()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Data", "Inventory.csv");
            var raw = CsvImportHelper.ReadCsv<InventoryRaw>(path, ',');
            var unique = CsvImportHelper.CreateUniqueDictionary(raw, i => i.SKU, "Inventory");

            return unique.Values.Select(i => new Inventory
            {
                SKU = i.SKU,
                Qty = CsvImportHelper.ParseInt(i.Qty),
                ShippingCost = CsvImportHelper.ParseDecimal(i.ShippingCost),
                Unit = i.Unit
            });
        }

        private (List<Price> valid, List<Price> rejected) LoadAndPreparePrices()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Data", "Prices.csv");
            var raw = CsvImportHelper.ReadCsv<PriceRaw>(path, ',');
            var unique = CsvImportHelper.CreateUniqueDictionary(raw, p => p.SKU, "Prices");

            var processed = unique.Values.Select(p => new Price
            {
                SKU = p.SKU,
                NetPrice = CsvImportHelper.ParseDecimal(p.NetPrice)
            });

            var valid = processed.Where(p => p.NetPrice.HasValue && p.NetPrice.Value <= 9999999999999999.99m).ToList();
            var rejected = processed.Except(valid).ToList();

            return (valid, rejected);
        }

        private async Task TruncateTables(SqlConnection connection, SqlTransaction transaction)
        {
            await connection.ExecuteAsync("TRUNCATE TABLE Products", transaction: transaction);
            await connection.ExecuteAsync("TRUNCATE TABLE Inventory", transaction: transaction);
            await connection.ExecuteAsync("TRUNCATE TABLE Prices", transaction: transaction);
        }

        private async Task InsertAllData(SqlConnection connection, SqlTransaction transaction, IEnumerable<Product> products, IEnumerable<Inventory> inventory, List<Price> prices)
        {
            await CsvImportHelper.BulkInsertAsync(connection, transaction, "Products", products);
            await CsvImportHelper.BulkInsertAsync(connection, transaction, "Inventory", inventory);
            await CsvImportHelper.BulkInsertAsync(connection, transaction, "Prices", prices);
        }

        public async Task<ProductDto> GetProductBySkuAsync(string sku)
        {
            if (string.IsNullOrWhiteSpace(sku))
                return null;

            var sql = @"
                SELECT 
                    p.SKU,
                    p.Name,
                    p.EAN,
                    p.Manufacturer,
                    p.Category,
                    p.ImageUrl,
                    i.Qty AS Stock,
                    i.ShippingCost AS DeliveryCost,
                    i.Unit AS LogisticUnit,
                    pr.NetPrice AS NetPurchasePrice
                FROM Products p
                LEFT JOIN Inventory i ON p.SKU = i.SKU
                LEFT JOIN Prices pr ON p.SKU = pr.SKU
                WHERE p.SKU = @Sku;";

            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<ProductDto>(sql, new { Sku = sku });
        }
    }
}
