using System.ComponentModel.DataAnnotations.Schema;

namespace InvestAI.Models
{
    public class PopulationGrowth
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("city")]
        public string City { get; set; }

        [Column("district")]
        public string District { get; set; }

        [Column("population_2024")]
        public int Population2024 { get; set; }

        [Column("population_2025")]
        public int Population2025 { get; set; }

        [Column("increase")]
        public int Increase { get; set; }

        [Column("increase_rate")]
        public decimal IncreaseRate { get; set; }

        [Column("trend")]
        public string Trend { get; set; }
    }
}