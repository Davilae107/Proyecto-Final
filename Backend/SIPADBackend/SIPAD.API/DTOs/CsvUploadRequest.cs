public class CsvUploadRequest
{
    public required IFormFile File { get; set; }
    public bool AutoProcess { get; set; } = false;
}