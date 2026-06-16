
namespace InvestAI.Models
{
    public class Region
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public int Population { get; set; }
        public int Workforce { get; set; }
        public int InfrastructureScore { get; set; }
        public string Description { get; set; } = string.Empty;

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime? PopulationUpdatedAt { get; set; }
        public string? WikidataId { get; set; }

        public ICollection<RegionSector> RegionSectors { get; set; } = new List<RegionSector>();
        public ICollection<RegionResource> RegionResources { get; set; } = new List<RegionResource>();
        public ICollection<InvestmentSuggestion> InvestmentSuggestions { get; set; } = new List<InvestmentSuggestion>();
    }
}