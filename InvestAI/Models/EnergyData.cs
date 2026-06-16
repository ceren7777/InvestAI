using System.ComponentModel.DataAnnotations.Schema;

namespace InvestAI.Models
{
    public class EnergyData
    {
        [Column("il")]
        public string Il { get; set; }

        [Column("ges_mw")]
        public double GesMw { get; set; }

        [Column("res_mw")]
        public double ResMw { get; set; }

        [Column("toplam_mw")]
        public double ToplamMw { get; set; }

        [Column("yil")]
        public int Yil { get; set; }

        [Column("elektrik_erisim_orani")]
        public double ElektrikErisimOrani { get; set; }
    }
}