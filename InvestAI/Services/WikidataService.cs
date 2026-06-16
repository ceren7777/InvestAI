using System.Net.Http.Headers;
using System.Text.Json;

namespace InvestAI.Services
{
    public class WikidataResult
    {
        public string  CityName   { get; set; } = string.Empty;
        public long?   Population { get; set; }
        public double? Latitude   { get; set; }
        public double? Longitude  { get; set; }
    }

    public interface IWikidataService
    {
        /// <summary>
        /// Bir bölgenin Wikidata verisini çeker.
        /// </summary>
        /// <param name="cityName">İlçe / bölge adı (ör. "Çan")</param>
        /// <param name="cityProvince">İlin adı (ör. "Çanakkale"). null geçilebilir.</param>
        /// <param name="wikidataId">Wikidata Q-ID (ör. "Q640634"). Doluysa direkt sorgulanır.</param>
        Task<WikidataResult?> GetCityInfoAsync(string cityName, string? cityProvince = null, string? wikidataId = null);
    }

    public class WikidataService : IWikidataService
    {
        private readonly HttpClient _http;
        private const string SparqlEndpoint = "https://query.wikidata.org/sparql";

        // ── Türkiye ili adı → Wikidata canonical label ──
        private static readonly Dictionary<string, string> ProvinceNameMap =
            new(StringComparer.OrdinalIgnoreCase)
        {
            ["adana"]          = "Adana",
            ["adıyaman"]       = "Adıyaman",
            ["afyonkarahisar"] = "Afyonkarahisar",
            ["ağrı"]           = "Ağrı",
            ["aksaray"]        = "Aksaray",
            ["amasya"]         = "Amasya",
            ["ankara"]         = "Ankara",
            ["antalya"]        = "Antalya",
            ["ardahan"]        = "Ardahan",
            ["artvin"]         = "Artvin",
            ["aydın"]          = "Aydın",
            ["balıkesir"]      = "Balıkesir",
            ["bartın"]         = "Bartın",
            ["batman"]         = "Batman",
            ["bayburt"]        = "Bayburt",
            ["bilecik"]        = "Bilecik",
            ["bingöl"]         = "Bingöl",
            ["bitlis"]         = "Bitlis",
            ["bolu"]           = "Bolu",
            ["burdur"]         = "Burdur",
            ["bursa"]          = "Bursa",
            ["çanakkale"]      = "Çanakkale",
            ["çankırı"]        = "Çankırı",
            ["çorum"]          = "Çorum",
            ["denizli"]        = "Denizli",
            ["diyarbakır"]     = "Diyarbakır",
            ["düzce"]          = "Düzce",
            ["edirne"]         = "Edirne",
            ["elazığ"]         = "Elazığ",
            ["erzincan"]       = "Erzincan",
            ["erzurum"]        = "Erzurum",
            ["eskişehir"]      = "Eskişehir",
            ["gaziantep"]      = "Gaziantep",
            ["giresun"]        = "Giresun",
            ["gümüşhane"]      = "Gümüşhane",
            ["hakkari"]        = "Hakkari",
            ["hatay"]          = "Hatay",
            ["ığdır"]          = "Iğdır",
            ["isparta"]        = "Isparta",
            ["istanbul"]       = "Istanbul",
            ["izmir"]          = "İzmir",
            ["kahramanmaraş"]  = "Kahramanmaraş",
            ["karabük"]        = "Karabük",
            ["karaman"]        = "Karaman",
            ["kars"]           = "Kars",
            ["kastamonu"]      = "Kastamonu",
            ["kayseri"]        = "Kayseri",
            ["kilis"]          = "Kilis",
            ["kırıkkale"]      = "Kırıkkale",
            ["kırklareli"]     = "Kırklareli",
            ["kırşehir"]       = "Kırşehir",
            ["kocaeli"]        = "Kocaeli",
            ["konya"]          = "Konya",
            ["kütahya"]        = "Kütahya",
            ["malatya"]        = "Malatya",
            ["manisa"]         = "Manisa",
            ["mardin"]         = "Mardin",
            ["mersin"]         = "Mersin",
            ["muğla"]          = "Muğla",
            ["muş"]            = "Muş",
            ["nevşehir"]       = "Nevşehir",
            ["niğde"]          = "Niğde",
            ["ordu"]           = "Ordu",
            ["osmaniye"]       = "Osmaniye",
            ["rize"]           = "Rize",
            ["sakarya"]        = "Sakarya",
            ["samsun"]         = "Samsun",
            ["siirt"]          = "Siirt",
            ["sinop"]          = "Sinop",
            ["sivas"]          = "Sivas",
            ["şanlıurfa"]      = "Şanlıurfa",
            ["şırnak"]         = "Şırnak",
            ["tekirdağ"]       = "Tekirdağ",
            ["tokat"]          = "Tokat",
            ["trabzon"]        = "Trabzon",
            ["tunceli"]        = "Tunceli",
            ["uşak"]           = "Uşak",
            ["van"]            = "Van",
            ["yalova"]         = "Yalova",
            ["yozgat"]         = "Yozgat",
            ["zonguldak"]      = "Zonguldak",
        };

        public WikidataService(HttpClient http)
        {
            _http = http;
            _http.Timeout = TimeSpan.FromSeconds(5);
            _http.DefaultRequestHeaders.Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/sparql-results+json"));
            _http.DefaultRequestHeaders.UserAgent
                .ParseAdd("InvestAI/1.0 (contact@investai.dev)");
        }

        // ─────────────────────────────────────────────────────────────────
        public async Task<WikidataResult?> GetCityInfoAsync(
            string cityName,
            string? cityProvince = null,
            string? wikidataId = null)
        {
            if (string.IsNullOrWhiteSpace(cityName))
                return null;

            var nameKey     = cityName.Trim().ToLowerInvariant();
            var provinceKey = cityProvince?.Trim().ToLowerInvariant() ?? string.Empty;

            Console.WriteLine($"=== Wikidata sorgusu gönderiliyor: {cityName}" +
                              (string.IsNullOrEmpty(provinceKey) ? "" : $", {cityProvince}") + " ===");

            // ── Adım 1: WikidataId ile doğrudan Q-ID sorgusu ───────────────
            if (!string.IsNullOrWhiteSpace(wikidataId))
            {
                Console.WriteLine($"    → WikidataId ile doğrudan sorgu: {wikidataId}");
                var directResult = await ExecuteQueryAsync(
                    BuildDirectEntityQuery(wikidataId), cityName);
                if (directResult != null) return directResult;
            }

            // ── Adım 3: İl (province) tam eşleşme ─────────────────────────
            if (ProvinceNameMap.TryGetValue(nameKey, out var provinceLabel))
            {
                Console.WriteLine($"    → İl sorgusu: {provinceLabel}");
                var provinceResult = await ExecuteQueryAsync(
                    BuildProvinceQuery(provinceLabel), cityName);
                if (provinceResult != null) return provinceResult;
            }

            // ── Adım 4: İl bağlamında ilçe label araması ──────────────────
            if (!string.IsNullOrEmpty(provinceKey))
            {
                var searchProvince = ProvinceNameMap.TryGetValue(provinceKey, out var pl)
                    ? pl : cityProvince!.Trim();

                Console.WriteLine($"    → İlçe araması: '{cityName}' in '{searchProvince}'");
                var districtResult = await ExecuteQueryAsync(
                    BuildDistrictQuery(nameKey, searchProvince.ToLowerInvariant()), cityName);
                if (districtResult != null) return districtResult;
            }

            Console.WriteLine($"    → Sonuç bulunamadı: {cityName}");
            return null;
        }

        // ─────────────────────────────────────────────────────────────────
        // Sorgu çalıştırıcı
        // ─────────────────────────────────────────────────────────────────
        private async Task<WikidataResult?> ExecuteQueryAsync(string sparql, string originalName)
        {
            var url = $"{SparqlEndpoint}?query={Uri.EscapeDataString(sparql)}&format=json";

            HttpResponseMessage response;
            try
            {
                response = await _http.GetAsync(url);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or OperationCanceledException)
            {
                Console.WriteLine($"    → Wikidata isteği başarısız (timeout veya ağ hatası): {ex.Message}");
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"=== Wikidata yanıtı: {json} ===");
            return ParseResult(json, originalName);
        }

        // ─────────────────────────────────────────────────────────────────
        // Sorgu 1 — Doğrudan Q-ID ile entity sorgusu
        // ─────────────────────────────────────────────────────────────────
        private static string BuildDirectEntityQuery(string qId)
        {
            return
                "SELECT ?population ?lat ?lon WHERE {\n" +
                "  OPTIONAL { wd:" + qId + " wdt:P1082 ?population }\n" +
                "  OPTIONAL {\n" +
                "    wd:" + qId + " p:P625 ?coord .\n" +
                "    ?coord psv:P625 ?cv .\n" +
                "    ?cv wikibase:geoLatitude  ?lat .\n" +
                "    ?cv wikibase:geoLongitude ?lon .\n" +
                "  }\n" +
                "}";
        }

        // ─────────────────────────────────────────────────────────────────
        // Sorgu 2 — Türkiye ili (Q48336) CONTAINS label araması
        // ─────────────────────────────────────────────────────────────────
        private static string BuildProvinceQuery(string provinceLabel)
        {
            var lower = provinceLabel.ToLowerInvariant();
            return
                "SELECT DISTINCT ?city ?cityLabel ?population ?lat ?lon WHERE {\n" +
                "  ?city wdt:P17 wd:Q43 .\n" +
                "  ?city wdt:P31 wd:Q48336 .\n" +
                "  ?city rdfs:label ?label .\n" +
                "  FILTER(LANG(?label) IN (\"tr\", \"en\") && CONTAINS(LCASE(STR(?label)), \"" + lower + "\"))\n" +
                "  OPTIONAL { ?city wdt:P1082 ?population . }\n" +
                "  OPTIONAL {\n" +
                "    ?city p:P625 ?coord .\n" +
                "    ?coord psv:P625 ?cv .\n" +
                "    ?cv wikibase:geoLatitude  ?lat .\n" +
                "    ?cv wikibase:geoLongitude ?lon .\n" +
                "  }\n" +
                "  SERVICE wikibase:label { bd:serviceParam wikibase:language \"tr,en\" . }\n" +
                "}\n" +
                "ORDER BY DESC(?population)\n" +
                "LIMIT 1";
        }

        // ─────────────────────────────────────────────────────────────────
        // Sorgu 3 — İl bağlamında ilçe / kasaba label araması
        //           P131+ ile üst idari birime bağlı olma koşulu
        // ─────────────────────────────────────────────────────────────────
        private static string BuildDistrictQuery(string cityNameLower, string provinceLower)
        {
            return
                "SELECT DISTINCT ?city ?cityLabel ?population ?lat ?lon WHERE {\n" +
                "  ?city wdt:P17 wd:Q43 .\n" +
                "  ?city rdfs:label ?nameLabel .\n" +
                "  FILTER(LANG(?nameLabel) IN (\"tr\", \"en\") && CONTAINS(LCASE(STR(?nameLabel)), \"" + cityNameLower + "\"))\n" +
                "  ?city wdt:P131+ ?prov .\n" +
                "  ?prov wdt:P31 wd:Q48336 .\n" +
                "  ?prov rdfs:label ?provLabel .\n" +
                "  FILTER(LANG(?provLabel) IN (\"tr\", \"en\") && CONTAINS(LCASE(STR(?provLabel)), \"" + provinceLower + "\"))\n" +
                "  OPTIONAL { ?city wdt:P1082 ?population . }\n" +
                "  OPTIONAL {\n" +
                "    ?city p:P625 ?coord .\n" +
                "    ?coord psv:P625 ?cv .\n" +
                "    ?cv wikibase:geoLatitude  ?lat .\n" +
                "    ?cv wikibase:geoLongitude ?lon .\n" +
                "  }\n" +
                "  SERVICE wikibase:label { bd:serviceParam wikibase:language \"tr,en\" . }\n" +
                "}\n" +
                "ORDER BY DESC(?population)\n" +
                "LIMIT 1";
        }

        // ─────────────────────────────────────────────────────────────────
        // JSON parser
        // ─────────────────────────────────────────────────────────────────
        private static WikidataResult? ParseResult(string json, string originalName)
        {
            using var doc = JsonDocument.Parse(json);
            var bindings  = doc.RootElement
                .GetProperty("results")
                .GetProperty("bindings");

            if (bindings.GetArrayLength() == 0)
                return null;

            var row    = bindings[0];
            var result = new WikidataResult
            {
                CityName = GetStringValue(row, "cityLabel") ?? originalName
            };

            if (TryGetLong(row, "population", out var pop))
                result.Population = pop;

            if (TryGetDouble(row, "lat", out var lat))
                result.Latitude = lat;

            if (TryGetDouble(row, "lon", out var lon))
                result.Longitude = lon;

            return result;
        }

        // ── Yardımcılar ──────────────────────────────────────────────────

        private static string? GetStringValue(JsonElement row, string key)
        {
            if (row.TryGetProperty(key, out var el) &&
                el.TryGetProperty("value", out var val))
                return val.GetString();
            return null;
        }

        private static bool TryGetLong(JsonElement row, string key, out long value)
        {
            value = 0;
            var raw = GetStringValue(row, key);
            if (raw is null) return false;
            return long.TryParse(raw.Split('^')[0],
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out value);
        }

        private static bool TryGetDouble(JsonElement row, string key, out double value)
        {
            value = 0;
            var raw = GetStringValue(row, key);
            if (raw is null) return false;
            return double.TryParse(raw.Split('^')[0],
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out value);
        }
    }
}
