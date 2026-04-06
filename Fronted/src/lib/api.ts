import axios from "axios";
import { clearAuthSession, getAuthToken } from "@/lib/auth";

const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_URL || "http://localhost:5186";

const api = axios.create({
  baseURL: `${API_BASE_URL}/api`,
  timeout: 120000,
  headers: {
    Accept: "application/json",
  },
});

api.interceptors.request.use((config) => {
  const token = getAuthToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (typeof window !== "undefined" && error?.response?.status === 401) {
      clearAuthSession();
      window.location.href = "/login";
    }

    return Promise.reject(error);
  }
);

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
}

export interface LoginResponse {
  success: boolean;
  data: {
    token: string;
    expiresAtUtc: string;
    email: string;
    fullName: string;
  };
}

export interface RegisterResponse {
  success: boolean;
  message: string;
}

export const authApi = {
  login: async (payload: LoginRequest): Promise<LoginResponse> => {
    const { data } = await api.post<LoginResponse>("/Auth/login", payload);
    return data;
  },

  register: async (payload: RegisterRequest): Promise<RegisterResponse> => {
    const { data } = await api.post<RegisterResponse>("/Auth/register", payload);
    return data;
  },
};

// ═══════════════════════════════════════════
// CSV Upload endpoints
// ═══════════════════════════════════════════

export interface UploadCsvResponse {
  success: boolean;
  message: string;
  fileName?: string;
  filePath?: string;
  fileSizeBytes?: number;
  autoProcessed: boolean;
  processingResult?: {
    inserted: number;
    updated: number;
    errors: number;
    errorMessage?: string;
  };
}

export interface UploadMultipleCsvsResponse {
  success: boolean;
  message: string;
  totalFiles: number;
  successfulUploads: number;
  failedUploads: number;
  results: {
    fileName: string;
    success: boolean;
    message?: string;
    fileSizeBytes?: number;
  }[];
}

export interface CsvFileInfo {
  fileName: string;
  filePath: string;
  sizeBytes: number;
  sizeMB: number;
  lastModified: string;
  fileType: string;
}

export interface ListFilesResponse {
  success: boolean;
  basePath: string;
  totalFiles: number;
  totalSizeMB: number;
  files: CsvFileInfo[];
}

export const csvApi = {
  uploadSingle: async (
    file: File,
    autoProcess: boolean = false
  ): Promise<UploadCsvResponse> => {
    const formData = new FormData();
    formData.append("File", file);
    formData.append("AutoProcess", String(autoProcess));
    const { data } = await api.post<UploadCsvResponse>(
      "/CsvUpload/upload-single",
      formData,
      { headers: { "Content-Type": "multipart/form-data" } }
    );
    return data;
  },

  uploadMultiple: async (
    files: File[],
    autoProcess: boolean = false
  ): Promise<UploadMultipleCsvsResponse> => {
    const formData = new FormData();
    files.forEach((f) => formData.append("files", f));
    formData.append("autoProcess", String(autoProcess));
    const { data } = await api.post<UploadMultipleCsvsResponse>(
      "/CsvUpload/upload-multiple",
      formData,
      { headers: { "Content-Type": "multipart/form-data" } }
    );
    return data;
  },

  listFiles: async (): Promise<ListFilesResponse> => {
    const { data } = await api.get<ListFilesResponse>(
      "/CsvUpload/list-files"
    );
    return data;
  },

  deleteFile: async (
    fileName: string
  ): Promise<{ success: boolean; message: string }> => {
    const { data } = await api.delete("/CsvUpload/delete-file", {
      params: { fileName },
    });
    return data;
  },
};

// ═══════════════════════════════════════════
// ETL Pipeline endpoints
// ═══════════════════════════════════════════

export interface EtlResult {
  [key: string]: { inserted: number; updated: number; errors: number };
}

export interface EtlFullPipelineResponse {
  success: boolean;
  message: string;
  results: EtlResult;
  totalInserted: number;
  totalUpdated: number;
  totalErrors: number;
}

export interface EtlSingleFileResponse {
  success: boolean;
  message: string;
  inserted: number;
  updated: number;
  errors: number;
}

export interface CheckCsvFilesResponse {
  success: boolean;
  basePath: string;
  totalFiles: number;
  files: { fileName: string; size: number; lastModified: string }[];
}

export const etlApi = {
  executeFullPipeline: async (): Promise<EtlFullPipelineResponse> => {
    const { data } = await api.post<EtlFullPipelineResponse>(
      "/Etl/execute-full-pipeline"
    );
    return data;
  },

  executeSingleFile: async (
    fileName: string
  ): Promise<EtlSingleFileResponse> => {
    const { data } = await api.post<EtlSingleFileResponse>(
      "/Etl/execute-single-file",
      null,
      { params: { fileName } }
    );
    return data;
  },

  checkCsvFiles: async (): Promise<CheckCsvFilesResponse> => {
    const { data } = await api.get<CheckCsvFilesResponse>(
      "/Etl/check-csv-files"
    );
    return data;
  },
};

// ═══════════════════════════════════════════
// Estadísticas endpoints
// ═══════════════════════════════════════════

export interface DesercionPorSector {
  sector: string;
  totalMatriculados: number;
  totalAbandonos: number;
  tasaAbandono: number;
  probabilidadAbandono: number;
  cantidadCentros: number;
  registros: number;
}

export interface DesercionPorNivel {
  nivel: string;
  totalMatriculados: number;
  totalAbandonos: number;
  tasaAbandono: number;
  probabilidadAbandono: number;
  registros: number;
}

export interface DesercionPorZona {
  zona: string;
  totalMatriculados: number;
  totalAbandonos: number;
  tasaAbandono: number;
  probabilidadAbandono: number;
  cantidadCentros: number;
}

export interface CausanteAbandono {
  causante: string;
  totalCasos: number;
  pesoPromedio: number;
  municipiosAfectados: number;
}

export interface DesercionPorCentro {
  centro: string;
  codigoCentro: string;
  sector: string;
  zona: string;
  nivel: string;
  matriculados: number;
  abandonos: number;
  tasaAbandono: number;
  periodo: string;
}

export interface ResumenDesercion {
  totalMatriculados: number;
  totalAbandonos: number;
  tasaGeneralAbandono: number;
  probabilidadGeneral: number;
  totalCentros: number;
  totalRegistros: number;
}

export interface RendimientoPorNivel {
  nivel: string;
  promedioCalificaciones: number;
  tasaAprobacion: number;
  tasaRepitencia: number;
  totalEstudiantes: number;
  estudiantesAprobados: number;
  estudiantesReprobados: number;
}

export interface RendimientoPorCentro {
  centro: string;
  sector: string;
  zona: string;
  nivel: string;
  promedio: number;
  tasaAprobacion: number;
  totalEstudiantes: number;
  aprobados: number;
  reprobados: number;
}

export interface SocioeconomicoPorMunicipio {
  municipio: string;
  ingresoPromedio: number;
  tasaPobreza: number;
  tasaPobrezaExtrema: number;
  trabajoInfantil: number;
  tasaDesempleo: number;
  informalidad: number;
}

export interface ServicioPorMunicipio {
  municipio: string;
  aguaPotable: number;
  energiaElectrica: number;
  saneamiento: number;
  internet: number;
}

export interface CoberturaPorMunicipio {
  municipio: string;
  nivel: string;
  tasaCobertura: number;
  brechaUrbanoRural: number;
  inequidad: number;
  centrosPorMil: number;
  distanciaPromedio: number;
}

export interface AiProvinceRecommendation {
  provincia: string;
  region: string;
  prioridad: string;
  hallazgos: string[];
  sugerencias: string[];
}

export interface AiProvinceRecommendationsResult {
  generatedAt: string;
  model: string;
  provinciasAnalizadas: number;
  recomendaciones: AiProvinceRecommendation[];
}

export const estadisticasApi = {
  // Deserción
  getDesercionPorSector: async (): Promise<{ success: boolean; data: DesercionPorSector[] }> => {
    const { data } = await api.get("/Estadisticas/desercion/por-sector");
    return data;
  },
  getDesercionPorNivel: async (): Promise<{ success: boolean; data: DesercionPorNivel[] }> => {
    const { data } = await api.get("/Estadisticas/desercion/por-nivel");
    return data;
  },
  getDesercionPorZona: async (): Promise<{ success: boolean; data: DesercionPorZona[] }> => {
    const { data } = await api.get("/Estadisticas/desercion/por-zona");
    return data;
  },
  getCausantesAbandono: async (): Promise<{ success: boolean; data: CausanteAbandono[] }> => {
    const { data } = await api.get("/Estadisticas/desercion/causantes");
    return data;
  },
  getDesercionPorCentro: async (): Promise<{ success: boolean; data: DesercionPorCentro[] }> => {
    const { data } = await api.get("/Estadisticas/desercion/por-centro");
    return data;
  },
  getResumenDesercion: async (): Promise<{ success: boolean; data: ResumenDesercion }> => {
    const { data } = await api.get("/Estadisticas/desercion/resumen");
    return data;
  },

  // Rendimiento
  getRendimientoPorNivel: async (): Promise<{ success: boolean; data: RendimientoPorNivel[] }> => {
    const { data } = await api.get("/Estadisticas/rendimiento/por-nivel");
    return data;
  },
  getRendimientoPorCentro: async (): Promise<{ success: boolean; data: RendimientoPorCentro[] }> => {
    const { data } = await api.get("/Estadisticas/rendimiento/por-centro");
    return data;
  },

  // Socioeconómicos
  getSocioeconomicos: async (): Promise<{ success: boolean; data: SocioeconomicoPorMunicipio[] }> => {
    const { data } = await api.get("/Estadisticas/socioeconomicos/por-municipio");
    return data;
  },

  // Servicios
  getServicios: async (): Promise<{ success: boolean; data: ServicioPorMunicipio[] }> => {
    const { data } = await api.get("/Estadisticas/servicios/por-municipio");
    return data;
  },

  // Cobertura
  getCobertura: async (): Promise<{ success: boolean; data: CoberturaPorMunicipio[] }> => {
    const { data } = await api.get("/Estadisticas/cobertura/por-municipio");
    return data;
  },
};

export const aiApi = {
  getRecomendacionesPorProvincia: async (
    top: number = 10
  ): Promise<{ success: boolean; data: AiProvinceRecommendationsResult }> => {
    const { data } = await api.get("/Ai/recomendaciones/provincias", {
      params: { top },
    });
    return data;
  },
};

export default api;
