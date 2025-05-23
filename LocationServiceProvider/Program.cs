using LocationServiceProvider.Data.Contexts;
using LocationServiceProvider.Data.Repositories;
using LocationServiceProvider.Helpers;
using LocationServiceProvider.Interfaces;
using LocationServiceProvider.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddMemoryCache();

builder.Services.AddDbContext<DataContext>(x => x.UseSqlServer(builder.Configuration.GetConnectionString("LocationDbConnection")));

builder.Services.AddTransient<IFieldValidator, RequiredFieldsValidator>();
builder.Services.AddTransient<ISeatValidator, LocationSeatValidator>();
builder.Services.AddTransient<ISeatGenerator, SeatGenerator>();
builder.Services.AddSingleton<ICacheHandler, CacheHandler>();
builder.Services.AddScoped<ILocationRepository, LocationRepository>();

var app = builder.Build();

app.MapGrpcService<LocationService>();
app.MapGet("/", () =>
    "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
