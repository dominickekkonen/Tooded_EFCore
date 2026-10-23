using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tooded_EFCore_Mihhailenko
{
    public class Toode
    {
        public int Id { get; set; }
        public string Toodenimetus { get; set; }
        public int Kogus { get; set; }
        public decimal Hind { get; set; }
        public string Pilt { get; set; }

        public int KategooriaId { get; set; }
        public Kategooria Kategooria { get; set; }
    }
}
