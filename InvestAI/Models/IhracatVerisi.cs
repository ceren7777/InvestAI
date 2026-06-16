using System.ComponentModel.DataAnnotations.Schema;

namespace InvestAI.Models
{
    public class IhracatVerisi
    {
        [Column("il")]
        public string? Il { get; set; }

        [Column("ihracat_usd")]
        public long IhracatUsd { get; set; }

        [Column("yil")]
        public int Yil { get; set; }
    }
}