using System.Linq;
using Rivr.Models;

namespace Rivr.Extensions;

/// <summary>
/// Contains extension methods for <see cref="OrderRequestError"/>.
/// </summary>
public static class PaymentRequestErrorExtensions
{
    /// <summary>
    /// Combines the errors into a single string.
    /// </summary>
    /// <param name="errors"></param>
    /// <returns></returns>
    public static string CombineToString(this OrderRequestError[] errors)
    {
        return string.Join(", ", errors.Select(e => $"{e.PropertyName}: {e.Message}"));
    }
}