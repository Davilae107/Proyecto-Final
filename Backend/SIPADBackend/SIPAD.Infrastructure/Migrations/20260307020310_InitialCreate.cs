using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SIPAD.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "causantes_abandono",
                columns: table => new
                {
                    id_causante = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre_causante = table.Column<string>(type: "text", nullable: false),
                    categoria = table.Column<string>(type: "text", nullable: true),
                    descripcion = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_causantes_abandono", x => x.id_causante);
                });

            migrationBuilder.CreateTable(
                name: "periodos_academicos",
                columns: table => new
                {
                    id_periodo = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    anio_escolar = table.Column<string>(type: "text", nullable: false),
                    periodo = table.Column<string>(type: "text", nullable: true),
                    fecha_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    fecha_fin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_periodos_academicos", x => x.id_periodo);
                });

            migrationBuilder.CreateTable(
                name: "provincias",
                columns: table => new
                {
                    id_provincia = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo_provincia = table.Column<string>(type: "text", nullable: false),
                    nombre_provincia = table.Column<string>(type: "text", nullable: false),
                    region_geografica = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_provincias", x => x.id_provincia);
                });

            migrationBuilder.CreateTable(
                name: "regionales_educativas",
                columns: table => new
                {
                    id_regional = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo_regional = table.Column<string>(type: "text", nullable: false),
                    nombre_regional = table.Column<string>(type: "text", nullable: false),
                    sede_regional = table.Column<string>(type: "text", nullable: true),
                    telefono = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_regionales_educativas", x => x.id_regional);
                });

            migrationBuilder.CreateTable(
                name: "municipios",
                columns: table => new
                {
                    id_municipio = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo_municipio = table.Column<string>(type: "text", nullable: false),
                    nombre_municipio = table.Column<string>(type: "text", nullable: false),
                    id_provincia = table.Column<int>(type: "integer", nullable: false),
                    poblacion_estimada = table.Column<int>(type: "integer", nullable: true),
                    area_km2 = table.Column<decimal>(type: "numeric", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_municipios", x => x.id_municipio);
                    table.ForeignKey(
                        name: "FK_municipios_provincias_id_provincia",
                        column: x => x.id_provincia,
                        principalTable: "provincias",
                        principalColumn: "id_provincia",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "abandono_causantes_zona",
                columns: table => new
                {
                    id_registro = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_municipio = table.Column<int>(type: "integer", nullable: false),
                    id_causante = table.Column<int>(type: "integer", nullable: false),
                    anio = table.Column<int>(type: "integer", nullable: false),
                    peso_causal = table.Column<decimal>(type: "numeric", nullable: true),
                    casos_reportados = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_abandono_causantes_zona", x => x.id_registro);
                    table.ForeignKey(
                        name: "FK_abandono_causantes_zona_causantes_abandono_id_causante",
                        column: x => x.id_causante,
                        principalTable: "causantes_abandono",
                        principalColumn: "id_causante",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_abandono_causantes_zona_municipios_id_municipio",
                        column: x => x.id_municipio,
                        principalTable: "municipios",
                        principalColumn: "id_municipio",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "democratizacion_educativa",
                columns: table => new
                {
                    id_democratizacion = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_municipio = table.Column<int>(type: "integer", nullable: false),
                    anio = table.Column<int>(type: "integer", nullable: false),
                    nivel_educativo = table.Column<string>(type: "text", nullable: true),
                    tasa_cobertura_neta = table.Column<decimal>(type: "numeric", nullable: true),
                    brecha_urbano_rural = table.Column<decimal>(type: "numeric", nullable: true),
                    indice_inequidad_educativa = table.Column<decimal>(type: "numeric", nullable: true),
                    centros_por_mil_estudiantes = table.Column<decimal>(type: "numeric", nullable: true),
                    distancia_promedio_centro_km = table.Column<decimal>(type: "numeric", nullable: true),
                    fuente_datos = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_democratizacion_educativa", x => x.id_democratizacion);
                    table.ForeignKey(
                        name: "FK_democratizacion_educativa_municipios_id_municipio",
                        column: x => x.id_municipio,
                        principalTable: "municipios",
                        principalColumn: "id_municipio",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "distritos_educativos",
                columns: table => new
                {
                    id_distrito = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo_distrito = table.Column<string>(type: "text", nullable: false),
                    nombre_distrito = table.Column<string>(type: "text", nullable: false),
                    id_regional = table.Column<int>(type: "integer", nullable: false),
                    id_provincia = table.Column<int>(type: "integer", nullable: true),
                    id_municipio = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProvinciaIdProvincia = table.Column<int>(type: "integer", nullable: true),
                    MunicipioIdMunicipio = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_distritos_educativos", x => x.id_distrito);
                    table.ForeignKey(
                        name: "FK_distritos_educativos_municipios_MunicipioIdMunicipio",
                        column: x => x.MunicipioIdMunicipio,
                        principalTable: "municipios",
                        principalColumn: "id_municipio");
                    table.ForeignKey(
                        name: "FK_distritos_educativos_provincias_ProvinciaIdProvincia",
                        column: x => x.ProvinciaIdProvincia,
                        principalTable: "provincias",
                        principalColumn: "id_provincia");
                    table.ForeignKey(
                        name: "FK_distritos_educativos_regionales_educativas_id_regional",
                        column: x => x.id_regional,
                        principalTable: "regionales_educativas",
                        principalColumn: "id_regional",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "factores_riesgo_social",
                columns: table => new
                {
                    id_riesgo_social = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_municipio = table.Column<int>(type: "integer", nullable: false),
                    anio = table.Column<int>(type: "integer", nullable: false),
                    tasa_embarazo_adolescente = table.Column<decimal>(type: "numeric", nullable: true),
                    indice_delincuencia_juvenil = table.Column<decimal>(type: "numeric", nullable: true),
                    casos_violencia_intrafamiliar = table.Column<int>(type: "integer", nullable: true),
                    tasa_consumo_sustancias = table.Column<decimal>(type: "numeric", nullable: true),
                    indice_desintegracion_familiar = table.Column<decimal>(type: "numeric", nullable: true),
                    fuente_datos = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_factores_riesgo_social", x => x.id_riesgo_social);
                    table.ForeignKey(
                        name: "FK_factores_riesgo_social_municipios_id_municipio",
                        column: x => x.id_municipio,
                        principalTable: "municipios",
                        principalColumn: "id_municipio",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "indicadores_socioeconomicos",
                columns: table => new
                {
                    id_indicador_socio = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_municipio = table.Column<int>(type: "integer", nullable: false),
                    anio = table.Column<int>(type: "integer", nullable: false),
                    ingreso_promedio_mensual = table.Column<decimal>(type: "numeric", nullable: true),
                    tasa_pobreza_monetaria = table.Column<decimal>(type: "numeric", nullable: true),
                    tasa_pobreza_extrema = table.Column<decimal>(type: "numeric", nullable: true),
                    indice_trabajo_infantil = table.Column<decimal>(type: "numeric", nullable: true),
                    tasa_desempleo = table.Column<decimal>(type: "numeric", nullable: true),
                    nivel_informalidad_laboral = table.Column<decimal>(type: "numeric", nullable: true),
                    fuente_datos = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_indicadores_socioeconomicos", x => x.id_indicador_socio);
                    table.ForeignKey(
                        name: "FK_indicadores_socioeconomicos_municipios_id_municipio",
                        column: x => x.id_municipio,
                        principalTable: "municipios",
                        principalColumn: "id_municipio",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "servicios_basicos",
                columns: table => new
                {
                    id_servicio = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_municipio = table.Column<int>(type: "integer", nullable: false),
                    anio = table.Column<int>(type: "integer", nullable: false),
                    cobertura_agua_potable = table.Column<decimal>(type: "numeric", nullable: true),
                    cobertura_energia_electrica = table.Column<decimal>(type: "numeric", nullable: true),
                    cobertura_saneamiento_basico = table.Column<decimal>(type: "numeric", nullable: true),
                    acceso_internet = table.Column<decimal>(type: "numeric", nullable: true),
                    fuente_datos = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_servicios_basicos", x => x.id_servicio);
                    table.ForeignKey(
                        name: "FK_servicios_basicos_municipios_id_municipio",
                        column: x => x.id_municipio,
                        principalTable: "municipios",
                        principalColumn: "id_municipio",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "centros_educativos",
                columns: table => new
                {
                    id_centro = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo_centro = table.Column<string>(type: "text", nullable: false),
                    nombre_centro = table.Column<string>(type: "text", nullable: false),
                    id_distrito = table.Column<int>(type: "integer", nullable: false),
                    id_municipio = table.Column<int>(type: "integer", nullable: true),
                    sector = table.Column<string>(type: "text", nullable: true),
                    nivel = table.Column<string>(type: "text", nullable: true),
                    zona = table.Column<string>(type: "text", nullable: true),
                    direccion = table.Column<string>(type: "text", nullable: true),
                    latitud = table.Column<decimal>(type: "numeric", nullable: true),
                    longitud = table.Column<decimal>(type: "numeric", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MunicipioIdMunicipio = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_centros_educativos", x => x.id_centro);
                    table.ForeignKey(
                        name: "FK_centros_educativos_distritos_educativos_id_distrito",
                        column: x => x.id_distrito,
                        principalTable: "distritos_educativos",
                        principalColumn: "id_distrito",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_centros_educativos_municipios_MunicipioIdMunicipio",
                        column: x => x.MunicipioIdMunicipio,
                        principalTable: "municipios",
                        principalColumn: "id_municipio");
                });

            migrationBuilder.CreateTable(
                name: "estadisticas_abandono",
                columns: table => new
                {
                    id_estadistica = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_centro = table.Column<int>(type: "integer", nullable: false),
                    id_periodo = table.Column<int>(type: "integer", nullable: false),
                    nivel_educativo = table.Column<string>(type: "text", nullable: true),
                    tasa_desercion_historica = table.Column<decimal>(type: "numeric", nullable: true),
                    tasa_abandono_interanual = table.Column<decimal>(type: "numeric", nullable: true),
                    total_abandonos = table.Column<int>(type: "integer", nullable: true),
                    total_matriculados = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_estadisticas_abandono", x => x.id_estadistica);
                    table.ForeignKey(
                        name: "FK_estadisticas_abandono_centros_educativos_id_centro",
                        column: x => x.id_centro,
                        principalTable: "centros_educativos",
                        principalColumn: "id_centro",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_estadisticas_abandono_periodos_academicos_id_periodo",
                        column: x => x.id_periodo,
                        principalTable: "periodos_academicos",
                        principalColumn: "id_periodo",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "recursos_educativos",
                columns: table => new
                {
                    id_recurso = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_centro = table.Column<int>(type: "integer", nullable: false),
                    id_periodo = table.Column<int>(type: "integer", nullable: false),
                    total_estudiantes = table.Column<int>(type: "integer", nullable: true),
                    total_docentes = table.Column<int>(type: "integer", nullable: true),
                    ratio_estudiante_docente = table.Column<decimal>(type: "numeric", nullable: true),
                    disponibilidad_materiales_didacticos = table.Column<string>(type: "text", nullable: true),
                    tiene_biblioteca = table.Column<bool>(type: "boolean", nullable: false),
                    tiene_laboratorio = table.Column<bool>(type: "boolean", nullable: false),
                    tiene_tecnologia_educativa = table.Column<bool>(type: "boolean", nullable: false),
                    programa_alimentacion_escolar = table.Column<bool>(type: "boolean", nullable: false),
                    programa_transporte_escolar = table.Column<bool>(type: "boolean", nullable: false),
                    estado_infraestructura = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recursos_educativos", x => x.id_recurso);
                    table.ForeignKey(
                        name: "FK_recursos_educativos_centros_educativos_id_centro",
                        column: x => x.id_centro,
                        principalTable: "centros_educativos",
                        principalColumn: "id_centro",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_recursos_educativos_periodos_academicos_id_periodo",
                        column: x => x.id_periodo,
                        principalTable: "periodos_academicos",
                        principalColumn: "id_periodo",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rendimiento_academico",
                columns: table => new
                {
                    id_rendimiento = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_centro = table.Column<int>(type: "integer", nullable: false),
                    id_periodo = table.Column<int>(type: "integer", nullable: false),
                    nivel_educativo = table.Column<string>(type: "text", nullable: true),
                    promedio_calificaciones = table.Column<decimal>(type: "numeric", nullable: true),
                    tasa_aprobacion = table.Column<decimal>(type: "numeric", nullable: true),
                    tasa_repitencia = table.Column<decimal>(type: "numeric", nullable: true),
                    tasa_sobreedad = table.Column<decimal>(type: "numeric", nullable: true),
                    total_estudiantes = table.Column<int>(type: "integer", nullable: true),
                    estudiantes_aprobados = table.Column<int>(type: "integer", nullable: true),
                    estudiantes_reprobados = table.Column<int>(type: "integer", nullable: true),
                    estudiantes_sobreedad = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rendimiento_academico", x => x.id_rendimiento);
                    table.ForeignKey(
                        name: "FK_rendimiento_academico_centros_educativos_id_centro",
                        column: x => x.id_centro,
                        principalTable: "centros_educativos",
                        principalColumn: "id_centro",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rendimiento_academico_periodos_academicos_id_periodo",
                        column: x => x.id_periodo,
                        principalTable: "periodos_academicos",
                        principalColumn: "id_periodo",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_abandono_causantes_zona_id_causante",
                table: "abandono_causantes_zona",
                column: "id_causante");

            migrationBuilder.CreateIndex(
                name: "IX_abandono_causantes_zona_id_municipio",
                table: "abandono_causantes_zona",
                column: "id_municipio");

            migrationBuilder.CreateIndex(
                name: "IX_centros_educativos_codigo_centro",
                table: "centros_educativos",
                column: "codigo_centro",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_centros_educativos_id_distrito",
                table: "centros_educativos",
                column: "id_distrito");

            migrationBuilder.CreateIndex(
                name: "IX_centros_educativos_MunicipioIdMunicipio",
                table: "centros_educativos",
                column: "MunicipioIdMunicipio");

            migrationBuilder.CreateIndex(
                name: "IX_democratizacion_educativa_id_municipio",
                table: "democratizacion_educativa",
                column: "id_municipio");

            migrationBuilder.CreateIndex(
                name: "IX_distritos_educativos_id_regional",
                table: "distritos_educativos",
                column: "id_regional");

            migrationBuilder.CreateIndex(
                name: "IX_distritos_educativos_MunicipioIdMunicipio",
                table: "distritos_educativos",
                column: "MunicipioIdMunicipio");

            migrationBuilder.CreateIndex(
                name: "IX_distritos_educativos_ProvinciaIdProvincia",
                table: "distritos_educativos",
                column: "ProvinciaIdProvincia");

            migrationBuilder.CreateIndex(
                name: "IX_estadisticas_abandono_id_centro",
                table: "estadisticas_abandono",
                column: "id_centro");

            migrationBuilder.CreateIndex(
                name: "IX_estadisticas_abandono_id_periodo",
                table: "estadisticas_abandono",
                column: "id_periodo");

            migrationBuilder.CreateIndex(
                name: "IX_factores_riesgo_social_id_municipio",
                table: "factores_riesgo_social",
                column: "id_municipio");

            migrationBuilder.CreateIndex(
                name: "IX_indicadores_socioeconomicos_id_municipio",
                table: "indicadores_socioeconomicos",
                column: "id_municipio");

            migrationBuilder.CreateIndex(
                name: "IX_municipios_codigo_municipio",
                table: "municipios",
                column: "codigo_municipio",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_municipios_id_provincia",
                table: "municipios",
                column: "id_provincia");

            migrationBuilder.CreateIndex(
                name: "IX_provincias_codigo_provincia",
                table: "provincias",
                column: "codigo_provincia",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recursos_educativos_id_centro",
                table: "recursos_educativos",
                column: "id_centro");

            migrationBuilder.CreateIndex(
                name: "IX_recursos_educativos_id_periodo",
                table: "recursos_educativos",
                column: "id_periodo");

            migrationBuilder.CreateIndex(
                name: "IX_rendimiento_academico_id_centro",
                table: "rendimiento_academico",
                column: "id_centro");

            migrationBuilder.CreateIndex(
                name: "IX_rendimiento_academico_id_periodo",
                table: "rendimiento_academico",
                column: "id_periodo");

            migrationBuilder.CreateIndex(
                name: "IX_servicios_basicos_id_municipio",
                table: "servicios_basicos",
                column: "id_municipio");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "abandono_causantes_zona");

            migrationBuilder.DropTable(
                name: "democratizacion_educativa");

            migrationBuilder.DropTable(
                name: "estadisticas_abandono");

            migrationBuilder.DropTable(
                name: "factores_riesgo_social");

            migrationBuilder.DropTable(
                name: "indicadores_socioeconomicos");

            migrationBuilder.DropTable(
                name: "recursos_educativos");

            migrationBuilder.DropTable(
                name: "rendimiento_academico");

            migrationBuilder.DropTable(
                name: "servicios_basicos");

            migrationBuilder.DropTable(
                name: "causantes_abandono");

            migrationBuilder.DropTable(
                name: "centros_educativos");

            migrationBuilder.DropTable(
                name: "periodos_academicos");

            migrationBuilder.DropTable(
                name: "distritos_educativos");

            migrationBuilder.DropTable(
                name: "municipios");

            migrationBuilder.DropTable(
                name: "regionales_educativas");

            migrationBuilder.DropTable(
                name: "provincias");
        }
    }
}
