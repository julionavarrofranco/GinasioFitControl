using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjetoFinal.Models;
using ProjetoFinal.Models.DTOs;
using ProjetoFinal.Services.Interfaces;
using System.Security.Claims;

namespace ProjetoFinal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // 1. RESET PASSWORD
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
        {
            try
            {
                await _userService.ResetPasswordAsync(request);
                return Ok(new { message = "Palavra-passe redefinida e enviada para o email." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }

        // 2. CHANGE PASSWORD
        [Authorize]
        [HttpPatch("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
        {
            try
            {
                var idUserClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (idUserClaim == null)
                    return NotFound(new { message = "Utilizador não encontrado." });

                int idUser = int.Parse(idUserClaim);
                await _userService.ChangePasswordAsync(idUser, request);
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
            catch (KeyNotFoundException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }

        // 3. CHANGE EMAIL
        [Authorize(Policy = "CanManageUsers")]
        [HttpPatch("change-email")]
        public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailDto request)
        {
            try
            {
                var (currentUserId, currentUserTipo, currentUserFuncao) = GetCurrentUserFromClaims();

                var targetUser = await _userService.GetUserByIdAsync(request.IdUser);
                if (targetUser == null)
                    return NotFound(new { message = "Utilizador não encontrado." });

                if (!CanRececaoModify(currentUserFuncao, targetUser.Tipo))
                    return Unauthorized(new { message = "Funcionário da receção só pode alterar emails de membros." });

                await _userService.ChangeEmailAsync(request);

                return Ok(new { message = "Email atualizado com sucesso." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }

        // 4. GET CURRENT USER (me)
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var idUserClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (idUserClaim == null)
                    return Unauthorized(new { message = "Utilizador não autenticado." });

                int idUser = int.Parse(idUserClaim);
                var user = await _userService.GetUserByIdAsync(idUser, includeFuncionario: true, includeMembro: true);

                if (user == null)
                    return NotFound(new { message = "Utilizador não encontrado." });

                var response = new UserResponseDto
                {
                    IdUser = user.IdUser,
                    Email = user.Email,
                    Tipo = user.Tipo.ToString(),
                    Ativo = user.Ativo
                };

                // Preencher dados do funcionário se existir
                if (user.Funcionario != null)
                {
                    response.IdFuncionario = user.Funcionario.IdFuncionario;
                    response.NomeFuncionario = user.Funcionario.Nome;
                    response.EmailFuncionario = user.Email;
                    response.TelemovelFuncionario = user.Funcionario.Telemovel;
                    response.FuncaoFuncionario = user.Funcionario.Funcao;
                    response.Funcao = user.Funcionario.Funcao.ToString();
                    response.Nome = user.Funcionario.Nome;
                    response.Telemovel = user.Funcionario.Telemovel;
                }

                // Preencher dados do membro se existir
                if (user.Membro != null)
                {
                    response.IdMembro = user.Membro.IdMembro;
                    response.Nome = user.Membro.Nome;
                    response.Telemovel = user.Membro.Telemovel;
                    response.DataNascimento = user.Membro.DataNascimento;
                    response.IdSubscricao = user.Membro.IdSubscricao;
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erro interno do servidor: {ex.Message}" });
            }
        }

        // 5. CHANGE ACTIVE STATUS
        [Authorize(Policy = "CanManageUsers")]
        [HttpPatch("change-active-status")]
        public async Task<IActionResult> ChangeUserStatus([FromBody] UserStatusDto request)
        {
            try
            {
                var (currentUserId, currentUserTipo, currentUserFuncao) = GetCurrentUserFromClaims();

                var targetUser = await _userService.GetUserByIdAsync(request.IdUser);
                if (targetUser == null)
                    return NotFound(new { message = "Utilizador não encontrado." });

                if (!CanRececaoModify(currentUserFuncao, targetUser.Tipo))
                    return Unauthorized(new { message = "Funcionário da receção só pode mudar o estado de membros." });

                await _userService.ChangeUserActiveStatusAsync(request);

                return Ok(new { message = "Estado do utilizador atualizado com sucesso." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }

        //Métodos auxiliares
        private (int userId, Tipo tipo, Funcao? funcao) GetCurrentUserFromClaims()
        {
            var idUserClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userTipoClaim = User.FindFirst("Tipo")?.Value;
            var userFuncaoClaim = User.FindFirst("Funcao")?.Value;

            if (idUserClaim == null || userTipoClaim == null)
                throw new UnauthorizedAccessException("Utilizador não autenticado.");

            int userId = int.Parse(idUserClaim);
            Tipo tipo = Enum.Parse<Tipo>(userTipoClaim);
            Funcao? funcao = null;

            if (!string.IsNullOrEmpty(userFuncaoClaim) && Enum.TryParse<Funcao>(userFuncaoClaim, out Funcao parsedFuncao))
                funcao = parsedFuncao;

            return (userId, tipo, funcao);
        }

        private bool CanRececaoModify(Funcao? currentUserFuncao, Tipo targetUserTipo)
        {
            // Receção só pode modificar membros
            if (currentUserFuncao == Funcao.Rececao && targetUserTipo != Tipo.Membro)
                return false;

            return true;
        }
    }
}