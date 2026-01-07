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
    }
}

