using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using POS.Server.Data;
using POS.Server.Repositories;
using POS.Server.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var jwtSecret = builder.Configuration["JwtSettings:SecretKey"];
if (string.IsNullOrWhiteSpace(jwtSecret))
    throw new InvalidOperationException(
        "Falta JwtSettings:SecretKey. Configúrela con User Secrets: " +
        "dotnet user-secrets set \"JwtSettings:SecretKey\" \"<clave-segura>\" --project POS/POS.Server/POS.Server.csproj");

builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IReceptionService, ReceptionService>();
builder.Services.AddScoped<IProductionLotService, ProductionLotService>();
builder.Services.AddScoped<IDispatchService, DispatchService>();
builder.Services.AddScoped(typeof(ICatalogRepository<>), typeof(CatalogRepository<>));
builder.Services.AddOpenApi();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"], ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
    };
});
builder.Services.AddAuthorization();
builder.Services.AddCors(options => options.AddPolicy("Client", policy => policy.WithOrigins("http://localhost:5010", "https://localhost:7007").AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
if (app.Environment.IsDevelopment()) app.MapOpenApi();
else app.UseHttpsRedirection();
app.UseCors("Client");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope()) await DbSeeder.SeedAsync(scope.ServiceProvider.GetRequiredService<AppDbContext>());
app.Run();

public partial class Program;
