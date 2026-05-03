using System.Text.Json;
using Bunnings.SizzlingHotProducts.Api.Models;

namespace Bunnings.SizzlingHotProducts.Api.Repositories;

public class JsonOrderRepository : IOrderRepository
{
    private readonly IWebHostEnvironment _environment;
    private readonly JsonSerializerOptions _jsonOptions;

    public JsonOrderRepository(IWebHostEnvironment environment)
    {
        _environment = environment;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<List<Order>> ReadOrdersAsync()
    {
        var path = Path.Combine(_environment.ContentRootPath, "Inputs", "orders.json");
        var json = await File.ReadAllTextAsync(path);

        return JsonSerializer.Deserialize<List<Order>>(json, _jsonOptions) ?? new List<Order>();
    }

    public async Task<List<Product>> ReadProductsAsync()
    {
        var path = Path.Combine(_environment.ContentRootPath, "Inputs", "products.json");
        var json = await File.ReadAllTextAsync(path);

        return JsonSerializer.Deserialize<List<Product>>(json, _jsonOptions) ?? new List<Product>();
    }
}