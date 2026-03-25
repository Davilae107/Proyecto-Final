"use client";

import { useCallback, useEffect, useState } from "react";
import { useDropzone } from "react-dropzone";
import {
  Upload,
  FileText,
  Trash2,
  CheckCircle2,
  XCircle,
  Loader2,
  RefreshCw,
  HardDriveDownload,
} from "lucide-react";
import { toast } from "sonner";
import {
  csvApi,
  type CsvFileInfo,
  type UploadCsvResponse,
} from "@/lib/api";

export default function CsvUploadPage() {
  const [files, setFiles] = useState<CsvFileInfo[]>([]);
  const [loading, setLoading] = useState(true);
  const [uploading, setUploading] = useState(false);
  const [autoProcess, setAutoProcess] = useState(false);
  const [uploadResults, setUploadResults] = useState<UploadCsvResponse[]>([]);

  useEffect(() => {
    loadFiles();
  }, []);

  const loadFiles = async () => {
    try {
      setLoading(true);
      const response = await csvApi.listFiles();
      setFiles(response.files);
    } catch {
      toast.error("Error cargando lista de archivos");
    } finally {
      setLoading(false);
    }
  };

  const onDrop = useCallback(
    async (acceptedFiles: File[]) => {
      if (acceptedFiles.length === 0) return;

      setUploading(true);
      setUploadResults([]);

      try {
        if (acceptedFiles.length === 1) {
          const result = await csvApi.uploadSingle(
            acceptedFiles[0],
            autoProcess
          );
          setUploadResults([result]);
          if (result.success) {
            toast.success(result.message);
          } else {
            toast.error(result.message);
          }
        } else {
          const result = await csvApi.uploadMultiple(
            acceptedFiles,
            autoProcess
          );
          toast.success(result.message);
          setUploadResults(
            result.results.map((r) => ({
              success: r.success,
              message: r.message || "",
              fileName: r.fileName,
              fileSizeBytes: r.fileSizeBytes ?? undefined,
              autoProcessed: false,
            }))
          );
        }

        await loadFiles();
      } catch {
        toast.error("Error subiendo archivos");
      } finally {
        setUploading(false);
      }
    },
    [autoProcess]
  );

  const { getRootProps, getInputProps, isDragActive } = useDropzone({
    onDrop,
    accept: { "text/csv": [".csv"] },
    disabled: uploading,
  });

  const handleDelete = async (fileName: string) => {
    if (!confirm(`¿Eliminar "${fileName}"?`)) return;
    try {
      await csvApi.deleteFile(fileName);
      toast.success(`Archivo "${fileName}" eliminado`);
      await loadFiles();
    } catch {
      toast.error("Error eliminando archivo");
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">
            Gestión de Archivos CSV
          </h1>
          <p className="text-gray-500 mt-1">
            Sube y gestiona los archivos de datos del sistema
          </p>
        </div>
        <button
          onClick={loadFiles}
          className="flex items-center gap-2 px-4 py-2 bg-white border border-gray-200 rounded-lg text-sm font-medium text-gray-700 hover:bg-gray-50"
        >
          <RefreshCw className="w-4 h-4" />
          Actualizar
        </button>
      </div>

      {/* Upload Zone */}
      <div className="bg-white rounded-xl border border-gray-200 p-6">
        <div
          {...getRootProps()}
          className={`border-2 border-dashed rounded-xl p-10 text-center cursor-pointer transition-colors ${
            isDragActive
              ? "border-blue-400 bg-blue-50"
              : uploading
              ? "border-gray-200 bg-gray-50 cursor-not-allowed"
              : "border-gray-300 hover:border-blue-400 hover:bg-blue-50/50"
          }`}
        >
          <input {...getInputProps()} />
          <div className="flex flex-col items-center gap-3">
            {uploading ? (
              <>
                <Loader2 className="w-12 h-12 text-blue-500 animate-spin" />
                <p className="text-gray-600 font-medium">
                  Subiendo archivos...
                </p>
              </>
            ) : (
              <>
                <div className="w-14 h-14 bg-blue-50 rounded-full flex items-center justify-center">
                  <Upload className="w-7 h-7 text-blue-500" />
                </div>
                <div>
                  <p className="text-gray-700 font-medium">
                    {isDragActive
                      ? "Suelta los archivos aquí"
                      : "Arrastra archivos CSV aquí o haz clic para seleccionar"}
                  </p>
                  <p className="text-sm text-gray-400 mt-1">
                    Archivos .csv, máximo 50MB por archivo
                  </p>
                </div>
              </>
            )}
          </div>
        </div>

        {/* Options */}
        <div className="mt-4 flex items-center gap-3">
          <label className="flex items-center gap-2 text-sm text-gray-600 cursor-pointer">
            <input
              type="checkbox"
              checked={autoProcess}
              onChange={(e) => setAutoProcess(e.target.checked)}
              className="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
            />
            <HardDriveDownload className="w-4 h-4" />
            Procesar automáticamente al subir (ETL)
          </label>
        </div>
      </div>

      {/* Upload Results */}
      {uploadResults.length > 0 && (
        <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
          <div className="px-5 py-3 border-b border-gray-200 bg-gray-50">
            <h3 className="font-medium text-sm text-gray-700">
              Resultado de la carga
            </h3>
          </div>
          <div className="divide-y divide-gray-100">
            {uploadResults.map((result, idx) => (
              <div
                key={idx}
                className="px-5 py-3 flex items-center gap-3"
              >
                {result.success ? (
                  <CheckCircle2 className="w-5 h-5 text-green-500 flex-shrink-0" />
                ) : (
                  <XCircle className="w-5 h-5 text-red-500 flex-shrink-0" />
                )}
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-medium text-gray-900 truncate">
                    {result.fileName ?? "Archivo"}
                  </p>
                  <p className="text-xs text-gray-500">{result.message}</p>
                </div>
                {result.processingResult && (
                  <div className="text-xs text-gray-500 space-x-3">
                    <span className="text-green-600">
                      +{result.processingResult.inserted} insertados
                    </span>
                    <span className="text-blue-600">
                      ~{result.processingResult.updated} actualizados
                    </span>
                    {result.processingResult.errors > 0 && (
                      <span className="text-red-600">
                        !{result.processingResult.errors} errores
                      </span>
                    )}
                  </div>
                )}
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Files List */}
      <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
        <div className="px-5 py-4 border-b border-gray-200 flex items-center justify-between">
          <h2 className="font-semibold text-gray-900">
            Archivos en el Servidor
          </h2>
          <span className="text-sm text-gray-500">
            {files.length} archivo{files.length !== 1 ? "s" : ""}
          </span>
        </div>
        {loading ? (
          <div className="p-8 text-center">
            <Loader2 className="w-8 h-8 text-blue-500 animate-spin mx-auto" />
            <p className="text-sm text-gray-500 mt-2">Cargando archivos...</p>
          </div>
        ) : files.length === 0 ? (
          <div className="p-8 text-center">
            <FileText className="w-10 h-10 text-gray-300 mx-auto mb-2" />
            <p className="text-sm text-gray-500">
              No hay archivos CSV en el servidor
            </p>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead className="bg-gray-50 text-gray-600">
                <tr>
                  <th className="px-5 py-3 text-left font-medium">Nombre</th>
                  <th className="px-5 py-3 text-left font-medium">Tipo</th>
                  <th className="px-5 py-3 text-right font-medium">Tamaño</th>
                  <th className="px-5 py-3 text-left font-medium">
                    Fecha Modificación
                  </th>
                  <th className="px-5 py-3 text-center font-medium">
                    Acciones
                  </th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {files.map((file) => (
                  <tr
                    key={file.fileName}
                    className="hover:bg-gray-50 transition-colors"
                  >
                    <td className="px-5 py-3">
                      <div className="flex items-center gap-2">
                        <FileText className="w-4 h-4 text-green-500" />
                        <span className="font-medium text-gray-900">
                          {file.fileName}
                        </span>
                      </div>
                    </td>
                    <td className="px-5 py-3">
                      <span className="inline-flex px-2 py-0.5 rounded-full text-xs font-medium bg-blue-50 text-blue-700">
                        {file.fileType}
                      </span>
                    </td>
                    <td className="px-5 py-3 text-right text-gray-600">
                      {file.sizeMB} MB
                    </td>
                    <td className="px-5 py-3 text-gray-600">
                      {new Date(file.lastModified).toLocaleString("es-DO")}
                    </td>
                    <td className="px-5 py-3 text-center">
                      <button
                        onClick={() => handleDelete(file.fileName)}
                        className="p-1.5 text-red-400 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                        title="Eliminar archivo"
                      >
                        <Trash2 className="w-4 h-4" />
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}
