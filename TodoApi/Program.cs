using Microsoft.EntityFrameworkCore;
using TodoApi.Configuration;
using TodoApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<TodoContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("TodoContext")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddMediatRServices();

// Add synchronization services
builder.Services.AddSynchronizationServices(builder.Configuration);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseAuthorization();
app.MapControllers();

// Ensure database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TodoContext>();
    context.Database.EnsureCreated();
}

app.Run();