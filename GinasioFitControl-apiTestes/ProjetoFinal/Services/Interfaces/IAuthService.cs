using ProjetoFinal.Models;
using ProjetoFinal.Models.DTOs;

namespace ProjetoFinal.Services.Interfaces
{
    public interface IAuthService
    {
        Task<TokenResponseDto> LoginAsync(UserLoginDto request);

        Task<User> RegisterAsync(UserRegisterDto request, CurrentUserInfo currentUser);

        Task<TokenResponseDto> RefreshTokensAsync(RefreshTokenRequestDto request);

        Task LogoutAsync(int idUser);
    }
}
