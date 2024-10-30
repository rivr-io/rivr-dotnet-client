using System.Threading.Tasks;
using Rivr.Core.Models;
using Rivr.Core.Models.Merchants;

namespace Rivr.Core;

/// <summary>
/// Represents the operations that can be performed as a platform.
/// </summary>
public interface IPlatformOperations
{
    /// <summary>
    /// Gets the health of the Rivr API.
    /// </summary>
    /// <returns></returns>
    Task<Health> GetHealthSecureAsync();

    /// <summary>
    /// Gets the merchants.
    /// </summary>
    /// <returns></returns>
    Task<GetMerchantsResponse> GetMerchantsAsync();
}