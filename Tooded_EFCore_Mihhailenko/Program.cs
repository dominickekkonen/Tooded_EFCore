using Tooded_EFCore_Mihhailenko;

namespace ToodedEFCore
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
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Andmebaasi seadistamine ebaõnnestus.\n" + ex.Message,
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

                using var pood = new PoodForm(login.Kasutaja);
                Application.Run(pood);

                // Return to login only when "Logi välja" was clicked.
                if (!pood.LogiValja)
                {
                    return;
                }
            }
        }
    }
}