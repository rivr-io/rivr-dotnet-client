namespace Rivr.Core.Models.Orders;

/// <summary>
/// Represents the payment method.
/// </summary>
public enum PaymentMethod
{
    /// <summary>
    /// The payment has not been paid.
    /// </summary>
    NotPaid = 0,

    /// <summary>
    /// Instalment payment with default settings.
    /// </summary>
    InstalmentDefault = 1 << 1,


    /// <summary>
    /// Instalment payment with interest free.
    /// </summary>
    InstalmentInterestFree = 1 << 2,

    /// <summary>
    /// Instalment payment with full KYC.
    /// </summary>
    InstalmentFullKyc = 1 << 3,

    /// <summary>
    /// Swish payment.
    /// </summary>
    Swish = 1 << 6,

    /// <summary>
    /// Invoice payment.
    /// </summary>
    Invoice = 1 << 7,

    /// <summary>
    /// Card payment.
    /// </summary>
    Card = 1 << 8,

    /// <summary>
    /// Digital wallet payment (e.g. Apple Pay, Google Pay).
    /// </summary>
    DigitalWallet = 1 << 9,

    /// <summary>
    /// Avarda invoice payment.
    /// </summary>
    AvardaInvoice = 1 << 10,

    /// <summary>
    /// Avarda account payment.
    /// </summary>
    AvardaAccount = 1 << 11,

    /// <summary>
    /// Avarda part payment.
    /// </summary>
    AvardaPartPayment = 1 << 12,

    /// <summary>
    /// Medical Finance payment.
    /// </summary>
    MedicalFinance = 1 << 13,

    /// <summary>
    /// Resurs Bank payment.
    /// </summary>
    Resurs = 1 << 15,

    /// <summary>
    /// Common instalment payment.
    /// </summary>
    Instalment = InstalmentDefault | InstalmentFullKyc | InstalmentInterestFree,
}