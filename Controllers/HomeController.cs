// ============================================================================
// CONTROLADOR HOME - PÁGINA PRINCIPAL DO WEBSITE
// Este controlador gere a página inicial pública do website FitControl
// ============================================================================

using Microsoft.AspNetCore.Mvc;
using TTFWebsite.Models;

namespace TTFWebsite.Controllers
{
    /// <summary>
    /// Controlador responsável pela página inicial do website.
    /// Apresenta informações sobre ginásios, benefícios, planos e testemunhos.
    /// Esta é uma página pública acessível a todos os visitantes.
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Ação principal que apresenta a página inicial do website.
        /// Carrega todos os dados necessários para apresentar a página.
        /// </summary>
        /// <returns>Vista da página inicial com modelo de dados completo</returns>
        public IActionResult Index()
        {
            // Criar o modelo de dados da página inicial
            // com todas as informações necessárias
            var model = new HomeViewModel
            {
                Gyms = GetGyms(),              // Lista de ginásios disponíveis
                Benefits = GetBenefits(),       // Lista de benefícios do ginásio
                Plans = GetPlans(),             // Lista de planos disponíveis
                Testimonials = GetTestimonials() // Lista de testemunhos de clientes
            };
            return View(model);
        }

        /// <summary>
        /// Obtém a lista de ginásios FitControl disponíveis.
        /// Em produção, estes dados viriam da base de dados ou API.
        /// </summary>
        /// <returns>Lista de ginásios com as suas localizações</returns>
        private List<Gym> GetGyms()
        {
            return new List<Gym>
            {
                // Lista de ginásios FitControl na zona de Lisboa e arredores
                new Gym { Id = 1, Name = "FitControl Almada", City = "Almada" },
                new Gym { Id = 2, Name = "FitControl Amadora", City = "Amadora" },
                new Gym { Id = 3, Name = "FitControl Cascais", City = "Cascais" },
                new Gym { Id = 4, Name = "FitControl Lisboa", City = "Lisboa" },
                new Gym { Id = 5, Name = "FitControl Loures", City = "Loures" },
                new Gym { Id = 6, Name = "FitControl Mafra", City = "Mafra" },
                new Gym { Id = 7, Name = "FitControl Oeiras", City = "Oeiras" },
                new Gym { Id = 8, Name = "FitControl Seixal", City = "Seixal" },
                new Gym { Id = 9, Name = "FitControl Setúbal", City = "Setúbal" },
                new Gym { Id = 10, Name = "FitControl Sintra", City = "Sintra" }
            };
        }

        /// <summary>
        /// Obtém a lista de benefícios oferecidos pelo ginásio.
        /// Cada benefício inclui título, descrição e classe de ícone.
        /// </summary>
        /// <returns>Lista de benefícios para apresentar na página</returns>
        private List<Benefit> GetBenefits()
        {
            return new List<Benefit>
            {
                // Benefício: Flexibilidade de horários
                new Benefit { 
                    Id = 1, 
                    Title = "Horário Alargado", 
                    Description = "Treina às 6 da manhã, à hora do almoço ou ao final do dia. Tu escolhes quando.", 
                    IconClass = "icon-horario" 
                },
                // Benefício: Sem compromissos de longa duração
                new Benefit { 
                    Id = 2, 
                    Title = "Sem Fidelização", 
                    Description = "Na FitControl não há fidelizações obrigatórias. Pagas quando treinas.", 
                    IconClass = "icon-fidelizacao" 
                },
                // Benefício: Acompanhamento profissional
                new Benefit { 
                    Id = 3, 
                    Title = "Treino com PT", 
                    Description = "Personal Trainers disponíveis para esclarecer dúvidas e treinos personalizados.", 
                    IconClass = "icon-acompanhamento" 
                },
                // Benefício: Área exclusiva feminina
                new Benefit { 
                    Id = 4, 
                    Title = "Espaço para Mulheres", 
                    Description = "Espaço dedicado às mulheres, pensado para treinares com conforto e privacidade.", 
                    IconClass = "icon-mulheres" 
                },
                // Benefício: Equipamentos de qualidade
                new Benefit { 
                    Id = 5, 
                    Title = "Equipamentos Premium", 
                    Description = "Treina com equipamentos topo de gama das melhores marcas internacionais.", 
                    IconClass = "icon-equipamentos-premium" 
                },
                // Benefício: Preços acessíveis
                new Benefit { 
                    Id = 6, 
                    Title = "Planos Acessíveis", 
                    Description = "A partir de 27€/mês, com acesso total e sem surpresas na mensalidade.", 
                    IconClass = "icon-planos-acessiveis" 
                }
            };
        }

        /// <summary>
        /// Obtém a lista de planos de subscrição disponíveis.
        /// Cada plano inclui preço, descrição e lista de funcionalidades.
        /// </summary>
        /// <returns>Lista de planos para apresentar na secção de preços</returns>
        private List<Plan> GetPlans()
        {
            return new List<Plan>
            {
                // Plano básico com débito direto
                new Plan
                {
                    Id = 1,
                    Name = "FitControl Plus",
                    Price = 35,
                    Description = "Plano completo com tudo incluído",
                    Features = new List<string>
                    {
                        "Débito Direto",          // Pagamento automático mensal
                        "Plano geral de treino",  // Plano de exercícios incluído
                        "Plano Geral De Alimentação" // Plano alimentar incluído
                    }
                },
                // Plano flexível sem débito direto (mais popular)
                new Plan
                {
                    Id = 2,
                    Name = "FitControl GO",
                    Price = 45,
                    Description = "Plano flexível sem débito direto",
                    Features = new List<string>
                    {
                        "Sem Débito Direto",      // Pagamento manual mensal
                        "Plano geral de treino",  // Plano de exercícios incluído
                        "Plano Geral De Alimentação" // Plano alimentar incluído
                    },
                    IsPopular = true // Marcado como o plano mais popular
                }
            };
        }

        /// <summary>
        /// Obtém a lista de testemunhos de clientes satisfeitos.
        /// Os testemunhos ajudam a criar confiança nos potenciais clientes.
        /// </summary>
        /// <returns>Lista de testemunhos para apresentar na página</returns>
        private List<Testimonial> GetTestimonials()
        {
            return new List<Testimonial>
            {
                // Testemunhos reais de membros do ginásio
                new Testimonial { 
                    Id = 1, 
                    Name = "André", 
                    Comment = "Achei que ia só experimentar... agora venho todos os dias." 
                },
                new Testimonial { 
                    Id = 2, 
                    Name = "Carla", 
                    Comment = "Sinto-me à vontade. Dá para treinar sem stress." 
                },
                new Testimonial { 
                    Id = 3, 
                    Name = "Pedro", 
                    Comment = "O ambiente é tranquilo, os horários ajudam. E o staff não está sempre em cima da gente." 
                },
                new Testimonial { 
                    Id = 4, 
                    Name = "Inês", 
                    Comment = "O pessoal é fixe, ninguém está a julgar ninguém. E é daqueles sítios que dá vontade de voltar." 
                },
                new Testimonial { 
                    Id = 5, 
                    Name = "Neto", 
                    Comment = "Consigo treinar ao meu ritmo, sem sentir pressão. E está sempre tudo limpo, o que é raro." 
                },
                new Testimonial { 
                    Id = 6, 
                    Name = "Filipa", 
                    Comment = "Descobri o FitControl porque uma amiga me trouxe. Vim fazer um treino grátis e passado uns tempos fiquei." 
                }
            };
        }
    }

    /// <summary>
    /// Modelo de dados para a página inicial.
    /// Agrupa todas as listas de dados necessárias para a vista.
    /// </summary>
    public class HomeViewModel
    {
        /// <summary>
        /// Lista de ginásios disponíveis para apresentar no mapa
        /// </summary>
        public List<Gym> Gyms { get; set; } = new();
        
        /// <summary>
        /// Lista de benefícios do ginásio
        /// </summary>
        public List<Benefit> Benefits { get; set; } = new();
        
        /// <summary>
        /// Lista de planos de subscrição disponíveis
        /// </summary>
        public List<Plan> Plans { get; set; } = new();
        
        /// <summary>
        /// Lista de testemunhos de clientes
        /// </summary>
        public List<Testimonial> Testimonials { get; set; } = new();
    }
}
