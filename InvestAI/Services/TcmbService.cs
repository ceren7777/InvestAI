using System.Globalization;
using System.Xml.Linq;
using InvestAI.Models;

namespace InvestAI.Services
{
    public class TcmbService
    {
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _context;

        public TcmbService(HttpClient httpClient, AppDbContext context)
        {
            _httpClient = httpClient;
            _context = context;
        }

        public async Task FetchExchangeRatesAsync()
        {
            string url = "https://www.tcmb.gov.tr/kurlar/today.xml";

            var xmlString = await _httpClient.GetStringAsync(url);

            XDocument doc = XDocument.Parse(xmlString);

            var usd = doc.Descendants("Currency")
                .FirstOrDefault(x => x.Attribute("CurrencyCode")?.Value == "USD");

            var eur = doc.Descendants("Currency")
                .FirstOrDefault(x => x.Attribute("CurrencyCode")?.Value == "EUR");

            if (usd != null && eur != null)
            {
                string usdValue =
                    usd.Element("ForexSelling")?.Value.Replace(",", ".") ?? "0";

                string eurValue =
                    eur.Element("ForexSelling")?.Value.Replace(",", ".") ?? "0";

                decimal usdTry = decimal.Parse(
                    usdValue,
                    CultureInfo.InvariantCulture);

                decimal eurTry = decimal.Parse(
                    eurValue,
                    CultureInfo.InvariantCulture);

                bool exists = _context.ExchangeRateDatas.Any(x =>
                    x.Date.Date == DateTime.UtcNow.Date);

                if (!exists)
                {
                    _context.ExchangeRateDatas.Add(
                        new ExchangeRateData
                        {
                            Date = DateTime.UtcNow,
                            USDTRY = usdTry,
                            EURTRY = eurTry,
                            Source = "TCMB API"
                        });
                }

                await _context.SaveChangesAsync();
            }
        }
    }
}