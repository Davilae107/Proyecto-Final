"use client";

import { useEffect, useState, useMemo } from "react";
import {
  GraduationCap,
  TrendingUp,
  TrendingDown,
  Users,
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
  PieChart,
  Pie,
  Cell,
  COLORS,
  RadarChart,
  PolarGrid,
  PolarAngleAxis,
  PolarRadiusAxis,
  Radar,
} from "@/components/charts/ChartComponents";
import { toast } from "sonner";
import {
  estadisticasApi,
  type RendimientoPorNivel,
  type RendimientoPorCentro,
} from "@/lib/api";

export default function RendimientoPage() {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [porNivel, setPorNivel] = useState<RendimientoPorNivel[]>([]);
  const [porCentro, setPorCentro] = useState<RendimientoPorCentro[]>([]);
  const [searchCentro, setSearchCentro] = useState("");

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    setLoading(true);
    setError(null);
    try {
      const [nivelRes, centroRes] = await Promise.all([
        estadisticasApi.getRendimientoPorNivel(),
        estadisticasApi.getRendimientoPorCentro(),
      ]);
      setPorNivel(nivelRes.data);
      setPorCentro(centroRes.data);
    } catch {
      setError(
        "No se pudieron cargar las estadísticas. Asegúrate de que el backend esté corriendo y que se haya ejecutado el pipeline ETL."
      );
      toast.error("Error cargando estadísticas de rendimiento");
    } finally {
      setLoading(false);
    }
  };

  /* ── Computed values ── */
  const totalEstudiantes = useMemo(
    () => porNivel.reduce((s, n) => s + n.totalEstudiantes, 0),
    [porNivel]
  );
  const totalAprobados = useMemo(
    () => porNivel.reduce((s, n) => s + n.estudiantesAprobados, 0),
    [porNivel]
  );
  const totalReprobados = useMemo(
    () => porNivel.reduce((s, n) => s + n.estudiantesReprobados, 0),
    [porNivel]
  );
  const promedioGeneral = useMemo(() => {
    if (porNivel.length === 0) return 0;
    return +(
      porNivel.reduce((s, n) => s + n.promedioCalificaciones, 0) /
      porNivel.length
    ).toFixed(2);
  }, [porNivel]);
  const tasaAprobacionGeneral = useMemo(() => {
    if (totalEstudiantes === 0) return 0;
    return +((totalAprobados / totalEstudiantes) * 100).toFixed(2);
  }, [totalAprobados, totalEstudiantes]);
  const tasaRepitenciaGeneral = useMemo(() => {
    if (porNivel.length === 0) return 0;
    return +(
      porNivel.reduce((s, n) => s + n.tasaRepitencia, 0) / porNivel.length
    ).toFixed(2);
  }, [porNivel]);

  /* Distribución para PieChart */
  const distribucionData = useMemo(
    () => [
      { name: "Aprobados", value: totalAprobados },
      { name: "Reprobados", value: totalReprobados },
    ],
    [totalAprobados, totalReprobados]
  );

  /* Radar por nivel */
  const radarData = useMemo(
    () =>
      porNivel.map((n) => ({
        nivel: n.nivel,
        "Promedio Calif.": n.promedioCalificaciones,
        "Tasa Aprobación": n.tasaAprobacion,
        "Tasa Repitencia": n.tasaRepitencia,
      })),
    [porNivel]
  );

  /* Centros filtrados */
  const centrosFiltrados = useMemo(() => {
    const filtered = searchCentro
      ? porCentro.filter((c) =>
          c.centro.toLowerCase().includes(searchCentro.toLowerCase())
        )
      : porCentro;
    return filtered.slice(0, 50);
  }, [porCentro, searchCentro]);

  /* ── Loading state ── */
  if (loading) {
    return (
      <div className="flex items-center justify-center h-96">
        <div className="text-center">
          <Loader2 className="w-10 h-10 text-blue-500 animate-spin mx-auto" />
          <p className="text-sm text-gray-500 mt-3">
            Cargando estadísticas de rendimiento...
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
            Rendimiento Académico
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
            Rendimiento Académico
          </h1>
          <p className="text-gray-500 mt-1">
            Indicadores de desempeño académico por nivel educativo y centro
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

      {/* Summary Stats */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard
          label="Tasa de Aprobación"
          value={`${tasaAprobacionGeneral}%`}
          change={`${totalAprobados.toLocaleString("es-DO")} aprobados`}
          positive
          icon={<TrendingUp className="w-5 h-5" />}
        />
        <StatCard
          label="Tasa de Repitencia"
          value={`${tasaRepitenciaGeneral}%`}
          change={`Promedio entre ${porNivel.length} niveles`}
          icon={<TrendingDown className="w-5 h-5" />}
        />
        <StatCard
          label="Promedio General"
          value={promedioGeneral}
          change="Promedio de calificaciones"
          positive
          icon={<GraduationCap className="w-5 h-5" />}
        />
        <StatCard
          label="Total Estudiantes"
          value={totalEstudiantes.toLocaleString("es-DO")}
          change={`${porCentro.length} centros evaluados`}
          positive
          icon={<Users className="w-5 h-5" />}
        />
      </div>

      {/* Charts Row 1 */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <ChartCard
          title="Rendimiento por Nivel Educativo"
          description="Tasas de aprobación y repitencia por nivel"
        >
          {porNivel.length > 0 ? (
            <ResponsiveContainer width="100%" height={320}>
              <BarChart data={porNivel}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="nivel" tick={{ fontSize: 12 }} />
                <YAxis />
                <Tooltip />
                <Legend />
                <Bar
                  dataKey="tasaAprobacion"
                  name="Aprobación %"
                  fill="#3b82f6"
                  radius={[4, 4, 0, 0]}
                />
                <Bar
                  dataKey="tasaRepitencia"
                  name="Repitencia %"
                  fill="#f59e0b"
                  radius={[4, 4, 0, 0]}
                />
              </BarChart>
            </ResponsiveContainer>
          ) : (
            <p className="text-gray-400 text-center py-10">Sin datos</p>
          )}
        </ChartCard>

        <ChartCard
          title="Distribución Aprobados vs Reprobados"
          description="Proporción global de estado académico"
        >
          {distribucionData.some((d) => d.value > 0) ? (
            <ResponsiveContainer width="100%" height={320}>
              <PieChart>
                <Pie
                  data={distribucionData}
                  cx="50%"
                  cy="50%"
                  innerRadius={70}
                  outerRadius={120}
                  paddingAngle={3}
                  dataKey="value"
                  label={({ name, value }) =>
                    `${name}: ${value.toLocaleString("es-DO")}`
                  }
                >
                  {distribucionData.map((_, index) => (
                    <Cell
                      key={`cell-${index}`}
                      fill={COLORS[index % COLORS.length]}
                    />
                  ))}
                </Pie>
                <Tooltip formatter={(v) => Number(v).toLocaleString("es-DO")} />
              </PieChart>
            </ResponsiveContainer>
          ) : (
            <p className="text-gray-400 text-center py-10">Sin datos</p>
          )}
        </ChartCard>
      </div>

      {/* Charts Row 2 */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <ChartCard
          title="Promedio de Calificaciones por Nivel"
          description="Calificación promedio de cada nivel educativo"
        >
          {porNivel.length > 0 ? (
            <ResponsiveContainer width="100%" height={320}>
              <BarChart data={porNivel} layout="vertical">
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis type="number" domain={[0, 100]} />
                <YAxis
                  type="category"
                  dataKey="nivel"
                  width={130}
                  tick={{ fontSize: 11 }}
                />
                <Tooltip />
                <Legend />
                <Bar
                  dataKey="promedioCalificaciones"
                  name="Promedio"
                  fill="#10b981"
                  radius={[0, 4, 4, 0]}
                />
              </BarChart>
            </ResponsiveContainer>
          ) : (
            <p className="text-gray-400 text-center py-10">Sin datos</p>
          )}
        </ChartCard>

        <ChartCard
          title="Indicadores por Nivel (Radar)"
          description="Visión integral del rendimiento por nivel educativo"
        >
          {radarData.length > 0 ? (
            <ResponsiveContainer width="100%" height={320}>
              <RadarChart data={radarData}>
                <PolarGrid />
                <PolarAngleAxis dataKey="nivel" tick={{ fontSize: 11 }} />
                <PolarRadiusAxis angle={30} domain={[0, 100]} />
                <Radar
                  name="Promedio Calif."
                  dataKey="Promedio Calif."
                  stroke="#3b82f6"
                  fill="#3b82f6"
                  fillOpacity={0.3}
                />
                <Radar
                  name="Tasa Aprobación"
                  dataKey="Tasa Aprobación"
                  stroke="#10b981"
                  fill="#10b981"
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
      </div>

      {/* Data Table – Rendimiento por Centro */}
      <ChartCard
        title="Rendimiento por Centro Educativo"
        description={`Mostrando ${centrosFiltrados.length} de ${porCentro.length} centros`}
      >
        <div className="mb-4 relative">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
          <input
            type="text"
            placeholder="Buscar centro educativo..."
            value={searchCentro}
            onChange={(e) => setSearchCentro(e.target.value)}
            className="w-full pl-10 pr-4 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-gray-200 text-left text-gray-500">
                <th className="pb-3 pr-4 font-medium">Centro</th>
                <th className="pb-3 pr-4 font-medium">Sector</th>
                <th className="pb-3 pr-4 font-medium">Zona</th>
                <th className="pb-3 pr-4 font-medium">Nivel</th>
                <th className="pb-3 pr-4 font-medium text-right">Promedio</th>
                <th className="pb-3 pr-4 font-medium text-right">
                  Aprobación %
                </th>
                <th className="pb-3 pr-4 font-medium text-right">
                  Estudiantes
                </th>
                <th className="pb-3 pr-4 font-medium text-right">Aprobados</th>
                <th className="pb-3 font-medium text-right">Reprobados</th>
              </tr>
            </thead>
            <tbody>
              {centrosFiltrados.map((c, i) => (
                <tr
                  key={i}
                  className="border-b border-gray-100 hover:bg-gray-50"
                >
                  <td className="py-2.5 pr-4 font-medium max-w-[220px] truncate">
                    {c.centro}
                  </td>
                  <td className="py-2.5 pr-4">{c.sector}</td>
                  <td className="py-2.5 pr-4">{c.zona}</td>
                  <td className="py-2.5 pr-4">{c.nivel}</td>
                  <td className="py-2.5 pr-4 text-right">{c.promedio}</td>
                  <td className="py-2.5 pr-4 text-right">
                    <span
                      className={
                        c.tasaAprobacion >= 80
                          ? "text-green-600"
                          : c.tasaAprobacion >= 60
                          ? "text-amber-600"
                          : "text-red-600"
                      }
                    >
                      {c.tasaAprobacion}%
                    </span>
                  </td>
                  <td className="py-2.5 pr-4 text-right">
                    {c.totalEstudiantes.toLocaleString("es-DO")}
                  </td>
                  <td className="py-2.5 pr-4 text-right">
                    {c.aprobados.toLocaleString("es-DO")}
                  </td>
                  <td className="py-2.5 text-right">
                    {c.reprobados.toLocaleString("es-DO")}
                  </td>
                </tr>
              ))}
              {centrosFiltrados.length === 0 && (
                <tr>
                  <td colSpan={9} className="py-8 text-center text-gray-400">
                    No se encontraron centros
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
