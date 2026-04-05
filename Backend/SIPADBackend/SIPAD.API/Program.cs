using Microsoft.EntityFrameworkCore;
using SIPAD.Application.Helpers;
using SIPAD.Application.Services;
using SIPAD.Application.Services.ETL;
using SIPAD.Infrastructure.Data;
using Microsoft.AspNetCore.Http.Features;
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();
