namespace Rivr.Core.Models.Merchants;

/// <summary>
/// Represents the response from the GetMerchants operation.
/// </summary>
public class GetMerchantsResponse
{
    /// <summary>
    /// The merchants.
    /// </summary>
    public Merchant[] Merchants { get; set; } = [];

    /// <summary>
    /// The metadata of the response.
    /// </summary>
    public Metadata? Metadata { get; set; }
}
