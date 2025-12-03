using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoFinal.Data;
using ProjetoFinal.Models.DTOs;
using ProjetoFinal.Services;
using System.Security.Claims;

namespace ProjetoFinal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly GinasioDbContext _context;

        public AuthController(IAuthService authService, GinasioDbContext context)
        {
            _authService = authService;
            _context = context;
        }

        // 1. REGISTO
        [Authorize(Policy = "CanManageUsers")]
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto request)
        {
            try
            {
                var idUser = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                // Além da policy (verifica se é funcionario(Admin ou rececao)), pega os dados do funcionario que está a registar o user
                // para verificar se tem permissões para registar o tipo de user pedido
                var currentUser = await _context.Users
                                                .Include(u => u.Funcionario)
                                                .FirstOrDefaultAsync(u => u.IdUser == idUser);
                if (currentUser == null)
                    return Unauthorized(new { message = "Utilizador não encontrado." });

                var user = await _authService.RegisterAsync(request, currentUser);

                return Ok(new {message = "Utilizador registado com sucesso." });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Você não tem permissão para registrar utilizadores." });

            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }

        // 2. LOGIN
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto request)
        {
            try
            {
                var tokenResponse = await _authService.LoginAsync(request);
                return Ok(tokenResponse);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new {message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }

        // 3. LOGOUT
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var idUser = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                await _authService.LogoutAsync(idUser);
                return Ok(new {message = "Logout efetuado com sucesso." });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }

        // 4. REFRESH TOKEN
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(RefreshTokenRequestDto request)
        {
            try {
                var result = await _authService.RefreshTokensAsync(request);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }         
        }

        // 5. RESET PASSWORD
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
        {
            try
            {
                await _authService.ResetPasswordAsync(request);
                return Ok(new {message = "Palavra-passe redefinida e enviada para o email." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }

        // 6. CHANGE PASSWORD
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                    return Unauthorized(new { message = "Utilizador não encontrado." });

                int idUser = int.Parse(userId);

                await _authService.ChangePasswordAsync(idUser, request);
                return Ok(new { message = "Palavra-passe alterada com sucesso." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }
    }
}

