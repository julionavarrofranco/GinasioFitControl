using ProjetoFinal.Models;
using ProjetoFinal.Models.DTOs;

namespace ProjetoFinal.Services.Interfaces
{
    public interface IEmployeeService
    {
        Task<List<EmployeeDto>> GetAllEmployeesAsync();

        Task<Funcionario> CreateEmployeeAsync(int idUser, UserRegisterDto request);

        Task<string> UpdateEmployeeAsync(int idFuncionario, UpdateEmployeeDto request);


    }
}
