using System.ComponentModel.DataAnnotations.Schema;

namespace InvestAI.Models
{
    public class DogalgazAnaHat
    {
        [Column("il")]
        public string? Il { get; set; }

        [Column("ana_iletim_hatti_var_mi")]
        public bool AnaIletimHattiVarMi { get; set; }

        [Column("kaynak")]
        public string? Kaynak { get; set; }

        [Column("yil")]
        public int Yil { get; set; }
    }
}