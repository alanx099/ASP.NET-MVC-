using System.ComponentModel.DataAnnotations;

namespace Servis_Centar_Za_Gitare.Dtos
{
    public class LoginRequestDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class RefreshTokenRequestDto
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }

    public record TokenResponseDto(
        string TokenType,
        string AccessToken,
        DateTime AccessTokenExpiresAtUtc,
        string RefreshToken,
        DateTime RefreshTokenExpiresAtUtc);

    public record CurrentUserDto(string Id, string Email, IEnumerable<string> Roles);
}
