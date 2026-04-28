namespace NikhilTestWebApplication.Models
{
    public class ErrorResponse
    {
        public bool Success { get; set; } = false;
        public string Message { get; set; } = string.Empty;

        public List<string>? Errors { get; set; }
    }
}