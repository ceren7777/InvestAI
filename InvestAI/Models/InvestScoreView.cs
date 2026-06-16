using System.ComponentModel.DataAnnotations.Schema;

namespace InvestAI.Models
{
    [Table("invest_score_view")]
    public class InvestScoreView
    {
        [Column("il")]
        public string Il { get; set; } = "";

        [Column("ilce")]
        public string Ilce { get; set; } = "";

        [Column("nufus_skoru")]
        public double? NufusSkoru { get; set; }

        [Column("buyume_skoru")]
        public double? BuyumeSkoru { get; set; }

        [Column("issizlik_skoru")]
        public double? IssizlikSkoru { get; set; }

        [Column("egitim_skoru")]
        public double? EgitimSkoru { get; set; }

        [Column("enerji_skoru")]
        public double? EnerjiSkoru { get; set; }

        [Column("dogalgaz_skoru")]
        public double? DogalgazSkoru { get; set; }

        [Column("maden_skoru")]
        public double? MadenSkoru { get; set; }

        [Column("osb_skoru")]
        public double? OsbSkoru { get; set; }

        [Column("tarim_skoru")]
        public double? TarimSkoru { get; set; }

        [Column("tesvik_skoru")]
        public double? TesvikSkoru { get; set; }

        [Column("yatirim_skoru")]
        public double? YatirimSkoru { get; set; }

        [Column("maden_turleri")]
        public string? MadenTurleri { get; set; }

        [Column("basli_urunler")]
        public string? BasliUrunler { get; set; }

        [Column("ilce_dominant_sektor_adi")]
        public string? IlceDominantSektorAdi { get; set; }

        [Column("il_dominant_sektor_adi")]
        public string? IlDominantSektorAdi { get; set; }

        [Column("tesvik_bolge")]
        public int? TesvikBolge { get; set; }

        [Column("maden_sayisi")]
        public int? MadenSayisi { get; set; }

        [Column("osb_sayisi")]
        public int? OsbSayisi { get; set; }
    }
}