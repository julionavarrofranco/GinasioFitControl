using Microsoft.EntityFrameworkCore;
using ProjetoFinal.Data;
using ProjetoFinal.Models;
using ProjetoFinal.Models.DTOs;
using ProjetoFinal.Services.Interfaces;

namespace ProjetoFinal.Services
{
    public class SubscriptionService: ISubscriptionService
    {
        private readonly GinasioDbContext _context;

        public SubscriptionService(GinasioDbContext context)
        {
            _context = context;
        }

        private async Task<Subscricao?> GetSubscriptionByIdAsync(int idSubscricao)
        {
            return await _context.Subscricoes
                .FirstOrDefaultAsync(s => s.IdSubscricao == idSubscricao);
        }

        //Criar subscrição
        public async Task<Subscricao> CreateSubscriptionAsync(SubscriptionDto request)
        {
            if (request == null)
                throw new InvalidOperationException("Dados da subscrição inválidos.");

            if (string.IsNullOrWhiteSpace(request.Tipo))
                throw new InvalidOperationException("O tipo da subscrição é obrigatório.");

            if (!Enum.TryParse<TipoSubscricao>(request.Tipo, true, out var tipoEnum))
                throw new InvalidOperationException("Tipo da subscrição inválido.");

            ValidateSubscription(request.Nome, tipoEnum, request.Preco, request.Descricao, false);

            Subscricao subscricao = new Subscricao
            {
                Nome = request.Nome,
                Tipo = tipoEnum,
                Preco = request.Preco,
                Descricao = request.Descricao
            };

            _context.Subscricoes.Add(subscricao);
            await _context.SaveChangesAsync();

            return subscricao;
        }

        //Atualizar dados da subcrição
        public async Task<string> UpdateSubscriptionAsync(int idSubscricao, UpdateSubscriptionDto request)
        {
            var subscricao = await GetSubscriptionByIdAsync(idSubscricao);

            if (subscricao == null)
                throw new KeyNotFoundException("Subscrição não encontrada.");

            ValidateSubscription(request.Nome, request.Tipo, request.Preco, request.Descricao, true);

            bool alterado = false;

            if (!string.IsNullOrWhiteSpace(request.Nome) && request.Nome != subscricao.Nome)
            {
                subscricao.Nome = request.Nome;
                alterado = true;
            }

            if (request.Tipo.HasValue && request.Tipo.Value != subscricao.Tipo)
            {
                subscricao.Tipo = request.Tipo.Value;
                alterado = true;
            }

            if (request.Preco.HasValue && request.Preco.Value != subscricao.Preco)
            {
                subscricao.Preco = request.Preco.Value;
                alterado = true;
            }

            if (request.Descricao != subscricao.Descricao)
            {
                subscricao.Descricao = request.Descricao;
                alterado = true;
            }

            if (alterado)
            {
                await _context.SaveChangesAsync();
                return "Subscrição atualizada com sucesso.";
            }

            return "Nenhuma alteração realizada.";
        }

        //Ativar ou desativar subscrição
        public async Task ChangeSubscriptionActiveStatusAsync(int idSubscricao, bool ativo)
        {
            var subscricao = await GetSubscriptionByIdAsync(idSubscricao);
            if (subscricao == null)
                throw new KeyNotFoundException("Subscrição não encontrada.");

            if (subscricao.Ativo != ativo)
            {
                subscricao.Ativo = ativo;
                await _context.SaveChangesAsync();
            }
        }

        //Listar subscrições por estado (ativo/inativo)
        public async Task<List<SubscriptionDto>> GetSubscriptionsByStateAsync(bool ativo, bool ordenarNomeAsc = true, bool? ordenarPrecoAsc = null) // null = não ordenar por preço
        {
            IQueryable<Subscricao> query = _context.Subscricoes
                .AsNoTracking()
                .Where(s => s.Ativo == ativo);

            IOrderedQueryable<Subscricao> orderedQuery;
            if (ordenarNomeAsc)
            {
                orderedQuery = query.OrderBy(s => s.Nome);
            }
            else
            {
                orderedQuery = query.OrderByDescending(s => s.Nome);
            }

            if (ordenarPrecoAsc.HasValue)
            {
                if (ordenarPrecoAsc.Value)
                {
                    orderedQuery = orderedQuery.ThenBy(s => s.Preco);
                }
                else
                {
                    orderedQuery = orderedQuery.ThenByDescending(s => s.Preco);
                }
            }

            var subscricoes = await orderedQuery.ToListAsync();

            return subscricoes
                .Select(s => new SubscriptionDto
                {
                    IdSubscricao = s.IdSubscricao,
                    Nome = s.Nome,
                    Descricao = s.Descricao,
                    Preco = s.Preco,
                    Tipo = s.Tipo.ToString()
                })
                .ToList();
        }

        //Listar subscrições por tipo
        public async Task<List<Subscricao>> GetSubscriptionsByTypeAsync(TipoSubscricao tipo, bool ordenarNomeAsc = true, bool? ordenarPrecoAsc = null)
        {
            IQueryable<Subscricao> query = _context.Subscricoes
                .AsNoTracking()
                .Where(s => s.Tipo == tipo);

            IOrderedQueryable<Subscricao> orderedQuery;
            if (ordenarNomeAsc)
            {
                orderedQuery = query.OrderBy(s => s.Nome);
            }
            else
            {
                orderedQuery = query.OrderByDescending(s => s.Nome);
            }

            if (ordenarPrecoAsc.HasValue)
            {
                if (ordenarPrecoAsc.Value)
                {
                    orderedQuery = orderedQuery.ThenBy(s => s.Preco);
                }
                else
                {
                    orderedQuery = orderedQuery.ThenByDescending(s => s.Preco);
                }
            }

            return await orderedQuery.ToListAsync();
        }

        //Listar subscrições por nome (busca parcial - like)
        public async Task<List<Subscricao>> GetSubscriptionsByNameAsync(string nome, bool ordenarNomeAsc = true, bool? ordenarPrecoAsc = null)
        {
            IQueryable<Subscricao> query = _context.Subscricoes
                .AsNoTracking()
                .Where(s => EF.Functions.Like(s.Nome, $"%{nome}%"));

            IOrderedQueryable<Subscricao> orderedQuery;
            if (ordenarNomeAsc)
            {
                orderedQuery = query.OrderBy(s => s.Nome);
            }
            else
            {
                orderedQuery = query.OrderByDescending(s => s.Nome);
            }

            if (ordenarPrecoAsc.HasValue)
            {
                if (ordenarPrecoAsc.Value)
                {
                    orderedQuery = orderedQuery.ThenBy(s => s.Preco);
                }
                else
                {
                    orderedQuery = orderedQuery.ThenByDescending(s => s.Preco);
                }
            }

            return await orderedQuery.ToListAsync();
        }

        //método para validar dados da subscrição
        private void ValidateSubscription(string? nome, TipoSubscricao? tipo, decimal? preco, string? descricao, bool isUpdate)
        {
            if (!isUpdate)
            {
                if (string.IsNullOrWhiteSpace(nome))
                    throw new InvalidOperationException("O nome da subscrição é obrigatório.");

                if (!tipo.HasValue)
                    throw new InvalidOperationException("O tipo da subscrição é obrigatório.");

                if (!preco.HasValue || preco <= 0)
                    throw new InvalidOperationException("O preço da subscrição deve ser superior a zero.");
            }

            if (preco.HasValue && preco < 0)
                throw new InvalidOperationException("O preço não pode ser negativo.");

        }
    }
}
