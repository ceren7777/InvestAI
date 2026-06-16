namespace InvestAI.Models
{
    public class Sector
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public float GrowthRate { get; set; }
        public float EmploymentRate { get; set; }
        public string Description { get; set; } = string.Empty;

        public ICollection<RegionSector> RegionSectors { get; set; } = new List<RegionSector>();
    }
}