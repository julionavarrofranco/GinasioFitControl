// =====================================================
// INSTRUÇÕES PARA IMPLEMENTAR O ENDPOINT GET /api/Class
// =====================================================
//
// 1. LOCALIZAR O CONTROLLER DA API
// Vá para o projeto da API (provavelmente FitControlAPI ou similar)
// Encontre o arquivo: Controllers/ClassController.cs
//
// 2. ADICIONAR ESTE MÉTODO NO CONTROLLER:
//
// [HttpGet]
// public async Task<IActionResult> GetAllClasses()
// {
//     try
//     {
//         var aulas = await _context.Aulas
//             .Include(a => a.Funcionario)
//             .ToListAsync();
//
//         var response = aulas.Select(a => new AulaResponseDto
//         {
//             IdAula = a.IdAula,
//             Nome = a.Nome,
//             DiaSemana = a.DiaSemana,
//             HoraInicio = a.HoraInicio,
//             HoraFim = a.HoraFim,
//             Capacidade = a.Capacidade,
//             IdFuncionario = a.IdFuncionario,
//             Funcionario = a.Funcionario != null ? new FuncionarioDto
//             {
//                 IdFuncionario = a.Funcionario.IdFuncionario,
//                 Nome = a.Funcionario.Nome,
//                 Email = a.Funcionario.Email,
//                 Telemovel = a.Funcionario.Telemovel,
//                 Funcao = a.Funcionario.Funcao
//             } : null
//         }).ToList();
//
//         return Ok(response);
//     }
//     catch (Exception ex)
//     {
//         return StatusCode(500, $"Erro interno: {ex.Message}");
//     }
// }
//
// 3. VERIFICAR SE OS MODELOS EXISTEM:
//
// AulaResponseDto - deve ter as propriedades:
// - int IdAula
// - string Nome
// - DiaSemana DiaSemana
// - TimeSpan HoraInicio
// - TimeSpan HoraFim
// - int Capacidade
// - int? IdFuncionario
// - FuncionarioDto? Funcionario
//
// FuncionarioDto - deve ter:
// - int IdFuncionario
// - string Nome
// - string Email
// - string Telemovel
// - Funcao Funcao
//
// 4. VERIFICAR SE O CONTEXTO TEM A TABELA:
//
// No ApplicationDbContext ou FitControlContext, deve ter:
// public DbSet<Aula> Aulas { get; set; }
//
// E o modelo Aula deve ter navegação:
// public Funcionario? Funcionario { get; set; }
//
// 5. TESTAR:
//
// Após implementar, reinicie a API e teste no frontend.
// Os logs devem mostrar:
// GetAllClassesAsync: Retrieved X classes from API using endpoint /api/Class
//
// =====================================================