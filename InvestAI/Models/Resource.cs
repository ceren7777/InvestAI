namespace InvestAI.Models
{
    public class Resource
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public float Quantity { get; set; }
        public string Description { get; set; } = string.Empty;

        public ICollection<RegionResource> RegionResources { get; set; } = new List<RegionResource>();
    }
}