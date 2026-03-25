using Microsoft.EntityFrameworkCore;
using SIPAD.Domain.Entities;

namespace SIPAD.Infrastructure.Data;

public class SipadDbContext : DbContext
{
    public SipadDbContext(DbContextOptions<SipadDbContext> options) : base(options)
    {
    }

    // DbSets (representan las tablas)
    public DbSet<Provincia> Provincias { get; set; }
    public DbSet<Municipio> Municipios { get; set; }
    public DbSet<RegionalEducativa> RegionalesEducativas { get; set; }
    public DbSet<DistritoEducativo> DistritosEducativos { get; set; }
    public DbSet<CentroEducativo> CentrosEducativos { get; set; }
    public DbSet<PeriodoAcademico> PeriodosAcademicos { get; set; }
    public DbSet<RendimientoAcademico> RendimientosAcademicos { get; set; }
    public DbSet<IndicadorSocioeconomico> IndicadoresSocioeconomicos { get; set; }
    public DbSet<ServicioBasico> ServiciosBasicos { get; set; }
    public DbSet<EstadisticaAbandono> EstadisticasAbandono { get; set; }
    public DbSet<RecursoEducativo> RecursosEducativos { get; set; }
    public DbSet<FactorRiesgoSocial> FactoresRiesgoSocial { get; set; }
    public DbSet<DemocratizacionEducativa> DemocratizacionesEducativas { get; set; }
    public DbSet<CausanteAbandono> CausantesAbandono { get; set; }
    public DbSet<AbandonoCausanteZona> AbandonoCausantesZona { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurar nombres de tablas (snake_case para PostgreSQL)
        modelBuilder.Entity<Provincia>().ToTable("provincias");
        modelBuilder.Entity<Municipio>().ToTable("municipios");
        modelBuilder.Entity<RegionalEducativa>().ToTable("regionales_educativas");
        modelBuilder.Entity<DistritoEducativo>().ToTable("distritos_educativos");
        modelBuilder.Entity<CentroEducativo>().ToTable("centros_educativos");
        modelBuilder.Entity<PeriodoAcademico>().ToTable("periodos_academicos");
        modelBuilder.Entity<RendimientoAcademico>().ToTable("rendimiento_academico");
        modelBuilder.Entity<IndicadorSocioeconomico>().ToTable("indicadores_socioeconomicos");
        modelBuilder.Entity<ServicioBasico>().ToTable("servicios_basicos");
        modelBuilder.Entity<EstadisticaAbandono>().ToTable("estadisticas_abandono");
        modelBuilder.Entity<RecursoEducativo>().ToTable("recursos_educativos");
        modelBuilder.Entity<FactorRiesgoSocial>().ToTable("factores_riesgo_social");
        modelBuilder.Entity<DemocratizacionEducativa>().ToTable("democratizacion_educativa");
        modelBuilder.Entity<CausanteAbandono>().ToTable("causantes_abandono");
        modelBuilder.Entity<AbandonoCausanteZona>().ToTable("abandono_causantes_zona");

        // Configurar claves primarias
        modelBuilder.Entity<Provincia>().HasKey(p => p.IdProvincia);
        modelBuilder.Entity<Municipio>().HasKey(m => m.IdMunicipio);
        modelBuilder.Entity<RegionalEducativa>().HasKey(r => r.IdRegional);
        modelBuilder.Entity<DistritoEducativo>().HasKey(d => d.IdDistrito);
        modelBuilder.Entity<CentroEducativo>().HasKey(c => c.IdCentro);
        modelBuilder.Entity<PeriodoAcademico>().HasKey(p => p.IdPeriodo);
        modelBuilder.Entity<RendimientoAcademico>().HasKey(r => r.IdRendimiento);
        modelBuilder.Entity<IndicadorSocioeconomico>().HasKey(i => i.IdIndicadorSocio);
        modelBuilder.Entity<ServicioBasico>().HasKey(s => s.IdServicio);
        modelBuilder.Entity<EstadisticaAbandono>().HasKey(e => e.IdEstadistica);
        modelBuilder.Entity<RecursoEducativo>().HasKey(r => r.IdRecurso);
        modelBuilder.Entity<FactorRiesgoSocial>().HasKey(f => f.IdRiesgoSocial);
        modelBuilder.Entity<DemocratizacionEducativa>().HasKey(d => d.IdDemocratizacion);
        modelBuilder.Entity<CausanteAbandono>().HasKey(c => c.IdCausante);
        modelBuilder.Entity<AbandonoCausanteZona>().HasKey(a => a.IdRegistro);

        // Configurar nombres de columnas (snake_case)
        ConfigureProvinciaColumns(modelBuilder);
        ConfigureMunicipioColumns(modelBuilder);
        ConfigureRegionalColumns(modelBuilder);
        ConfigureDistritoColumns(modelBuilder);
        ConfigureCentroColumns(modelBuilder);
        ConfigurePeriodoColumns(modelBuilder);
        ConfigureRendimientoColumns(modelBuilder);
        ConfigureIndicadorSocioColumns(modelBuilder);
        ConfigureServicioColumns(modelBuilder);
        ConfigureEstadisticaAbandonoColumns(modelBuilder);
        ConfigureRecursoEducativoColumns(modelBuilder);
        ConfigureFactorRiesgoSocialColumns(modelBuilder);
        ConfigureDemocratizacionColumns(modelBuilder);
        ConfigureCausanteAbandonoColumns(modelBuilder);
        ConfigureAbandonoCausanteZonaColumns(modelBuilder);

        // Configurar relaciones
        ConfigureRelationships(modelBuilder);

        // Configurar índices únicos
        modelBuilder.Entity<Provincia>()
            .HasIndex(p => p.CodigoProvincia)
            .IsUnique();

        modelBuilder.Entity<Municipio>()
            .HasIndex(m => m.CodigoMunicipio)
            .IsUnique();

        modelBuilder.Entity<CentroEducativo>()
            .HasIndex(c => c.CodigoCentro)
            .IsUnique();

        // Configurar valores por defecto
        modelBuilder.Entity<Provincia>()
            .Property(p => p.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<Provincia>()
            .Property(p => p.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
    }

    private void ConfigureProvinciaColumns(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Provincia>()
            .Property(p => p.IdProvincia).HasColumnName("id_provincia");
        modelBuilder.Entity<Provincia>()
            .Property(p => p.CodigoProvincia).HasColumnName("codigo_provincia");
        modelBuilder.Entity<Provincia>()
            .Property(p => p.NombreProvincia).HasColumnName("nombre_provincia");
        modelBuilder.Entity<Provincia>()
            .Property(p => p.RegionGeografica).HasColumnName("region_geografica");
        modelBuilder.Entity<Provincia>()
            .Property(p => p.CreatedAt).HasColumnName("created_at");
        modelBuilder.Entity<Provincia>()
            .Property(p => p.UpdatedAt).HasColumnName("updated_at");
    }

    private void ConfigureMunicipioColumns(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Municipio>()
            .Property(m => m.IdMunicipio).HasColumnName("id_municipio");
        modelBuilder.Entity<Municipio>()
            .Property(m => m.CodigoMunicipio).HasColumnName("codigo_municipio");
        modelBuilder.Entity<Municipio>()
            .Property(m => m.NombreMunicipio).HasColumnName("nombre_municipio");
        modelBuilder.Entity<Municipio>()
            .Property(m => m.IdProvincia).HasColumnName("id_provincia");
        modelBuilder.Entity<Municipio>()
            .Property(m => m.PoblacionEstimada).HasColumnName("poblacion_estimada");
        modelBuilder.Entity<Municipio>()
            .Property(m => m.AreaKm2).HasColumnName("area_km2");
        modelBuilder.Entity<Municipio>()
            .Property(m => m.CreatedAt).HasColumnName("created_at");
        modelBuilder.Entity<Municipio>()
            .Property(m => m.UpdatedAt).HasColumnName("updated_at");
    }

    private void ConfigureRegionalColumns(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RegionalEducativa>()
            .Property(r => r.IdRegional).HasColumnName("id_regional");
        modelBuilder.Entity<RegionalEducativa>()
            .Property(r => r.CodigoRegional).HasColumnName("codigo_regional");
        modelBuilder.Entity<RegionalEducativa>()
            .Property(r => r.NombreRegional).HasColumnName("nombre_regional");
        modelBuilder.Entity<RegionalEducativa>()
            .Property(r => r.SedeRegional).HasColumnName("sede_regional");
        modelBuilder.Entity<RegionalEducativa>()
            .Property(r => r.Telefono).HasColumnName("telefono");
        modelBuilder.Entity<RegionalEducativa>()
            .Property(r => r.CreatedAt).HasColumnName("created_at");
        modelBuilder.Entity<RegionalEducativa>()
            .Property(r => r.UpdatedAt).HasColumnName("updated_at");
    }

    private void ConfigureDistritoColumns(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DistritoEducativo>()
            .Property(d => d.IdDistrito).HasColumnName("id_distrito");
        modelBuilder.Entity<DistritoEducativo>()
            .Property(d => d.CodigoDistrito).HasColumnName("codigo_distrito");
        modelBuilder.Entity<DistritoEducativo>()
            .Property(d => d.NombreDistrito).HasColumnName("nombre_distrito");
        modelBuilder.Entity<DistritoEducativo>()
            .Property(d => d.IdRegional).HasColumnName("id_regional");
        modelBuilder.Entity<DistritoEducativo>()
            .Property(d => d.IdProvincia).HasColumnName("id_provincia");
        modelBuilder.Entity<DistritoEducativo>()
            .Property(d => d.IdMunicipio).HasColumnName("id_municipio");
        modelBuilder.Entity<DistritoEducativo>()
            .Property(d => d.CreatedAt).HasColumnName("created_at");
        modelBuilder.Entity<DistritoEducativo>()
            .Property(d => d.UpdatedAt).HasColumnName("updated_at");
    }

    private void ConfigureCentroColumns(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CentroEducativo>()
            .Property(c => c.IdCentro).HasColumnName("id_centro");
        modelBuilder.Entity<CentroEducativo>()
            .Property(c => c.CodigoCentro).HasColumnName("codigo_centro");
        modelBuilder.Entity<CentroEducativo>()
            .Property(c => c.NombreCentro).HasColumnName("nombre_centro");
        modelBuilder.Entity<CentroEducativo>()
            .Property(c => c.IdDistrito).HasColumnName("id_distrito");
        modelBuilder.Entity<CentroEducativo>()
            .Property(c => c.IdMunicipio).HasColumnName("id_municipio");
        modelBuilder.Entity<CentroEducativo>()
            .Property(c => c.Sector).HasColumnName("sector");
        modelBuilder.Entity<CentroEducativo>()
            .Property(c => c.Nivel).HasColumnName("nivel");
        modelBuilder.Entity<CentroEducativo>()
            .Property(c => c.Zona).HasColumnName("zona");
        modelBuilder.Entity<CentroEducativo>()
            .Property(c => c.Direccion).HasColumnName("direccion");
        modelBuilder.Entity<CentroEducativo>()
            .Property(c => c.Latitud).HasColumnName("latitud");
        modelBuilder.Entity<CentroEducativo>()
            .Property(c => c.Longitud).HasColumnName("longitud");
        modelBuilder.Entity<CentroEducativo>()
            .Property(c => c.CreatedAt).HasColumnName("created_at");
        modelBuilder.Entity<CentroEducativo>()
            .Property(c => c.UpdatedAt).HasColumnName("updated_at");
    }

    private void ConfigurePeriodoColumns(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PeriodoAcademico>()
            .Property(p => p.IdPeriodo).HasColumnName("id_periodo");
        modelBuilder.Entity<PeriodoAcademico>()
            .Property(p => p.AnioEscolar).HasColumnName("anio_escolar");
        modelBuilder.Entity<PeriodoAcademico>()
            .Property(p => p.Periodo).HasColumnName("periodo");
        modelBuilder.Entity<PeriodoAcademico>()
            .Property(p => p.FechaInicio).HasColumnName("fecha_inicio");
        modelBuilder.Entity<PeriodoAcademico>()
            .Property(p => p.FechaFin).HasColumnName("fecha_fin");
        modelBuilder.Entity<PeriodoAcademico>()
            .Property(p => p.Activo).HasColumnName("activo");
        modelBuilder.Entity<PeriodoAcademico>()
            .Property(p => p.CreatedAt).HasColumnName("created_at");
    }

    private void ConfigureRendimientoColumns(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RendimientoAcademico>()
            .Property(r => r.IdRendimiento).HasColumnName("id_rendimiento");
        modelBuilder.Entity<RendimientoAcademico>()
            .Property(r => r.IdCentro).HasColumnName("id_centro");
        modelBuilder.Entity<RendimientoAcademico>()
            .Property(r => r.IdPeriodo).HasColumnName("id_periodo");
        modelBuilder.Entity<RendimientoAcademico>()
            .Property(r => r.NivelEducativo).HasColumnName("nivel_educativo");
        modelBuilder.Entity<RendimientoAcademico>()
            .Property(r => r.PromedioCalificaciones).HasColumnName("promedio_calificaciones");
        modelBuilder.Entity<RendimientoAcademico>()
            .Property(r => r.TasaAprobacion).HasColumnName("tasa_aprobacion");
        modelBuilder.Entity<RendimientoAcademico>()
            .Property(r => r.TasaRepitencia).HasColumnName("tasa_repitencia");
        modelBuilder.Entity<RendimientoAcademico>()
            .Property(r => r.TasaSobreedad).HasColumnName("tasa_sobreedad");
        modelBuilder.Entity<RendimientoAcademico>()
            .Property(r => r.TotalEstudiantes).HasColumnName("total_estudiantes");
        modelBuilder.Entity<RendimientoAcademico>()
            .Property(r => r.EstudiantesAprobados).HasColumnName("estudiantes_aprobados");
        modelBuilder.Entity<RendimientoAcademico>()
            .Property(r => r.EstudiantesReprobados).HasColumnName("estudiantes_reprobados");
        modelBuilder.Entity<RendimientoAcademico>()
            .Property(r => r.EstudiantesSobreedad).HasColumnName("estudiantes_sobreedad");
        modelBuilder.Entity<RendimientoAcademico>()
            .Property(r => r.CreatedAt).HasColumnName("created_at");
        modelBuilder.Entity<RendimientoAcademico>()
            .Property(r => r.UpdatedAt).HasColumnName("updated_at");
    }

    private void ConfigureIndicadorSocioColumns(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IndicadorSocioeconomico>()
            .Property(i => i.IdIndicadorSocio).HasColumnName("id_indicador_socio");
        modelBuilder.Entity<IndicadorSocioeconomico>()
            .Property(i => i.IdMunicipio).HasColumnName("id_municipio");
        modelBuilder.Entity<IndicadorSocioeconomico>()
            .Property(i => i.Anio).HasColumnName("anio");
        modelBuilder.Entity<IndicadorSocioeconomico>()
            .Property(i => i.IngresoPromedioMensual).HasColumnName("ingreso_promedio_mensual");
        modelBuilder.Entity<IndicadorSocioeconomico>()
            .Property(i => i.TasaPobrezaMonetaria).HasColumnName("tasa_pobreza_monetaria");
        modelBuilder.Entity<IndicadorSocioeconomico>()
            .Property(i => i.TasaPobrezaExtrema).HasColumnName("tasa_pobreza_extrema");
        modelBuilder.Entity<IndicadorSocioeconomico>()
            .Property(i => i.IndiceTrabajoInfantil).HasColumnName("indice_trabajo_infantil");
        modelBuilder.Entity<IndicadorSocioeconomico>()
            .Property(i => i.TasaDesempleo).HasColumnName("tasa_desempleo");
        modelBuilder.Entity<IndicadorSocioeconomico>()
            .Property(i => i.NivelInformalidadLaboral).HasColumnName("nivel_informalidad_laboral");
        modelBuilder.Entity<IndicadorSocioeconomico>()
            .Property(i => i.FuenteDatos).HasColumnName("fuente_datos");
        modelBuilder.Entity<IndicadorSocioeconomico>()
            .Property(i => i.CreatedAt).HasColumnName("created_at");
        modelBuilder.Entity<IndicadorSocioeconomico>()
            .Property(i => i.UpdatedAt).HasColumnName("updated_at");
    }

    private void ConfigureServicioColumns(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ServicioBasico>()
            .Property(s => s.IdServicio).HasColumnName("id_servicio");
        modelBuilder.Entity<ServicioBasico>()
            .Property(s => s.IdMunicipio).HasColumnName("id_municipio");
        modelBuilder.Entity<ServicioBasico>()
            .Property(s => s.Anio).HasColumnName("anio");
        modelBuilder.Entity<ServicioBasico>()
            .Property(s => s.CoberturaAguaPotable).HasColumnName("cobertura_agua_potable");
        modelBuilder.Entity<ServicioBasico>()
            .Property(s => s.CoberturaEnergiaElectrica).HasColumnName("cobertura_energia_electrica");
        modelBuilder.Entity<ServicioBasico>()
            .Property(s => s.CoberturaSaneamientoBasico).HasColumnName("cobertura_saneamiento_basico");
        modelBuilder.Entity<ServicioBasico>()
            .Property(s => s.AccesoInternet).HasColumnName("acceso_internet");
        modelBuilder.Entity<ServicioBasico>()
            .Property(s => s.FuenteDatos).HasColumnName("fuente_datos");
        modelBuilder.Entity<ServicioBasico>()
            .Property(s => s.CreatedAt).HasColumnName("created_at");
        modelBuilder.Entity<ServicioBasico>()
            .Property(s => s.UpdatedAt).HasColumnName("updated_at");
    }

    private void ConfigureRelationships(ModelBuilder modelBuilder)
    {
        // Provincia -> Municipios
        modelBuilder.Entity<Municipio>()
            .HasOne(m => m.Provincia)
            .WithMany(p => p.Municipios)
            .HasForeignKey(m => m.IdProvincia)
            .OnDelete(DeleteBehavior.Cascade);

        // Regional -> Distritos
        modelBuilder.Entity<DistritoEducativo>()
            .HasOne(d => d.Regional)
            .WithMany(r => r.DistritosEducativos)
            .HasForeignKey(d => d.IdRegional)
            .OnDelete(DeleteBehavior.Cascade);

        // Distrito -> Centros
        modelBuilder.Entity<CentroEducativo>()
            .HasOne(c => c.Distrito)
            .WithMany(d => d.CentrosEducativos)
            .HasForeignKey(c => c.IdDistrito)
            .OnDelete(DeleteBehavior.Cascade);

        // Centro -> Rendimientos
        modelBuilder.Entity<RendimientoAcademico>()
            .HasOne(r => r.Centro)
            .WithMany(c => c.RendimientosAcademicos)
            .HasForeignKey(r => r.IdCentro)
            .OnDelete(DeleteBehavior.Cascade);

        // Periodo -> Rendimientos
        modelBuilder.Entity<RendimientoAcademico>()
            .HasOne(r => r.Periodo)
            .WithMany(p => p.RendimientosAcademicos)
            .HasForeignKey(r => r.IdPeriodo)
            .OnDelete(DeleteBehavior.Cascade);

        // Municipio -> IndicadoresSocio
        modelBuilder.Entity<IndicadorSocioeconomico>()
            .HasOne(i => i.Municipio)
            .WithMany(m => m.IndicadoresSocioeconomicos)
            .HasForeignKey(i => i.IdMunicipio)
            .OnDelete(DeleteBehavior.Cascade);

        // Municipio -> ServiciosBasicos
        modelBuilder.Entity<ServicioBasico>()
            .HasOne(s => s.Municipio)
            .WithMany(m => m.ServiciosBasicos)
            .HasForeignKey(s => s.IdMunicipio)
            .OnDelete(DeleteBehavior.Cascade);

        // Centro -> EstadisticasAbandono
        modelBuilder.Entity<EstadisticaAbandono>()
            .HasOne(e => e.Centro)
            .WithMany()
            .HasForeignKey(e => e.IdCentro)
            .OnDelete(DeleteBehavior.Cascade);

        // Periodo -> EstadisticasAbandono
        modelBuilder.Entity<EstadisticaAbandono>()
            .HasOne(e => e.Periodo)
            .WithMany()
            .HasForeignKey(e => e.IdPeriodo)
            .OnDelete(DeleteBehavior.Cascade);

        // Centro -> RecursosEducativos
        modelBuilder.Entity<RecursoEducativo>()
            .HasOne(r => r.Centro)
            .WithMany()
            .HasForeignKey(r => r.IdCentro)
            .OnDelete(DeleteBehavior.Cascade);

        // Periodo -> RecursosEducativos
        modelBuilder.Entity<RecursoEducativo>()
            .HasOne(r => r.Periodo)
            .WithMany()
            .HasForeignKey(r => r.IdPeriodo)
            .OnDelete(DeleteBehavior.Cascade);

        // Municipio -> FactoresRiesgoSocial
        modelBuilder.Entity<FactorRiesgoSocial>()
            .HasOne(f => f.Municipio)
            .WithMany()
            .HasForeignKey(f => f.IdMunicipio)
            .OnDelete(DeleteBehavior.Cascade);

        // Municipio -> DemocratizacionEducativa
        modelBuilder.Entity<DemocratizacionEducativa>()
            .HasOne(d => d.Municipio)
            .WithMany()
            .HasForeignKey(d => d.IdMunicipio)
            .OnDelete(DeleteBehavior.Cascade);

        // Municipio -> AbandonoCausanteZona
        modelBuilder.Entity<AbandonoCausanteZona>()
            .HasOne(a => a.Municipio)
            .WithMany()
            .HasForeignKey(a => a.IdMunicipio)
            .OnDelete(DeleteBehavior.Cascade);

        // CausanteAbandono -> AbandonoCausanteZona
        modelBuilder.Entity<AbandonoCausanteZona>()
            .HasOne(a => a.Causante)
            .WithMany()
            .HasForeignKey(a => a.IdCausante)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private void ConfigureEstadisticaAbandonoColumns(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EstadisticaAbandono>()
            .Property(e => e.IdEstadistica).HasColumnName("id_estadistica");
        modelBuilder.Entity<EstadisticaAbandono>()
            .Property(e => e.IdCentro).HasColumnName("id_centro");
        modelBuilder.Entity<EstadisticaAbandono>()
            .Property(e => e.IdPeriodo).HasColumnName("id_periodo");
        modelBuilder.Entity<EstadisticaAbandono>()
            .Property(e => e.NivelEducativo).HasColumnName("nivel_educativo");
        modelBuilder.Entity<EstadisticaAbandono>()
            .Property(e => e.TasaDesercionHistorica).HasColumnName("tasa_desercion_historica");
        modelBuilder.Entity<EstadisticaAbandono>()
            .Property(e => e.TasaAbandonoInteranual).HasColumnName("tasa_abandono_interanual");
        modelBuilder.Entity<EstadisticaAbandono>()
            .Property(e => e.TotalAbandonos).HasColumnName("total_abandonos");
        modelBuilder.Entity<EstadisticaAbandono>()
            .Property(e => e.TotalMatriculados).HasColumnName("total_matriculados");
        modelBuilder.Entity<EstadisticaAbandono>()
            .Property(e => e.CreatedAt).HasColumnName("created_at");
        modelBuilder.Entity<EstadisticaAbandono>()
            .Property(e => e.UpdatedAt).HasColumnName("updated_at");
    }

    private void ConfigureRecursoEducativoColumns(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RecursoEducativo>()
            .Property(r => r.IdRecurso).HasColumnName("id_recurso");
        modelBuilder.Entity<RecursoEducativo>()
            .Property(r => r.IdCentro).HasColumnName("id_centro");
        modelBuilder.Entity<RecursoEducativo>()
            .Property(r => r.IdPeriodo).HasColumnName("id_periodo");
        modelBuilder.Entity<RecursoEducativo>()
            .Property(r => r.TotalEstudiantes).HasColumnName("total_estudiantes");
        modelBuilder.Entity<RecursoEducativo>()
            .Property(r => r.TotalDocentes).HasColumnName("total_docentes");
        modelBuilder.Entity<RecursoEducativo>()
            .Property(r => r.RatioEstudianteDocente).HasColumnName("ratio_estudiante_docente");
        modelBuilder.Entity<RecursoEducativo>()
            .Property(r => r.DisponibilidadMaterialesDidacticos).HasColumnName("disponibilidad_materiales_didacticos");
        modelBuilder.Entity<RecursoEducativo>()
            .Property(r => r.TieneBiblioteca).HasColumnName("tiene_biblioteca");
        modelBuilder.Entity<RecursoEducativo>()
            .Property(r => r.TieneLaboratorio).HasColumnName("tiene_laboratorio");
        modelBuilder.Entity<RecursoEducativo>()
            .Property(r => r.TieneTecnologiaEducativa).HasColumnName("tiene_tecnologia_educativa");
        modelBuilder.Entity<RecursoEducativo>()
            .Property(r => r.ProgramaAlimentacionEscolar).HasColumnName("programa_alimentacion_escolar");
        modelBuilder.Entity<RecursoEducativo>()
            .Property(r => r.ProgramaTransporteEscolar).HasColumnName("programa_transporte_escolar");
        modelBuilder.Entity<RecursoEducativo>()
            .Property(r => r.EstadoInfraestructura).HasColumnName("estado_infraestructura");
        modelBuilder.Entity<RecursoEducativo>()
            .Property(r => r.CreatedAt).HasColumnName("created_at");
        modelBuilder.Entity<RecursoEducativo>()
            .Property(r => r.UpdatedAt).HasColumnName("updated_at");
    }

    private void ConfigureFactorRiesgoSocialColumns(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FactorRiesgoSocial>()
            .Property(f => f.IdRiesgoSocial).HasColumnName("id_riesgo_social");
        modelBuilder.Entity<FactorRiesgoSocial>()
            .Property(f => f.IdMunicipio).HasColumnName("id_municipio");
        modelBuilder.Entity<FactorRiesgoSocial>()
            .Property(f => f.Anio).HasColumnName("anio");
        modelBuilder.Entity<FactorRiesgoSocial>()
            .Property(f => f.TasaEmbarazoAdolescente).HasColumnName("tasa_embarazo_adolescente");
        modelBuilder.Entity<FactorRiesgoSocial>()
            .Property(f => f.IndiceDelincuenciaJuvenil).HasColumnName("indice_delincuencia_juvenil");
        modelBuilder.Entity<FactorRiesgoSocial>()
            .Property(f => f.CasosViolenciaIntrafamiliar).HasColumnName("casos_violencia_intrafamiliar");
        modelBuilder.Entity<FactorRiesgoSocial>()
            .Property(f => f.TasaConsumoSustancias).HasColumnName("tasa_consumo_sustancias");
        modelBuilder.Entity<FactorRiesgoSocial>()
            .Property(f => f.IndiceDesintegracionFamiliar).HasColumnName("indice_desintegracion_familiar");
        modelBuilder.Entity<FactorRiesgoSocial>()
            .Property(f => f.FuenteDatos).HasColumnName("fuente_datos");
        modelBuilder.Entity<FactorRiesgoSocial>()
            .Property(f => f.CreatedAt).HasColumnName("created_at");
        modelBuilder.Entity<FactorRiesgoSocial>()
            .Property(f => f.UpdatedAt).HasColumnName("updated_at");
    }

    private void ConfigureDemocratizacionColumns(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DemocratizacionEducativa>()
            .Property(d => d.IdDemocratizacion).HasColumnName("id_democratizacion");
        modelBuilder.Entity<DemocratizacionEducativa>()
            .Property(d => d.IdMunicipio).HasColumnName("id_municipio");
        modelBuilder.Entity<DemocratizacionEducativa>()
            .Property(d => d.Anio).HasColumnName("anio");
        modelBuilder.Entity<DemocratizacionEducativa>()
            .Property(d => d.NivelEducativo).HasColumnName("nivel_educativo");
        modelBuilder.Entity<DemocratizacionEducativa>()
            .Property(d => d.TasaCoberturaNeta).HasColumnName("tasa_cobertura_neta");
        modelBuilder.Entity<DemocratizacionEducativa>()
            .Property(d => d.BrechaUrbanoRural).HasColumnName("brecha_urbano_rural");
        modelBuilder.Entity<DemocratizacionEducativa>()
            .Property(d => d.IndiceInequidadEducativa).HasColumnName("indice_inequidad_educativa");
        modelBuilder.Entity<DemocratizacionEducativa>()
            .Property(d => d.CentrosPorMilEstudiantes).HasColumnName("centros_por_mil_estudiantes");
        modelBuilder.Entity<DemocratizacionEducativa>()
            .Property(d => d.DistanciaPromedioCentroKm).HasColumnName("distancia_promedio_centro_km");
        modelBuilder.Entity<DemocratizacionEducativa>()
            .Property(d => d.FuenteDatos).HasColumnName("fuente_datos");
        modelBuilder.Entity<DemocratizacionEducativa>()
            .Property(d => d.CreatedAt).HasColumnName("created_at");
        modelBuilder.Entity<DemocratizacionEducativa>()
            .Property(d => d.UpdatedAt).HasColumnName("updated_at");
    }

    private void ConfigureCausanteAbandonoColumns(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CausanteAbandono>()
            .Property(c => c.IdCausante).HasColumnName("id_causante");
        modelBuilder.Entity<CausanteAbandono>()
            .Property(c => c.NombreCausante).HasColumnName("nombre_causante");
        modelBuilder.Entity<CausanteAbandono>()
            .Property(c => c.Categoria).HasColumnName("categoria");
        modelBuilder.Entity<CausanteAbandono>()
            .Property(c => c.Descripcion).HasColumnName("descripcion");
        modelBuilder.Entity<CausanteAbandono>()
            .Property(c => c.CreatedAt).HasColumnName("created_at");
    }

    private void ConfigureAbandonoCausanteZonaColumns(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AbandonoCausanteZona>()
            .Property(a => a.IdRegistro).HasColumnName("id_registro");
        modelBuilder.Entity<AbandonoCausanteZona>()
            .Property(a => a.IdMunicipio).HasColumnName("id_municipio");
        modelBuilder.Entity<AbandonoCausanteZona>()
            .Property(a => a.IdCausante).HasColumnName("id_causante");
        modelBuilder.Entity<AbandonoCausanteZona>()
            .Property(a => a.Anio).HasColumnName("anio");
        modelBuilder.Entity<AbandonoCausanteZona>()
            .Property(a => a.PesoCausal).HasColumnName("peso_causal");
        modelBuilder.Entity<AbandonoCausanteZona>()
            .Property(a => a.CasosReportados).HasColumnName("casos_reportados");
        modelBuilder.Entity<AbandonoCausanteZona>()
            .Property(a => a.CreatedAt).HasColumnName("created_at");
    }
}