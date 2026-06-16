using System.ComponentModel.DataAnnotations.Schema;

namespace InvestAI.Models
{
    public class DistrictEducation
    {
        [Column("il")]
        public string Il { get; set; }

        [Column("ilce")]
        public string Ilce { get; set; }

        [Column("ilkogretim_orani")]
        public decimal IlkogretimOrani { get; set; }

        [Column("lise_orani")]
        public decimal LiseOrani { get; set; }

        [Column("universite_orani")]
        public decimal UniversiteOrani { get; set; }

        [Column("egitim_puani")]
        public decimal EgitimPuani { get; set; }
    }
}