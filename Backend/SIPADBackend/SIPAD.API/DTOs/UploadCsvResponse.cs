namespace SIPAD.API.DTOs;

public class UploadCsvResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = null!;
    public string? FileName { get; set; }
    public string? FilePath { get; set; }
    public long? FileSizeBytes { get; set; }
    public bool AutoProcessed { get; set; }
    public ProcessingResult? ProcessingResult { get; set; }
}

public class ProcessingResult
{
    public int Inserted { get; set; }
    public int Updated { get; set; }
    public int Errors { get; set; }
    public string? ErrorMessage { get; set; }
}