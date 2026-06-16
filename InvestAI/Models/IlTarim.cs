using System.ComponentModel.DataAnnotations.Schema;

namespace InvestAI.Models
{
    public class IlTarim
    {
        [Column("il_kodu")]
        public int IlKodu { get; set; }

        [Column("il")]
        public string Il { get; set; }

        [Column("tarim_alani_ha")]
        public decimal TarimAlaniHa { get; set; }

        [Column("ekilebilir_alan_ha")]
        public decimal EkilebilirAlanHa { get; set; }

        [Column("ekilebilir_alan_orani_pct")]
        public decimal EkilebilirAlanOraniPct { get; set; }

        [Column("sulama_orani_pct")]
        public decimal SulamaOraniPct { get; set; }

        [Column("basli_urunler")]
        public string BasliUrunler { get; set; }

        [Column("urun_sayisi")]
        public int UrunSayisi { get; set; }

        [Column("tarim_istihdami_pct")]
        public decimal TarimIstihdamiPct { get; set; }

        [Column("yatirim_potansiyel_skoru")]
        public decimal YatirimPotansiyelSkoru { get; set; }

        [Column("yatirim_seviyesi")]
        public string YatirimSeviyesi { get; set; }

        [Column("onerilen_yatirim_alani")]
        public string OnerilenYatirimAlani { get; set; }

        [Column("kaynak")]
        public string Kaynak { get; set; }

        [Column("yil")]
        public string Yil { get; set; }
    }
}