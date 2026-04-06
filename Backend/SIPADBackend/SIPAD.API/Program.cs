using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SIPAD.Application.Helpers;
using SIPAD.Application.Services;
using SIPAD.Application.Services.ETL;
using SIPAD.Infrastructure.Data;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.IdentityModel.Tokens;
using SIPAD.API.Options;
using SIPAD.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS Configuration - Modificar seg�n el entorno del FRONT

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new string[] { "http://localhost:3000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyMethod()                 // Permite GET, POST, PUT, DELETE, etc.
            .AllowAnyHeader()                 // Permite cualquier header
            .AllowCredentials();              // Permite cookies y autenticaci�n
    });
});


// Configurar DbContext con PostgreSQL

builder.Services.AddDbContext<SipadDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.EnableRetryOnFailure()
    ));

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
    })
    .AddRoles<IdentityRole>()
    .AddSignInManager()
    .AddEntityFrameworkStores<SipadDbContext>();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key));

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

builder.Services.AddAuthorization();

// Registrar Helper
builder.Services.AddScoped<CsvReaderHelper>();

// Registrar Servicios ETL
builder.Services.AddScoped<DivisionTerritorialEtlService>();
builder.Services.AddScoped<CentroEducativoEtlService>();
builder.Services.AddScoped<RendimientoAcademicoEtlService>();
builder.Services.AddScoped<IndicadorSocioeconomicoEtlService>();
builder.Services.AddScoped<ServicioBasicoEtlService>();
builder.Services.AddScoped<MatriculaDesercionEtlService>();
builder.Services.AddScoped<RecursosCentrosEtlService>();
builder.Services.AddScoped<EmbarazoAdolescenteEtlService>();
builder.Services.AddScoped<ViolenciaDelincuenciaEtlService>();
builder.Services.AddScoped<TrabajoInfantilEtlService>();
builder.Services.AddScoped<CoberturaEducativaEtlService>();
builder.Services.AddScoped<PruebasNacionalesEtlService>();
builder.Services.AddScoped<CsvValidationService>();

builder.Services.AddScoped<EtlPipelineService>();

builder.Services.Configure<GeminiAiOptions>(builder.Configuration.GetSection("GeminiAi"));
builder.Services.AddHttpClient<AiRecommendationsService>();
builder.Services.AddScoped<AiRecommendationsService>();
builder.Services.AddScoped<JwtTokenService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var requiredRoles = new[] { "Admin", "User" };
    foreach (var role in requiredRoles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
