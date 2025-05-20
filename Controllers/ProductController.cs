using Microsoft.AspNetCore.Mvc;
using ProductImportApi.Services;
using System.Threading.Tasks;

namespace ProductImportApi.Controllers
{
    /// <summary>
    /// Operacje na produktach.
    /// </summary>
    [ApiController]
    [Route("api/products")]
    public class ProductController : ControllerBase
    {
        private readonly ProductService _service;

        public ProductController(ProductService service)
        {
            _service = service;
        }

        /// <summary>
        /// Importuje dane produkt�w, stan�w magazynowych i cen z plik�w CSV do bazy danych.
        /// </summary>
        /// <returns>Komunikat o powodzeniu operacji importu.</returns>
        [HttpPost("import")]
        public async Task<IActionResult> ImportData()
        {
            await _service.ImportDataFromCsvAsync();
            return Ok("Import completed");
        }

        /// <summary>
        /// Pobiera szczeg�y produktu na podstawie podanego SKU.
        /// </summary>
        /// <param name="sku">Numer SKU produktu.</param>
        /// <returns>Dane produktu lub kod 404 je�li nie znaleziono.</returns>
        [HttpGet("{sku}")]
        public async Task<ActionResult<ProductDto>> GetBySku(string sku)
        {
            var product = await _service.GetProductBySkuAsync(sku);
            if (product == null)
                return NotFound();

            return Ok(product);
        }
    }
}
