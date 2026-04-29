using NikhilTestWebApplication.Models;

namespace NikhilTestWebApplication.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAll();
        Task<User?> GetById(Guid id);
        Task<User> Add(User user);
        Task<User?> Update(User user);
        Task<bool> Delete(Guid id);

        Task<bool> RestoreUser(Guid id);

        Task<UploadFileModel> UploadFile(UploadFile uploadFile);

        Task<PagedResponse<List<User>>> GetUsersAsync(PaginationParams pagination);
    }
}
