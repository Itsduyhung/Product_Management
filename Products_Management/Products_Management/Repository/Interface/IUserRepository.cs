using Products_Management.Model;

namespace Products_Management.Repository.Interface
{
    public interface IUserRepository
    {
        Task<UserEntity?> GetByEmailAsync(string email);
        Task<UserEntity> AddAsync(UserEntity user);
    }
}