using System.Text.Json;
using InvestAI.Models;

namespace InvestAI.Services
{
    public class OverpassService
    {
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _context;

        public OverpassService(HttpClient httpClient, AppDbContext context)
        {
            _httpClient = httpClient;
            _context = context;
        }

        public async Task FetchAirportsAsync()
        {
            string query = @"
[out:json][timeout:25];
area[""ISO3166-1""=""TR""][admin_level=2]->.turkey;
(
  node[""aeroway""=""aerodrome""](area.turkey);
  way[""aeroway""=""aerodrome""](area.turkey);
  relation[""aeroway""=""aerodrome""](area.turkey);
);
out center tags;
";

            await FetchAndSaveAsync(query, "airport");
        }

        public async Task FetchPortsAsync()
        {
            string query = @"
[out:json][timeout:25];
area[""ISO3166-1""=""TR""][admin_level=2]->.turkey;
(
  node[""harbour""=""yes""](area.turkey);
  way[""harbour""=""yes""](area.turkey);
  relation[""harbour""=""yes""](area.turkey);
);
out center tags;
";

            await FetchAndSaveAsync(query, "port");
        }

        public async Task FetchDistrictLocationsAsync()
        {
            string query = @"
[out:json][timeout:60];
area[""ISO3166-1""=""TR""][admin_level=2]->.turkey;
relation[""admin_level""=""6""](area.turkey);
out center tags;
";

            var json = await SendOverpassRequestAsync(query);
            using JsonDocument doc = JsonDocument.Parse(json);

            var elements = doc.RootElement.GetProperty("elements");

            foreach (var item in elements.EnumerateArray())
            {
                if (!item.TryGetProperty("tags", out JsonElement tags))
                    continue;

                string district = tags.TryGetProperty("name", out JsonElement nameElement)
                    ? nameElement.GetString() ?? ""
                    : "";

                if (!item.TryGetProperty("center", out JsonElement center))
                    continue;

                double lat = center.GetProperty("lat").GetDouble();
                double lon = center.GetProperty("lon").GetDouble();

                bool exists = _context.DistrictLocations.Any(x => x.District == district);

                if (!exists)
                {
                    _context.DistrictLocations.Add(new DistrictLocation
                    {
                        City = "",
                        District = district,
                        Latitude = lat,
                        Longitude = lon,
                        Source = "OSM Overpass API"
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

        private async Task FetchAndSaveAsync(string query, string type)
        {
            var json = await SendOverpassRequestAsync(query);
            using JsonDocument doc = JsonDocument.Parse(json);

            var elements = doc.RootElement.GetProperty("elements");

            foreach (var item in elements.EnumerateArray())
            {
                string name = "Unknown";

                if (item.TryGetProperty("tags", out JsonElement tags) &&
                    tags.TryGetProperty("name", out JsonElement nameElement))
                {
                    name = nameElement.GetString() ?? "Unknown";
                }

                double lat = 0;
                double lon = 0;

                if (item.TryGetProperty("lat", out JsonElement latElement))
                    lat = latElement.GetDouble();

                if (item.TryGetProperty("lon", out JsonElement lonElement))
                    lon = lonElement.GetDouble();

                if (item.TryGetProperty("center", out JsonElement center))
                {
                    lat = center.GetProperty("lat").GetDouble();
                    lon = center.GetProperty("lon").GetDouble();
                }

                if (type == "airport")
                {
                    bool exists = _context.AirportDatas.Any(x => x.Name == name);

                    if (!exists)
                    {
                        _context.AirportDatas.Add(new AirportData
                        {
                            Name = name,
                            City = "",
                            Latitude = lat,
                            Longitude = lon,
                            Source = "OSM Overpass API"
                        });
                    }
                }

                if (type == "port")
                {
                    bool exists = _context.PortDatas.Any(x => x.Name == name);

                    if (!exists)
                    {
                        _context.PortDatas.Add(new PortData
                        {
                            Name = name,
                            City = "",
                            Latitude = lat,
                            Longitude = lon,
                            Source = "OSM Overpass API"
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        private async Task<string> SendOverpassRequestAsync(string query)
        {
            var requestUrl =
                "https://overpass-api.de/api/interpreter?data=" + Uri.EscapeDataString(query);

            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("InvestAI/1.0");

            var response = await _httpClient.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        private double CalculateDistanceKm(
    double lat1,
    double lon1,
    double lat2,
    double lon2)
        {
            double R = 6371;

            double dLat = DegreesToRadians(lat2 - lat1);
            double dLon = DegreesToRadians(lon2 - lon1);

            double a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(lat1)) *
                Math.Cos(DegreesToRadians(lat2)) *
                Math.Sin(dLon / 2) *
                Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }

        private double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        public async Task CalculateDistrictAirportDistancesAsync()
        {
            var districts = _context.DistrictLocations.ToList();
            var airports = _context.AirportDatas.ToList();

            foreach (var district in districts)
            {
                double minDistance = double.MaxValue;
                string nearestAirport = "";

                foreach (var airport in airports)
                {
                    double distance = CalculateDistanceKm(
                        district.Latitude,
                        district.Longitude,
                        airport.Latitude,
                        airport.Longitude);

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearestAirport = airport.Name;
                    }
                }

                bool exists = _context.DistrictAirportDistances.Any(x =>
                    x.District == district.District);

                if (!exists)
                {
                    _context.DistrictAirportDistances.Add(
                        new DistrictAirportDistance
                        {
                            City = district.City,
                            District = district.District,
                            AirportName = nearestAirport,
                            DistanceKm = Math.Round(minDistance, 2)
                        });
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task CalculateDistrictPortDistancesAsync()
        {
            var districts = _context.DistrictLocations.ToList();
            var ports = _context.PortDatas.ToList();

            foreach (var district in districts)
            {
                double minDistance = double.MaxValue;
                string nearestPort = "";

                foreach (var port in ports)
                {
                    double distance = CalculateDistanceKm(
                        district.Latitude,
                        district.Longitude,
                        port.Latitude,
                        port.Longitude);

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearestPort = port.Name;
                    }
                }

                bool exists = _context.DistrictPortDistances.Any(x =>
                    x.District == district.District &&
                    x.PortName == nearestPort);

                if (!exists)
                {
                    _context.DistrictPortDistances.Add(
                        new DistrictPortDistance
                        {
                            City = district.City,
                            District = district.District,
                            PortName = nearestPort,
                            DistanceKm = Math.Round(minDistance, 2)
                        });
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task FetchRailwaysAsync()
        {
            string query = @"
[out:json][timeout:60];
area[""ISO3166-1""=""TR""][admin_level=2]->.turkey;

(
  node[""railway""=""station""](area.turkey);
  way[""railway""=""station""](area.turkey);
  relation[""railway""=""station""](area.turkey);
);

out center tags;
";

            var json = await SendOverpassRequestAsync(query);

            using JsonDocument doc = JsonDocument.Parse(json);

            var elements = doc.RootElement.GetProperty("elements");

            foreach (var item in elements.EnumerateArray())
            {
                string name = "Unknown Railway";

                if (item.TryGetProperty("tags", out JsonElement tags) &&
                    tags.TryGetProperty("name", out JsonElement nameElement))
                {
                    name = nameElement.GetString() ?? "Unknown Railway";
                }

                double lat = 0;
                double lon = 0;

                if (item.TryGetProperty("lat", out JsonElement latElement))
                    lat = latElement.GetDouble();

                if (item.TryGetProperty("lon", out JsonElement lonElement))
                    lon = lonElement.GetDouble();

                if (item.TryGetProperty("center", out JsonElement center))
                {
                    lat = center.GetProperty("lat").GetDouble();
                    lon = center.GetProperty("lon").GetDouble();
                }

                bool exists = _context.RailwayDatas.Any(x =>
                    x.Name == name);

                if (!exists)
                {
                    _context.RailwayDatas.Add(new RailwayData
                    {
                        Name = name,
                        Latitude = lat,
                        Longitude = lon,
                        Source = "OSM Overpass API"
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task CalculateDistrictRailwayDistancesAsync()
        {
            var districts = _context.DistrictLocations.ToList();
            var railways = _context.RailwayDatas.ToList();

            foreach (var district in districts)
            {
                double minDistance = double.MaxValue;
                string nearestRailway = "";

                foreach (var railway in railways)
                {
                    double distance = CalculateDistanceKm(
                        district.Latitude,
                        district.Longitude,
                        railway.Latitude,
                        railway.Longitude);

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearestRailway = railway.Name;
                    }
                }

                bool exists = _context.DistrictRailwayDistances.Any(x =>
                    x.District == district.District &&
                    x.RailwayName == nearestRailway);

                if (!exists)
                {
                    _context.DistrictRailwayDistances.Add(
                        new DistrictRailwayDistance
                        {
                            City = district.City,
                            District = district.District,
                            RailwayName = nearestRailway,
                            DistanceKm = Math.Round(minDistance, 2)
                        });
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}