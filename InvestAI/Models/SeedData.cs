using System.IO;
using System.Linq;

namespace InvestAI.Models
{
    public static class SeedData
    {
        public static void LoadDistrictPopulations(AppDbContext context, IWebHostEnvironment env)
        {
            if(context.DistrictPopulations.Any());
            {
                Console.WriteLine("Tabloda veri zaten var.");
                return;
            }

            // Dinamik yol - sunucuda da çalışır
            var filePath = Path.Combine(env.WebRootPath, "data", "turkiye_ilce_nufus.csv");
            Console.WriteLine("CSV yolu: " + filePath);

            if (!File.Exists(filePath))
            {
                Console.WriteLine("CSV bulunamadı.");
                return;
            }

            var lines = File.ReadAllLines(filePath).Skip(1); // başlık satırını atla
            int addedCount = 0;

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split(',');
                if (parts.Length < 9) continue;

                // Sütun sırası: plaka(0), il(1), bolge(2), il_nufus(3),
                //               il_erkek(4), il_kadin(5), ilce(6), ilce_nufus(7), ...
                var city = parts[1].Trim(); // il_adi
                var district = parts[6].Trim(); // ilce_adi

                if (!int.TryParse(parts[7].Trim(), out int population)) // ilce_nufus
                    continue;

                context.DistrictPopulations.Add(new DistrictPopulation
                {
                    City = city,
                    District = district,
                    Population = population
                });
                addedCount++;
            }

            context.SaveChanges();
            Console.WriteLine($"Yüklenen kayıt sayısı: {addedCount}");
        }
    }
}