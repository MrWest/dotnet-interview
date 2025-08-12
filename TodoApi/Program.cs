using Microsoft.EntityFrameworkCore;
using TodoApi.Configuration;
// using TodoApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<TodoContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("TodoContext")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddMediatRServices(); // Add MediatR services

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

app.UseAuthorization();
app.MapControllers();
app.Run();
