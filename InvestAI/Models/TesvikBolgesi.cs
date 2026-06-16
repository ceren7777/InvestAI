using System.ComponentModel.DataAnnotations.Schema;

namespace InvestAI.Models
{
    public class TesvikBolgesi
    {
        [Column("il")]
        public string Il { get; set; }

        [Column("tesvik_bolge")]
        public int TesvikBolge { get; set; }

        [Column("year")]
        public int Year { get; set; }
    }
}