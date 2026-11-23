namespace Tooded_EFCore
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            try
            {
                DatabaseSetup.Initialize();

                using var db = new ToodedContext();
                KasutajaOigused.LooAdmin(db);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Andmebaasi seadistamine ebaõnnestus.\n" +
                    ex.GetBaseException().Message,
                    "Andmebaasi viga",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            while (true)
            {
                using var login = new LoginForm();

                if (login.ShowDialog() != DialogResult.OK ||
                    login.Kasutaja == null)
                {
                    return;
                }

                AktiivneKasutaja.LogiSisse(login.Kasutaja);

                using var pood = new PoodForm(login.Kasutaja);
                Application.Run(pood);

                AktiivneKasutaja.LogiValja();

                if (!pood.LogiValja)
                    return;
            }
        }
    }
}