using Bunnings.SizzlingHotProducts.Api.Models;
using Bunnings.SizzlingHotProducts.Api.Repositories;

namespace Bunnings.SizzlingHotProducts.Tests.Fakes;

public class FakeOrderRepository : IOrderRepository
{
    private readonly List<Order> _orders;
    private readonly List<Product> _products;

    public FakeOrderRepository(List<Order> orders, List<Product> products)
    {
        _orders = orders;
        _products = products;
    }

    public Task<List<Order>> ReadOrdersAsync()
        => Task.FromResult(_orders);

    public Task<List<Product>> ReadProductsAsync()
        => Task.FromResult(_products);
}