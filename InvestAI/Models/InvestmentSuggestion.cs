namespace InvestAI.Models
{
    public class InvestmentSuggestion
    {
        public int Id { get; set; }
        public int RegionId { get; set; }
        public Region Region { get; set; } = null!;

        public string Title { get; set; } = string.Empty;
        public float Score { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Rationale { get; set; } = string.Empty;
    }
}