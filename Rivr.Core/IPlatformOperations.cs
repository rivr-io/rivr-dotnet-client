﻿using System.Threading;
using System.Threading.Tasks;
using Rivr.Core.Models;
using Rivr.Core.Models.Heartbeats;
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
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Health> GetHealthSecureAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets the merchants.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<GetMerchantsResponse> GetMerchantsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a heartbeat.
    /// </summary>
    /// <param name="heartbeat"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SendHeartbeat(SendHeartbeatRequest heartbeat, CancellationToken cancellationToken = default);
}