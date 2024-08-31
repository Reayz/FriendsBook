using Microsoft.EntityFrameworkCore;
using PostService.Data;
using PostService.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<PostServiceContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("PostDB")));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

// Register RabbitMQReceiverService as a background service
builder.Services.AddHostedService<RabbitMQReceiverService>();


var app = builder.Build();

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

app.Run();
