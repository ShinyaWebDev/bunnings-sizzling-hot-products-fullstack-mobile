using Bunnings.SizzlingHotProducts.Api.Models;
using Bunnings.SizzlingHotProducts.Api.Repositories;
namespace Bunnings.SizzlingHotProducts.Api.Services;

public class SizzlingHotProductService
{
    private readonly IOrderRepository _repository;

    public SizzlingHotProductService(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<SizzlingHotProductResult>> GetSizzlingHotProductsAsync()
    {
        var orders = await _repository.ReadOrdersAsync();
        var products = await _repository.ReadProductsAsync();

        var productLookup = products.ToDictionary(p => p.Id, p => p.Name);

        var validCompletedOrders = GetValidCompletedOrders(orders);
        var groupedOrders = GroupByDate(validCompletedOrders);

        var today = ParseDate("23/04/2026");
        var startDate = today.AddDays(-2);

        var results = new List<SizzlingHotProductResult>();

        // Daily results
        for (var date = startDate; date <= today; date = date.AddDays(1))
        {
            var dateText = FormatDate(date);
            var ordersForDay = GetOrdersForPeriod(groupedOrders, dateText, dateText);

            results.Add(CalculateTopProduct("single", dateText, ordersForDay, productLookup));
        }

        // Range result
        var rangeOrders = GetOrdersForPeriod(groupedOrders, FormatDate(startDate), FormatDate(today));
        results.Add(CalculateTopProduct(
            "range",
            $"{FormatDate(startDate)} - {FormatDate(today)}",
            rangeOrders,
            productLookup
        ));

        return results;
    }

    private static List<Order> GetValidCompletedOrders(List<Order> orders)
    {
        var cancelledOrderIds = orders
            .Where(o => o.Status.Equals("cancelled", StringComparison.OrdinalIgnoreCase))
            .Select(o => o.OrderId)
            .ToHashSet();

        return orders
            .Where(o =>
                o.Status.Equals("completed", StringComparison.OrdinalIgnoreCase) &&
                !cancelledOrderIds.Contains(o.OrderId))
            .ToList();
    }

    private static Dictionary<string, List<Order>> GroupByDate(List<Order> orders)
    {
        return orders
            .GroupBy(o => o.Date)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    private static List<Order> GetOrdersForPeriod(
        Dictionary<string, List<Order>> groupedOrders,
        string startDate,
        string endDate)
    {
        var parsedStart = ParseDate(startDate);
        var parsedEnd = ParseDate(endDate);

        return groupedOrders
            .Where(g => ParseDate(g.Key) >= parsedStart && ParseDate(g.Key) <= parsedEnd)
            .SelectMany(g => g.Value)
            .ToList();
    }

    private static SizzlingHotProductResult CalculateTopProduct(
        string type,
        string label,
        List<Order> orders,
        Dictionary<string, string> productLookup)
    {
        var countedSales = new HashSet<string>();
        var productTotals = new Dictionary<string, int>();

        foreach (var order in orders)
        {
            var uniqueProductIds = order.Entries
                .Select(e => e.Id)
                .Distinct();

            foreach (var productId in uniqueProductIds)
            {
                var saleKey = $"{order.CustomerId}|{productId}|{order.Date}";

                if (countedSales.Add(saleKey))
                {
                    productTotals[productId] = productTotals.GetValueOrDefault(productId, 0) + 1;
                }
            }
        }

        var topProductId = productTotals
            .Where(t => t.Value > 0)
            .OrderByDescending(t => t.Value)
            .ThenBy(t => productLookup.GetValueOrDefault(t.Key, t.Key))
            .FirstOrDefault()
            .Key;

        return new SizzlingHotProductResult
        {
            Type = type,
            Period = label,
            ProductName = topProductId is null
                ? "No product sales"
                : productLookup.GetValueOrDefault(topProductId, topProductId)
        };
    }

    private static DateTime ParseDate(string date)
    {
        return DateTime.ParseExact(date, "dd/MM/yyyy", null);
    }

    private static string FormatDate(DateTime date)
    {
        return date.ToString("dd/MM/yyyy");
    }
}
