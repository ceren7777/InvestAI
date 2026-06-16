namespace InvestAI.Models
{
    public class RegionResource
    {
        public int RegionId { get; set; }
        public Region Region { get; set; } = null!;

        public int ResourceId { get; set; }
        public Resource Resource { get; set; } = null!;

        public float Amount { get; set; }
    }
}