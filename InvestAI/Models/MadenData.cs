using System.ComponentModel.DataAnnotations.Schema;

namespace InvestAI.Models
{
    public class MadenData
    {
        [Column("il")]
        public string? Il { get; set; }

        [Column("il_kodu")]
        public int? IlKodu { get; set; }

        [Column("ilce")]
        public string? Ilce { get; set; }

        [Column("maden_turu")]
        public string? MadenTuru { get; set; }

        [Column("rezerv_durumu")]
        public string? RezervDurumu { get; set; }

        [Column("kaynak")]
        public string? Kaynak { get; set; }

        [Column("yil")]
        public int Yil { get; set; }

        [Column("notlar")]
        public string? Notlar { get; set; }
    }
}