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

builder.Services.AddSingleton<HFile>();

// Add core services to the container
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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
    dbContext.Database.EnsureCreated();

    var nodeContext = scope.ServiceProvider.GetRequiredService<NodeService>();
    nodeContext.UpdateLocalNode();
    nodeContext.UpdateIsLocal();

    var fileContext = scope.ServiceProvider.GetRequiredService<FileService>();
    fileContext.UpdateFileList();
}

app.Run();
