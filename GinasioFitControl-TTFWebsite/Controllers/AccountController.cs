using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using TTFWebsite.Models;
using TTFWebsite.Services;

namespace TTFWebsite.Controllers
{
    public class AccountController : Controller
    {
        private readonly IApiService _apiService;
        private readonly JwtService _jwtService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IApiService apiService, JwtService jwtService, ILogger<AccountController> logger)
        {
            _apiService = apiService;
            _jwtService = jwtService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Dashboard", "Member");
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            var tokenResponse = await _apiService.LoginAsync(model.Email, model.Password);

            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
            {
                ModelState.AddModelError("", "Email ou palavra-passe incorretos.");
                return View(model);
            }

            // Decodificar JWT
            var principal = _jwtService.DecodeToken(tokenResponse.AccessToken);
            if (principal == null)
            {
                ModelState.AddModelError("", "Erro ao processar o token de autenticação.");
                return View(model);
            }

            // Criar claims e adicionar o token JWT como claim
            var claims = principal.Claims.ToList();
            claims.Add(new Claim("jwt", tokenResponse.AccessToken)); // <-- JWT guardado aqui
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties
            );

            // Guardar refresh token na sessão
            HttpContext.Session.SetString("RefreshToken", tokenResponse.RefreshToken);
            
            // Guardar se precisa alterar password na sessão (para verificação posterior)
            HttpContext.Session.SetString("NeedsPasswordChange", tokenResponse.NeedsPasswordChange.ToString());
            
            _logger.LogInformation("AccessToken saved to claims, length: {Length}", tokenResponse.AccessToken.Length);

            // Apenas membros podem usar a área de membros: a API coloca a claim "Tipo" no JWT (Membro / Funcionario)
            var tipoClaim = principal.FindFirst("Tipo")?.Value;
            if (!string.Equals(tipoClaim, "Membro", StringComparison.OrdinalIgnoreCase))
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                HttpContext.Session.Clear();
                ModelState.AddModelError("", "Apenas membros podem aceder a esta área. Utilize o painel de administração com as suas credenciais de administrador.");
                return View(model);
            }

            // Se precisa alterar password
            if (tokenResponse.NeedsPasswordChange)
            {
                TempData["NeedsPasswordChange"] = true;
                TempData["Message"] = tokenResponse.Message ?? "Por favor, altere a sua palavra-passe.";
                return RedirectToAction("ChangePassword", "Account");
            }

            return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? Redirect(returnUrl)
                : RedirectToAction("Dashboard", "Member");
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Pegar token dos claims antes de limpar
            var token = HttpContext.User.FindFirst("jwt")?.Value;

            if (!string.IsNullOrEmpty(token))
            {
                // Tentar fazer logout na API
                await _apiService.LogoutAsync(token);
            }

            // Limpar cookie de autenticação e sessão
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();

            _logger.LogInformation("Usuário deslogado e sessão limpa");

            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        public IActionResult AccessDenied()
        {
            // Se o utilizador está autenticado mas não alterou a password, redireciona para ChangePassword
            if (User?.Identity?.IsAuthenticated == true)
            {
                var needsPasswordChange = HttpContext.Session.GetString("NeedsPasswordChange");
                if (needsPasswordChange == "True")
                {
                    TempData["NeedsPasswordChange"] = true;
                    TempData["Message"] = "Por favor, altere a sua palavra-passe antes de continuar.";
                    return RedirectToAction("ChangePassword");
                }
            }
            
            return View();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ChangePassword()
        {
            await PopulateMemberNameAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateMemberNameAsync();
                return View(model);
            }

            var result = await _apiService.ChangePasswordAsync(model.CurrentPassword, model.NewPassword);

            if (!result.Success)
            {
                ModelState.AddModelError("", result.ErrorMessage ?? "Erro ao alterar a palavra-passe.");
                await PopulateMemberNameAsync();
                return View(model);
            }

            // Limpar flag de NeedsPasswordChange da sessão antes de logout
            HttpContext.Session.Remove("NeedsPasswordChange");

            // Logout para forçar re-login (agora com PrimeiraVez=false)
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();

            return RedirectToAction("Login");
        }

        private async Task PopulateMemberNameAsync()
        {
            try
            {
                var currentUser = await _apiService.GetCurrentUserAsync();
                var name = currentUser?.Nome;

                if (string.IsNullOrWhiteSpace(name))
                {
                    var email = currentUser?.Email ?? User.Identity?.Name ?? "";
                    if (!string.IsNullOrWhiteSpace(email) && email.Contains("@"))
                    {
                        name = email.Split('@')[0];
                    }
                }

                if (!string.IsNullOrWhiteSpace(name))
                {
                    ViewData["MemberName"] = name;
                }
            }
            catch
            {
                // Se falhar, simplesmente não define o nome; o fallback do layout trata do resto.
            }
        }



    }

    public class LoginViewModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "A palavra-passe atual é obrigatória.")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "A nova palavra-passe é obrigatória.")]
        [DataType(DataType.Password)]
        [MinLength(12, ErrorMessage = "A palavra-passe deve ter pelo menos 12 caracteres.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%\^&\*\?_~\-]).{12,}$",
            ErrorMessage = "A palavra-passe deve ter maiúscula, minúscula, número e símbolo (!@#$%^&*?_~-).")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "A confirmação da palavra-passe é obrigatória.")]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "A confirmação da palavra-passe não corresponde.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}

