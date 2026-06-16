using Microsoft.AspNetCore.Identity;

namespace InvestAI.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
        public string? DefaultIl { get; set; }
        public bool BildirimAktif { get; set; } = true;
        public string AIYanitUzunlugu { get; set; } = "Orta";
    }
}