using System.ComponentModel.DataAnnotations.Schema;

namespace InvestAI.Models
{
    public class OsbPresence
    {
        [Column("il")]
        public string Il { get; set; }

        [Column("ilce")]
        public string Ilce { get; set; }

        [Column("osb_adi")]
        public string OsbAdi { get; set; }

        [Column("durum")]
        public string Durum { get; set; }

        [Column("osb_kapasite")]
        public int OsbKapasite { get; set; }
    }
}