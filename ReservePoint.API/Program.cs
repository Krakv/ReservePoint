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
    var keycloakIssuer = configuration["Authentication:TokenValidationParameters:ValidIssuers:0"];

    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri($"{keycloakIssuer}/protocol/openid-connect/auth"),
                TokenUrl = new Uri($"{keycloakIssuer}/protocol/openid-connect/token"),
                Scopes = new Dictionary<string, string>
                {
                    { "openid", "openid" }
                }
            }
        }
    });

    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("oauth2", document), ["openid"]
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options =>
    {
        options.PreSerializeFilters.Add((swaggerDocument, httpRequest) =>
        {
            swaggerDocument.Servers =
            [
                new OpenApiServer
            {
                Url = app.Configuration["ASPNETCORE_PATHBASE"]
            }
            ];
        });
    });
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint($"./v1/swagger.json", "API v1");
        c.OAuthClientId("bookings-public-client");
        c.OAuthUsePkce();
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