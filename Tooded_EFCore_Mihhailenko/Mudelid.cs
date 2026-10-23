using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tooded_EFCore_Mihhailenko
{
    [Table("Kasutajatabel")]
    public class Kasutaja
    {
        public int Id { get; set; }
        public string Eesnimi { get; set; } = "";
        public string Perenimi { get; set; } = "";
        public string Epost { get; set; } = "";
        public string Kasutajanimi { get; set; } = "";
        public string ParoolHash { get; set; } = "";
        public string Sool { get; set; } = "";
        public string Roll { get; set; } = "Müüja";
        public DateTime Loodud { get; set; } = DateTime.Now;

        [NotMapped]
        public string TaisNimi => Eesnimi + " " + Perenimi;
    }

    [Table("Tellimustabel")]
    public class Tellimus
    {
        public int Id { get; set; }
        public int KasutajaId { get; set; }
        public Kasutaja? Kasutaja { get; set; }
        public DateTime Aeg { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(10,2)")]
        public decimal Summa { get; set; }

        public List<TellimuseRida> Read { get; set; } = new List<TellimuseRida>();
    }

    [Table("Tellimusereatabel")]
    public class TellimuseRida
    {
        public int Id { get; set; }
        public int TellimusId { get; set; }
        public Tellimus? Tellimus { get; set; }
        public int? ToodeId { get; set; }
        public string ToodeNimi { get; set; } = "";
        public int Kogus { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Hind { get; set; }
    }
}