"use client";

import { useEffect, useState, useMemo } from "react";
import {
  Droplets,
  Zap,
  Wifi,
  Shield,
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
  RadarChart,
  PolarGrid,
  PolarAngleAxis,
  PolarRadiusAxis,
  Radar,
  PieChart,
  Pie,
  Cell,
  COLORS,
} from "@/components/charts/ChartComponents";
import { toast } from "sonner";
import {
  estadisticasApi,
  type ServicioPorMunicipio,
} from "@/lib/api";

export default function ServiciosPage() {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [datos, setDatos] = useState<ServicioPorMunicipio[]>([]);
  const [searchMunicipio, setSearchMunicipio] = useState("");

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    setLoading(true);
    setError(null);
    try {
      const res = await estadisticasApi.getServicios();
      setDatos(res.data);
    } catch {
      setError(
        "No se pudieron cargar las estadísticas. Asegúrate de que el backend esté corriendo y que se haya ejecutado el pipeline ETL."
      );
      toast.error("Error cargando estadísticas de servicios básicos");
    } finally {
      setLoading(false);
    }
  };

  /* ── Computed stats ── */
  const avg = (fn: (d: ServicioPorMunicipio) => number) => {
    if (datos.length === 0) return 0;
    return +(datos.reduce((s, d) => s + fn(d), 0) / datos.length).toFixed(2);
  };

  const avgAgua = useMemo(() => avg((d) => d.aguaPotable), [datos]);
  const avgEnergia = useMemo(() => avg((d) => d.energiaElectrica), [datos]);
  const avgInternet = useMemo(() => avg((d) => d.internet), [datos]);
  const avgSaneamiento = useMemo(() => avg((d) => d.saneamiento), [datos]);

  /* Promedio general de todos los servicios combinados */
  const avgGeneral = useMemo(
    () => +((avgAgua + avgEnergia + avgInternet + avgSaneamiento) / 4).toFixed(2),
    [avgAgua, avgEnergia, avgInternet, avgSaneamiento]
  );

  /* Top 10 municipios con menor cobertura promedio */
  const bottom10 = useMemo(
    () =>
      [...datos]
        .map((d) => ({
          ...d,
          promedio: +((d.aguaPotable + d.energiaElectrica + d.saneamiento + d.internet) / 4).toFixed(2),
        }))
        .sort((a, b) => a.promedio - b.promedio)
        .slice(0, 10),
    [datos]
  );

  /* Radar: top 8 municipios con mejor cobertura */
  const radarData = useMemo(
    () =>
      [...datos]
        .map((d) => ({
          ...d,
          promedio: (d.aguaPotable + d.energiaElectrica + d.saneamiento + d.internet) / 4,
        }))
        .sort((a, b) => b.promedio - a.promedio)
        .slice(0, 6)
        .map((d) => ({
          municipio: d.municipio.length > 14 ? d.municipio.slice(0, 14) + "…" : d.municipio,
          "Agua": d.aguaPotable,
          "Energía": d.energiaElectrica,
          "Saneamiento": d.saneamiento,
          "Internet": d.internet,
        })),
    [datos]
  );

  /* Pie: distribución de cobertura promedio por servicio */
  const pieData = useMemo(
    () => [
      { name: "Agua Potable", value: avgAgua },
      { name: "Energía Eléctrica", value: avgEnergia },
      { name: "Saneamiento", value: avgSaneamiento },
      { name: "Internet", value: avgInternet },
    ],
    [avgAgua, avgEnergia, avgSaneamiento, avgInternet]
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
            Cargando estadísticas de servicios básicos...
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
            Servicios Básicos
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
            Servicios Básicos
          </h1>
          <p className="text-gray-500 mt-1">
            Cobertura de agua, energía, saneamiento e internet por municipio
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
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard
          label="Agua Potable"
          value={`${avgAgua}%`}
          change={`Promedio de ${datos.length} municipios`}
          positive={avgAgua >= 70}
          icon={<Droplets className="w-5 h-5" />}
        />
        <StatCard
          label="Energía Eléctrica"
          value={`${avgEnergia}%`}
          change="Cobertura promedio"
          positive={avgEnergia >= 70}
          icon={<Zap className="w-5 h-5" />}
        />
        <StatCard
          label="Acceso a Internet"
          value={`${avgInternet}%`}
          change="Cobertura promedio"
          positive={avgInternet >= 50}
          icon={<Wifi className="w-5 h-5" />}
        />
        <StatCard
          label="Saneamiento"
          value={`${avgSaneamiento}%`}
          change="Cobertura promedio"
          positive={avgSaneamiento >= 60}
          icon={<Shield className="w-5 h-5" />}
        />
      </div>

      {/* Charts Row 1 */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <ChartCard
          title="Municipios con Menor Cobertura"
          description="Los 10 municipios más rezagados en servicios"
        >
          {bottom10.length > 0 ? (
            <ResponsiveContainer width="100%" height={380}>
              <BarChart data={bottom10} layout="vertical">
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
                  dataKey="aguaPotable"
                  name="Agua %"
                  fill="#3b82f6"
                  radius={[0, 2, 2, 0]}
                />
                <Bar
                  dataKey="energiaElectrica"
                  name="Energía %"
                  fill="#f59e0b"
                  radius={[0, 2, 2, 0]}
                />
                <Bar
                  dataKey="internet"
                  name="Internet %"
                  fill="#8b5cf6"
                  radius={[0, 2, 2, 0]}
                />
              </BarChart>
            </ResponsiveContainer>
          ) : (
            <p className="text-gray-400 text-center py-10">Sin datos</p>
          )}
        </ChartCard>

        <ChartCard
          title="Perfil de Servicios por Municipio (Radar)"
          description="Municipios con mejor cobertura general"
        >
          {radarData.length > 0 ? (
            <ResponsiveContainer width="100%" height={380}>
              <RadarChart data={radarData}>
                <PolarGrid />
                <PolarAngleAxis dataKey="municipio" tick={{ fontSize: 10 }} />
                <PolarRadiusAxis angle={30} domain={[0, 100]} />
                <Radar
                  name="Agua"
                  dataKey="Agua"
                  stroke="#3b82f6"
                  fill="#3b82f6"
                  fillOpacity={0.2}
                />
                <Radar
                  name="Energía"
                  dataKey="Energía"
                  stroke="#f59e0b"
                  fill="#f59e0b"
                  fillOpacity={0.2}
                />
                <Radar
                  name="Internet"
                  dataKey="Internet"
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
      </div>

      {/* Charts Row 2 */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <ChartCard
          title="Cobertura Promedio por Tipo de Servicio"
          description={`Índice general: ${avgGeneral}%`}
        >
          {pieData.some((d) => d.value > 0) ? (
            <ResponsiveContainer width="100%" height={320}>
              <PieChart>
                <Pie
                  data={pieData}
                  cx="50%"
                  cy="50%"
                  innerRadius={70}
                  outerRadius={120}
                  paddingAngle={3}
                  dataKey="value"
                  label={({ name, value }) => `${name}: ${value}%`}
                >
                  {pieData.map((_, index) => (
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
          title="Top 10 Municipios con Mayor Cobertura"
          description="Promedio de los 4 servicios combinados"
        >
          {datos.length > 0 ? (
            <ResponsiveContainer width="100%" height={320}>
              <BarChart
                data={[...datos]
                  .map((d) => ({
                    municipio:
                      d.municipio.length > 14
                        ? d.municipio.slice(0, 14) + "…"
                        : d.municipio,
                    promedio: +(
                      (d.aguaPotable +
                        d.energiaElectrica +
                        d.saneamiento +
                        d.internet) /
                      4
                    ).toFixed(2),
                  }))
                  .sort((a, b) => b.promedio - a.promedio)
                  .slice(0, 10)}
              >
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="municipio" tick={{ fontSize: 10 }} />
                <YAxis domain={[0, 100]} />
                <Tooltip formatter={(v) => `${v}%`} />
                <Legend />
                <Bar
                  dataKey="promedio"
                  name="Cobertura Promedio %"
                  fill="#10b981"
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
        title="Servicios Básicos por Municipio"
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
                <th className="pb-3 pr-4 font-medium text-right">Agua Potable %</th>
                <th className="pb-3 pr-4 font-medium text-right">Energía %</th>
                <th className="pb-3 pr-4 font-medium text-right">Saneamiento %</th>
                <th className="pb-3 font-medium text-right">Internet %</th>
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
                    <span
                      className={
                        d.aguaPotable >= 80
                          ? "text-green-600"
                          : d.aguaPotable >= 50
                          ? "text-amber-600"
                          : "text-red-600"
                      }
                    >
                      {d.aguaPotable}%
                    </span>
                  </td>
                  <td className="py-2.5 pr-4 text-right">
                    <span
                      className={
                        d.energiaElectrica >= 80
                          ? "text-green-600"
                          : d.energiaElectrica >= 50
                          ? "text-amber-600"
                          : "text-red-600"
                      }
                    >
                      {d.energiaElectrica}%
                    </span>
                  </td>
                  <td className="py-2.5 pr-4 text-right">
                    <span
                      className={
                        d.saneamiento >= 80
                          ? "text-green-600"
                          : d.saneamiento >= 50
                          ? "text-amber-600"
                          : "text-red-600"
                      }
                    >
                      {d.saneamiento}%
                    </span>
                  </td>
                  <td className="py-2.5 text-right">
                    <span
                      className={
                        d.internet >= 60
                          ? "text-green-600"
                          : d.internet >= 30
                          ? "text-amber-600"
                          : "text-red-600"
                      }
                    >
                      {d.internet}%
                    </span>
                  </td>
                </tr>
              ))}
              {municipiosFiltrados.length === 0 && (
                <tr>
                  <td colSpan={5} className="py-8 text-center text-gray-400">
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
