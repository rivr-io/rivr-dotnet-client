namespace Rivr.Models.Orders;

public enum PaymentMethod
{
    Unknown = -1,
    NotPaid = 0,
    Giro = 1 << 0,
    InstalmentDefault = 1 << 1,
    InstalmentInterestFree = 1 << 2,
    InstalmentFullKyc = 1 << 3,
    Direct = 1 << 4,
    Postponement = 1 << 5,
    Swish = 1 << 6,
    Invoice = 1 << 7,
    Card = 1 << 8,
    DigitalWallet = 1 << 9,
    AvardaInvoice = 1 << 10,
    AvardaAccount = 1 << 11,
    AvardaPartPayment = 1 << 12,
    MedicalFinance = 1 << 13,
    Instalment = InstalmentDefault | InstalmentFullKyc | InstalmentInterestFree,
}