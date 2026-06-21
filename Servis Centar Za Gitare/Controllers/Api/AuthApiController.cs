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
        private readonly AppDbContext _context;
        private readonly JwtTokenService _jwtTokenService;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;

        public AuthApiController(
            AppDbContext context,
            JwtTokenService jwtTokenService,
            SignInManager<AppUser> signInManager,
            UserManager<AppUser> userManager)
        {
            _context = context;
            _jwtTokenService = jwtTokenService;
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
