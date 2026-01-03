using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjetoFinal.Models.DTOs;
using ProjetoFinal.Services.Interfaces;

namespace ProjetoFinal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [Authorize(Policy = "OnlyAdmin")]
        [HttpGet]
        public async Task<IActionResult> GetAllEmployees()
        {
            try
            {
                var funcionarios = await _employeeService.GetAllEmployeesAsync();
                return Ok(funcionarios);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }      
        }

        [Authorize(Policy = "OnlyAdmin")]
        [HttpPatch("{idFuncionario}")]
        public async Task<IActionResult> UpdateEmployee(int idFuncionario, [FromBody] UpdateEmployeeDto request)
        {
            try
            {
                if (request == null)
                    return BadRequest(new { message = "Dados de atualização inválidos." });

                var result = await _employeeService.UpdateEmployeeAsync(idFuncionario, request);
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
