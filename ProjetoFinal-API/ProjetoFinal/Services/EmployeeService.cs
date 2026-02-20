using Microsoft.EntityFrameworkCore;
using ProjetoFinal.Data;
using ProjetoFinal.Models;
using ProjetoFinal.Models.DTOs;
using ProjetoFinal.Services.Interfaces;
using System.Text.RegularExpressions;

namespace ProjetoFinal.Services
{
    public class EmployeeService: IEmployeeService
    {
        private readonly GinasioDbContext _context;
        public EmployeeService(GinasioDbContext context)
        {
            _context = context;
        }

        // Retorna lista de funcionários com dados básicos (sem incluir dados sensíveis do User)
        public async Task<List<EmployeeDto>> GetAllEmployeesAsync()
        {
            return await _context.Funcionarios
                .AsNoTracking()
                .Select(f => new EmployeeDto
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

        // Cria um novo funcionário
        public Task<Funcionario> CreateEmployeeAsync(int idUser, UserRegisterDto request)
        {
            ValidateEmployeeAsync(request.Nome, request.Telemovel, request.Funcao, false);

            Enum.TryParse<Funcao>(request.Funcao, true, out var funcaoEnum);

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

        // Atualiza os dados do funcionário 
        public async Task<string> UpdateEmployeeAsync(int idFuncionario, UpdateEmployeeDto request)
        {
            var funcionario = await _context.Funcionarios
                .FirstOrDefaultAsync(f => f.IdFuncionario == idFuncionario);

            if (funcionario == null)
                throw new KeyNotFoundException("Funcionário não encontrado.");

            bool alterado = false;

            // Validação
            ValidateEmployeeAsync(request.Nome, request.Telemovel, request.Funcao, true);

            // Atualização de campos
            if (!string.IsNullOrWhiteSpace(request.Nome) && request.Nome != funcionario.Nome)
            {
                funcionario.Nome = request.Nome;
                alterado = true;
            }

            if (!string.IsNullOrWhiteSpace(request.Telemovel) && request.Telemovel != funcionario.Telemovel)
            {
                funcionario.Telemovel = request.Telemovel;
                alterado = true;
            }

            if (!string.IsNullOrWhiteSpace(request.Funcao) &&
                Enum.TryParse<Funcao>(request.Funcao, true, out var funcaoEnum) &&
                funcaoEnum != funcionario.Funcao)
            {
                funcionario.Funcao = funcaoEnum;
                alterado = true;
            }

            if (alterado)
                await _context.SaveChangesAsync();

            return alterado ? "Funcionário atualizado com sucesso." : "Nenhuma alteração realizada.";
        }

        // Validação dos dados do funcionário
        private void ValidateEmployeeAsync(string? nome, string? telemovel, string? funcaoStr, bool isUpdate)
        {
            if (!isUpdate)
            {
                if (string.IsNullOrWhiteSpace(nome))
                    throw new InvalidOperationException("O nome do funcionário não pode estar vazio.");

                if (string.IsNullOrWhiteSpace(funcaoStr) ||
                    !Enum.TryParse<Funcao>(funcaoStr, true, out var _))
                    throw new InvalidOperationException("Função de funcionário inválida.");
            }

            if (!string.IsNullOrWhiteSpace(telemovel))
            {
                var phoneRegex = new Regex(@"^\+\d{7,15}$");
                if (!phoneRegex.IsMatch(telemovel))
                    throw new InvalidOperationException("Por favor, insira um Nº de telemóvel válido.");
            }
        }


    }
}
