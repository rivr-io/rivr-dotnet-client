using System.Linq;
using Rivr.Models;

namespace Rivr.Extensions;

public static class PaymentRequestErrorExtensions
{
    public static string CombineToString(this PaymentRequestError[] errors)
    {
        return string.Join(", ", errors.Select(e => $"{e.PropertyName}: {e.Message}"));
    }
}