using Microsoft.AspNetCore.Mvc;
using ProductImportApi.Services;
using System.Threading.Tasks;
[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
    private readonly ProductService _service;

    public ProductController(ProductService service)
    {
        _service = service;
    }

    [HttpPost("import")]
    public async Task<IActionResult> ImportProducts()
    {
        await _service.ImportProductsFromCsvAsync();
        return Ok("Import completed");
    }

    [HttpGet("{sku}")]
    public async Task<ActionResult<ProductDto>> GetBySku(string sku)
    {
        var product = await _service.GetProductBySkuAsync(sku);
        if (product == null) return NotFound();
        return Ok(product);
    }
}
