using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
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

builder.Services.AddSwaggerGen(c =>
{
    var keycloakIssuer = configuration["Authentication:TokenValidationParameters:ValidIssuers:2"]
        ?? configuration["Authentication:TokenValidationParameters:ValidIssuers:0"];

    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            Password = new OpenApiOAuthFlow
            {
                TokenUrl = new Uri($"{keycloakIssuer}/protocol/openid-connect/token"),
            }
        }
    });
});

var app = builder.Build();

app.UsePathBase(app.Configuration["ASPNETCORE_PATHBASE"]);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint($"{app.Configuration["ASPNETCORE_PATHBASE"]}/swagger/v1/swagger.json", "API v1");
    });
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();