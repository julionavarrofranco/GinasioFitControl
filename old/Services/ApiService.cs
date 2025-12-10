using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using FitControlAdmin.Models;
using System.Text.Json;
using System.Windows;

namespace FitControlAdmin.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://localhost:7267";
        private string? _accessToken;

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

        //public async Task<TokenResponse?> LoginAsync(string email, string password)
        //{
        //    try
        //    {
        //        var loginDto = new LoginDto { Email = email, Password = password };
        //        var response = await _httpClient.PostAsJsonAsync("/api/Auth/login", loginDto);

        //        if (response.IsSuccessStatusCode)
        //        {
        //            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
        //            if (tokenResponse != null && !string.IsNullOrEmpty(tokenResponse.AccessToken))
        //            {
        //                SetToken(tokenResponse.AccessToken);
        //            }
        //            return tokenResponse;
        //        }
        //        return null;
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}


        public async Task<TokenResponse?> LoginAsync(string email, string password)
        {
            var loginDto = new LoginDto { Email = email, Password = password };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/Auth/login", loginDto);

                // ? Se a API devolveu erro, tentar ler a mensagem
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

                // Sucesso ? Ler token
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
                // ? Agora o LoginWindow consegue mostrar ex.Message
                throw new Exception(ex.Message);
            }
        }



        public async Task<List<MemberDto>?> GetUsersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/member");
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
                return null;
            }
            catch
            {
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

        private static string? ExtractMessage(string content)
        {
            try
            {
                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("message", out var messageElement) &&
                    messageElement.ValueKind == JsonValueKind.String)
                {
                    return messageElement.GetString();
                }
            }
            catch
            {
                // ignore parsing errors
            }

            return null;
        }
    }
}

