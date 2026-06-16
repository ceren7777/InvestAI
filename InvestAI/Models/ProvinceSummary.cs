using System.ComponentModel.DataAnnotations.Schema;

namespace InvestAI.Models
{
    public class ProvinceSummary
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("il")]
        public string? Il { get; set; }

        [Column("total_estimated_employment")]
        public int TotalEstimatedEmployment { get; set; }

        [Column("dominant_sector_code")]
        public string? DominantSectorCode { get; set; }

        [Column("dominant_sector_name")]
        public string? DominantSectorName { get; set; }

        [Column("year")]
        public int Year { get; set; }
    }
}