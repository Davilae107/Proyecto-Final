"use client";

import { useState, useEffect } from "react";
import {
  Database,
  Play,
  Loader2,
  CheckCircle2,
  XCircle,
  FileText,
  RefreshCw,
  Zap,
} from "lucide-react";
import { toast } from "sonner";
import {
  etlApi,
  csvApi,
  type EtlFullPipelineResponse,
  type CsvFileInfo,
} from "@/lib/api";

export default function EtlPage() {
  const [files, setFiles] = useState<CsvFileInfo[]>([]);
  const [loading, setLoading] = useState(true);
  const [runningFull, setRunningFull] = useState(false);
  const [runningSingle, setRunningSingle] = useState<string | null>(null);
  const [pipelineResult, setPipelineResult] =
    useState<EtlFullPipelineResponse | null>(null);

  useEffect(() => {
    loadFiles();
  }, []);

  const loadFiles = async () => {
    try {
      setLoading(true);
      const response = await csvApi.listFiles();
      setFiles(response.files);
    } catch {
      toast.error("Error cargando archivos");
    } finally {
      setLoading(false);
    }
  };

  const executeFullPipeline = async () => {
    if (
      !confirm(
        "¿Ejecutar el pipeline ETL completo? Esto procesará todos los archivos CSV."
      )
    )
      return;

    setRunningFull(true);
    setPipelineResult(null);
    try {
      const result = await etlApi.executeFullPipeline();
      setPipelineResult(result);
      toast.success(
        `Pipeline completado: ${result.totalInserted} insertados, ${result.totalUpdated} actualizados`
      );
    } catch {
      toast.error("Error ejecutando pipeline ETL");
    } finally {
      setRunningFull(false);
    }
  };

  const executeSingleFile = async (fileName: string) => {
    setRunningSingle(fileName);
    try {
      const result = await etlApi.executeSingleFile(fileName);
      toast.success(
        `${fileName}: ${result.inserted} insertados, ${result.updated} actualizados`
      );
    } catch {
      toast.error(`Error procesando ${fileName}`);
    } finally {
      setRunningSingle(null);
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Pipeline ETL</h1>
          <p className="text-gray-500 mt-1">
            Extrae, transforma y carga datos de los archivos CSV a la base de
            datos
          </p>
        </div>
        <div className="flex gap-2">
          <button
            onClick={loadFiles}
            className="flex items-center gap-2 px-4 py-2 bg-white border border-gray-200 rounded-lg text-sm font-medium text-gray-700 hover:bg-gray-50"
          >
            <RefreshCw className="w-4 h-4" />
            Actualizar
          </button>
          <button
            onClick={executeFullPipeline}
            disabled={runningFull || files.length === 0}
            className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg text-sm font-medium hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {runningFull ? (
              <Loader2 className="w-4 h-4 animate-spin" />
            ) : (
              <Zap className="w-4 h-4" />
            )}
            Ejecutar Pipeline Completo
          </button>
        </div>
      </div>

      {/* Pipeline Result Summary */}
      {pipelineResult && (
        <div className="bg-white rounded-xl border border-gray-200 p-5">
          <h3 className="font-semibold text-gray-900 mb-4 flex items-center gap-2">
            <CheckCircle2 className="w-5 h-5 text-green-500" />
            Resultado del Pipeline
          </h3>
          <div className="grid grid-cols-3 gap-4 mb-4">
            <div className="bg-green-50 rounded-lg p-4 text-center">
              <p className="text-2xl font-bold text-green-700">
                {pipelineResult.totalInserted}
              </p>
              <p className="text-sm text-green-600">Registros Insertados</p>
            </div>
            <div className="bg-blue-50 rounded-lg p-4 text-center">
              <p className="text-2xl font-bold text-blue-700">
                {pipelineResult.totalUpdated}
              </p>
              <p className="text-sm text-blue-600">Registros Actualizados</p>
            </div>
            <div className="bg-red-50 rounded-lg p-4 text-center">
              <p className="text-2xl font-bold text-red-700">
                {pipelineResult.totalErrors}
              </p>
              <p className="text-sm text-red-600">Errores</p>
            </div>
          </div>
          {/* Per-file results */}
          <div className="space-y-2">
            {Object.entries(pipelineResult.results).map(([key, val]) => (
              <div
                key={key}
                className="flex items-center justify-between px-3 py-2 bg-gray-50 rounded-lg text-sm"
              >
                <span className="font-medium text-gray-700">{key}</span>
                <div className="flex gap-4 text-xs">
                  <span className="text-green-600">+{val.inserted}</span>
                  <span className="text-blue-600">~{val.updated}</span>
                  {val.errors > 0 && (
                    <span className="text-red-600">!{val.errors}</span>
                  )}
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Files to Process */}
      <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
        <div className="px-5 py-4 border-b border-gray-200">
          <h2 className="font-semibold text-gray-900">
            Archivos Disponibles para Procesar
          </h2>
        </div>
        {loading ? (
          <div className="p-8 text-center">
            <Loader2 className="w-8 h-8 text-blue-500 animate-spin mx-auto" />
          </div>
        ) : files.length === 0 ? (
          <div className="p-8 text-center">
            <Database className="w-10 h-10 text-gray-300 mx-auto mb-2" />
            <p className="text-sm text-gray-500">
              No hay archivos CSV disponibles. Sube archivos primero.
            </p>
          </div>
        ) : (
          <div className="divide-y divide-gray-100">
            {files.map((file) => (
              <div
                key={file.fileName}
                className="px-5 py-3 flex items-center gap-3 hover:bg-gray-50"
              >
                <FileText className="w-5 h-5 text-green-500 flex-shrink-0" />
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-medium text-gray-900 truncate">
                    {file.fileName}
                  </p>
                  <p className="text-xs text-gray-500">
                    {file.sizeMB} MB · Tipo: {file.fileType}
                  </p>
                </div>
                <button
                  onClick={() => executeSingleFile(file.fileName)}
                  disabled={runningSingle === file.fileName || runningFull}
                  className="flex items-center gap-1.5 px-3 py-1.5 bg-green-50 text-green-700 rounded-lg text-xs font-medium hover:bg-green-100 disabled:opacity-50"
                >
                  {runningSingle === file.fileName ? (
                    <Loader2 className="w-3.5 h-3.5 animate-spin" />
                  ) : (
                    <Play className="w-3.5 h-3.5" />
                  )}
                  Procesar
                </button>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
