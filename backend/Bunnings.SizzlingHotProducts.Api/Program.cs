using Bunnings.SizzlingHotProducts.Api.Services;
using Bunnings.SizzlingHotProducts.Api.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IOrderRepository, JsonOrderRepository>();
builder.Services.AddScoped<SizzlingHotProductService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("ExpoDev", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:8081",
                "http://127.0.0.1:8081"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("ExpoDev");

app.MapGet("/api/sizzling-hot-products", async (SizzlingHotProductService service) =>
{
    var results = await service.GetSizzlingHotProductsAsync();
    return Results.Ok(results);
})
.WithName("GetSizzlingHotProducts");

app.Run();