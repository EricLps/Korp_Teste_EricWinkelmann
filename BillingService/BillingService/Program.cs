using BillingService.Data;
using BillingService.Repositories;
using BillingService.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
builder.Services.AddDbContext<BillingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient<StockHttpClient>((sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var baseUrl = config["StockService:BaseUrl"] ?? "http://localhost:5001";
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddScoped<InvoiceRepository>();
builder.Services.AddScoped<InvoiceService>();
builder.Services.AddHostedService<InvoiceExpiryBackgroundService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.MapControllers();

app.Run();