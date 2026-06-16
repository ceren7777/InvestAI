namespace InvestAI.Models
{
    public class ExchangeRateData
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public decimal USDTRY { get; set; }

        public decimal EURTRY { get; set; }

        public string Source { get; set; } = "TCMB API";
    }
}