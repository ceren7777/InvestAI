using System.ComponentModel.DataAnnotations.Schema;

namespace InvestAI.Models
{
    public class YerelKalkinmaWide
    {
        [Column("il")]
        public string? Il { get; set; }

        [Column("konu_1")]
        public string? Konu1 { get; set; }

        [Column("konu_2")]
        public string? Konu2 { get; set; }

        [Column("konu_3")]
        public string? Konu3 { get; set; }

        [Column("konu_4")]
        public string? Konu4 { get; set; }

        [Column("yil")]
        public int Yil { get; set; }
    }
}