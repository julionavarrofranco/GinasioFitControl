using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoFinal.Data;
using ProjetoFinal.Models;
using ProjetoFinal.Models.DTOs;
using System.Security.Claims;

namespace ProjetoFinal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MembersController : ControllerBase
    {
        private readonly GinasioDbContext _context;

        public MembersController(GinasioDbContext context)
        {
            _context = context;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim))
                return Unauthorized(new { message = "Utilizador não autenticado." });

            if (!int.TryParse(userIdClaim, out var userId))
                return BadRequest(new { message = "Identificador de utilizador inválido." });

            var user = await _context.Users
                                     .Include(u => u.Membro)
                                     .ThenInclude(m => m!.Subscricao)
                                     .FirstOrDefaultAsync(u => u.IdUser == userId);

            if (user == null)
                return NotFound(new { message = "Utilizador não encontrado." });

            if (user.Membro == null)
                return BadRequest(new { message = "Utilizador não é membro." });

            var member = user.Membro;
            var dto = new MemberProfileDto
            {
                IdUser = user.IdUser,
                Email = user.Email,
                Name = member.Nome,
                Phone = member.Telemovel,
                BirthDate = member.DataNascimento,
                MembershipNumber = $"FC{member.IdMembro:0000}",
                Gym = member.Subscricao?.Descricao ?? string.Empty,
                Plan = member.Subscricao?.Tipo.ToString() ?? "Sem Plano",
                MembershipStartDate = member.DataRegisto
            };

            return Ok(dto);
        }
    }
}

