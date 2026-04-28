namespace NikhilTestWebApplication.Models
{
    public class PagedResponse<T>
    {
        public bool Success { get; set; }

        public T Data { get; set; } = default!;

        public int PageNumber { get; set; }

        public int PageSize {  get; set; }

        public int TotalRecords { get; set; }

        public int TotalPages { get; set; }

        public PagedResponse(
            T data,
            int pageNumber,
            int pageSize,
            int totalRecords
            )
        {
            Success = true;
            Data = data; 
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalRecords = totalRecords;
            TotalPages = (int)Math.Ceiling(totalRecords/(double)pageSize);

        }
    }
}
