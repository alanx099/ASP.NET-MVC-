using System.ComponentModel.DataAnnotations;

namespace Servis_Centar_Za_Gitare.Options
{
    public class JwtOptions
    {
        public const string SectionName = "Jwt";

        [Required]
        public string Issuer { get; set; } = string.Empty;

        [Required]
        public string Audience { get; set; } = string.Empty;

        [Required, MinLength(32)]
        public string Secret { get; set; } = string.Empty;

        [Range(1, 1440)]
        public int AccessTokenMinutes { get; set; } = 30;

        [Range(1, 90)]
        public int RefreshTokenDays { get; set; } = 14;
    }
}
