using System.ComponentModel.DataAnnotations.Schema;

namespace InvestAI.Models
{
    public class ProvinceSectorDistribution
    {
        [Column("il")]
        public string? Il { get; set; }

        [Column("sector_code")]
        public string? SectorCode { get; set; }

        [Column("sector_name")]
        public string? SectorName { get; set; }

        [Column("employee_count")]
        public int EmployeeCount { get; set; }

        [Column("percentage")]
        public double Percentage { get; set; }

        [Column("year")]
        public int Year { get; set; }
    }
}