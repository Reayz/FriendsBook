using Microsoft.EntityFrameworkCore;
using PostService.Data;
using PostService.Services;
using Prometheus;
using Serilog;

// Configure Serilog
var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Replace default logging with Serilog
builder.Host.UseSerilog();

builder.Services.AddDbContext<PostServiceContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("PostDB")));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

// Register RabbitMQReceiverService as a background service
builder.Services.AddHostedService<RabbitMQReceiverService>();


var app = builder.Build();

// Add Prometheus metrics endpoint
app.UseHttpMetrics(); // Collect HTTP metrics like request count, duration, etc.
app.MapMetrics("/metrics"); // Expose metrics at /metrics endpoint

//Apply pending migrations on startup
//using (var scope = app.Services.CreateScope())
//{
//    var dbContext = scope.ServiceProvider.GetRequiredService<PostServiceContext>();
//    dbContext.Database.Migrate();
//}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

try
{
    Log.Information("Starting the application...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed!");
}
finally
{
    Log.CloseAndFlush(); // Ensure all logs are flushed on shutdown
}
