using Bunnings.SizzlingHotProducts.Api.Models;
using Bunnings.SizzlingHotProducts.Api.Services;
using Bunnings.SizzlingHotProducts.Tests.Fakes;

namespace Bunnings.SizzlingHotProducts.Tests.Services;

public class SizzlingHotProductServiceTests
{
    private static SizzlingHotProductService CreateService(
        List<Order> orders,
        List<Product> products)
    {
        var fakeRepo = new FakeOrderRepository(orders, products);
        return new SizzlingHotProductService(fakeRepo);
    }

    private static List<Product> DefaultProducts => new()
    {
        new Product { Id = "P1", Name = "Hammer" },
        new Product { Id = "P2", Name = "BBQ" },
        new Product { Id = "P3", Name = "Drill" },
        new Product { Id = "P4", Name = "Shovel" },
    };

    // -----------------------------------------------------------------------
    // Testing on buisiness rules
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetSizzlingHotProducts_ReturnsTopProductForEachDay()
    {
        // Arrange
        var orders = new List<Order>
        {
            new() { OrderId = "O10", CustomerId = "C1", Status = "completed",
                    Date = "21/04/2026", Entries = [new() { Id = "P1", Quantity = 1 }] },
            new() { OrderId = "O20", CustomerId = "C2", Status = "completed",
                    Date = "21/04/2026", Entries = [new() { Id = "P1", Quantity = 1 }] },
            new() { OrderId = "O30", CustomerId = "C3", Status = "completed",
                    Date = "21/04/2026", Entries = [new() { Id = "P2", Quantity = 1 }] },
        };

        var service = CreateService(orders, DefaultProducts);

        // Act
        var results = await service.GetSizzlingHotProductsAsync();

        // Assert
        var day21 = results.First(r => r.Period == "21/04/2026");
        Assert.Equal("Hammer", day21.ProductName);
    }

    [Fact]
    public async Task GetSizzlingHotProducts_ReturnsTopProductForRange()
    {
        // Arrange
        var orders = new List<Order>
        {
            new() { OrderId = "O10", CustomerId = "C1", Status = "completed",
                    Date = "21/04/2026", Entries = [new() { Id = "P1", Quantity = 1 }] },
            new() { OrderId = "O20", CustomerId = "C2", Status = "completed",
                    Date = "22/04/2026", Entries = [new() { Id = "P1", Quantity = 1 }] },
            new() { OrderId = "O30", CustomerId = "C3", Status = "completed",
                    Date = "23/04/2026", Entries = [new() { Id = "P2", Quantity = 1 }] },
        };

        var service = CreateService(orders, DefaultProducts);

        // Act
        var results = await service.GetSizzlingHotProductsAsync();

        // Assert
        var range = results.First(r => r.Period == "21/04/2026 - 23/04/2026");
        Assert.Equal("Hammer", range.ProductName);
    }

    // -----------------------------------------------------------------------
    // Business rule 1 - quantity ignored, counted once per order
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetSizzlingHotProducts_CountsProductOncePerOrder_IgnoringQuantity()
    {
        // Arrange - C1 buys 99 of P1, C2 buys 1 of P2
        // P1 and P2 should both have 1 sale each → tiebreaker decides
        var orders = new List<Order>
        {
            new() { OrderId = "O10", CustomerId = "C1", Status = "completed",
                    Date = "21/04/2026", Entries = [new() { Id = "P1", Quantity = 99 }] },
            new() { OrderId = "O20", CustomerId = "C2", Status = "completed",
                    Date = "21/04/2026", Entries = [new() { Id = "P2", Quantity = 1 }] },
        };

        var service = CreateService(orders, DefaultProducts);

        // Act
        var results = await service.GetSizzlingHotProductsAsync();

        // Assert - BBQ comes before Hammer alphabetically → BBQ wins tiebreaker
        var day21 = results.First(r => r.Period == "21/04/2026");
        Assert.Equal("BBQ", day21.ProductName);
    }

    // -----------------------------------------------------------------------
    // Business rule 2 - same customer, same product, same day = 1 sale
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetSizzlingHotProducts_DeduplicatesSameCustomerSameProductSameDay()
    {
        // Arrange - C1 orders P1 twice on same day in different orders
        // C2 orders P2 once → P2 should win with 1 sale vs P1's 1 sale (deduplicated)
        var orders = new List<Order>
        {
            new() { OrderId = "O10", CustomerId = "C1", Status = "completed",
                    Date = "21/04/2026", Entries = [new() { Id = "P1", Quantity = 2 }] },
            new() { OrderId = "O11", CustomerId = "C1", Status = "completed",
                    Date = "21/04/2026", Entries = [new() { Id = "P1", Quantity = 3 }] },
            new() { OrderId = "O20", CustomerId = "C2", Status = "completed",
                    Date = "21/04/2026", Entries = [new() { Id = "P2", Quantity = 1 }] },
        };

        var service = CreateService(orders, DefaultProducts);

        // Act
        var results = await service.GetSizzlingHotProductsAsync();

        // Assert - P1 = 1 sale (deduplicated), P2 = 1 sale → BBQ wins alphabetically
        var day21 = results.First(r => r.Period == "21/04/2026");
        Assert.Equal("BBQ", day21.ProductName);
    }

    // -----------------------------------------------------------------------
    // Business rule 3 - cancelled orders
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetSizzlingHotProducts_ExcludesCancelledOrders()
    {
        // Arrange - O10 placed on 21st, cancelled on 22nd
        var orders = new List<Order>
        {
            new() { OrderId = "O10", CustomerId = "C1", Status = "completed",
                    Date = "21/04/2026", Entries = [new() { Id = "P1", Quantity = 1 }] },
            new() { OrderId = "O10", CustomerId = "C1", Status = "cancelled",
                    Date = "22/04/2026" },
        };

        var service = CreateService(orders, DefaultProducts);

        // Act
        var results = await service.GetSizzlingHotProductsAsync();

        // Assert - no valid sales remain
        var day21 = results.First(r => r.Period == "21/04/2026");
        Assert.Equal("No product sales", day21.ProductName);
    }

    // -----------------------------------------------------------------------
    // Business rule 4 - tiebreaker
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetSizzlingHotProducts_BreaksTieAlphabetically()
    {
        // Arrange - P1 (Hammer) and P2 (BBQ) both have 1 sale
        var orders = new List<Order>
        {
            new() { OrderId = "O10", CustomerId = "C1", Status = "completed",
                    Date = "21/04/2026", Entries = [new() { Id = "P1", Quantity = 1 }] },
            new() { OrderId = "O20", CustomerId = "C2", Status = "completed",
                    Date = "21/04/2026", Entries = [new() { Id = "P2", Quantity = 1 }] },
        };

        var service = CreateService(orders, DefaultProducts);

        // Act
        var results = await service.GetSizzlingHotProductsAsync();

        // Assert - BBQ before Hammer alphabetically
        var day21 = results.First(r => r.Period == "21/04/2026");
        Assert.Equal("BBQ", day21.ProductName);
    }

    // -----------------------------------------------------------------------
    // Edge cases testing
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetSizzlingHotProducts_ReturnsNoProductSales_WhenNoOrders()
    {
        // Arrange - empty orders
        var orders = new List<Order>();
        var service = CreateService(orders, DefaultProducts);

        // Act
        var results = await service.GetSizzlingHotProductsAsync();

        // Assert - all periods return no product sales
        Assert.All(results, r => Assert.Equal("No product sales", r.ProductName));
    }

    [Fact]
    public async Task GetSizzlingHotProducts_ReturnsNoProductSales_WhenAllOrdersCancelled()
    {
        // Arrange - all orders cancelled
        var orders = new List<Order>
        {
            new() { OrderId = "O10", CustomerId = "C1", Status = "completed",
                    Date = "21/04/2026", Entries = [new() { Id = "P1", Quantity = 1 }] },
            new() { OrderId = "O20", CustomerId = "C2", Status = "completed",
                    Date = "21/04/2026", Entries = [new() { Id = "P2", Quantity = 1 }] },
            new() { OrderId = "O10", CustomerId = "C1", Status = "cancelled", Date = "21/04/2026" },
            new() { OrderId = "O20", CustomerId = "C2", Status = "cancelled", Date = "21/04/2026" },
        };

        var service = CreateService(orders, DefaultProducts);

        // Act
        var results = await service.GetSizzlingHotProductsAsync();

        // Assert
        var day21 = results.First(r => r.Period == "21/04/2026");
        Assert.Equal("No product sales", day21.ProductName);
    }

    [Fact]
    public async Task GetSizzlingHotProducts_HandlesDuplicateProductEntriesInSameOrder()
    {
        // Arrange - same product appears twice in one order's entries
        var orders = new List<Order>
        {
            new() { OrderId = "O10", CustomerId = "C1", Status = "completed",
                    Date = "21/04/2026", Entries = [
                        new() { Id = "P1", Quantity = 1 },
                        new() { Id = "P1", Quantity = 2 }   // ← duplicate product in entries
                    ]},
            new() { OrderId = "O20", CustomerId = "C2", Status = "completed",
                    Date = "21/04/2026", Entries = [new() { Id = "P2", Quantity = 1 }] },
        };

        var service = CreateService(orders, DefaultProducts);

        // Act
        var results = await service.GetSizzlingHotProductsAsync();

        // Assert - P1 = 1 sale (duplicate entry ignored), P2 = 1 sale → BBQ wins
        var day21 = results.First(r => r.Period == "21/04/2026");
        Assert.Equal("BBQ", day21.ProductName);
    }

    [Fact]
    public async Task GetSizzlingHotProducts_FallsBackToProductId_WhenProductNotInLookup()
    {
        // Arrange - P99 not in products.json
        var orders = new List<Order>
        {
            new() { OrderId = "O10", CustomerId = "C1", Status = "completed",
                    Date = "21/04/2026", Entries = [new() { Id = "P99", Quantity = 1 }] },
        };

        var service = CreateService(orders, DefaultProducts);

        // Act
        var results = await service.GetSizzlingHotProductsAsync();

        // Assert - falls back to product ID
        var day21 = results.First(r => r.Period == "21/04/2026");
        Assert.Equal("P99", day21.ProductName);
    }

    [Fact]
    public async Task GetSizzlingHotProducts_ReturnsFourResults_ThreeDailyOneRange()
    {
        // Arrange
        var orders = new List<Order>
        {
            new() { OrderId = "O10", CustomerId = "C1", Status = "completed",
                    Date = "21/04/2026", Entries = [new() { Id = "P1", Quantity = 1 }] },
        };

        var service = CreateService(orders, DefaultProducts);

        // Act
        var results = await service.GetSizzlingHotProductsAsync();

        // Assert - always 4 results (3 daily + 1 range)
        Assert.Equal(4, results.Count);
    }
}