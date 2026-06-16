namespace InvestAI.Models
{

    using System.ComponentModel.DataAnnotations.Schema;

    namespace InvestAI.Models
    {
        public class IlIlceIssizlik
        {
            [Column("id")]
            public int Id { get; set; }

            [Column("il")]
            public string Il { get; set; }

            [Column("ilce")]
            public string Ilce { get; set; }

            [Column("yil")]
            public int Yil { get; set; }

            [Column("issizlik_orani")]
            public decimal IssizlikOrani { get; set; }
        }
    }
}