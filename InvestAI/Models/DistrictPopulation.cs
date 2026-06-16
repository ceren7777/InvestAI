namespace InvestAI.Models
{
    public class DistrictPopulation
    {
        public int Id { get; set; }
        public string City { get; set; } = null!;
        public string District { get; set; } = null!;
        public int Population { get; set; }
    }
}