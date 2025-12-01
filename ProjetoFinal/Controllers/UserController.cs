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
    public class UserController : ControllerBase
    {
        private readonly GinasioDbContext _context;

        public UserController(GinasioDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Policy = "CanRegisterUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _context.Users
                    .Include(u => u.Membro)
                    .Include(u => u.Funcionario)
                    .Select(u => new UserListDto
                    {
                        IdUser = u.IdUser,
                        Email = u.Email,
                        Tipo = u.Tipo.ToString(),
                        Nome = u.Tipo == Tipo.Membro ? u.Membro!.Nome : u.Funcionario!.Nome,
                        Telemovel = u.Tipo == Tipo.Membro ? u.Membro!.Telemovel : u.Funcionario!.Telemovel,
                        Ativo = u.Ativo,
                        Funcao = u.Funcionario != null ? u.Funcionario.Funcao.ToString() : null,
                        IdSubscricao = u.Membro != null ? u.Membro.IdSubscricao : null,
                        DataNascimento = u.Membro != null ? u.Membro.DataNascimento : null
                    })
                    .ToListAsync();

                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao listar utilizadores.", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "CanRegisterUsers")]
        public async Task<IActionResult> GetUser(int id)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Membro)
                    .Include(u => u.Funcionario)
                    .FirstOrDefaultAsync(u => u.IdUser == id);

                if (user == null)
                    return NotFound(new { message = "Utilizador não encontrado." });

                var userDto = new UserListDto
                {
                    IdUser = user.IdUser,
                    Email = user.Email,
                    Tipo = user.Tipo.ToString(),
                    Nome = user.Tipo == Tipo.Membro ? user.Membro!.Nome : user.Funcionario!.Nome,
                    Telemovel = user.Tipo == Tipo.Membro ? user.Membro!.Telemovel : user.Funcionario!.Telemovel,
                    Ativo = user.Ativo,
                    Funcao = user.Funcionario != null ? user.Funcionario.Funcao.ToString() : null,
                    IdSubscricao = user.Membro != null ? user.Membro.IdSubscricao : null,
                    DataNascimento = user.Membro != null ? user.Membro.DataNascimento : null
                };

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao obter utilizador.", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "CanRegisterUsers")]
        public async Task<IActionResult> UpdateUser(int id, UserUpdateDto updateDto)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Membro)
                    .Include(u => u.Funcionario)
                    .FirstOrDefaultAsync(u => u.IdUser == id);

                if (user == null)
                    return NotFound(new { message = "Utilizador não encontrado." });

                if (user.Tipo == Tipo.Membro && user.Membro != null)
                {
                    if (!string.IsNullOrEmpty(updateDto.Nome))
                        user.Membro.Nome = updateDto.Nome;
                    if (!string.IsNullOrEmpty(updateDto.Telemovel))
                        user.Membro.Telemovel = updateDto.Telemovel;
                    if (updateDto.DataNascimento.HasValue)
                        user.Membro.DataNascimento = updateDto.DataNascimento.Value;
                    if (updateDto.IdSubscricao.HasValue)
                        user.Membro.IdSubscricao = updateDto.IdSubscricao.Value;
                }
                else if (user.Tipo == Tipo.Funcionario && user.Funcionario != null)
                {
                    if (!string.IsNullOrEmpty(updateDto.Nome))
                        user.Funcionario.Nome = updateDto.Nome;
                    if (!string.IsNullOrEmpty(updateDto.Telemovel))
                        user.Funcionario.Telemovel = updateDto.Telemovel;
                    if (!string.IsNullOrEmpty(updateDto.Funcao) && 
                        Enum.TryParse<Funcao>(updateDto.Funcao, true, out var funcao))
                    {
                        user.Funcionario.Funcao = funcao;
                    }
                }

                if (updateDto.Ativo.HasValue)
                {
                    user.Ativo = updateDto.Ativo.Value;
                    if (!updateDto.Ativo.Value)
                    {
                        if (user.Tipo == Tipo.Membro && user.Membro != null)
                            user.Membro.DataDesativacao = DateTime.UtcNow;
                        else if (user.Tipo == Tipo.Funcionario && user.Funcionario != null)
                            user.Funcionario.DataDesativacao = DateTime.UtcNow;
                    }
                    else
                    {
                        if (user.Tipo == Tipo.Membro && user.Membro != null)
                            user.Membro.DataDesativacao = null;
                        else if (user.Tipo == Tipo.Funcionario && user.Funcionario != null)
                            user.Funcionario.DataDesativacao = null;
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "Utilizador atualizado com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao atualizar utilizador.", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "CanRegisterUsers")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Membro)
                    .Include(u => u.Funcionario)
                    .FirstOrDefaultAsync(u => u.IdUser == id);

                if (user == null)
                    return NotFound(new { message = "Utilizador não encontrado." });

                user.Ativo = false;
                if (user.Tipo == Tipo.Membro && user.Membro != null)
                    user.Membro.DataDesativacao = DateTime.UtcNow;
                else if (user.Tipo == Tipo.Funcionario && user.Funcionario != null)
                    user.Funcionario.DataDesativacao = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return Ok(new { message = "Utilizador desativado com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao desativar utilizador.", error = ex.Message });
            }
        }
    }
}

