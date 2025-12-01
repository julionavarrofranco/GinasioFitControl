using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using FitControlAdmin.Models;
using System.Text.Json;

namespace FitControlAdmin.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "http://localhost:5295";
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

        public async Task<TokenResponse?> LoginAsync(string email, string password)
        {
            try
            {
                var loginDto = new LoginDto { Email = email, Password = password };
                var response = await _httpClient.PostAsJsonAsync("/api/Auth/login", loginDto);

                if (response.IsSuccessStatusCode)
                {
                    var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
                    if (tokenResponse != null && !string.IsNullOrEmpty(tokenResponse.AccessToken))
                    {
                        SetToken(tokenResponse.AccessToken);
                    }
                    return tokenResponse;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<UserDto>?> GetUsersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/User");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<UserDto>>();
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

