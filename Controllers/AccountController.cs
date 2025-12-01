// ============================================================================
// CONTROLADOR DE CONTA - GESTÃO DE AUTENTICAÇÃO E UTILIZADORES
// Este controlador trata do login, logout e alteração de palavra-passe
// ============================================================================

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using TTFWebsite.Models;
using TTFWebsite.Services;

namespace TTFWebsite.Controllers
{
    /// <summary>
    /// Controlador responsável pela gestão de autenticação dos utilizadores.
    /// Inclui funcionalidades de login, logout e alteração de palavra-passe.
    /// </summary>
    public class AccountController : Controller
    {
        // Serviço para comunicação com a API do backend
        private readonly IApiService _apiService;
        // Serviço para descodificação de tokens JWT
        private readonly JwtService _jwtService;
        // Sistema de registo de logs para diagnóstico
        private readonly ILogger<AccountController> _logger;

        /// <summary>
        /// Construtor do controlador com injeção de dependências.
        /// Os serviços são injetados automaticamente pelo contentor de DI.
        /// </summary>
        /// <param name="apiService">Serviço de comunicação com a API</param>
        /// <param name="jwtService">Serviço de tokens JWT</param>
        /// <param name="logger">Sistema de logs</param>
        public AccountController(IApiService apiService, JwtService jwtService, ILogger<AccountController> logger)
        {
            _apiService = apiService;
            _jwtService = jwtService;
            _logger = logger;
        }

        /// <summary>
        /// Apresenta a página de login.
        /// Se o utilizador já estiver autenticado, redireciona para o dashboard.
        /// </summary>
        /// <param name="returnUrl">URL de retorno após login bem-sucedido</param>
        /// <returns>Vista de login ou redirecionamento</returns>
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // Verificar se o utilizador já está autenticado
            if (User.Identity?.IsAuthenticated == true)
            {
                // Redirecionar para o dashboard do membro
                return RedirectToAction("Dashboard", "Member");
            }
            // Guardar o URL de retorno para uso após o login
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        /// <summary>
        /// Processa o pedido de login do utilizador.
        /// Valida as credenciais através da API e cria a sessão de autenticação.
        /// </summary>
        /// <param name="model">Dados de login (email e palavra-passe)</param>
        /// <param name="returnUrl">URL de retorno após login</param>
        /// <returns>Redirecionamento ou vista com erros</returns>
        [HttpPost]
        [ValidateAntiForgeryToken] // Proteção contra ataques CSRF
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            // Verificar se os dados do formulário são válidos
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Tentar fazer login através da API do backend
            var tokenResponse = await _apiService.LoginAsync(model.Email, model.Password);

            // Verificar se o login foi bem-sucedido
            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
            {
                // Adicionar mensagem de erro ao modelo
                ModelState.AddModelError("", "Email ou palavra-passe incorretos.");
                return View(model);
            }

            // Descodificar o token JWT para extrair as informações do utilizador
            var principal = _jwtService.DecodeToken(tokenResponse.AccessToken);
            if (principal == null)
            {
                ModelState.AddModelError("", "Erro ao processar o token de autenticação.");
                return View(model);
            }

            // Converter as claims do JWT para claims de autenticação por cookies
            var claims = principal.Claims.ToList();
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            
            // Configurar as propriedades do cookie de autenticação
            var authProperties = new AuthenticationProperties
            {
                // Se "Lembrar-me" estiver ativo, o cookie persiste 30 dias; caso contrário, 8 horas
                IsPersistent = model.RememberMe,
                ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(8)
            };

            // Criar o cookie de autenticação e autenticar o utilizador
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Armazenar os tokens na sessão para renovação e chamadas à API
            HttpContext.Session.SetString("AccessToken", tokenResponse.AccessToken);
            HttpContext.Session.SetString("RefreshToken", tokenResponse.RefreshToken);

            // Verificar se o utilizador precisa de alterar a palavra-passe (primeiro login)
            if (tokenResponse.NeedsPasswordChange)
            {
                TempData["NeedsPasswordChange"] = true;
                TempData["Message"] = tokenResponse.Message ?? "Por favor, altere a sua palavra-passe.";
                // Redirecionar para a página de alteração de palavra-passe
                return RedirectToAction("ChangePassword", "Account");
            }

            // Redirecionar para o URL de retorno (se válido) ou para o dashboard
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Dashboard", "Member");
        }

        /// <summary>
        /// Termina a sessão do utilizador.
        /// Remove os cookies de autenticação e limpa a sessão.
        /// </summary>
        /// <returns>Redirecionamento para a página de login</returns>
        [HttpPost]
        [ValidateAntiForgeryToken] // Proteção contra ataques CSRF
        public async Task<IActionResult> Logout()
        {
            // Tentar fazer logout na API se existir um token de acesso
            var accessToken = HttpContext.Session.GetString("AccessToken");
            if (!string.IsNullOrEmpty(accessToken))
            {
                await _apiService.LogoutAsync(accessToken);
            }

            // Remover o cookie de autenticação
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            // Limpar todos os dados da sessão
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        /// <summary>
        /// Apresenta a página de acesso negado.
        /// Mostrada quando o utilizador não tem permissões para aceder a um recurso.
        /// </summary>
        /// <returns>Vista de acesso negado</returns>
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        /// <summary>
        /// Apresenta a página de alteração de palavra-passe.
        /// Requer que o utilizador esteja autenticado.
        /// </summary>
        /// <returns>Vista de alteração de palavra-passe</returns>
        [HttpGet]
        [Authorize] // Apenas utilizadores autenticados podem aceder
        public IActionResult ChangePassword()
        {
            return View();
        }

        /// <summary>
        /// Processa o pedido de alteração de palavra-passe.
        /// Valida a palavra-passe atual e define a nova palavra-passe.
        /// </summary>
        /// <param name="model">Dados do formulário de alteração de palavra-passe</param>
        /// <returns>Redirecionamento ou vista com erros</returns>
        [HttpPost]
        [ValidateAntiForgeryToken] // Proteção contra ataques CSRF
        [Authorize] // Apenas utilizadores autenticados podem aceder
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            // Validação adicional: verificar se a nova palavra-passe e confirmação coincidem
            if (model.NewPassword != model.ConfirmPassword)
            {
                ModelState.AddModelError(nameof(model.ConfirmPassword), "A confirmação da palavra-passe não corresponde.");
            }

            // Verificar se o modelo é válido
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Obter o token de acesso da sessão
            var accessToken = HttpContext.Session.GetString("AccessToken");
            if (string.IsNullOrEmpty(accessToken))
            {
                ModelState.AddModelError("", "Sessão expirada. Por favor, faça login novamente.");
                return View(model);
            }

            // Tentar alterar a palavra-passe através da API
            var success = await _apiService.ChangePasswordAsync(accessToken, model.CurrentPassword, model.NewPassword);

            if (!success)
            {
                ModelState.AddModelError("", "Erro ao alterar a palavra-passe. Verifique se a palavra-passe atual está correta.");
                return View(model);
            }

            // Palavra-passe alterada com sucesso
            TempData["SuccessMessage"] = "Palavra-passe alterada com sucesso. Por favor, faça login novamente.";
            // Terminar a sessão para forçar novo login com a nova palavra-passe
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }

    /// <summary>
    /// Modelo de dados para o formulário de login.
    /// Contém as credenciais do utilizador.
    /// </summary>
    public class LoginViewModel
    {
        /// <summary>
        /// Endereço de email do utilizador
        /// </summary>
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// Palavra-passe do utilizador
        /// </summary>
        public string Password { get; set; } = string.Empty;
        
        /// <summary>
        /// Indica se o utilizador quer manter a sessão ativa (cookie persistente)
        /// </summary>
        public bool RememberMe { get; set; }
    }

    /// <summary>
    /// Modelo de dados para o formulário de alteração de palavra-passe.
    /// Inclui validações para garantir a segurança da nova palavra-passe.
    /// </summary>
    public class ChangePasswordViewModel
    {
        /// <summary>
        /// Palavra-passe atual do utilizador (para verificação)
        /// </summary>
        [Required(ErrorMessage = "A palavra-passe atual é obrigatória.")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;

        /// <summary>
        /// Nova palavra-passe escolhida pelo utilizador
        /// </summary>
        [Required(ErrorMessage = "A nova palavra-passe é obrigatória.")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "A palavra-passe deve ter pelo menos 6 caracteres.")]
        public string NewPassword { get; set; } = string.Empty;

        /// <summary>
        /// Confirmação da nova palavra-passe (deve coincidir com NewPassword)
        /// </summary>
        [Required(ErrorMessage = "A confirmação da palavra-passe é obrigatória.")]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "A confirmação da palavra-passe não corresponde.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
