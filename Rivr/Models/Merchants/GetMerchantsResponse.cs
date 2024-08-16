namespace Rivr.Models.Merchants;

public class GetMerchantsResponse
{
    public Merchant[] Merchants { get; set; } = null!;
    public Metadata Metadata { get; set; } = new();
}