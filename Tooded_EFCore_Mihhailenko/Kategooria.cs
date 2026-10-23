using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tooded_EFCore_Mihhailenko
{
    public class Kategooria
    {
        public int Id { get; set; }
        public string Kategooria_nimetus { get; set; }
        public string? Kirjeldus { get; set; }

        public ICollection<Toode> Tooded { get; set; }
    }
}
