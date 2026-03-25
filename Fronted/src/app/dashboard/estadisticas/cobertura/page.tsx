"use client";

import { useEffect, useState, useMemo } from "react";
import {
  MapPin,
  School,
  Users,
  Globe,
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
  AreaChart,
  Area,
  COLORS,
} from "@/components/charts/ChartComponents";
import { toast } from "sonner";
import {
  estadisticasApi,
  type CoberturaPorMunicipio,
} from "@/lib/api";

export default function CoberturaPage() {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [datos, setDatos] = useState<CoberturaPorMunicipio[]>([]);
  const [searchMunicipio, setSearchMunicipio] = useState("");

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    setLoading(true);
    setError(null);
    try {
      const res = await estadisticasApi.getCobertura();
      setDatos(res.data);
    } catch {
      setError(
        "No se pudieron cargar las estadísticas. Asegúrate de que el backend esté corriendo y que se haya ejecutado el pipeline ETL."
      );
      toast.error("Error cargando estadísticas de cobertura educativa");
    } finally {
      setLoading(false);
    }
  };

  /* ── Computed stats ── */
  const avgCobertura = useMemo(() => {
    if (datos.length === 0) return 0;
    return +(datos.reduce((s, d) => s + d.tasaCobertura, 0) / datos.length).toFixed(2);
  }, [datos]);

  const avgBrecha = useMemo(() => {
    if (datos.length === 0) return 0;
    return +(datos.reduce((s, d) => s + d.brechaUrbanoRural, 0) / datos.length).toFixed(2);
  }, [datos]);

  const avgInequidad = useMemo(() => {
    if (datos.length === 0) return 0;
    return +(datos.reduce((s, d) => s + d.inequidad, 0) / datos.length).toFixed(2);
  }, [datos]);

  const avgCentrosPorMil = useMemo(() => {
    if (datos.length === 0) return 0;
    return +(datos.reduce((s, d) => s + d.centrosPorMil, 0) / datos.length).toFixed(2);
  }, [datos]);

  const avgDistancia = useMemo(() => {
    if (datos.length === 0) return 0;
    return +(datos.reduce((s, d) => s + d.distanciaPromedio, 0) / datos.length).toFixed(2);
  }, [datos]);

  /* Niveles únicos */
  const niveles = useMemo(
    () => [...new Set(datos.map((d) => d.nivel))],
    [datos]
  );

  /* Cobertura promedio agrupada por nivel para PieChart */
  const coberturaPorNivel = useMemo(() => {
    const grouped = new Map<string, { sum: number; count: number }>();
    datos.forEach((d) => {
      const entry = grouped.get(d.nivel) || { sum: 0, count: 0 };
      entry.sum += d.tasaCobertura;
      entry.count += 1;
      grouped.set(d.nivel, entry);
    });
    return Array.from(grouped.entries()).map(([nivel, { sum, count }]) => ({
      name: nivel,
      value: +(sum / count).toFixed(2),
    }));
  }, [datos]);

  /* Top 12 municipios con menor cobertura para Area chart */
  const bottomCobertura = useMemo(
    () =>
      [...datos]
        .sort((a, b) => a.tasaCobertura - b.tasaCobertura)
        .slice(0, 12)
        .map((d) => ({
          municipio: d.municipio.length > 12 ? d.municipio.slice(0, 12) + "…" : d.municipio,
          "Cobertura": d.tasaCobertura,
          "Brecha U-R": d.brechaUrbanoRural,
          "Inequidad": d.inequidad,
        })),
    [datos]
  );

  /* Top 10 municipios con mayor brecha */
  const topBrecha = useMemo(
    () =>
      [...datos]
        .sort((a, b) => b.brechaUrbanoRural - a.brechaUrbanoRural)
        .slice(0, 10),
    [datos]
  );

  /* Filtro tabla */
  const municipiosFiltrados = useMemo(() => {
    const filtered = searchMunicipio
      ? datos.filter((d) =>
          d.municipio.toLowerCase().includes(searchMunicipio.toLowerCase()) ||
          d.nivel.toLowerCase().includes(searchMunicipio.toLowerCase())
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
            Cargando estadísticas de cobertura educativa...
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
            Cobertura Educativa
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
            Cobertura Educativa
          </h1>
          <p className="text-gray-500 mt-1">
            Acceso educativo, brechas y distribución territorial por municipio
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
          label="Cobertura Promedio"
          value={`${avgCobertura}%`}
          change={`${datos.length} registros`}
          positive={avgCobertura >= 70}
          icon={<School className="w-5 h-5" />}
        />
        <StatCard
          label="Brecha Urbano-Rural"
          value={`${avgBrecha}%`}
          change="Diferencia promedio"
          positive={avgBrecha <= 15}
          icon={<Globe className="w-5 h-5" />}
        />
        <StatCard
          label="Índice de Inequidad"
          value={`${avgInequidad}%`}
          change="Promedio general"
          icon={<Users className="w-5 h-5" />}
        />
        <StatCard
          label="Centros por 1,000 hab."
          value={avgCentrosPorMil}
          change="Densidad promedio"
          icon={<MapPin className="w-5 h-5" />}
        />
        <StatCard
          label="Distancia Promedio"
          value={`${avgDistancia} km`}
          change="Al centro más cercano"
          icon={<MapPin className="w-5 h-5" />}
        />
      </div>

      {/* Charts Row 1 */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <ChartCard
          title="Cobertura Promedio por Nivel Educativo"
          description={`${niveles.length} niveles identificados`}
        >
          {coberturaPorNivel.length > 0 ? (
            <ResponsiveContainer width="100%" height={320}>
              <PieChart>
                <Pie
                  data={coberturaPorNivel}
                  cx="50%"
                  cy="50%"
                  innerRadius={70}
                  outerRadius={120}
                  paddingAngle={3}
                  dataKey="value"
                  label={({ name, value }) => `${name}: ${value}%`}
                >
                  {coberturaPorNivel.map((_, index) => (
                    <Cell
                      key={`cell-${index}`}
                      fill={COLORS[index % COLORS.length]}
                    />
                  ))}
                </Pie>
                <Tooltip formatter={(v) => `${v}%`} />
              </PieChart>
            </ResponsiveContainer>
          ) : (
            <p className="text-gray-400 text-center py-10">Sin datos</p>
          )}
        </ChartCard>

        <ChartCard
          title="Top 10 Municipios con Mayor Brecha Urbano-Rural"
          description="Disparidad en acceso educativo"
        >
          {topBrecha.length > 0 ? (
            <ResponsiveContainer width="100%" height={320}>
              <BarChart data={topBrecha} layout="vertical">
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis type="number" />
                <YAxis
                  type="category"
                  dataKey="municipio"
                  width={130}
                  tick={{ fontSize: 11 }}
                />
                <Tooltip />
                <Legend />
                <Bar
                  dataKey="brechaUrbanoRural"
                  name="Brecha Urbano-Rural %"
                  fill="#ef4444"
                  radius={[0, 4, 4, 0]}
                />
                <Bar
                  dataKey="inequidad"
                  name="Inequidad %"
                  fill="#f59e0b"
                  radius={[0, 4, 4, 0]}
                />
              </BarChart>
            </ResponsiveContainer>
          ) : (
            <p className="text-gray-400 text-center py-10">Sin datos</p>
          )}
        </ChartCard>
      </div>

      {/* Charts Row 2 */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <ChartCard
          title="Municipios con Menor Cobertura"
          description="Los 12 municipios más rezagados"
        >
          {bottomCobertura.length > 0 ? (
            <ResponsiveContainer width="100%" height={350}>
              <AreaChart data={bottomCobertura}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="municipio" tick={{ fontSize: 10 }} />
                <YAxis />
                <Tooltip />
                <Legend />
                <Area
                  type="monotone"
                  dataKey="Cobertura"
                  stroke="#3b82f6"
                  fill="#dbeafe"
                  strokeWidth={2}
                />
                <Area
                  type="monotone"
                  dataKey="Brecha U-R"
                  stroke="#ef4444"
                  fill="#fecaca"
                  strokeWidth={2}
                />
                <Area
                  type="monotone"
                  dataKey="Inequidad"
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

        <ChartCard
          title="Densidad de Centros Educativos"
          description="Top 10 municipios con mayor densidad de centros por 1,000 hab."
        >
          {datos.length > 0 ? (
            <ResponsiveContainer width="100%" height={350}>
              <BarChart
                data={[...datos]
                  .sort((a, b) => b.centrosPorMil - a.centrosPorMil)
                  .slice(0, 10)
                  .map((d) => ({
                    municipio:
                      d.municipio.length > 14
                        ? d.municipio.slice(0, 14) + "…"
                        : d.municipio,
                    centrosPorMil: d.centrosPorMil,
                    distanciaPromedio: d.distanciaPromedio,
                  }))}
              >
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="municipio" tick={{ fontSize: 10 }} />
                <YAxis />
                <Tooltip />
                <Legend />
                <Bar
                  dataKey="centrosPorMil"
                  name="Centros/1000 hab."
                  fill="#10b981"
                  radius={[4, 4, 0, 0]}
                />
                <Bar
                  dataKey="distanciaPromedio"
                  name="Distancia Prom. (km)"
                  fill="#8b5cf6"
                  radius={[4, 4, 0, 0]}
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
        title="Cobertura Educativa por Municipio"
        description={`Mostrando ${municipiosFiltrados.length} de ${datos.length} registros`}
      >
        <div className="mb-4 relative">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
          <input
            type="text"
            placeholder="Buscar municipio o nivel..."
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
                <th className="pb-3 pr-4 font-medium">Nivel</th>
                <th className="pb-3 pr-4 font-medium text-right">Cobertura %</th>
                <th className="pb-3 pr-4 font-medium text-right">Brecha U-R %</th>
                <th className="pb-3 pr-4 font-medium text-right">Inequidad %</th>
                <th className="pb-3 pr-4 font-medium text-right">Centros/1000</th>
                <th className="pb-3 font-medium text-right">Dist. Prom. (km)</th>
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
                  <td className="py-2.5 pr-4">{d.nivel}</td>
                  <td className="py-2.5 pr-4 text-right">
                    <span
                      className={
                        d.tasaCobertura >= 80
                          ? "text-green-600"
                          : d.tasaCobertura >= 50
                          ? "text-amber-600"
                          : "text-red-600"
                      }
                    >
                      {d.tasaCobertura}%
                    </span>
                  </td>
                  <td className="py-2.5 pr-4 text-right">
                    <span
                      className={
                        d.brechaUrbanoRural <= 10
                          ? "text-green-600"
                          : d.brechaUrbanoRural <= 25
                          ? "text-amber-600"
                          : "text-red-600"
                      }
                    >
                      {d.brechaUrbanoRural}%
                    </span>
                  </td>
                  <td className="py-2.5 pr-4 text-right">{d.inequidad}%</td>
                  <td className="py-2.5 pr-4 text-right">{d.centrosPorMil}</td>
                  <td className="py-2.5 text-right">{d.distanciaPromedio}</td>
                </tr>
              ))}
              {municipiosFiltrados.length === 0 && (
                <tr>
                  <td colSpan={7} className="py-8 text-center text-gray-400">
                    No se encontraron registros
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
