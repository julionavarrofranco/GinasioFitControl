using ProjetoFinal.Models;
using ProjetoFinal.Models.DTOs;

namespace ProjetoFinal.Services
{
    public interface IAuthService
    {
        Task<TokenResponseDto> LoginAsync(UserLoginDto request);

        Task<User> RegisterAsync(UserRegisterDto request, User CurrentUser);

        Task<TokenResponseDto> RefreshTokensAsync(RefreshTokenRequestDto request);

        Task LogoutAsync(int idUser);

        Task ResetPasswordAsync(string email);

        Task ChangePasswordAsync(ChangePasswordDto request);

        Task<string> RotateRefreshTokenAsync(int idUser, string refreshToken);

    }
}
