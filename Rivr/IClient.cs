using System;
using System.Threading.Tasks;
using Rivr.Models;

namespace Rivr;

/// <summary>
/// Represents the Rivr API client.
/// </summary>
public interface IClient
{
    /// <summary>
    /// Gets the configuration of the client.
    /// </summary>
    Config Config { get; }

    /// <summary>
    /// Gets the health of the Rivr API.
    /// </summary>
    /// <returns></returns>
    Task<Health> GetHealthAsync();

    /// <summary>
    /// Gets the operations that can be performed as a platform.
    /// </summary>
    IPlatformOperations AsPlatform();

    /// <summary>
    /// Gets the operations that can be performed on behalf of a merchant.
    /// </summary>
    /// <param name="merchantId"></param>
    /// <returns></returns>
    IMerchantOperations OnBehalfOfMerchant(Guid merchantId);
}