using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjetoFinal.Models.DTOs;
using ProjetoFinal.Services;

namespace ProjetoFinal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemberController : ControllerBase
    {
        private readonly IMemberService _memberService;

        public MemberController(IMemberService memberService)
        {
            _memberService = memberService;
        }

        [Authorize(Policy = "CanManageUsers")]
        [HttpGet]
        public async Task<IActionResult> GetAllMembros()
        {
            try
            {
                var membros = await _memberService.GetAllMembersAsync();
                return Ok(membros);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }

        }

        [Authorize(Policy = "CanManageUsers")]
        [HttpPatch("update-member/{idMembro}")]
        public async Task<IActionResult> UpdateMember(int idMembro, [FromBody] UpdateMemberDto request)
        {
            if (request == null)
                return BadRequest(new { message = "Dados de atualização inválidos." });

            try
            {
                var result = await _memberService.UpdateMemberAsync(idMembro, request);
                return Ok(new { message = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
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
