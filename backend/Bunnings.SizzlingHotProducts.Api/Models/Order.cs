namespace Bunnings.SizzlingHotProducts.Api.Models;

public class Order
{
    public required string OrderId { get; set; }
    public required string CustomerId { get; set; }
    public List<OrderEntry> Entries { get; set; } = new();
    public required string Date { get; set; }
    public required string Status { get; set; }
}