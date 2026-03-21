using Microsoft.EntityFrameworkCore;
using ReservePoint.Application.Interfaces;
using ReservePoint.Application.Services;
using ReservePoint.Infrastructure.Clients;
using ReservePoint.Infrastructure.Persistence;
using ReservePoint.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddAuthorization();
builder.Services.AddAuthentication().AddJwtBearer(options =>
{
    configuration.GetSection("Authentication").Bind(options);
});
builder.Services.AddHttpContextAccessor();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<IBookingRepository, BookingRepository>();

// Services
builder.Services.AddScoped<IBookingService, BookingService>();

// HTTP Clients
builder.Services.AddHttpClient<IResourcesClient, ResourcesClient>(client =>
{
    client.BaseAddress = new Uri(configuration["Services:ResourcesServiceUrl"]!);
});

builder.Services.AddHttpClient<IOrgClient, OrgClient>(client =>
{
    client.BaseAddress = new Uri(configuration["Services:OrgServiceUrl"]!);
});

builder.Services.AddHttpClient<IUserClient, UserClient>(client =>
{
    client.BaseAddress = new Uri(configuration["Services:UserServiceUrl"]!);
}); 

builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.UsePathBase(app.Configuration["ASPNETCORE_PATHBASE"]);
app.UseRouting();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();