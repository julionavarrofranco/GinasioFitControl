using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjetoFinal.Models;
using ProjetoFinal.Models.DTOs;
using ProjetoFinal.Services;
using System.Security.Claims;

namespace ProjetoFinal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IAuthService authService, IUserService userService)
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
                var (currentUserId, currentUserTipo, currentUserFuncao) = GetCurrentUser();

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

        // 4. CHANGE ACTIVE STATUS
        [Authorize(Policy = "CanManageUsers")]
        [HttpPatch("change-active-status")]
        public async Task<IActionResult> ChangeActiveStatus([FromBody] UserStatusDto request)
        {
            try
            {
                var (currentUserId, currentUserTipo, currentUserFuncao) = GetCurrentUser();

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
        private (int userId, Tipo tipo, Funcao? funcao) GetCurrentUser()
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