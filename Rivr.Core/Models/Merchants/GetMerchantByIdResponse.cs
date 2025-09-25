namespace Rivr.Core.Models.Merchants;

/// <summary>
/// Represents the response from the GetMerchantById operation.
/// </summary>
public class GetMerchantByIdResponse
{
    /// <summary>
    /// The merchant.
    /// </summary>
    public Merchant Merchant { get; set; } = new();
}