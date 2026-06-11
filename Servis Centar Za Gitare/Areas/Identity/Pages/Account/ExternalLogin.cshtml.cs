using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Servis_Centar_Za_Gitare.Data;
using Servis_Centar_Za_Gitare.models;

namespace Servis_Centar_Za_Gitare.Areas.Identity.Pages.Account
{
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ExternalLoginModel(
            SignInManager<AppUser> signInManager,
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ReturnUrl { get; set; }

        public class InputModel
        {
            [Required, EmailAddress]
            public string Email { get; set; } = string.Empty;

        }

        public IActionResult OnPost(string provider, string? returnUrl = null)
        {
            var redirectUrl = Url.Page("./ExternalLogin", "Callback", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string? returnUrl = null, string? remoteError = null)
        {
            ReturnUrl = returnUrl;
            if (!string.IsNullOrWhiteSpace(remoteError))
            {
                ModelState.AddModelError(string.Empty, $"External provider error: {remoteError}");
                return Page();
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                return LocalRedirect(GetSafeReturnUrl(returnUrl));
            }

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError(string.Empty, "Google account did not provide an email address.");
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new AppUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    AddErrors(createResult);
                    return Page();
                }
            }

            await EnsureUserRoleAsync(user);

            var loginResult = await _userManager.AddLoginAsync(user, info);
            if (!loginResult.Succeeded && !loginResult.Errors.Any(error => error.Code == "LoginAlreadyAssociated"))
            {
                AddErrors(loginResult);
                return Page();
            }

            await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
            return LocalRedirect(GetSafeReturnUrl(returnUrl));
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email.Trim());
            if (user == null)
            {
                user = new AppUser
                {
                    UserName = Input.Email.Trim(),
                    Email = Input.Email.Trim(),
                    EmailConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    AddErrors(createResult);
                    return Page();
                }
            }

            await EnsureUserRoleAsync(user);

            var loginResult = await _userManager.AddLoginAsync(user, info);
            if (!loginResult.Succeeded && !loginResult.Errors.Any(error => error.Code == "LoginAlreadyAssociated"))
            {
                AddErrors(loginResult);
                return Page();
            }

            await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
            return LocalRedirect(GetSafeReturnUrl(returnUrl));
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private async Task EnsureUserRoleAsync(AppUser user)
        {
            await IdentitySeed.EnsureDefaultRolesAsync(_roleManager);

            if (!await _userManager.IsInRoleAsync(user, IdentitySeed.UserRole))
            {
                var result = await _userManager.AddToRoleAsync(user, IdentitySeed.UserRole);
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(string.Join("; ", result.Errors.Select(error => error.Description)));
                }
            }
        }

        private string GetSafeReturnUrl(string? returnUrl)
        {
            return Url.IsLocalUrl(returnUrl) ? returnUrl! : Url.Content("~/");
        }
    }
}
