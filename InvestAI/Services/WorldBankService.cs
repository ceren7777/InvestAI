using System.Text.Json;
using InvestAI.Models;

namespace InvestAI.Services
{
    public class WorldBankService
    {
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _context;

        public WorldBankService(HttpClient httpClient, AppDbContext context)
        {
            _httpClient = httpClient;
            _context = context;
        }

        public async Task FetchIndicatorDataAsync(string indicatorName, string indicatorCode)
        {
            string url =
                $"https://api.worldbank.org/v2/country/TUR/indicator/{indicatorCode}?format=json";

            var response = await _httpClient.GetStringAsync(url);

            using JsonDocument doc = JsonDocument.Parse(response);

            var dataArray = doc.RootElement[1];

            foreach (var item in dataArray.EnumerateArray())
            {
                if (item.GetProperty("value").ValueKind != JsonValueKind.Null)
                {
                    int year = int.Parse(item.GetProperty("date").GetString()!);

                    bool alreadyExists = _context.WorldBankIndicators.Any(x =>
                        x.IndicatorCode == indicatorCode &&
                        x.Year == year &&
                        x.CountryCode == "TUR");

                    if (!alreadyExists)
                    {
                        var indicator = new WorldBankIndicator
                        {
                            IndicatorName = indicatorName,
                            IndicatorCode = indicatorCode,
                            CountryCode = "TUR",
                            CountryName = "Turkiye",
                            Year = year,
                            Value = item.GetProperty("value").GetDecimal(),
                            Source = "World Bank API",
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.WorldBankIndicators.Add(indicator);
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task FetchAllWorldBankIndicatorsAsync()
        {
            var indicators = new List<(string Name, string Code)>
            {
                ("GDP Growth", "NY.GDP.MKTP.KD.ZG"),
                ("Inflation", "FP.CPI.TOTL.ZG"),
                ("GDP Per Capita USD", "NY.GDP.PCAP.CD"),
                ("Labor Force Participation", "SL.TLF.CACT.ZS"),
                ("Industry Value Added", "NV.IND.TOTL.ZS"),
                ("Foreign Direct Investment", "BX.KLT.DINV.WD.GD.ZS"),
                ("Internet Usage", "IT.NET.USER.ZS"),
                ("Energy Intensity", "EG.EGY.PRIM.PP.KD")
            };

            foreach (var indicator in indicators)
            {
                await FetchIndicatorDataAsync(indicator.Name, indicator.Code);
            }
        }
    }
}