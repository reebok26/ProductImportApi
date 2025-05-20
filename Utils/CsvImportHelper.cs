using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Data.SqlClient;
using ProductImportApi.Models;
using ProductImportApi.Models.Csv;

namespace ProductImportApi.Utils
{
    public static class CsvImportHelper
    {
        public static List<T> ReadCsv<T>(string path, char delimiter)
        {
            using var reader = new StreamReader(path);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = delimiter.ToString(),
                HeaderValidated = null,
                MissingFieldFound = null,
                HasHeaderRecord = typeof(T) != typeof(PriceRaw),
                PrepareHeaderForMatch = args => args.Header.ToLower()
            });

            RegisterClassMap<T>(csv);

            return csv.GetRecords<T>().ToList();
        }

        private static void RegisterClassMap<T>(CsvReader csv)
        {
            if (typeof(T) == typeof(ProductRaw))
                csv.Context.RegisterClassMap<ProductRawMap>();
            else if (typeof(T) == typeof(InventoryRaw))
                csv.Context.RegisterClassMap<InventoryRawMap>();
            else if (typeof(T) == typeof(PriceRaw))
                csv.Context.RegisterClassMap<PriceRawMap>();
            else
                throw new InvalidOperationException($"No class map registered for type {typeof(T).Name}");
        }

        public static Dictionary<string, T> CreateUniqueDictionary<T>(IEnumerable<T> records, Func<T, string> keySelector, string sourceName)
        {
            var result = new Dictionary<string, T>();
            var duplicates = new HashSet<string>();
            var nullOrEmptyKeys = new List<T>();

            foreach (var record in records)
            {
                var key = keySelector(record);

                if (string.IsNullOrWhiteSpace(key))
                {
                    nullOrEmptyKeys.Add(record);
                    continue;
                }

                if (!result.TryAdd(key, record))
                {
                    duplicates.Add(key);
                }
            }

            if (nullOrEmptyKeys.Any())
            {
                Console.WriteLine($"[WARN] Ignored {nullOrEmptyKeys.Count} records with null or empty keys in {sourceName}.");
            }

            if (duplicates.Any())
            {
                Console.WriteLine($"[WARN] Duplicate keys found in {sourceName}:");
                foreach (var dup in duplicates)
                {
                    Console.WriteLine($" - {dup}");
                }
            }

            return result;
        }

        public static async Task BulkInsertAsync<T>(SqlConnection connection, SqlTransaction transaction, string tableName, IEnumerable<T> data)
        {
            using var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction)
            {
                DestinationTableName = tableName
            };

            var dataTable = new DataTable();
            var properties = typeof(T).GetProperties();

            foreach (var prop in properties)
            {
                dataTable.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }

            foreach (var item in data)
            {
                var values = properties.Select(p => p.GetValue(item) ?? DBNull.Value).ToArray();
                dataTable.Rows.Add(values);
            }

            await bulkCopy.WriteToServerAsync(dataTable);
        }
        public static int? ParseInt(string? value)
        {
            if (int.TryParse(value, out var result))
                return result;
            return null;
        }

        public static decimal? ParseDecimal(string? value)
        {
            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                return result;
            return null;
        }

        public static void LogRejectedPrices(List<Price> rejectedPrices)
        {
            if (!rejectedPrices.Any()) return;

            Console.WriteLine($"[WARN] Skipped {rejectedPrices.Count} price records with invalid or too large NetPrice:");
            foreach (var price in rejectedPrices.Take(10))
            {
                Console.WriteLine($" - SKU: {price.SKU}, NetPrice: {price.NetPrice}");
            }
        }
    }
}
