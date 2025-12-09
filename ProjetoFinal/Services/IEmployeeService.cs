using ProjetoFinal.Models;
using ProjetoFinal.Models.DTOs;

namespace ProjetoFinal.Services
{
    public interface IEmployeeService
    {
        Task<List<FuncionarioDto>> GetAllEmployeesAsync();

        Task<Funcionario> CreateEmployeeAsync(int idUser, UserRegisterDto request);
    }
}
