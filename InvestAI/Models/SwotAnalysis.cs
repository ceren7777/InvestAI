namespace InvestAI.Models
{
    public class SwotAnalysis
    {
        public int Id { get; set; }
        public int RegionId { get; set; }
        public Region Region { get; set; } = null!;

        public string Strengths { get; set; } = string.Empty;
        public string Weaknesses { get; set; } = string.Empty;
        public string Opportunities { get; set; } = string.Empty;
        public string Threats { get; set; } = string.Empty;
        public string AiGenerated { get; set; } = string.Empty;
    }
}