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

            Application.Run(new LoginForm());
        }
    }
}