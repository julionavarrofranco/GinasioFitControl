using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjetoFinal.Services;

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
    }
}
