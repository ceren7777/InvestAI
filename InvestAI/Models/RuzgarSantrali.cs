using System.ComponentModel.DataAnnotations.Schema;

namespace InvestAI.Models
{
    public class RuzgarSantrali
    {
        [Column("il")]
        public string Il { get; set; }

        [Column("santral_adi")]
        public string SantralAdi { get; set; }

        [Column("firma")]
        public string Firma { get; set; }

        [Column("kurulu_guc_mw")]
        public decimal KuruluGucMw { get; set; }
    }
}