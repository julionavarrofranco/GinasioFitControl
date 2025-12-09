using ProjetoFinal.Models;
using ProjetoFinal.Models.DTOs;

namespace ProjetoFinal.Services

{
    public interface IUserService
    {
        Task<User?> GetUserByIdAsync(int idUser, bool includeFuncionario = false, bool includeMembro = false);

        Task<User> CreateUserAsync(UserRegisterDto dto, CurrentUserInfo currentUser);

        Task ChangeUserActiveStatusAsync(UserStatusDto request);

        Task CancelarActiveTokensAsync(int userId);

        Task ResetPasswordAsync(ResetPasswordDto email);

        Task ChangePasswordAsync(int idUser, ChangePasswordDto request);

        Task ChangeEmailAsync(ChangeEmailDto request);
    }
}
