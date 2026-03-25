"use client";

import { useEffect, useState } from "react";
import {
  AlertTriangle,
  TrendingDown,
  Users,
  MapPin,
  Loader2,
  RefreshCw,
  Building2,
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
} from "@/components/charts/ChartComponents";
import { toast } from "sonner";
import {
  estadisticasApi,
  type DesercionPorSector,
  type DesercionPorNivel,
  type DesercionPorZona,
  type CausanteAbandono,
  type DesercionPorCentro,
  type ResumenDesercion,
} from "@/lib/api";

export default function DesercionPage() {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [resumen, setResumen] = useState<ResumenDesercion | null>(null);
  const [porSector, setPorSector] = useState<DesercionPorSector[]>([]);
  const [porNivel, setPorNivel] = useState<DesercionPorNivel[]>([]);
  const [porZona, setPorZona] = useState<DesercionPorZona[]>([]);
  const [causantes, setCausantes] = useState<CausanteAbandono[]>([]);
  const [porCentro, setPorCentro] = useState<DesercionPorCentro[]>([]);

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    setLoading(true);
    setError(null);
    try {
      const [resRes, sectorRes, nivelRes, zonaRes, causantesRes, centroRes] =
        await Promise.all([
          estadisticasApi.getResumenDesercion(),
          estadisticasApi.getDesercionPorSector(),
          estadisticasApi.getDesercionPorNivel(),
          estadisticasApi.getDesercionPorZona(),
          estadisticasApi.getCausantesAbandono(),
          estadisticasApi.getDesercionPorCentro(),
        ]);

      setResumen(resRes.data);
      setPorSector(sectorRes.data);
      setPorNivel(nivelRes.data);
      setPorZona(zonaRes.data);
      setCausantes(causantesRes.data);
      setPorCentro(centroRes.data);
    } catch {
      setError(
        "No se pudieron cargar las estadísticas. Asegúrate de que el backend esté corriendo y que se haya ejecutado el pipeline ETL."
      );
      toast.error("Error cargando estadísticas de deserción");
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center h-96">
        <div className="text-center">
          <Loader2 className="w-10 h-10 text-blue-500 animate-spin mx-auto" />
          <p className="text-sm text-gray-500 mt-3">
            Cargando estadísticas de deserción...
          </p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="space-y-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">
            Deserción y Abandono Escolar
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

  // Preparar datos para gráficas de causantes (para PieChart)
  const causantesChartData = causantes.map((c) => ({
    name: c.causante,
    value: c.totalCasos,
  }));

  // Datos para gráfica de probabilidad por sector
  const sectorChartData = porSector.map((s) => ({
    sector: s.sector,
    "Tasa Abandono (%)": s.tasaAbandono,
    "Probabilidad": +(s.probabilidadAbandono * 100).toFixed(2),
    matriculados: s.totalMatriculados,
    abandonos: s.totalAbandonos,
  }));

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">
            Deserción y Abandono Escolar
          </h1>
          <p className="text-gray-500 mt-1">
            Probabilidad de abandono escolar por sector, nivel, zona y causas
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

      {/* Stats Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard
          label="Tasa General de Abandono"
          value={`${resumen?.tasaGeneralAbandono ?? 0}%`}
          change={`Probabilidad: ${((resumen?.probabilidadGeneral ?? 0) * 100).toFixed(2)}%`}
          icon={<TrendingDown className="w-5 h-5" />}
        />
        <StatCard
          label="Total Abandonos"
          value={(resumen?.totalAbandonos ?? 0).toLocaleString("es-DO")}
          change={`De ${(resumen?.totalMatriculados ?? 0).toLocaleString("es-DO")} matriculados`}
          icon={<AlertTriangle className="w-5 h-5" />}
        />
        <StatCard
          label="Centros Evaluados"
          value={resumen?.totalCentros ?? 0}
          icon={<Building2 className="w-5 h-5" />}
        />
        <StatCard
          label="Registros Procesados"
          value={resumen?.totalRegistros ?? 0}
          icon={<Users className="w-5 h-5" />}
        />
      </div>

      {/* ═══════════════════════════════════════════════ */}
      {/* PROBABILIDAD POR SECTOR - Gráfica principal */}
      {/* ═══════════════════════════════════════════════ */}
      <ChartCard
        title="Probabilidad de Abandono Escolar por Sector"
        description="Comparación de la tasa y probabilidad de deserción entre sectores educativos (Público vs Privado)"
      >
        {porSector.length > 0 ? (
          <>
            <ResponsiveContainer width="100%" height={350}>
              <BarChart data={sectorChartData} barGap={8}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="sector" tick={{ fontSize: 13, fontWeight: 600 }} />
                <YAxis
                  label={{
                    value: "Porcentaje (%)",
                    angle: -90,
                    position: "insideLeft",
                    style: { fontSize: 12 },
                  }}
                />
                <Tooltip
                  formatter={(value, name) => [
                    `${value}%`,
                    String(name),
                  ]}
                  contentStyle={{
                    borderRadius: "8px",
                    border: "1px solid #e5e7eb",
                  }}
                />
                <Legend />
                <Bar
                  dataKey="Tasa Abandono (%)"
                  fill="#ef4444"
                  radius={[6, 6, 0, 0]}
                  maxBarSize={80}
                />
                <Bar
                  dataKey="Probabilidad"
                  fill="#f59e0b"
                  radius={[6, 6, 0, 0]}
                  maxBarSize={80}
                />
              </BarChart>
            </ResponsiveContainer>

            {/* Tarjetas de detalle por sector */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-6">
              {porSector.map((s, idx) => {
                const probabilidadPct = (s.probabilidadAbandono * 100).toFixed(2);
                const riesgo =
                  s.tasaAbandono >= 5
                    ? { label: "Alto", color: "text-red-600 bg-red-50 border-red-200" }
                    : s.tasaAbandono >= 2
                    ? { label: "Medio", color: "text-amber-600 bg-amber-50 border-amber-200" }
                    : { label: "Bajo", color: "text-green-600 bg-green-50 border-green-200" };

                return (
                  <div
                    key={s.sector}
                    className={`rounded-xl border p-5 ${riesgo.color}`}
                  >
                    <div className="flex items-center justify-between mb-3">
                      <h4 className="font-bold text-lg">{s.sector}</h4>
                      <span
                        className={`px-2.5 py-1 rounded-full text-xs font-semibold ${riesgo.color}`}
                      >
                        Riesgo {riesgo.label}
                      </span>
                    </div>
                    <div className="grid grid-cols-2 gap-3 text-sm">
                      <div>
                        <p className="opacity-70">Matriculados</p>
                        <p className="font-bold text-xl">
                          {s.totalMatriculados.toLocaleString("es-DO")}
                        </p>
                      </div>
                      <div>
                        <p className="opacity-70">Abandonos</p>
                        <p className="font-bold text-xl">
                          {s.totalAbandonos.toLocaleString("es-DO")}
                        </p>
                      </div>
                      <div>
                        <p className="opacity-70">Tasa de Abandono</p>
                        <p className="font-bold text-xl">{s.tasaAbandono}%</p>
                      </div>
                      <div>
                        <p className="opacity-70">Probabilidad</p>
                        <p className="font-bold text-xl">{probabilidadPct}%</p>
                      </div>
                    </div>
                    <p className="text-xs mt-3 opacity-60">
                      {s.cantidadCentros} centro{s.cantidadCentros !== 1 ? "s" : ""} evaluado
                      {s.cantidadCentros !== 1 ? "s" : ""}
                    </p>
                  </div>
                );
              })}
            </div>
          </>
        ) : (
          <p className="text-center text-gray-500 py-8">
            No hay datos de deserción por sector. Ejecuta el pipeline ETL primero.
          </p>
        )}
      </ChartCard>

      {/* Charts Row: Nivel y Causantes */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Deserción por nivel */}
        <ChartCard
          title="Deserción por Nivel Educativo"
          description="Tasa de abandono según el nivel"
        >
          {porNivel.length > 0 ? (
            <ResponsiveContainer width="100%" height={320}>
              <BarChart
                data={porNivel.map((n) => ({
                  nivel: n.nivel,
                  "Tasa Abandono (%)": n.tasaAbandono,
                  abandonos: n.totalAbandonos,
                  matriculados: n.totalMatriculados,
                }))}
              >
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="nivel" tick={{ fontSize: 12 }} />
                <YAxis />
                <Tooltip />
                <Legend />
                <Bar
                  dataKey="Tasa Abandono (%)"
                  fill="#ef4444"
                  radius={[4, 4, 0, 0]}
                />
              </BarChart>
            </ResponsiveContainer>
          ) : (
            <p className="text-center text-gray-500 py-8">Sin datos</p>
          )}
        </ChartCard>

        {/* Causantes de abandono */}
        <ChartCard
          title="Principales Causas de Abandono"
          description="Distribución de casos reportados por causa"
        >
          {causantesChartData.length > 0 ? (
            <ResponsiveContainer width="100%" height={320}>
              <PieChart>
                <Pie
                  data={causantesChartData}
                  cx="50%"
                  cy="50%"
                  innerRadius={55}
                  outerRadius={110}
                  paddingAngle={3}
                  dataKey="value"
                  label={({ name, value }) => `${name}: ${value}`}
                >
                  {causantesChartData.map((_, index) => (
                    <Cell
                      key={`cell-${index}`}
                      fill={COLORS[index % COLORS.length]}
                    />
                  ))}
                </Pie>
                <Tooltip />
              </PieChart>
            </ResponsiveContainer>
          ) : (
            <p className="text-center text-gray-500 py-8">Sin datos</p>
          )}
        </ChartCard>
      </div>

      {/* Deserción por Zona */}
      <ChartCard
        title="Deserción por Zona Geográfica (Urbana / Rural)"
        description="Tasa de abandono y probabilidad por zona"
      >
        {porZona.length > 0 ? (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <ResponsiveContainer width="100%" height={280}>
              <BarChart
                data={porZona.map((z) => ({
                  zona: z.zona,
                  "Tasa Abandono (%)": z.tasaAbandono,
                  "Probabilidad (%)": +(z.probabilidadAbandono * 100).toFixed(2),
                }))}
              >
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="zona" />
                <YAxis />
                <Tooltip />
                <Legend />
                <Bar dataKey="Tasa Abandono (%)" fill="#8b5cf6" radius={[4, 4, 0, 0]} />
                <Bar dataKey="Probabilidad (%)" fill="#06b6d4" radius={[4, 4, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
            <div className="space-y-3">
              {porZona.map((z) => (
                <div
                  key={z.zona}
                  className="bg-gray-50 rounded-lg p-4 flex items-center justify-between"
                >
                  <div>
                    <p className="font-semibold text-gray-900">{z.zona}</p>
                    <p className="text-xs text-gray-500">
                      {z.totalMatriculados.toLocaleString("es-DO")} matriculados ·{" "}
                      {z.cantidadCentros} centro(s)
                    </p>
                  </div>
                  <div className="text-right">
                    <p className="text-lg font-bold text-red-600">
                      {z.tasaAbandono}%
                    </p>
                    <p className="text-xs text-gray-500">
                      {z.totalAbandonos} abandonos
                    </p>
                  </div>
                </div>
              ))}
            </div>
          </div>
        ) : (
          <p className="text-center text-gray-500 py-8">Sin datos</p>
        )}
      </ChartCard>

      {/* Tabla detallada por centro */}
      <ChartCard
        title="Detalle por Centro Educativo"
        description="Tasa de abandono y probabilidad para cada centro"
      >
        {porCentro.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead className="bg-gray-50 text-gray-600">
                <tr>
                  <th className="px-4 py-3 text-left font-medium">Centro</th>
                  <th className="px-4 py-3 text-left font-medium">Sector</th>
                  <th className="px-4 py-3 text-left font-medium">Zona</th>
                  <th className="px-4 py-3 text-left font-medium">Nivel</th>
                  <th className="px-4 py-3 text-right font-medium">Matriculados</th>
                  <th className="px-4 py-3 text-right font-medium">Abandonos</th>
                  <th className="px-4 py-3 text-right font-medium">Tasa (%)</th>
                  <th className="px-4 py-3 text-right font-medium">Probabilidad</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {porCentro.map((c, idx) => {
                  const prob =
                    c.matriculados > 0
                      ? ((c.abandonos / c.matriculados) * 100).toFixed(2)
                      : "0.00";
                  const riesgoColor =
                    c.tasaAbandono >= 5
                      ? "text-red-600 bg-red-50"
                      : c.tasaAbandono >= 2
                      ? "text-amber-600 bg-amber-50"
                      : "text-green-600 bg-green-50";
                  return (
                    <tr key={idx} className="hover:bg-gray-50">
                      <td className="px-4 py-3 font-medium text-gray-900 max-w-[200px] truncate">
                        {c.centro}
                      </td>
                      <td className="px-4 py-3">
                        <span
                          className={`px-2 py-0.5 rounded-full text-xs font-medium ${
                            c.sector === "Público"
                              ? "bg-blue-50 text-blue-700"
                              : "bg-purple-50 text-purple-700"
                          }`}
                        >
                          {c.sector}
                        </span>
                      </td>
                      <td className="px-4 py-3 text-gray-600">{c.zona}</td>
                      <td className="px-4 py-3 text-gray-600">{c.nivel}</td>
                      <td className="px-4 py-3 text-right">
                        {c.matriculados.toLocaleString("es-DO")}
                      </td>
                      <td className="px-4 py-3 text-right font-medium text-red-600">
                        {c.abandonos}
                      </td>
                      <td className="px-4 py-3 text-right">
                        <span
                          className={`px-2 py-0.5 rounded-full text-xs font-bold ${riesgoColor}`}
                        >
                          {c.tasaAbandono}%
                        </span>
                      </td>
                      <td className="px-4 py-3 text-right font-medium">
                        {prob}%
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        ) : (
          <p className="text-center text-gray-500 py-8">Sin datos</p>
        )}
      </ChartCard>
    </div>
  );
}
