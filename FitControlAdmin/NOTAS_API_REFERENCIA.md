# Notas para a equipa da API (referência – não alterar API a partir da app)

Estas notas descrevem comportamentos da API que afetam a app WPF. **A app não altera a API;** usa-se isto apenas como referência para correções no backend.

## 1. Aulas – GET por estado (500 / ciclo infinito)

- **Problema:** No Swagger, `GET /api/Class/by-state?ativo=true` devolve **500** (ex.: "ciclo infinito" ou timeout).
- **Causa provável:** O endpoint devolve a entidade `Aula` com propriedades de navegação (ex.: `Funcionario`, `AulasMarcadas`, etc.). Na serialização JSON isso pode provocar referências circulares e 500.
- **Sugestão:** Em vez de devolver `List<Aula>`, devolver um DTO (ex.: `AulaResponseDto`) sem coleções ou propriedades que gerem ciclos. Ou configurar o serializador para ignorar ciclos (menos recomendável que usar DTOs).
- **App:** A app já usa `GET /api/Class/by-state?ativo=true` (e `ativo=false`) para obter aulas. Quando o 500 for corrigido na API, as aulas devem passar a aparecer na app.

## 2. Fluxo de reservas de avaliação física (ATUALIZADO)

### Fluxo completo em 4 passos:

#### 1. Criar reserva (Membro)
- **Endpoint:** `POST /api/PhysicalEvaluationReservation/{idMembro}?dataReserva=...`
- **Policy:** `[OnlyMembers]` - Apenas membros podem criar
- **Estado inicial:** `Reservado`

#### 2. Listar reservas pendentes (PT)
- **Endpoint:** `GET /api/PhysicalEvaluationReservation/active`
- **Policy:** `[OnlyPT]`
- **Retorna:** Todas as reservas com estado `Reservado`

#### 3. Confirmar reserva (PT) - NOVO ENDPOINT
- **Endpoint:** `PATCH /api/PhysicalEvaluationReservation/confirm-reservation/{idMembro}/{idAvaliacao}`
- **Policy:** `[OnlyPT]`
- **Body:** `{ "IdFuncionario": 123 }`
- **Ação:** Muda estado de `Reservado` → `Presente`
- **NÃO cria avaliação física ainda**

#### 4. Completar avaliação física (PT)
- **Endpoint:** `PATCH /api/PhysicalEvaluationReservation/attendance/{idMembro}/{idAvaliacao}`
- **Policy:** `[OnlyPT]`
- **Body:** `MarkAttendanceDto` com Peso, Altura, IMC, etc.
- **Requisito:** Reserva deve estar no estado `Presente`
- **Ação:** Cria a `AvaliacaoFisica` e associa à reserva
- **Estado final:** `Presente` + `DataDesativacao` preenchida

### Sobre o 403:
- **403 no POST:** Autenticado como PT/Admin/Receção (correto - POST é só para membros)
- **403 no PATCH confirm-reservation:** Autenticado como Membro/Admin/Receção (correto - PATCH é só para PT)
- **403 no PATCH attendance:** Autenticado como Membro/Admin/Receção (correto - PATCH é só para PT)

### Na App WPF (PT):
1. **Botão "Reservar" (Pendentes):** Chama `confirm-reservation` → move para Completas
2. **Botão "Completar Avaliação" (Completas):** Abre janela, chama `attendance` → move para Histórico

## 3. Utilizador atual e IdFuncionario

- A app chama `GET /api/User/me` para obter o utilizador atual e usa **IdFuncionario** para confirmar reservas e abrir a janela de avaliação física (apenas PT).
- Se o endpoint `/api/User/me` não existir ou não devolver **IdFuncionario** (e dados de funcionário quando for PT), a app mostra: "Não foi possível identificar o PT. Faça logout e login novamente."
- **Sugestão:** Garantir que `GET /api/User/me` existe e que, para utilizadores do tipo Funcionário, a resposta inclui `IdFuncionario` (e restantes campos necessários ao DTO do utilizador atual).
