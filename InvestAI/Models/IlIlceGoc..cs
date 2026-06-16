using System.ComponentModel.DataAnnotations.Schema;

namespace InvestAI.Models
{
    public class IlIlceGoc
    {
        [Column("yil")]
        public int Yil { get; set; }

        [Column("il")]
        public string Il { get; set; }

        [Column("ilce")]
        public string Ilce { get; set; }

        [Column("net_goc")]
        public int NetGoc { get; set; }

        [Column("goc_durumu")]
        public string GocDurumu { get; set; }
    }
}