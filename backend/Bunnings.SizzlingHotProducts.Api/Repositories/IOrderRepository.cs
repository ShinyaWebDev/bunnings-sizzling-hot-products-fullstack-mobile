using Bunnings.SizzlingHotProducts.Api.Models;

namespace Bunnings.SizzlingHotProducts.Api.Repositories;

public interface IOrderRepository
{
    Task<List<Order>> ReadOrdersAsync();
    Task<List<Product>> ReadProductsAsync();
}