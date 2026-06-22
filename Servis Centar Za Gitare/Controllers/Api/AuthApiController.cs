using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Servis_Centar_Za_Gitare.Data;
using Servis_Centar_Za_Gitare.Dtos;
using Servis_Centar_Za_Gitare.models;
using Servis_Centar_Za_Gitare.Services;

namespace Servis_Centar_Za_Gitare.Controllers.Api
{
    [ApiController]
    [Route("api/auth")]
    public class AuthApiController : ControllerBase
    {
        private const string GoogleLoginProvider = "Google";
        private readonly AppDbContext _context;
        private readonly CustomerAccountLinker _customerAccountLinker;
        private readonly IConfiguration _configuration;
        private readonly JwtTokenService _jwtTokenService;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;

        public AuthApiController(
            AppDbContext context,
            CustomerAccountLinker customerAccountLinker,
            IConfiguration configuration,
            JwtTokenService jwtTokenService,
            RoleManager<IdentityRole> roleManager,
            SignInManager<AppUser> signInManager,
            UserManager<AppUser> userManager)
        {
            _context = context;
            _customerAccountLinker = customerAccountLinker;
            _configuration = configuration;
            _jwtTokenService = jwtTokenService;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<TokenResponseDto>> Login(LoginRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(request.Email.Trim());
            if (user == null)
            {
                return Unauthorized();
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
            if (!result.Succeeded)
            {
                return Unauthorized();
            }

            var response = await _jwtTokenService.CreateTokenResponseAsync(user);
            await StoreRefreshTokenAsync(user, response);

            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("google")]
        public async Task<ActionResult<TokenResponseDto>> GoogleLogin(GoogleLoginRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var allowedClientIds = GetAllowedGoogleClientIds();
            if (allowedClientIds.Count == 0)
            {
                return Problem(
                    "Google login is not configured. Add at least one value under Authentication:Google:AllowedClientIds.",
                    statusCode: StatusCodes.Status500InternalServerError);
            }

            GoogleJsonWebSignature.Payload payload;
            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(
                    request.IdToken,
                    new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = allowedClientIds
                    });
            }
            catch (InvalidJwtException)
            {
                return Unauthorized();
            }

            if (payload.EmailVerified != true || string.IsNullOrWhiteSpace(payload.Email))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByLoginAsync(GoogleLoginProvider, payload.Subject)
                ?? await _userManager.FindByEmailAsync(payload.Email);

            if (user == null)
            {
                user = new AppUser
                {
                    UserName = payload.Email,
                    Email = payload.Email,
                    EmailConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    return Problem(string.Join("; ", createResult.Errors.Select(error => error.Description)));
                }
            }
            else if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);
            }

            await EnsureGoogleLoginLinkedAsync(user, payload.Subject);
            await EnsureDefaultUserRoleAsync(user);
            await _customerAccountLinker.LinkOrCreateCustomerAsync(user, payload.GivenName, payload.FamilyName, "+385000000");

            var response = await _jwtTokenService.CreateTokenResponseAsync(user);
            await StoreRefreshTokenAsync(user, response);

            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<ActionResult<TokenResponseDto>> Refresh(RefreshTokenRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var tokenHash = JwtTokenService.HashRefreshToken(request.RefreshToken);
            var storedToken = await _context.RefreshTokens
                .Include(token => token.User)
                .FirstOrDefaultAsync(token => token.TokenHash == tokenHash);

            if (storedToken?.User == null || !storedToken.IsActive)
            {
                return Unauthorized();
            }

            var response = await _jwtTokenService.CreateTokenResponseAsync(storedToken.User);
            storedToken.RevokedAtUtc = DateTime.UtcNow;
            storedToken.ReplacedByTokenHash = JwtTokenService.HashRefreshToken(response.RefreshToken);
            AddRefreshToken(storedToken.User, response);
            await _context.SaveChangesAsync();

            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke(RefreshTokenRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var tokenHash = JwtTokenService.HashRefreshToken(request.RefreshToken);
            var storedToken = await _context.RefreshTokens.FirstOrDefaultAsync(token => token.TokenHash == tokenHash);
            if (storedToken == null)
            {
                return NoContent();
            }

            storedToken.RevokedAtUtc ??= DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<CurrentUserDto>> Me()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var roles = await _userManager.GetRolesAsync(user);
            return new CurrentUserDto(user.Id, user.Email ?? string.Empty, roles);
        }

        private List<string> GetAllowedGoogleClientIds()
        {
            var configuredClientIds = _configuration
                .GetSection("Authentication:Google:AllowedClientIds")
                .Get<string[]>()?
                .Where(clientId => !string.IsNullOrWhiteSpace(clientId))
                .Select(clientId => clientId.Trim())
                .ToList() ?? new List<string>();

            var serverClientId = _configuration["Authentication:Google:ServerClientId"];
            if (!string.IsNullOrWhiteSpace(serverClientId))
            {
                configuredClientIds.Add(serverClientId.Trim());
            }

            return configuredClientIds.Distinct(StringComparer.Ordinal).ToList();
        }

        private async Task EnsureGoogleLoginLinkedAsync(AppUser user, string googleSubject)
        {
            var existingLogins = await _userManager.GetLoginsAsync(user);
            if (existingLogins.Any(login =>
                    login.LoginProvider == GoogleLoginProvider &&
                    login.ProviderKey == googleSubject))
            {
                return;
            }

            var result = await _userManager.AddLoginAsync(
                user,
                new UserLoginInfo(GoogleLoginProvider, googleSubject, GoogleLoginProvider));

            if (!result.Succeeded && !result.Errors.Any(error => error.Code == "LoginAlreadyAssociated"))
            {
                throw new InvalidOperationException(string.Join("; ", result.Errors.Select(error => error.Description)));
            }
        }

        private async Task EnsureDefaultUserRoleAsync(AppUser user)
        {
            if (!await _roleManager.RoleExistsAsync(IdentitySeed.UserRole))
            {
                await _roleManager.CreateAsync(new IdentityRole(IdentitySeed.UserRole));
            }

            if (!await _userManager.IsInRoleAsync(user, IdentitySeed.UserRole))
            {
                var result = await _userManager.AddToRoleAsync(user, IdentitySeed.UserRole);
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(string.Join("; ", result.Errors.Select(error => error.Description)));
                }
            }
        }

        private async Task StoreRefreshTokenAsync(AppUser user, TokenResponseDto response)
        {
            AddRefreshToken(user, response);
            await _context.SaveChangesAsync();
        }

        private void AddRefreshToken(AppUser user, TokenResponseDto response)
        {
            _context.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.Id,
                TokenHash = JwtTokenService.HashRefreshToken(response.RefreshToken),
                CreatedAtUtc = DateTime.UtcNow,
                ExpiresAtUtc = response.RefreshTokenExpiresAtUtc
            });
        }
    }
}
