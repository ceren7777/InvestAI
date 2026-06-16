namespace InvestAI.Models
{
    public class RegionSector
    {
        public int RegionId { get; set; }
        public Region Region { get; set; } = null!;

        public int SectorId { get; set; }
        public Sector Sector { get; set; } = null!;

        public int Strength { get; set; }
    }
}