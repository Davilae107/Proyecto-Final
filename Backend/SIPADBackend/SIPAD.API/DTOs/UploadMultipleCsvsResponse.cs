namespace SIPAD.API.DTOs;

public class UploadMultipleCsvsResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = null!;
    public int TotalFiles { get; set; }
    public int SuccessfulUploads { get; set; }
    public int FailedUploads { get; set; }
    public List<FileUploadResult> Results { get; set; } = new();
}

public class FileUploadResult
{
    public string FileName { get; set; } = null!;
    public bool Success { get; set; }
    public string? Message { get; set; }
    public long? FileSizeBytes { get; set; }
}