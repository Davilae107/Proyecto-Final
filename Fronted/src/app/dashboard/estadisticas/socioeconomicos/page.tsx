"use client";

import { useEffect, useState, useMemo } from "react";
import {
  DollarSign,
  TrendingDown,
  Briefcase,
  Home,
  Loader2,
  RefreshCw,
  AlertTriangle,
  Search,
} from "lucide-react";
import {
  ChartCard,
  StatCard,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
  AreaChart,
  Area,
  RadarChart,
  PolarGrid,
  PolarAngleAxis,
  PolarRadiusAxis,
  Radar,
  COLORS,
} from "@/components/charts/ChartComponents";
import { toast } from "sonner";
import {
  estadisticasApi,
  type SocioeconomicoPorMunicipio,
} from "@/lib/api";

export default function SocioeconomicosPage() {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [datos, setDatos] = useState<SocioeconomicoPorMunicipio[]>([]);
  const [searchMunicipio, setSearchMunicipio] = useState("");

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    setLoading(true);
    setError(null);
    try {
      const res = await estadisticasApi.getSocioeconomicos();
      setDatos(res.data);
    } catch {
      setError(
        "No se pudieron cargar las estadísticas. Asegúrate de que el backend esté corriendo y que se haya ejecutado el pipeline ETL."
      );
      toast.error("Error cargando indicadores socioeconómicos");
    } finally {
      setLoading(false);
    }
  };

  /* ── Computed stats ── */
  const avgPobreza = useMemo(() => {
    if (datos.length === 0) return 0;
    return +(datos.reduce((s, d) => s + d.tasaPobreza, 0) / datos.length).toFixed(2);
  }, [datos]);

  const avgPobrezaExtrema = useMemo(() => {
    if (datos.length === 0) return 0;
    return +(datos.reduce((s, d) => s + d.tasaPobrezaExtrema, 0) / datos.length).toFixed(2);
  }, [datos]);

  const avgDesempleo = useMemo(() => {
    if (datos.length === 0) return 0;
    return +(datos.reduce((s, d) => s + d.tasaDesempleo, 0) / datos.length).toFixed(2);
  }, [datos]);

  const avgTrabajoInfantil = useMemo(() => {
    if (datos.length === 0) return 0;
    return +(datos.reduce((s, d) => s + d.trabajoInfantil, 0) / datos.length).toFixed(2);
  }, [datos]);

  const avgIngreso = useMemo(() => {
    if (datos.length === 0) return 0;
    return Math.round(datos.reduce((s, d) => s + d.ingresoPromedio, 0) / datos.length);
  }, [datos]);

  /* Top 10 municipios con mayor pobreza para gráficas */
  const top10Pobreza = useMemo(
    () => [...datos].sort((a, b) => b.tasaPobreza - a.tasaPobreza).slice(0, 10),
    [datos]
  );

  /* Radar: top 8 por informalidad */
  const radarData = useMemo(
    () =>
      [...datos]
        .sort((a, b) => b.informalidad - a.informalidad)
        .slice(0, 8)
        .map((d) => ({
          municipio: d.municipio.length > 15 ? d.municipio.slice(0, 15) + "…" : d.municipio,
          "Pobreza": d.tasaPobreza,
          "Desempleo": d.tasaDesempleo,
          "Informalidad": d.informalidad,
          "Trabajo Infantil": d.trabajoInfantil,
        })),
    [datos]
  );

  /* Datos para gráfica de área: pobreza vs pobreza extrema por municipio (top 15) */
  const areaData = useMemo(
    () =>
      [...datos]
        .sort((a, b) => b.tasaPobreza - a.tasaPobreza)
        .slice(0, 15)
        .map((d) => ({
          municipio: d.municipio.length > 12 ? d.municipio.slice(0, 12) + "…" : d.municipio,
          "Pobreza": d.tasaPobreza,
          "Pobreza Extrema": d.tasaPobrezaExtrema,
          "Desempleo": d.tasaDesempleo,
        })),
    [datos]
  );

  /* Filtro tabla */
  const municipiosFiltrados = useMemo(() => {
    const filtered = searchMunicipio
      ? datos.filter((d) =>
          d.municipio.toLowerCase().includes(searchMunicipio.toLowerCase())
        )
      : datos;
    return filtered.slice(0, 50);
  }, [datos, searchMunicipio]);

  /* ── Loading state ── */
  if (loading) {
    return (
      <div className="flex items-center justify-center h-96">
        <div className="text-center">
          <Loader2 className="w-10 h-10 text-blue-500 animate-spin mx-auto" />
          <p className="text-sm text-gray-500 mt-3">
            Cargando indicadores socioeconómicos...
          </p>
        </div>
      </div>
    );
  }

  /* ── Error state ── */
  if (error) {
    return (
      <div className="space-y-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">
            Indicadores Socioeconómicos
          </h1>
        </div>
        <div className="bg-amber-50 border border-amber-200 rounded-xl p-6 text-center">
          <AlertTriangle className="w-10 h-10 text-amber-500 mx-auto mb-3" />
          <p className="text-amber-800 font-medium">{error}</p>
          <button
            onClick={loadData}
            className="mt-4 px-4 py-2 bg-amber-100 text-amber-700 rounded-lg text-sm font-medium hover:bg-amber-200"
          >
            Reintentar
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">
            Indicadores Socioeconómicos
          </h1>
          <p className="text-gray-500 mt-1">
            Pobreza, empleo, trabajo infantil e ingreso por municipio
          </p>
        </div>
        <button
          onClick={loadData}
          className="flex items-center gap-2 px-4 py-2 bg-white border border-gray-200 rounded-lg text-sm font-medium text-gray-700 hover:bg-gray-50"
        >
          <RefreshCw className="w-4 h-4" />
          Actualizar
        </button>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-5 gap-4">
        <StatCard
          label="Tasa de Pobreza"
          value={`${avgPobreza}%`}
          change={`Promedio de ${datos.length} municipios`}
          icon={<Home className="w-5 h-5" />}
        />
        <StatCard
          label="Pobreza Extrema"
          value={`${avgPobrezaExtrema}%`}
          change="Promedio general"
          icon={<DollarSign className="w-5 h-5" />}
        />
        <StatCard
          label="Tasa de Desempleo"
          value={`${avgDesempleo}%`}
          change="Promedio general"
          icon={<Briefcase className="w-5 h-5" />}
        />
        <StatCard
          label="Trabajo Infantil"
          value={`${avgTrabajoInfantil}%`}
          change="Indicador promedio"
          icon={<TrendingDown className="w-5 h-5" />}
        />
        <StatCard
          label="Ingreso Promedio"
          value={`RD$${avgIngreso.toLocaleString("es-DO")}`}
          change="Mensual por municipio"
          icon={<DollarSign className="w-5 h-5" />}
        />
      </div>

      {/* Charts Row 1 */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <ChartCard
          title="Top 10 Municipios con Mayor Pobreza"
          description="Tasa de pobreza y desempleo"
        >
          {top10Pobreza.length > 0 ? (
            <ResponsiveContainer width="100%" height={380}>
              <BarChart data={top10Pobreza} layout="vertical">
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis type="number" domain={[0, 100]} />
                <YAxis
                  type="category"
                  dataKey="municipio"
                  width={130}
                  tick={{ fontSize: 11 }}
                />
                <Tooltip />
                <Legend />
                <Bar
                  dataKey="tasaPobreza"
                  name="Pobreza %"
                  fill="#ef4444"
                  radius={[0, 4, 4, 0]}
                />
                <Bar
                  dataKey="tasaDesempleo"
                  name="Desempleo %"
                  fill="#f59e0b"
                  radius={[0, 4, 4, 0]}
                />
              </BarChart>
            </ResponsiveContainer>
          ) : (
            <p className="text-gray-400 text-center py-10">Sin datos</p>
          )}
        </ChartCard>

        <ChartCard
          title="Pobreza y Desempleo por Municipio"
          description="Los 15 municipios con más pobreza"
        >
          {areaData.length > 0 ? (
            <ResponsiveContainer width="100%" height={380}>
              <AreaChart data={areaData}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="municipio" tick={{ fontSize: 10 }} />
                <YAxis />
                <Tooltip />
                <Legend />
                <Area
                  type="monotone"
                  dataKey="Pobreza"
                  stroke="#ef4444"
                  fill="#fecaca"
                  strokeWidth={2}
                />
                <Area
                  type="monotone"
                  dataKey="Pobreza Extrema"
                  stroke="#7c3aed"
                  fill="#ddd6fe"
                  strokeWidth={2}
                />
                <Area
                  type="monotone"
                  dataKey="Desempleo"
                  stroke="#f59e0b"
                  fill="#fef3c7"
                  strokeWidth={2}
                />
              </AreaChart>
            </ResponsiveContainer>
          ) : (
            <p className="text-gray-400 text-center py-10">Sin datos</p>
          )}
        </ChartCard>
      </div>

      {/* Charts Row 2 */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <ChartCard
          title="Perfil Socioeconómico (Radar)"
          description="Municipios con mayor informalidad laboral"
        >
          {radarData.length > 0 ? (
            <ResponsiveContainer width="100%" height={380}>
              <RadarChart data={radarData}>
                <PolarGrid />
                <PolarAngleAxis dataKey="municipio" tick={{ fontSize: 10 }} />
                <PolarRadiusAxis angle={30} domain={[0, 100]} />
                <Radar
                  name="Pobreza"
                  dataKey="Pobreza"
                  stroke="#ef4444"
                  fill="#ef4444"
                  fillOpacity={0.2}
                />
                <Radar
                  name="Informalidad"
                  dataKey="Informalidad"
                  stroke="#8b5cf6"
                  fill="#8b5cf6"
                  fillOpacity={0.2}
                />
                <Tooltip />
                <Legend />
              </RadarChart>
            </ResponsiveContainer>
          ) : (
            <p className="text-gray-400 text-center py-10">Sin datos</p>
          )}
        </ChartCard>

        <ChartCard
          title="Ingreso Promedio por Municipio"
          description="Top 10 municipios con mayor ingreso"
        >
          {datos.length > 0 ? (
            <ResponsiveContainer width="100%" height={380}>
              <BarChart
                data={[...datos]
                  .sort((a, b) => b.ingresoPromedio - a.ingresoPromedio)
                  .slice(0, 10)}
                layout="vertical"
              >
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis type="number" />
                <YAxis
                  type="category"
                  dataKey="municipio"
                  width={130}
                  tick={{ fontSize: 11 }}
                />
                <Tooltip
                  formatter={(v) =>
                    `RD$${Number(v).toLocaleString("es-DO")}`
                  }
                />
                <Legend />
                <Bar
                  dataKey="ingresoPromedio"
                  name="Ingreso Promedio (RD$)"
                  fill="#10b981"
                  radius={[0, 4, 4, 0]}
                />
              </BarChart>
            </ResponsiveContainer>
          ) : (
            <p className="text-gray-400 text-center py-10">Sin datos</p>
          )}
        </ChartCard>
      </div>

      {/* Data Table */}
      <ChartCard
        title="Indicadores Socioeconómicos por Municipio"
        description={`Mostrando ${municipiosFiltrados.length} de ${datos.length} municipios`}
      >
        <div className="mb-4 relative">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
          <input
            type="text"
            placeholder="Buscar municipio..."
            value={searchMunicipio}
            onChange={(e) => setSearchMunicipio(e.target.value)}
            className="w-full pl-10 pr-4 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-gray-200 text-left text-gray-500">
                <th className="pb-3 pr-4 font-medium">Municipio</th>
                <th className="pb-3 pr-4 font-medium text-right">Ingreso Prom.</th>
                <th className="pb-3 pr-4 font-medium text-right">Pobreza %</th>
                <th className="pb-3 pr-4 font-medium text-right">Pobreza Ext. %</th>
                <th className="pb-3 pr-4 font-medium text-right">Trabajo Inf. %</th>
                <th className="pb-3 pr-4 font-medium text-right">Desempleo %</th>
                <th className="pb-3 font-medium text-right">Informalidad %</th>
              </tr>
            </thead>
            <tbody>
              {municipiosFiltrados.map((d, i) => (
                <tr
                  key={i}
                  className="border-b border-gray-100 hover:bg-gray-50"
                >
                  <td className="py-2.5 pr-4 font-medium max-w-[200px] truncate">
                    {d.municipio}
                  </td>
                  <td className="py-2.5 pr-4 text-right">
                    RD${d.ingresoPromedio.toLocaleString("es-DO")}
                  </td>
                  <td className="py-2.5 pr-4 text-right">
                    <span
                      className={
                        d.tasaPobreza >= 40
                          ? "text-red-600"
                          : d.tasaPobreza >= 25
                          ? "text-amber-600"
                          : "text-green-600"
                      }
                    >
                      {d.tasaPobreza}%
                    </span>
                  </td>
                  <td className="py-2.5 pr-4 text-right">{d.tasaPobrezaExtrema}%</td>
                  <td className="py-2.5 pr-4 text-right">{d.trabajoInfantil}%</td>
                  <td className="py-2.5 pr-4 text-right">{d.tasaDesempleo}%</td>
                  <td className="py-2.5 text-right">{d.informalidad}%</td>
                </tr>
              ))}
              {municipiosFiltrados.length === 0 && (
                <tr>
                  <td colSpan={7} className="py-8 text-center text-gray-400">
                    No se encontraron municipios
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </ChartCard>
    </div>
  );
}
