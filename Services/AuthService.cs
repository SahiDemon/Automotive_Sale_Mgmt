using Automotive_Sale_Mgmt.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace Automotive_Sale_Mgmt.Services
{
    public class AuthService
    {
        private readonly InMemoryUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(InMemoryUserService userService, IHttpContextAccessor httpContextAccessor)
        {
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> SignInAsync(string email, string password, bool rememberMe = false)
        {
            if (!_userService.ValidateUser(email, password))
                return false;

            var user = _userService.GetUserByEmail(email);
            if (user == null)
                return false;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName),
                new Claim("UserId", user.Id)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(rememberMe ? 30 : 1)
            };

            await _httpContextAccessor.HttpContext!.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return true;
        }

        public async Task SignOutAsync()
        {
            await _httpContextAccessor.HttpContext!.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        public ApplicationUser? GetCurrentUser()
        {
            if (_httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated != true)
                return null;

            var email = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Email);
            return _userService.GetUserByEmail(email ?? string.Empty);
        }

        public bool IsSignedIn()
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;
        }
    }
}