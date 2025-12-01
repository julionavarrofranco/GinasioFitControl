// ============================================================================
// CONTROLADOR DE MEMBROS - ÁREA PRIVADA DOS MEMBROS DO GINÁSIO
// Este controlador gere todas as funcionalidades exclusivas para membros
// ============================================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using TTFWebsite.Models;

namespace TTFWebsite.Controllers
{
    /// <summary>
    /// Controlador responsável pela área de membros do ginásio.
    /// Requer autenticação para acesso a todas as ações.
    /// Inclui: dashboard, perfil, aulas, avaliações físicas e reservas.
    /// </summary>
    [Authorize] // Todas as ações requerem utilizador autenticado
    public class MemberController : Controller
    {
        // Fábrica para criar clientes HTTP configurados
        private readonly IHttpClientFactory _httpClientFactory;
        // Sistema de registo de logs para diagnóstico
        private readonly ILogger<MemberController> _logger;
        // Opções de serialização JSON (case-insensitive para propriedades)
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Construtor do controlador com injeção de dependências.
        /// </summary>
        /// <param name="httpClientFactory">Fábrica de clientes HTTP</param>
        /// <param name="logger">Sistema de logs</param>
        public MemberController(IHttpClientFactory httpClientFactory, ILogger<MemberController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        /// <summary>
        /// Obtém a lista de avaliações físicas do utilizador.
        /// NOTA: Em produção, estes dados viriam da base de dados.
        /// </summary>
        /// <param name="userId">Identificador do utilizador</param>
        /// <returns>Lista de avaliações físicas ordenadas por data</returns>
        private List<PhysicalAssessment> GetPhysicalAssessments(int userId)
        {
            // Dados simulados para demonstração
            return new List<PhysicalAssessment>
            {
                // Avaliação mais recente (há 1 mês)
                new PhysicalAssessment
                {
                    Id = 1,
                    UserId = userId,
                    AssessmentDate = DateTime.Now.AddMonths(-1),
                    Weight = 75.5m,        // Peso em kg
                    Height = 1.80m,        // Altura em metros
                    BodyFat = 18.5m,       // Percentagem de gordura corporal
                    MuscleMass = 61.5m,    // Massa muscular em kg
                    BMI = 23.3m,           // Índice de Massa Corporal
                    Notes = "Bom progresso. Manter treino de força.",
                    TrainerName = "Carlos Mendes"
                },
                // Avaliação inicial (há 3 meses)
                new PhysicalAssessment
                {
                    Id = 2,
                    UserId = userId,
                    AssessmentDate = DateTime.Now.AddMonths(-3),
                    Weight = 78.2m,
                    Height = 1.80m,
                    BodyFat = 20.1m,
                    MuscleMass = 62.5m,
                    BMI = 24.1m,
                    Notes = "Primeira avaliação. Objetivo: reduzir gordura corporal.",
                    TrainerName = "Carlos Mendes"
                }
            };
        }

        /// <summary>
        /// Gera a lista de aulas disponíveis para os próximos 7 dias.
        /// NOTA: Em produção, estes dados viriam da base de dados.
        /// </summary>
        /// <param name="gym">Nome do ginásio do membro</param>
        /// <returns>Lista de aulas disponíveis com horários e vagas</returns>
        private List<Class> GetAvailableClasses(string gym)
        {
            var today = DateTime.Today;
            var classes = new List<Class>();

            // Gerar aulas para os próximos 7 dias
            for (int i = 0; i < 7; i++)
            {
                var date = today.AddDays(i);
                
                // === AULAS DA MANHÃ ===
                
                // Aula de Pilates às 9h
                classes.Add(new Class
                {
                    Id = classes.Count + 1,
                    Name = "Pilates",
                    Description = "Aula de pilates para fortalecimento e flexibilidade",
                    Instructor = "Ana Costa",
                    StartTime = date.AddHours(9),
                    EndTime = date.AddHours(10),
                    MaxCapacity = 20,         // Capacidade máxima
                    CurrentBookings = 12,     // Lugares já reservados
                    Gym = gym,
                    Room = "Sala 1"
                });

                // Aula de Yoga às 10h
                classes.Add(new Class
                {
                    Id = classes.Count + 1,
                    Name = "Yoga",
                    Description = "Aula de yoga para relaxamento e flexibilidade",
                    Instructor = "Sofia Martins",
                    StartTime = date.AddHours(10),
                    EndTime = date.AddHours(11),
                    MaxCapacity = 25,
                    CurrentBookings = 18,
                    Gym = gym,
                    Room = "Sala 2"
                });

                // === AULAS DA TARDE ===
                
                // Aula de HIIT às 18h
                classes.Add(new Class
                {
                    Id = classes.Count + 1,
                    Name = "HIIT",
                    Description = "Treino intervalado de alta intensidade",
                    Instructor = "Pedro Alves",
                    StartTime = date.AddHours(18),
                    EndTime = date.AddHours(19),
                    MaxCapacity = 30,
                    CurrentBookings = 25,
                    Gym = gym,
                    Room = "Sala Principal"
                });

                // Aula de Spinning às 19h
                classes.Add(new Class
                {
                    Id = classes.Count + 1,
                    Name = "Spinning",
                    Description = "Aula de ciclismo indoor",
                    Instructor = "Rita Silva",
                    StartTime = date.AddHours(19),
                    EndTime = date.AddHours(20),
                    MaxCapacity = 25,
                    CurrentBookings = 20,
                    Gym = gym,
                    Room = "Sala Spinning"
                });
            }

            return classes;
        }

        /// <summary>
        /// Obtém a lista de reservas do utilizador.
        /// NOTA: Em produção, estes dados viriam da base de dados.
        /// </summary>
        /// <param name="userId">Identificador do utilizador</param>
        /// <returns>Lista de reservas ativas do utilizador</returns>
        private List<Reservation> GetUserReservations(int userId)
        {
            return new List<Reservation>
            {
                // Reserva para amanhã às 9h
                new Reservation
                {
                    Id = 1,
                    UserId = userId,
                    ClassId = 1,
                    ReservationDate = DateTime.Today.AddDays(1).AddHours(9),
                    CreatedAt = DateTime.Now.AddDays(-2),
                    IsCancelled = false
                },
                // Reserva para depois de amanhã às 18h
                new Reservation
                {
                    Id = 2,
                    UserId = userId,
                    ClassId = 5,
                    ReservationDate = DateTime.Today.AddDays(2).AddHours(18),
                    CreatedAt = DateTime.Now.AddDays(-1),
                    IsCancelled = false
                }
            };
        }

        /// <summary>
        /// Obtém os dados do utilizador a partir das claims de autenticação.
        /// Combina informações do token JWT com dados simulados.
        /// </summary>
        /// <param name="userId">Identificador do utilizador</param>
        /// <returns>Objeto User com os dados do utilizador</returns>
        private User GetUser(int userId)
        {
            // Extrair dados do utilizador das claims do token JWT
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                        ?? User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
                        ?? "";
            var name = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? email;
            var phone = User.Claims.FirstOrDefault(c => c.Type == "Telemovel")?.Value ?? "912345678";
            var gym = User.Claims.FirstOrDefault(c => c.Type == "Gym")?.Value ?? "";
            var membershipNumber = User.Claims.FirstOrDefault(c => c.Type == "MembershipNumber")?.Value ?? "";

            return new User
            {
                Id = userId,
                Email = email,
                Name = name,
                Phone = phone,
                BirthDate = new DateTime(1990, 5, 15), // Data simulada
                Gym = gym,
                MembershipNumber = membershipNumber,
                MembershipStartDate = new DateTime(2024, 1, 1), // Data simulada
                Plan = "FitControl GO"
            };
        }

        /// <summary>
        /// Apresenta o dashboard principal do membro.
        /// Inclui: próximas reservas, última avaliação física e aulas de hoje.
        /// </summary>
        /// <returns>Vista do dashboard com modelo de dados completo</returns>
        public async Task<IActionResult> Dashboard()
        {
            // Tentar obter o perfil do membro através da API
            var profile = await GetProfileFromApiAsync();
            MemberProfileViewModel profileViewModel;
            int userId;
            
            if (profile != null)
            {
                // Usar dados da API
                profileViewModel = profile;
                userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "1");
            }
            else
            {
                // Fallback: usar dados simulados caso a API falhe
                userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "1");
                var fallbackUser = GetUser(userId);
                profileViewModel = new MemberProfileViewModel
                {
                    Name = fallbackUser.Name,
                    Email = fallbackUser.Email,
                    Phone = fallbackUser.Phone,
                    BirthDate = fallbackUser.BirthDate,
                    MembershipNumber = fallbackUser.MembershipNumber,
                    Gym = fallbackUser.Gym,
                    Plan = fallbackUser.Plan,
                    MembershipStartDate = fallbackUser.MembershipStartDate
                };
            }

            // Construir o modelo do dashboard
            var model = new DashboardViewModel
            {
                Profile = profileViewModel,
                // Próximas 3 reservas não canceladas
                UpcomingReservations = GetUserReservations(userId)
                    .Where(r => !r.IsCancelled && r.ReservationDate >= DateTime.Now)
                    .Take(3)
                    .ToList(),
                // Última avaliação física
                RecentAssessment = GetPhysicalAssessments(userId)
                    .OrderByDescending(a => a.AssessmentDate)
                    .FirstOrDefault(),
                // Aulas disponíveis hoje
                AvailableClassesToday = GetAvailableClasses(profileViewModel.Gym)
                    .Where(c => c.StartTime.Date == DateTime.Today)
                    .ToList()
            };

            return View(model);
        }

        /// <summary>
        /// Apresenta a página de perfil do membro.
        /// Mostra todas as informações pessoais e de associação.
        /// </summary>
        /// <returns>Vista do perfil com dados do membro</returns>
        public async Task<IActionResult> Profile()
        {
            // Tentar obter o perfil através da API
            var profile = await GetProfileFromApiAsync();
            if (profile != null)
                return View(profile);

            // Fallback: usar dados simulados caso a API falhe
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "1");
            var fallback = GetUser(userId);
            var fallbackViewModel = new MemberProfileViewModel
            {
                Name = fallback.Name,
                Email = fallback.Email,
                Phone = fallback.Phone,
                BirthDate = fallback.BirthDate,
                MembershipNumber = fallback.MembershipNumber,
                Gym = fallback.Gym,
                Plan = fallback.Plan,
                MembershipStartDate = fallback.MembershipStartDate
            };
            return View(fallbackViewModel);
        }

        /// <summary>
        /// Apresenta o histórico de avaliações físicas do membro.
        /// Mostra todas as avaliações ordenadas da mais recente para a mais antiga.
        /// </summary>
        /// <returns>Vista com lista de avaliações físicas</returns>
        public IActionResult PhysicalAssessment()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "1");
            var assessments = GetPhysicalAssessments(userId)
                .OrderByDescending(a => a.AssessmentDate)
                .ToList();
            return View(assessments);
        }

        /// <summary>
        /// Apresenta a página de aulas com lista de aulas disponíveis e reservas.
        /// Permite ao membro ver o calendário de aulas e fazer reservas.
        /// </summary>
        /// <returns>Vista com aulas disponíveis e reservas do utilizador</returns>
        public IActionResult Classes()
        {
            var gym = User.Claims.FirstOrDefault(c => c.Type == "Gym")?.Value ?? "";
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "1");
            
            var model = new ClassesViewModel
            {
                AvailableClasses = GetAvailableClasses(gym),
                // Apenas reservas futuras não canceladas
                UserReservations = GetUserReservations(userId)
                    .Where(r => !r.IsCancelled && r.ReservationDate >= DateTime.Now)
                    .ToList()
            };

            return View(model);
        }

        /// <summary>
        /// Processa o pedido de reserva de uma aula.
        /// Recebe o pedido via AJAX e retorna resultado em JSON.
        /// </summary>
        /// <param name="request">Dados da reserva (ID da aula)</param>
        /// <returns>Resultado da operação em formato JSON</returns>
        [HttpPost]
        public IActionResult BookClass([FromBody] BookClassRequest request)
        {
            // Validar os dados do pedido
            if (request == null || request.ClassId <= 0)
            {
                return Json(new { success = false, message = "Dados inválidos." });
            }

            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "1");
            
            // NOTA: Em produção, guardaria a reserva na base de dados
            // e verificaria se há vagas disponíveis
            return Json(new { success = true, message = "Aula reservada com sucesso!" });
        }

        /// <summary>
        /// Processa o cancelamento de uma reserva.
        /// Recebe o pedido via AJAX e retorna resultado em JSON.
        /// </summary>
        /// <param name="request">Dados do cancelamento (ID da reserva)</param>
        /// <returns>Resultado da operação em formato JSON</returns>
        [HttpPost]
        public IActionResult CancelReservation([FromBody] CancelReservationRequest request)
        {
            // Validar os dados do pedido
            if (request == null || request.ReservationId <= 0)
            {
                return Json(new { success = false, message = "Dados inválidos." });
            }

            // NOTA: Em produção, atualizaria o estado da reserva na base de dados
            return Json(new { success = true, message = "Reserva cancelada com sucesso!" });
        }

        /// <summary>
        /// Obtém o perfil do membro através da API do backend.
        /// Usa o token de acesso armazenado na sessão para autenticação.
        /// </summary>
        /// <returns>Perfil do membro ou null em caso de erro</returns>
        private async Task<MemberProfileViewModel?> GetProfileFromApiAsync()
        {
            // Obter o token de acesso da sessão
            var token = HttpContext.Session.GetString("AccessToken");
            if (string.IsNullOrEmpty(token))
                return null;

            try
            {
                // Criar cliente HTTP configurado para a API FitControl
                var client = _httpClientFactory.CreateClient("FitControlApi");
                var request = new HttpRequestMessage(HttpMethod.Get, "api/Members/me");
                // Adicionar token de autorização Bearer
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Enviar pedido e processar resposta
                var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Falha ao obter perfil do membro: {StatusCode}", response.StatusCode);
                    return null;
                }

                // Desserializar a resposta para o modelo
                var dto = await response.Content.ReadFromJsonAsync<MemberProfileViewModel>(_jsonOptions);
                return dto;
            }
            catch (Exception ex)
            {
                // Registar erro e retornar null
                _logger.LogError(ex, "Erro ao chamar API de perfil do membro.");
                return null;
            }
        }
    }

    /// <summary>
    /// Modelo de dados para pedido de reserva de aula via AJAX.
    /// </summary>
    public class BookClassRequest
    {
        /// <summary>
        /// Identificador da aula a reservar
        /// </summary>
        public int ClassId { get; set; }
    }

    /// <summary>
    /// Modelo de dados para pedido de cancelamento de reserva via AJAX.
    /// </summary>
    public class CancelReservationRequest
    {
        /// <summary>
        /// Identificador da reserva a cancelar
        /// </summary>
        public int ReservationId { get; set; }
    }

    /// <summary>
    /// Modelo de dados para a vista do dashboard.
    /// Agrupa todas as informações apresentadas no painel principal.
    /// </summary>
    public class DashboardViewModel
    {
        /// <summary>
        /// Perfil do membro com informações pessoais
        /// </summary>
        public MemberProfileViewModel Profile { get; set; } = new();
        
        /// <summary>
        /// Lista das próximas reservas do membro
        /// </summary>
        public List<Reservation> UpcomingReservations { get; set; } = new();
        
        /// <summary>
        /// Última avaliação física do membro (pode ser null)
        /// </summary>
        public PhysicalAssessment? RecentAssessment { get; set; }
        
        /// <summary>
        /// Lista de aulas disponíveis hoje
        /// </summary>
        public List<Class> AvailableClassesToday { get; set; } = new();
    }

    /// <summary>
    /// Modelo de dados para a vista de aulas.
    /// Inclui aulas disponíveis e reservas do utilizador.
    /// </summary>
    public class ClassesViewModel
    {
        /// <summary>
        /// Lista de todas as aulas disponíveis
        /// </summary>
        public List<Class> AvailableClasses { get; set; } = new();
        
        /// <summary>
        /// Lista das reservas do utilizador atual
        /// </summary>
        public List<Reservation> UserReservations { get; set; } = new();
    }
}
