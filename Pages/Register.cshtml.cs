using Automotive_Sale_Mgmt.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Automotive_Sale_Mgmt.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly InMemoryUserService _userService;
        private readonly AuthService _authService;

        public RegisterModel(InMemoryUserService userService, AuthService authService)
        {
            _userService = userService;
            _authService = authService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "First Name")]
            public string FirstName { get; set; } = string.Empty;
            
            [Required]
            [Display(Name = "Last Name")]
            public string LastName { get; set; } = string.Empty;

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
            
            // Redirect if user is already signed in
            if (_authService.IsSignedIn())
            {
                Response.Redirect("/Dashboard");
            }
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/Dashboard");
            
            if (ModelState.IsValid)
            {
                var existingUser = _userService.GetUserByEmail(Input.Email);
                
                if (existingUser != null)
                {
                    ModelState.AddModelError(string.Empty, "Email is already taken.");
                    return Page();
                }
                
                var result = _userService.RegisterUser(
                    Input.Email, 
                    Input.Password,
                    Input.FirstName,
                    Input.LastName);

                if (result)
                {
                    // Automatically sign in the user after successful registration
                    await _authService.SignInAsync(Input.Email, Input.Password);
                    return LocalRedirect(returnUrl);
                }
                
                ModelState.AddModelError(string.Empty, "Registration failed. Please try again.");
            }

            // Something failed, redisplay form
            return Page();
        }
    }
}