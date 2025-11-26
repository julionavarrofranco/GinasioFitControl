using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoFinal.Data;
using ProjetoFinal.Models.DTOs;
using ProjetoFinal.Services;

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
        [Authorize(Roles = "Funcionario")]
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto request)
        {
            try
            {
                var email = User.Identity?.Name;
                if (string.IsNullOrWhiteSpace(email))
                    return Unauthorized("Usuário não autenticado.");

                var currentUser = await _context.Users
                                                .Include(u => u.Funcionario)
                                                .FirstOrDefaultAsync(u => u.Email == email);
                if (currentUser == null)
                    return Unauthorized("Usuário não encontrado.");

                var user = await _authService.RegisterAsync(request, currentUser);

                return Ok(new { message = "Usuário registado com sucesso." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erro interno: " + ex.Message);
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
                return Unauthorized("credenciais inválidas"); // retorna 401 com a mensagem
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erro interno: " + ex.Message); // outros erros inesperados
            }
        }


        // 3. REFRESH TOKEN
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(RefreshTokenRequestDto request)
        {
            try {
                var result = await _authService.RefreshTokensAsync(request);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erro interno: " + ex.Message);
            }         
        }


        // 4. LOGOUT
        [Authorize]
        [HttpPost("logout/{idUser}")]
        public async Task<IActionResult> Logout(int idUser)
        {
            try
            {
                await _authService.LogoutAsync(idUser);
                return Ok(new { message = "Logout efetuado com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erro interno: " + ex.Message);
            }
        }


        // 5. RESET PASSWORD
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] string email)
        {
            try
            {
                await _authService.ResetPasswordAsync(email);
                return Ok(new { message = "Senha redefinida e enviada por email." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erro interno: " + ex.Message);
            }
        }


        // 6. CHANGE PASSWORD
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto request)
        {
            try
            {
                await _authService.ChangePasswordAsync(request);
                return Ok(new { message = "Senha alterada com sucesso." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erro interno: " + ex.Message);
            }
        }


        // 7. ROTATE REFRESH TOKEN
        [Authorize]
        [HttpPost("rotate")]
        public async Task<IActionResult> Rotate(RefreshTokenRequestDto request)
        {
            try
            {
                var newToken = await _authService.RotateRefreshTokenAsync(request.IdUser, request.RefreshToken);
                return Ok(new { RefreshToken = newToken }); //retorna o novo refresh token com JSON "refreshToken" = novo token
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erro interno: " + ex.Message);
            }
        }
    }
}

