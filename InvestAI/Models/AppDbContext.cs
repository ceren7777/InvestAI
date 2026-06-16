using InvestAI.Models.InvestAI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InvestAI.Models
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Mevcut DbSet'ler aynen kalıyor
        public DbSet<Region> Regions { get; set; }
        public DbSet<Sector> Sectors { get; set; }
        public DbSet<Resource> Resources { get; set; }
        public DbSet<RegionSector> RegionSectors { get; set; }
        public DbSet<RegionResource> RegionResources { get; set; }
        public DbSet<InvestmentSuggestion> InvestmentSuggestions { get; set; }
        public DbSet<SwotAnalysis> SwotAnalyses { get; set; }
        public DbSet<DistrictPopulation> DistrictPopulations { get; set; }
        public DbSet<IlIlceIssizlik> IlIlceIssizlikler { get; set; }
        public DbSet<IlIlceGoc> IlIlceGocler { get; set; }
        public DbSet<DistrictEducation> DistrictEducations { get; set; }
        public DbSet<IlTarim> IlTarimlar { get; set; }
        public DbSet<DistrictSectorDistribution> DistrictSectorDistributions { get; set; }
        public DbSet<PopulationGrowth> PopulationGrowths { get; set; }
        public DbSet<OsbPresence> OsbPresences { get; set; }
        public DbSet<TesvikBolgesi> TesvikBolgeleri { get; set; }
        public DbSet<EnergyData> EnergyDatas { get; set; }
        public DbSet<GunesSantrali> GunesSantralleri { get; set; }
        public DbSet<RuzgarSantrali> RuzgarSantralleri { get; set; }
        public DbSet<HidroelektrikData> HidroelektrikDatas { get; set; }
        public DbSet<JeotermalData> JeotermalDatas { get; set; }
        public DbSet<MadenData> MadenDatas { get; set; }
        public DbSet<IhracatVerisi> IhracatVerileri { get; set; }
        public DbSet<DistrictSummary> DistrictSummaries { get; set; }
        public DbSet<ProvinceSummary> ProvinceSummaries { get; set; }
        public DbSet<ProvinceSectorDistribution> ProvinceSectorDistributions { get; set; }
        public DbSet<YerelKalkinmaWide> YerelKalkinmaWide { get; set; }
        public DbSet<DogalgazAnaHat> DogalgazAnaHat { get; set; }
        public DbSet<WorldBankIndicator> WorldBankIndicators { get; set; }
        public DbSet<AirportData> AirportDatas { get; set; }
        public DbSet<PortData> PortDatas { get; set; }
        public DbSet<DistrictLocation> DistrictLocations { get; set; }
        public DbSet<DistrictAirportDistance> DistrictAirportDistances { get; set; }
        public DbSet<DistrictPortDistance> DistrictPortDistances { get; set; }
        public DbSet<RailwayData> RailwayDatas { get; set; }
        public DbSet<DistrictRailwayDistance> DistrictRailwayDistances { get; set; }
        public DbSet<ExchangeRateData> ExchangeRateDatas { get; set; }
        public DbSet<WeatherData> WeatherDatas { get; set; }
        public DbSet<GoogleRouteDistance> GoogleRouteDistances { get; set; }
        public DbSet<InvestmentAnalysisView> InvestmentAnalysisView { get; set; }
        public DbSet<InvestScoreView> InvestScoreViews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Identity tabloları için şart

            modelBuilder.Entity<RegionSector>()
                .HasKey(rs => new { rs.RegionId, rs.SectorId });
            modelBuilder.Entity<RegionResource>()
                .HasKey(rr => new { rr.RegionId, rr.ResourceId });
            modelBuilder.Entity<IlIlceIssizlik>()
                .ToTable("il_ilce_issizlik");
            modelBuilder.Entity<DistrictEducation>()
                .HasNoKey().ToTable("district_education");
            modelBuilder.Entity<IlIlceGoc>()
                .HasNoKey().ToTable("il_ilce_goc");
            modelBuilder.Entity<IlTarim>()
                .HasNoKey().ToTable("il_tarim");
            modelBuilder.Entity<DistrictSectorDistribution>()
                .ToTable("district_sector_distribution");
            modelBuilder.Entity<PopulationGrowth>()
                .ToTable("population_growth");
            modelBuilder.Entity<OsbPresence>()
                .HasNoKey().ToTable("osb_presence");
            modelBuilder.Entity<TesvikBolgesi>()
                .HasNoKey().ToTable("tesvik_bolgesi");
            modelBuilder.Entity<EnergyData>()
                .HasNoKey().ToTable("energy_data");
            modelBuilder.Entity<GunesSantrali>()
                .HasNoKey().ToTable("gunes_santralleri");
            modelBuilder.Entity<RuzgarSantrali>()
                .HasNoKey().ToTable("ruzgar_santralleri");
            modelBuilder.Entity<HidroelektrikData>()
                .HasNoKey().ToTable("hidroelektrik_data");
            modelBuilder.Entity<JeotermalData>()
                .HasNoKey().ToTable("jeotermal_data");
            modelBuilder.Entity<MadenData>()
                .HasNoKey().ToTable("maden_data");
            modelBuilder.Entity<IhracatVerisi>()
                .HasNoKey().ToTable("ihracat_verileri");
            modelBuilder.Entity<DistrictSummary>()
                .ToTable("district_summary");
            modelBuilder.Entity<ProvinceSummary>()
                .ToTable("province_summary");
            modelBuilder.Entity<ProvinceSectorDistribution>()
                .HasNoKey().ToTable("province_sector_distribution");
            modelBuilder.Entity<YerelKalkinmaWide>()
                .HasNoKey().ToTable("yerel_kalkinma_wide");
            modelBuilder.Entity<DogalgazAnaHat>()
                .HasNoKey().ToTable("dogalgaz_ana_hat");
            modelBuilder.Entity<InvestmentAnalysisView>()
                .HasNoKey().ToView("invest_score_view");
            modelBuilder.Entity<InvestScoreView>()
                .HasNoKey();
        }
    }
}