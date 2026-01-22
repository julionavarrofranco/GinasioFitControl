using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using FitControlAdmin.Models;
using System.Text.Json;
using FitControlAdmin.Helper;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace FitControlAdmin.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        // Use HTTPS to match the API's launch profile and avoid losing the Authorization header on redirects
        // Se a API estiver a correr em HTTP, mude para: "http://localhost:5295"
        private readonly string _baseUrl = "https://localhost:7267";
        private string? _accessToken;
        public string? CurrentAccessToken => _accessToken;

        public ApiService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public void SetToken(string token)
        {
            _accessToken = token;
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", token);
        }

            public async Task<TokenResponse?> LoginAsync(string email, string password)
        {
            var loginDto = new LoginDto { Email = email, Password = password };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/Auth/login", loginDto);

                // ❗ Se a API devolveu erro, tentar ler a mensagem
                if (!response.IsSuccessStatusCode)
                {
                    var errorText = await response.Content.ReadAsStringAsync();

                    // Se vier JSON do tipo { error: "mensagem" }
                    try
                    {
                        throw new Exception("Erro no login.");
                    }
                    catch
                    {
                        // Se não der parse, mostras o texto original
                        throw new Exception(errorText);
                    }
                }

                // Sucesso → Ler token
                var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
                if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
                    throw new Exception("A resposta do servidor não contém um token válido.");

                // 1. Extrair claim Tipo
                var tipo = JwtHelper.GetClaim(tokenResponse.AccessToken, "Tipo");

                // 2. BLOQUEAR membros
                if (tipo == "Membro")
                {
                    throw new Exception("Apenas funcionários podem aceder à aplicação de gestão.");
                }

                // 3. Guardar token
                SetToken(tokenResponse.AccessToken);

                return tokenResponse;
            }
            catch (Exception ex)
            {
                // ❗ Agora o LoginWindow consegue mostrar ex.Message
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<MemberDto>?> GetUsersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/Member");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<MemberDto>>();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<UserDto?> GetUserAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/User/{id}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<UserDto>();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"GetUserAsync failed: {response.StatusCode} - {errorContent}");
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetUserAsync exception: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateUserAsync(int id, UserUpdateDto updateDto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"/api/User/{id}", updateDto);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateMemberAsync(int idMembro, UpdateMemberDto updateDto)
        {
            try
            {
                var response = await _httpClient.PatchAsJsonAsync($"/api/Member/update-member/{idMembro}", updateDto);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return (true, null);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var message = ExtractMessage(errorContent);
                    return (false, message ?? $"Erro ao atualizar membro ({response.StatusCode}).");
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<List<EmployeeDto>?> GetAllEmployeesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/Employee");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<EmployeeDto>>();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateEmployeeAsync(int idFuncionario, UpdateEmployeeDto updateDto)
        {
            try
            {
                var response = await _httpClient.PatchAsJsonAsync($"/api/Employee/{idFuncionario}", updateDto);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return (true, null);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var message = ExtractMessage(errorContent);
                    return (false, message ?? $"Erro ao atualizar funcionário ({response.StatusCode}).");
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"/api/User/{id}");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ChangeUserActiveStatusAsync(int idUser, bool isActive)
        {
            try
            {
                var statusDto = new UserStatusDto
                {
                    IdUser = idUser,
                    IsActive = isActive
                };
                var response = await _httpClient.PatchAsJsonAsync("/api/User/change-active-status", statusDto);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> RegisterUserAsync(UserRegisterDto registerDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/Auth/register", registerDto);
                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var message = ExtractMessage(errorContent);
                return (false, message ?? $"Erro ao criar utilizador ({response.StatusCode}).");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<bool> LogoutAsync()
        {
            try
            {
                var response = await _httpClient.PostAsync("/api/Auth/logout", null);
                if (response.IsSuccessStatusCode)
                {
                    _accessToken = null;
                    _httpClient.DefaultRequestHeaders.Authorization = null;
                    return true;
                }
                return false;
            }
            catch
            {
                // Even if logout fails, clear local token
                _accessToken = null;
                _httpClient.DefaultRequestHeaders.Authorization = null;
                return false;
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> ResetPasswordAsync(string email)
        {
            try
            {
                var resetDto = new ResetPasswordDto { Email = email };
                var response = await _httpClient.PostAsJsonAsync("/api/Auth/reset-password", resetDto);
                
                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var message = ExtractMessage(errorContent);
                return (false, message ?? $"Erro ao redefinir palavra-passe ({response.StatusCode}).");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<UserDto?> GetCurrentUserAsync()
        {
            try
            {
                // Use the new /api/User/me endpoint instead of parsing the token
                var response = await _httpClient.GetAsync("/api/User/me");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<UserDto>();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"GetCurrentUserAsync failed: {response.StatusCode} - {errorContent}");
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetCurrentUserAsync error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"GetCurrentUserAsync stack trace: {ex.StackTrace}");
                return null;
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> ChangePasswordAsync(string passwordAtual, string novaPassword)
        {
            try
            {
                var changePasswordDto = new ChangePasswordDto 
                { 
                    PasswordAtual = passwordAtual, 
                    NovaPassword = novaPassword 
                };
                
                System.Diagnostics.Debug.WriteLine($"ChangePasswordAsync: Attempting to change password (new password length: {novaPassword?.Length ?? 0})");
                
                var response = await _httpClient.PatchAsJsonAsync("/api/User/change-password", changePasswordDto);
                
                if (response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine("ChangePasswordAsync: Password changed successfully");
                    return (true, null);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"ChangePasswordAsync: Error response ({response.StatusCode}): {errorContent}");
                var message = ExtractMessage(errorContent);
                return (false, message ?? $"Erro ao alterar palavra-passe ({response.StatusCode}).");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ChangePasswordAsync: Exception: {ex.Message}");
                return (false, ex.Message);
            }
        }

        private static string? ExtractMessage(string content)
        {
            try
            {
                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;
                
                // Try "message" first
                if (root.TryGetProperty("message", out var messageElement) &&
                    messageElement.ValueKind == JsonValueKind.String)
                {
                    return messageElement.GetString();
                }
                
                // Try "error" as fallback
                if (root.TryGetProperty("error", out var errorElement) &&
                    errorElement.ValueKind == JsonValueKind.String)
                {
                    return errorElement.GetString();
                }
            }
            catch
            {
                // If parsing fails, return the raw content (might be plain text error)
                if (!string.IsNullOrWhiteSpace(content) && content.Length < 500)
                {
                    return content;
                }
            }

            return null;
        }

        #region Exercise Methods

        public async Task<(bool Success, string? ErrorMessage, ExerciseResponseDto? Exercise)> CreateExerciseAsync(ExerciseDto exerciseDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/Exercise", exerciseDto);
                if (response.IsSuccessStatusCode)
                {
                    var exercise = await response.Content.ReadFromJsonAsync<ExerciseResponseDto>();
                    return (true, null, exercise);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var message = ExtractMessage(errorContent);
                return (false, message ?? $"Erro ao criar exercício ({response.StatusCode}).", null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateExerciseAsync(int idExercicio, UpdateExerciseDto updateDto)
        {
            try
            {
                var response = await _httpClient.PatchAsJsonAsync($"/api/Exercise/update-exercise/{idExercicio}", updateDto);
                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var message = ExtractMessage(errorContent);
                return (false, message ?? $"Erro ao atualizar exercício ({response.StatusCode}).");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> ChangeExerciseStatusAsync(int idExercicio, bool ativo)
        {
            try
            {
                var response = await _httpClient.PatchAsync($"/api/Exercise/change-active-status/{idExercicio}?ativo={ativo}", null);
                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var message = ExtractMessage(errorContent);
                return (false, message ?? $"Erro ao alterar estado do exercício ({response.StatusCode}).");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<List<ExerciseResponseDto>?> GetExercisesByStateAsync(bool ativo, bool ordenarAsc = true)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/Exercise/by-state?ativo={ativo}&ordenarAsc={ordenarAsc}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<ExerciseResponseDto>>();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<ExerciseResponseDto>?> GetExercisesByMuscleGroupAsync(GrupoMuscular grupo, bool ordenarAsc = true)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/Exercise/by-muscle-group/{grupo}?ordenarAsc={ordenarAsc}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<ExerciseResponseDto>>();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<ExerciseResponseDto>?> GetExercisesByNameAsync(string nome, bool ordenarAsc = true)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/Exercise/by-name?nome={Uri.EscapeDataString(nome)}&ordenarAsc={ordenarAsc}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<ExerciseResponseDto>>();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Payment Methods

        public async Task<(bool Success, string? ErrorMessage, PaymentResponseDto? Payment)> CreatePaymentAsync(PaymentDto paymentDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/Payment", paymentDto);
                if (response.IsSuccessStatusCode)
                {
                    var payment = await response.Content.ReadFromJsonAsync<PaymentResponseDto>();
                    return (true, null, payment);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var message = ExtractMessage(errorContent);
                
                // If no message extracted, try to get the full error content for debugging
                if (string.IsNullOrEmpty(message))
                {
                    message = errorContent.Length > 500 ? errorContent.Substring(0, 500) + "..." : errorContent;
                }
                
                return (false, message ?? $"Erro ao criar pagamento ({response.StatusCode}).", null);
            }
            catch (Exception ex)
            {
                return (false, $"Exceção: {ex.Message}\n\nStack: {ex.StackTrace}", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdatePaymentAsync(int idPagamento, UpdatePaymentDto updateDto)
        {
            try
            {
                // Debug: Log the request data
                System.Diagnostics.Debug.WriteLine($"UpdatePaymentAsync: Updating payment {idPagamento}");
                System.Diagnostics.Debug.WriteLine($"UpdatePaymentAsync: MetodoPagamento={updateDto.MetodoPagamento}, EstadoPagamento={updateDto.EstadoPagamento}");

                var response = await _httpClient.PatchAsJsonAsync($"/api/Payment/update-payment/{idPagamento}", updateDto);

                // Debug: Log response details
                System.Diagnostics.Debug.WriteLine($"UpdatePaymentAsync: Response status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine("UpdatePaymentAsync: Update successful");
                    return (true, null);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"UpdatePaymentAsync: Error content: {errorContent}");

                var message = ExtractMessage(errorContent);
                return (false, message ?? $"Erro ao atualizar pagamento ({response.StatusCode}).");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdatePaymentAsync: Exception: {ex.Message}");
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> ChangePaymentStatusAsync(int idPagamento, bool ativo)
        {
            try
            {
                var response = await _httpClient.PatchAsync($"/api/Payment/change-active-status/{idPagamento}?ativo={ativo}", null);
                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var message = ExtractMessage(errorContent);
                return (false, message ?? $"Erro ao alterar estado do pagamento ({response.StatusCode}).");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<List<PaymentResponseDto>?> GetPaymentsByActiveStateAsync(bool ativo)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/Payment/by-state?ativo={ativo}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<PaymentResponseDto>>();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<PaymentResponseDto>?> GetPaymentsByDateAsync(DateTime inicio, DateTime fim)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/Payment/by-date?inicio={inicio:yyyy-MM-dd}&fim={fim:yyyy-MM-dd}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<PaymentResponseDto>>();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<PaymentResponseDto>?> GetPaymentsByPaymentStateAsync(EstadoPagamento estado)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/Payment/by-state-payment?estado={estado}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<PaymentResponseDto>>();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<(bool Success, string? ErrorMessage, decimal? Revenue)> GetMonthlyRevenueAsync(int ano, int mes)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/Payment/monthly-revenue?ano={ano}&mes={mes}");
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                    if (result.TryGetProperty("receita", out var receitaElement))
                    {
                        var receita = receitaElement.GetDecimal();
                        return (true, null, receita);
                    }
                    return (false, "Resposta inválida do servidor.", null);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var message = ExtractMessage(errorContent);
                return (false, message ?? $"Erro ao obter receita mensal ({response.StatusCode}).", null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        #endregion

        #region Physical Evaluation Methods

        public async Task<(bool Success, string? ErrorMessage, PhysicalEvaluationResponseDto? Evaluation)> CreatePhysicalEvaluationAsync(PhysicalEvaluationDto evaluationDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/PhysicalEvaluation", evaluationDto);
                if (response.IsSuccessStatusCode)
                {
                    var evaluation = await response.Content.ReadFromJsonAsync<PhysicalEvaluationResponseDto>();
                    return (true, null, evaluation);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var message = ExtractMessage(errorContent);
                return (false, message ?? $"Erro ao criar avaliação física ({response.StatusCode}).", null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdatePhysicalEvaluationAsync(int idAvaliacao, UpdatePhysicalEvaluationDto updateDto)
        {
            try
            {
                var response = await _httpClient.PatchAsJsonAsync($"/api/PhysicalEvaluation/update/{idAvaliacao}", updateDto);
                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var message = ExtractMessage(errorContent);
                return (false, message ?? $"Erro ao atualizar avaliação física ({response.StatusCode}).");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> ChangePhysicalEvaluationStatusAsync(int idAvaliacao, bool ativo)
        {
            try
            {
                var response = await _httpClient.PatchAsync($"/api/PhysicalEvaluation/change-active-status/{idAvaliacao}?ativo={ativo}", null);
                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var message = ExtractMessage(errorContent);
                return (false, message ?? $"Erro ao alterar estado da avaliação física ({response.StatusCode}).");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<List<PhysicalEvaluationResponseDto>?> GetAllEvaluationsForMemberAsync(int idMembro)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/PhysicalEvaluation/member/evaluations/{idMembro}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<PhysicalEvaluationResponseDto>>();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<PhysicalEvaluationResponseDto?> GetLatestEvaluationForMemberAsync(int idMembro)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/PhysicalEvaluation/member/latest-evaluation/{idMembro}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<PhysicalEvaluationResponseDto>();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Physical Evaluation Reservation Methods

        public async Task<(bool Success, string? ErrorMessage, PhysicalEvaluationReservationResponseDto? Reservation)> CreateReservationAsync(int idMembro, DateTime dataReserva)
        {
            try
            {
                var response = await _httpClient.PostAsync($"/api/PhysicalEvaluationReservation/{idMembro}?dataReserva={dataReserva:yyyy-MM-ddTHH:mm:ss}", null);
                if (response.IsSuccessStatusCode)
                {
                    var rawReservation = await response.Content.ReadFromJsonAsync<MembroAvaliacao>();
                    if (rawReservation != null)
                    {
                        var reservation = new PhysicalEvaluationReservationResponseDto
                        {
                            IdAvaliacao = rawReservation.IdMembroAvaliacao,
                            IdMembro = rawReservation.IdMembro,
                            IdFuncionario = null,
                            DataAvaliacao = rawReservation.DataReserva,
                            Estado = rawReservation.Estado.ToString(),
                            NomeMembro = rawReservation.Membro?.Nome,
                            NomeFuncionario = null
                        };
                        return (true, null, reservation);
                    }
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var message = ExtractMessage(errorContent);
                return (false, message ?? $"Erro ao criar reserva ({response.StatusCode}).", null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> CancelReservationAsync(int idMembro, int idAvaliacao)
        {
            try
            {
                var response = await _httpClient.PatchAsync($"/api/PhysicalEvaluationReservation/cancel/{idMembro}/{idAvaliacao}", null);
                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var message = ExtractMessage(errorContent);
                return (false, message ?? $"Erro ao cancelar reserva ({response.StatusCode}).");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> MarkAttendanceAsync(int idMembro, int idAvaliacao, MarkAttendanceDto markAttendanceDto)
        {
            try
            {
                var response = await _httpClient.PatchAsJsonAsync($"/api/PhysicalEvaluationReservation/attendance/{idMembro}/{idAvaliacao}", markAttendanceDto);
                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var message = ExtractMessage(errorContent);
                return (false, message ?? $"Erro ao marcar presença ({response.StatusCode}).");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<List<PhysicalEvaluationReservationResponseDto>?> GetActiveReservationsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/PhysicalEvaluationReservation/active");
                if (response.IsSuccessStatusCode)
                {
                    var rawReservations = await response.Content.ReadFromJsonAsync<List<MembroAvaliacao>>();
                    if (rawReservations != null)
                    {
                        return rawReservations.Select(r => new PhysicalEvaluationReservationResponseDto
                        {
                            IdAvaliacao = r.IdMembroAvaliacao,
                            IdMembro = r.IdMembro,
                            IdFuncionario = null, // Active reservations don't have assigned PT yet
                            DataAvaliacao = r.DataReserva,
                            Estado = r.Estado.ToString(),
                            NomeMembro = r.Membro?.Nome,
                            NomeFuncionario = null
                        }).ToList();
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<PhysicalEvaluationReservationResponseDto>?> GetCompletedReservationsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/PhysicalEvaluationReservation/completed");
                if (response.IsSuccessStatusCode)
                {
                    var rawReservations = await response.Content.ReadFromJsonAsync<List<MembroAvaliacao>>();
                    if (rawReservations != null)
                    {
                        return rawReservations.Select(r => new PhysicalEvaluationReservationResponseDto
                        {
                            IdAvaliacao = r.IdMembroAvaliacao,
                            IdMembro = r.IdMembro,
                            IdFuncionario = r.AvaliacaoFisica?.IdFuncionario,
                            DataAvaliacao = r.DataReserva,
                            Estado = r.Estado.ToString(),
                            NomeMembro = r.Membro?.Nome,
                            NomeFuncionario = r.AvaliacaoFisica?.Funcionario?.Nome
                        }).ToList();
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Member Methods

        public async Task<List<MemberDto>?> GetAllMembersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/Member");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<MemberDto>>();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Subscription Methods

        public async Task<List<SubscriptionResponseDto>?> GetSubscriptionsByStateAsync(bool ativo, bool ordenarNomeAsc = true)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/Subscription/by-state?ativo={ativo}&ordenarNomeAsc={ordenarNomeAsc}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<SubscriptionResponseDto>>();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }
}
