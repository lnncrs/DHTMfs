using System.Collections.Concurrent;
using System.Reflection;
using DHTMfs.Data;
using DHTMfs.Models;
using DHTMfs.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add custom services to the container
// builder.Services.AddDbContext<AppDbContext>(Options =>
// {
//     Options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
// });

var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
    .Options;

builder.Services.AddSingleton<AppDbContext>(new AppDbContext(options, builder.Configuration));

builder.Services.AddSingleton<NodeService>();
builder.Services.AddSingleton<FileService>();

builder.Services.AddSingleton<IDictionary<string, object>>(new ConcurrentDictionary<string, object>());

// Add core services to the container
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Get configuration
var configuration = builder.Services.BuildServiceProvider().GetService<IConfiguration>();

// Get host and port
var url = configuration["Urls"].Split(';')[1];
var host = url.Split(':')[1].Trim('/');
var port = int.Parse(url.Split(':')[2]);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Apply migrations
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();


    var nodeContext = scope.ServiceProvider.GetRequiredService<NodeService>();

    nodeContext.UpdateLocalNode();
    nodeContext.UpdateIsLocal();
}

app.Run();
