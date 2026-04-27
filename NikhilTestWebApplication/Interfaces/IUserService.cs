using NikhilTestWebApplication.Models;

namespace NikhilTestWebApplication.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAll();
        Task<User?> GetById(int id);
        Task<User> Add(User user);
        Task<User?> Update(int id, User user);
        Task<bool> Delete(int id);

        Task<UploadFileModel> UploadFile(UploadFile uploadFile);
    }
}
