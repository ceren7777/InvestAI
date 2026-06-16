using System;

namespace InvestAI.Models
{
    public class WorldBankIndicator
    {
        public int Id { get; set; }

        public string IndicatorName { get; set; } = string.Empty;
        public string IndicatorCode { get; set; } = string.Empty;

        public string CountryCode { get; set; } = "TUR";
        public string CountryName { get; set; } = "Turkiye";

        public int Year { get; set; }
        public decimal? Value { get; set; }

        public string Source { get; set; } = "World Bank API";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}