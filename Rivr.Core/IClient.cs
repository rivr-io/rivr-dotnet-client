using System;
using System.Threading.Tasks;
using Rivr.Core.Models;

namespace Rivr.Core;

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
    /// Gets a value indicating whether the client can perform platform operations.
    /// </summary>
    bool IsConfiguredForSingleMerchant { get; }

    /// <summary>
    /// Gets or sets the configured merchantId.
    /// </summary>
    Guid ConfiguredMerchantId { get; }

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
    IMerchantOperations AsOrOnBehalfOfMerchant(Guid merchantId);
}