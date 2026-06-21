using System.ComponentModel.DataAnnotations;

namespace Servis_Centar_Za_Gitare.models
{
    public class RefreshToken
    {
        public long Id { get; set; }

        [Required]
        public string TokenHash { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = string.Empty;

        public DateTime CreatedAtUtc { get; set; }

        public DateTime ExpiresAtUtc { get; set; }

        public DateTime? RevokedAtUtc { get; set; }

        public string? ReplacedByTokenHash { get; set; }

        public bool IsActive => RevokedAtUtc == null && ExpiresAtUtc > DateTime.UtcNow;

        public AppUser User { get; set; } = null!;
    }
}
