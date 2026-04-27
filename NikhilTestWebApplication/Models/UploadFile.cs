using Newtonsoft.Json;

namespace NikhilTestWebApplication.Models
{
    public class UploadFile
    {
        [JsonProperty(PropertyName = "uId")]
        public string uId { get; set; }

        public IFormFile File { get; set; }
    }

    public class UploadFileModel
    {
        public bool IsSuccess {  get; set; }
        public string Message { get; set; }

        public string FileName { get; set; }

        public DateTime UploadedOn { get; set; }
    }

    public class EmailModel
    {
        public string OfficialEmailAddress { get; set; }
    }
}
