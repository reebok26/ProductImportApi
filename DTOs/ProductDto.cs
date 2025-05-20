/// <summary>
/// Reprezentuje szczegó³y produktu dostêpnego przez API.
/// </summary>
public class ProductDto
{
    /// <summary>
    /// Unikalny identyfikator produktu (SKU).
    /// </summary>
    public string SKU { get; set; }

    /// <summary>
    /// Nazwa produktu.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Kod EAN produktu.
    /// </summary>
    public string EAN { get; set; }

    /// <summary>
    /// Producent produktu.
    /// </summary>
    public string Manufacturer { get; set; }

    /// <summary>
    /// Kategoria produktu.
    /// </summary>
    public string Category { get; set; }

    /// <summary>
    /// URL do domyœlnego zdjêcia produktu.
    /// </summary>
    public string ImageUrl { get; set; }

    /// <summary>
    /// Stan magazynowy produktu.
    /// </summary>
    public int? Stock { get; set; }

    /// <summary>
    /// Jednostka logistyczna (np. szt., opak.).
    /// </summary>
    public string LogisticUnit { get; set; }

    /// <summary>
    /// Cena zakupu netto.
    /// </summary>
    public decimal? NetPurchasePrice { get; set; }

    /// <summary>
    /// Koszt dostawy.
    /// </summary>
    public decimal? DeliveryCost { get; set; }
}
