using DriverService.Data;
using DriverService.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DriverDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DriverDb")));
builder.Services.AddScoped<DriversService>();
builder.Services.AddHostedService<DriverRpcConsumer>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();
