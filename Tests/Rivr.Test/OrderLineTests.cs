using Rivr.Core.Models.Orders;
using Shouldly;

namespace Rivr.Test;

public class OrderLineTests
{
    [Test]
    public void UnitPriceInclVat_WithStandardVat_CalculatesCorrectly()
    {
        // Arrange
        var orderLine = new OrderLine
        {
            UnitPriceExclVat = 100,
            VatPercentage = 25,
            Quantity = 1
        };

        // Act & Assert
        orderLine.UnitPriceInclVat.ShouldBe(125m);
    }

    [Test]
    public void UnitPriceInclVat_WithZeroVat_EqualsExclVat()
    {
        // Arrange
        var orderLine = new OrderLine
        {
            UnitPriceExclVat = 200,
            VatPercentage = 0,
            Quantity = 1
        };

        // Act & Assert
        orderLine.UnitPriceInclVat.ShouldBe(200m);
    }

    [Test]
    public void AmountInclVat_WithMultipleQuantity_CalculatesCorrectly()
    {
        // Arrange
        var orderLine = new OrderLine
        {
            UnitPriceExclVat = 100,
            VatPercentage = 25,
            Quantity = 3
        };

        // Act & Assert
        // 100 * 1.25 * 3 = 375
        orderLine.AmountInclVat.ShouldBe(375m);
    }

    [Test]
    public void AmountExclVat_WithMultipleQuantity_CalculatesCorrectly()
    {
        // Arrange
        var orderLine = new OrderLine
        {
            UnitPriceExclVat = 100,
            VatPercentage = 25,
            Quantity = 3
        };

        // Act & Assert
        // 100 * 3 = 300
        orderLine.AmountExclVat.ShouldBe(300m);
    }

    [Test]
    public void AmountExclVat_ShouldNotDependOnVatPercentage()
    {
        // Arrange
        var orderLine = new OrderLine
        {
            UnitPriceExclVat = 100,
            VatPercentage = 25,
            Quantity = 2
        };

        // Act & Assert
        // AmountExclVat = UnitPriceExclVat * Quantity, regardless of VAT
        orderLine.AmountExclVat.ShouldBe(200m);
    }

    [Test]
    public void VatAmount_CalculatesCorrectly()
    {
        // Arrange
        var orderLine = new OrderLine
        {
            UnitPriceExclVat = 100,
            VatPercentage = 25,
            Quantity = 2
        };

        // Act & Assert
        // AmountInclVat = 100 * 1.25 * 2 = 250
        // AmountExclVat = 100 * 2 = 200
        // VatAmount = 250 - 200 = 50
        orderLine.AmountInclVat.ShouldBe(250m);
        orderLine.AmountExclVat.ShouldBe(200m);
        (orderLine.AmountInclVat - orderLine.AmountExclVat).ShouldBe(50m);
    }

    [TestCase(0)]
    [TestCase(6)]
    [TestCase(12)]
    [TestCase(25)]
    public void VatPercentage_AcceptsValidValues(int vat)
    {
        // Arrange & Act
        var orderLine = new OrderLine { VatPercentage = vat };

        // Assert
        orderLine.VatPercentage.ShouldBe(vat);
    }

    [Test]
    public void VatPercentage_WhenAboveMax_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var orderLine = new OrderLine();

        // Act & Assert
        Should.Throw<ArgumentOutOfRangeException>(() => orderLine.VatPercentage = 26);
    }

    [Test]
    public void VatPercentage_WhenBelowMin_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var orderLine = new OrderLine();

        // Act & Assert
        Should.Throw<ArgumentOutOfRangeException>(() => orderLine.VatPercentage = -1);
    }

    [Test]
    public void ComputedProperties_WithDecimalPrices_CalculateCorrectly()
    {
        // Arrange
        var orderLine = new OrderLine
        {
            UnitPriceExclVat = 99.50m,
            VatPercentage = 12,
            Quantity = 4
        };

        // Act & Assert
        // UnitPriceInclVat = 99.50 * 1.12 = 111.44
        orderLine.UnitPriceInclVat.ShouldBe(111.44m);

        // AmountInclVat = 111.44 * 4 = 445.76
        orderLine.AmountInclVat.ShouldBe(445.76m);

        // AmountExclVat = 99.50 * 4 = 398.00
        orderLine.AmountExclVat.ShouldBe(398.00m);
    }

    [Test]
    public void ComputedProperties_WithZeroQuantity_ReturnsZero()
    {
        // Arrange
        var orderLine = new OrderLine
        {
            UnitPriceExclVat = 100,
            VatPercentage = 25,
            Quantity = 0
        };

        // Act & Assert
        orderLine.AmountInclVat.ShouldBe(0m);
        orderLine.AmountExclVat.ShouldBe(0m);
    }

    [Test]
    public void ComputedProperties_With6PercentVat_CalculateCorrectly()
    {
        // Arrange
        var orderLine = new OrderLine
        {
            UnitPriceExclVat = 1000,
            VatPercentage = 6,
            Quantity = 1
        };

        // Act & Assert
        orderLine.UnitPriceInclVat.ShouldBe(1060m);
        orderLine.AmountInclVat.ShouldBe(1060m);
        orderLine.AmountExclVat.ShouldBe(1000m);
    }
}
