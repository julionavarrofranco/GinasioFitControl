using ProjetoFinal.Models;
using ProjetoFinal.Models.DTOs;

namespace ProjetoFinal.Services
{
    public interface IAuthService
    {
        Task<TokenResponseDto> LoginAsync(UserLoginDto request);

        Task<User> RegisterAsync(UserRegisterDto request, User currentUser);

        Task<TokenResponseDto> RefreshTokensAsync(RefreshTokenRequestDto request);

        Task LogoutAsync(int idUser);

        Task ResetPasswordAsync(ResetPasswordDto email);

        Task ChangePasswordAsync(int idUser,ChangePasswordDto request);

        Task AdminChangePasswordAsync(AdminChangePasswordDto request);
    }
}
