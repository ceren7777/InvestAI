namespace InvestAI.Models
{
    public class WeatherData
    {
        public int Id { get; set; }

        public string District { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public double Temperature { get; set; }
        public double WindSpeed { get; set; }

        public DateTime RetrievedAt { get; set; } = DateTime.UtcNow;

        public string Source { get; set; } = "Open-Meteo API";
    }
}