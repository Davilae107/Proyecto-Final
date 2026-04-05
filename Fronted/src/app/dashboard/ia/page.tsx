"use client";

import { useEffect, useState } from "react";
import axios from "axios";
import {
  Brain,
  RefreshCw,
  Loader2,
  AlertTriangle,
  ShieldAlert,
  CheckCircle2,
} from "lucide-react";
import { aiApi, type AiProvinceRecommendation } from "@/lib/api";
import { toast } from "sonner";

export default function IaPage() {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [top, setTop] = useState(10);
  const [model, setModel] = useState<string>("");
  const [recomendaciones, setRecomendaciones] = useState<AiProvinceRecommendation[]>([]);
  const [provinciasAnalizadas, setProvinciasAnalizadas] = useState(0);

  useEffect(() => {
    loadData(top);
  }, [top]);

  const loadData = async (topValue: number) => {
    setLoading(true);
    setError(null);

    try {
      const response = await aiApi.getRecomendacionesPorProvincia(topValue);
      setModel(response.data.model);
      setProvinciasAnalizadas(response.data.provinciasAnalizadas);
      setRecomendaciones(response.data.recomendaciones);
    } catch (err: unknown) {
      const backendMessage = axios.isAxiosError(err)
        ? err.response?.data?.message
        : null;
      const finalMessage =
        backendMessage ||
        "No se pudo generar el análisis de IA. Verifica que el backend esté activo y que GeminiAi:ApiKey esté configurada.";
      setError(finalMessage);
      toast.error("Error generando recomendaciones de IA");
    } finally {
      setLoading(false);
    }
  };

  const prioridadBadge = (prioridad: string) => {
    if (prioridad.toLowerCase() === "alta") {
      return "bg-red-100 text-red-700 border-red-200";
    }
    if (prioridad.toLowerCase() === "media") {
      return "bg-amber-100 text-amber-700 border-amber-200";
    }
    return "bg-emerald-100 text-emerald-700 border-emerald-200";
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 flex items-center gap-2">
            <Brain className="w-7 h-7 text-blue-600" />
            Módulo de IA
          </h1>
          <p className="text-gray-500 mt-1">
            Recomendaciones automáticas de mejora por provincia/región basadas en KPIs educativos.
          </p>
        </div>

        <div className="flex items-center gap-3">
          <div className="flex items-center gap-2 bg-white border border-gray-200 rounded-lg px-3 py-2">
            <label htmlFor="top" className="text-sm text-gray-600">
              Provincias:
            </label>
            <select
              id="top"
              value={top}
              onChange={(e) => setTop(Number(e.target.value))}
              className="text-sm bg-transparent outline-none"
            >
              {[5, 10, 15, 20, 25, 32].map((n) => (
                <option key={n} value={n}>
                  Top {n}
                </option>
              ))}
            </select>
          </div>

          <button
            onClick={() => loadData(top)}
            className="inline-flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg text-sm font-medium hover:bg-blue-700"
          >
            <RefreshCw className="w-4 h-4" />
            Analizar
          </button>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className="bg-white border border-gray-200 rounded-xl p-4">
          <p className="text-sm text-gray-500">Provincias Analizadas</p>
          <p className="text-2xl font-bold text-gray-900 mt-1">{provinciasAnalizadas}</p>
        </div>
        <div className="bg-white border border-gray-200 rounded-xl p-4">
          <p className="text-sm text-gray-500">Modelo IA</p>
          <p className="text-lg font-semibold text-gray-900 mt-1">{model || "—"}</p>
        </div>
        <div className="bg-white border border-gray-200 rounded-xl p-4">
          <p className="text-sm text-gray-500">Estado</p>
          <p className="text-lg font-semibold text-emerald-700 mt-1">
            {loading ? "Procesando" : "Análisis listo"}
          </p>
        </div>
      </div>

      {loading && (
        <div className="bg-white border border-gray-200 rounded-xl p-10 text-center">
          <Loader2 className="w-9 h-9 text-blue-600 mx-auto animate-spin" />
          <p className="text-gray-600 mt-3">Generando recomendaciones con IA...</p>
        </div>
      )}

      {!loading && error && (
        <div className="bg-amber-50 border border-amber-200 rounded-xl p-6">
          <div className="flex items-start gap-3">
            <AlertTriangle className="w-6 h-6 text-amber-600 mt-0.5" />
            <div>
              <p className="font-semibold text-amber-800">No fue posible completar el análisis</p>
              <p className="text-amber-700 mt-1 text-sm">{error}</p>
            </div>
          </div>
        </div>
      )}

      {!loading && !error && recomendaciones.length === 0 && (
        <div className="bg-white border border-gray-200 rounded-xl p-10 text-center">
          <ShieldAlert className="w-10 h-10 text-gray-400 mx-auto" />
          <p className="text-gray-600 mt-3">No hay recomendaciones disponibles todavía.</p>
        </div>
      )}

      {!loading && !error && recomendaciones.length > 0 && (
        <div className="space-y-4">
          {recomendaciones.map((item) => (
            <div key={`${item.provincia}-${item.region}`} className="bg-white border border-gray-200 rounded-xl p-5">
              <div className="flex flex-wrap items-center justify-between gap-3">
                <div>
                  <h2 className="text-xl font-semibold text-gray-900">{item.provincia}</h2>
                  <p className="text-sm text-gray-500">Región: {item.region}</p>
                </div>
                <span
                  className={`px-3 py-1 rounded-full text-xs font-semibold border ${prioridadBadge(item.prioridad)}`}
                >
                  Prioridad {item.prioridad}
                </span>
              </div>

              <div className="mt-4 grid grid-cols-1 lg:grid-cols-2 gap-4">
                <div className="bg-gray-50 rounded-lg p-4">
                  <h3 className="text-sm font-semibold text-gray-700 mb-2">Hallazgos</h3>
                  <ul className="space-y-2">
                    {item.hallazgos.map((h, idx) => (
                      <li key={idx} className="text-sm text-gray-700 flex items-start gap-2">
                        <span className="mt-1 inline-block w-1.5 h-1.5 rounded-full bg-blue-500" />
                        <span>{h}</span>
                      </li>
                    ))}
                  </ul>
                </div>

                <div className="bg-emerald-50 rounded-lg p-4">
                  <h3 className="text-sm font-semibold text-emerald-700 mb-2">Sugerencias de mejora</h3>
                  <ul className="space-y-2">
                    {item.sugerencias.map((s, idx) => (
                      <li key={idx} className="text-sm text-emerald-800 flex items-start gap-2">
                        <CheckCircle2 className="w-4 h-4 mt-0.5 shrink-0" />
                        <span>{s}</span>
                      </li>
                    ))}
                  </ul>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
