using System;
using System.Windows.Forms;

namespace Tooded_EFCore_Mihhailenko
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // 1) Login / registration first. 2) The store opens only after a successful login.
            // "Logi välja" in the store returns here and shows the login window again.
            while (true)
            {
                using (var login = new LoginForm())
                {
                    if (login.ShowDialog() != DialogResult.OK || login.Kasutaja == null) break;

                    using (var pood = new PoodForm(login.Kasutaja))
                    {
                        Application.Run(pood);
                        if (!pood.LogiValja) break;
                    }
                }
            }
        }
    }
}