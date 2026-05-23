using DriverService.Data;
using DriverService.Services;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DriverDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DriverDb")));
builder.Services.AddScoped<DriversService>();
builder.Services.AddHostedService<DriverRpcConsumer>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

await EnsureDatabaseExistsAsync(app.Configuration);
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DriverDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();

static async Task EnsureDatabaseExistsAsync(IConfiguration configuration)
{
    var connectionString = configuration.GetConnectionString("DriverDb")
        ?? throw new InvalidOperationException("Connection string 'DriverDb' is not configured.");

    var builder = new NpgsqlConnectionStringBuilder(connectionString);
    var databaseName = builder.Database;
    if (string.IsNullOrWhiteSpace(databaseName))
    {
        throw new InvalidOperationException("Database name is missing in 'DriverDb' connection string.");
    }

    builder.Database = "postgres";
    await using var connection = new NpgsqlConnection(builder.ConnectionString);
    await connection.OpenAsync();

    var commandText = $"CREATE DATABASE \"{databaseName.Replace("\"", "\"\"")}\"";
    await using var command = new NpgsqlCommand(commandText, connection);

    try
    {
        await command.ExecuteNonQueryAsync();
    }
    catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.DuplicateDatabase)
    {
    }
}
