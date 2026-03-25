"use client";

import { useEffect, useState } from "react";
import {
  BarChart3,
  Upload,
  Database,
  FileText,
  AlertTriangle,
  CheckCircle2,
  TrendingUp,
  Users,
} from "lucide-react";
import Link from "next/link";
import { csvApi, etlApi, type CsvFileInfo } from "@/lib/api";

interface DashboardStats {
  totalFiles: number;
  totalSizeMB: number;
  recentFiles: CsvFileInfo[];
}

export default function DashboardPage() {
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadStats();
  }, []);

  const loadStats = async () => {
    try {
      setLoading(true);
      const filesResponse = await csvApi.listFiles();
      setStats({
        totalFiles: filesResponse.totalFiles,
        totalSizeMB: filesResponse.totalSizeMB,
        recentFiles: filesResponse.files.slice(0, 5),
      });
    } catch {
      setError("No se pudo conectar con el servidor. Verifica que el backend esté ejecutándose.");
    } finally {
      setLoading(false);
    }
  };

  const statCards = [
    {
      label: "Archivos CSV",
      value: stats?.totalFiles ?? "—",
      icon: FileText,
      color: "bg-blue-500",
      href: "/dashboard/csv-upload",
    },
    {
      label: "Tamaño Total",
      value: stats ? `${stats.totalSizeMB} MB` : "—",
      icon: Database,
      color: "bg-green-500",
      href: "/dashboard/etl",
    },
    {
      label: "Módulos ETL",
      value: 12,
      icon: TrendingUp,
      color: "bg-purple-500",
      href: "/dashboard/etl",
    },
    {
      label: "Indicadores",
      value: 5,
      icon: BarChart3,
      color: "bg-orange-500",
      href: "/dashboard/estadisticas/rendimiento",
    },
  ];

  const quickActions = [
    {
      label: "Subir Archivos CSV",
      description: "Carga nuevos archivos de datos al sistema",
      icon: Upload,
      href: "/dashboard/csv-upload",
      color: "text-blue-600 bg-blue-50",
    },
    {
      label: "Ejecutar Pipeline ETL",
      description: "Procesa y transforma los datos cargados",
      icon: Database,
      href: "/dashboard/etl",
      color: "text-green-600 bg-green-50",
    },
    {
      label: "Ver Estadísticas",
      description: "Consulta gráficas e indicadores educativos",
      icon: BarChart3,
      href: "/dashboard/estadisticas/rendimiento",
      color: "text-purple-600 bg-purple-50",
    },
  ];

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Dashboard</h1>
        <p className="text-gray-500 mt-1">
          Sistema de Indicadores para la Prevención del Abandono Escolar
        </p>
      </div>

      {/* Error Banner */}
      {error && (
        <div className="bg-amber-50 border border-amber-200 rounded-lg p-4 flex items-start gap-3">
          <AlertTriangle className="w-5 h-5 text-amber-500 flex-shrink-0 mt-0.5" />
          <div>
            <p className="text-sm font-medium text-amber-800">{error}</p>
            <button
              onClick={loadStats}
              className="text-sm text-amber-600 underline mt-1"
            >
              Reintentar conexión
            </button>
          </div>
        </div>
      )}

      {/* Stats Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        {statCards.map((card) => (
          <Link
            key={card.label}
            href={card.href}
            className="bg-white rounded-xl border border-gray-200 p-5 hover:shadow-md transition-shadow"
          >
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-500">{card.label}</p>
                <p className="text-2xl font-bold mt-1">
                  {loading ? (
                    <span className="inline-block w-16 h-7 bg-gray-200 rounded animate-pulse" />
                  ) : (
                    card.value
                  )}
                </p>
              </div>
              <div
                className={`${card.color} w-12 h-12 rounded-lg flex items-center justify-center`}
              >
                <card.icon className="w-6 h-6 text-white" />
              </div>
            </div>
          </Link>
        ))}
      </div>

      {/* Quick Actions */}
      <div>
        <h2 className="text-lg font-semibold text-gray-900 mb-3">
          Acciones Rápidas
        </h2>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          {quickActions.map((action) => (
            <Link
              key={action.label}
              href={action.href}
              className="bg-white rounded-xl border border-gray-200 p-5 hover:shadow-md transition-shadow group"
            >
              <div className={`w-10 h-10 rounded-lg flex items-center justify-center ${action.color} mb-3`}>
                <action.icon className="w-5 h-5" />
              </div>
              <h3 className="font-semibold text-gray-900 group-hover:text-blue-600 transition-colors">
                {action.label}
              </h3>
              <p className="text-sm text-gray-500 mt-1">
                {action.description}
              </p>
            </Link>
          ))}
        </div>
      </div>

      {/* Recent Files */}
      <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
        <div className="px-5 py-4 border-b border-gray-200 flex items-center justify-between">
          <h2 className="font-semibold text-gray-900">Archivos Recientes</h2>
          <Link
            href="/dashboard/csv-upload"
            className="text-sm text-blue-600 hover:text-blue-800"
          >
            Ver todos
          </Link>
        </div>
        <div className="divide-y divide-gray-100">
          {loading ? (
            Array.from({ length: 3 }).map((_, i) => (
              <div key={i} className="px-5 py-3 flex items-center gap-3">
                <div className="w-8 h-8 bg-gray-200 rounded animate-pulse" />
                <div className="flex-1">
                  <div className="h-4 w-48 bg-gray-200 rounded animate-pulse" />
                  <div className="h-3 w-24 bg-gray-100 rounded animate-pulse mt-1" />
                </div>
              </div>
            ))
          ) : stats?.recentFiles.length ? (
            stats.recentFiles.map((file) => (
              <div
                key={file.fileName}
                className="px-5 py-3 flex items-center gap-3"
              >
                <div className="w-8 h-8 bg-green-50 rounded-lg flex items-center justify-center">
                  <CheckCircle2 className="w-4 h-4 text-green-500" />
                </div>
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-medium text-gray-900 truncate">
                    {file.fileName}
                  </p>
                  <p className="text-xs text-gray-500">
                    {file.sizeMB} MB · Tipo: {file.fileType}
                  </p>
                </div>
                <p className="text-xs text-gray-400">
                  {new Date(file.lastModified).toLocaleDateString("es-DO")}
                </p>
              </div>
            ))
          ) : (
            <div className="px-5 py-8 text-center">
              <Users className="w-10 h-10 text-gray-300 mx-auto mb-2" />
              <p className="text-sm text-gray-500">
                No hay archivos cargados aún
              </p>
              <Link
                href="/dashboard/csv-upload"
                className="text-sm text-blue-600 hover:underline mt-1 inline-block"
              >
                Subir primer archivo
              </Link>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
