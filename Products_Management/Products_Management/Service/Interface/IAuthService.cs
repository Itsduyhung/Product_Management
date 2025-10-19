using Products_Management.DTOs.Request;
using Products_Management.DTOs.Response;

namespace Products_Management.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse?> LoginAsync(LoginRequest request);
    }
}