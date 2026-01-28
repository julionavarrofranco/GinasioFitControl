using Microsoft.AspNetCore.Mvc;
using TTFWebsite.Models;

namespace TTFWebsite.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var model = new HomeViewModel
            {
                Gyms = GetGyms(),
                Benefits = GetBenefits(),
                Plans = GetPlans(),
                Testimonials = GetTestimonials()
            };
            return View(model);
        }

        private List<Gym> GetGyms()
        {
            return new List<Gym>
            {
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

        private List<Benefit> GetBenefits()
        {
            return new List<Benefit>
            {
                new Benefit { Id = 1, Title = "Horário Alargado", Description = "Treina às 6 da manhã, à hora do almoço ou ao final do dia. Tu escolhes quando.", IconClass = "icon-horario" },
                new Benefit { Id = 2, Title = "Sem Fidelização", Description = "Na FitControl não há fidelizações obrigatórias. Pagas quando treinas.", IconClass = "icon-fidelizacao" },
                new Benefit { Id = 3, Title = "Treino com PT", Description = "Personal Trainers disponíveis para esclarecer dúvidas e treinos personalizados.", IconClass = "icon-acompanhamento" },
                new Benefit { Id = 4, Title = "Espaço para Mulheres", Description = "Espaço dedicado às mulheres, pensado para treinares com conforto e privacidade.", IconClass = "icon-mulheres" },
                new Benefit { Id = 5, Title = "Equipamentos Premium", Description = "Treina com equipamentos topo de gama das melhores marcas internacionais.", IconClass = "icon-equipamentos-premium" },
                new Benefit { Id = 6, Title = "Planos Acessíveis", Description = "A partir de 27€/mês, com acesso total e sem surpresas na mensalidade.", IconClass = "icon-planos-acessiveis" }
            };
        }

        private List<Plan> GetPlans()
        {
            return new List<Plan>
            {
                new Plan
                {
                    Id = 1,
                    Name = "FitControl Plus",
                    Price = 35,
                    Description = "Plano completo com tudo incluído",
                    Features = new List<string>
                    {
                        "Débito Direto",
                        "Plano geral de treino",
                        "Plano Geral De Alimentação"
                    }
                },
                new Plan
                {
                    Id = 2,
                    Name = "FitControl GO",
                    Price = 45,
                    Description = "Plano flexível sem débito direto",
                    Features = new List<string>
                    {
                        "Sem Débito Direto",
                        "Plano geral de treino",
                        "Plano Geral De Alimentação"
                    },
                    IsPopular = true
                }
            };
        }

        private List<Testimonial> GetTestimonials()
        {
            return new List<Testimonial>
            {
                new Testimonial { Id = 1, Name = "André", Comment = "Achei que ia só experimentar... agora venho todos os dias." },
                new Testimonial { Id = 2, Name = "Carla", Comment = "Sinto-me à vontade. Dá para treinar sem stress." },
                new Testimonial { Id = 3, Name = "Pedro", Comment = "O ambiente é tranquilo, os horários ajudam. E o staff não está sempre em cima da gente." },
                new Testimonial { Id = 4, Name = "Inês", Comment = "O pessoal é fixe, ninguém está a julgar ninguém. E é daqueles sítios que dá vontade de voltar." },
                new Testimonial { Id = 5, Name = "Neto", Comment = "Consigo treinar ao meu ritmo, sem sentir pressão. E está sempre tudo limpo, o que é raro." },
                new Testimonial { Id = 6, Name = "Filipa", Comment = "Descobri o FitControl porque uma amiga me trouxe. Vim fazer um treino grátis e passado uns tempos fiquei." }
            };
        }
    }

    public class HomeViewModel
    {
        public List<Gym> Gyms { get; set; } = new();
        public List<Benefit> Benefits { get; set; } = new();
        public List<Plan> Plans { get; set; } = new();
        public List<Testimonial> Testimonials { get; set; } = new();
    }
}
