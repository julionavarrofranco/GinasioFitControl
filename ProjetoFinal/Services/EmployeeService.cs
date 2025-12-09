using Microsoft.EntityFrameworkCore;
using ProjetoFinal.Data;
using ProjetoFinal.Models;
using ProjetoFinal.Models.DTOs;

namespace ProjetoFinal.Services
{
    public class EmployeeService: IEmployeeService
    {
        private readonly GinasioDbContext _context;
        public EmployeeService(GinasioDbContext context)
        {
            _context = context;
        }

        public async Task<List<FuncionarioDto>> GetAllEmployeesAsync()
        {
            return await _context.Funcionarios
                .AsNoTracking()
                .Select(f => new FuncionarioDto
                {
                    IdUser = f.IdUser,
                    IdFuncionario = f.IdFuncionario,
                    Nome = f.Nome,
                    Email = f.User.Email,
                    Telemovel = f.Telemovel,
                    Funcao = f.Funcao
                })
                .ToListAsync();
        }


        public Task<Funcionario> CreateEmployeeAsync(int idUser, UserRegisterDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Funcao) ||
                !Enum.TryParse<Funcao>(request.Funcao, true, out var funcaoEnum))
                throw new InvalidOperationException("Função de funcionário inválida.");

            var funcionario = new Funcionario
            {
                IdUser = idUser,
                Nome = request.Nome,
                Telemovel = request.Telemovel,
                Funcao = funcaoEnum
            };

            _context.Funcionarios.Add(funcionario);
            return Task.FromResult(funcionario);
        }


    }
}
