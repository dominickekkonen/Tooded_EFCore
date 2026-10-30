using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Tooded_EFCore_Mihhailenko
{
    /// <summary>The shape of the settings file postiseaded.json.</summary>
    internal sealed class PostiAndmed
    {
        public string Server { get; set; } = "smtp.gmail.com";
        public int Port { get; set; } = 587;
        public string SmtpKasutaja { get; set; } = "SIIA-OMA-GMAIL@gmail.com";
        public string Parool { get; set; } = "SIIA-RAKENDUSE-PAROOL";
        public string SaatjaNimi { get; set; } = "Tooded pood";
    }

    /// <summary>
    /// Login of the mailbox the receipts are sent FROM. This is not the buyer's address (that comes from the
    /// database table Kasutajatabel). The values are read from a file outside the project, so no password is
    /// stored in the code or in Git:  %AppData%\ToodedPood\postiseaded.json
    /// The file is created automatically the first time a receipt is sent.
    /// </summary>
    internal static class PostiSeaded
    {
        public static string Server { get; private set; } = "smtp.gmail.com";
        public static int Port { get; private set; } = 587;
        public static string SmtpKasutaja { get; private set; } = "";
        public static string Parool { get; private set; } = "";
        public static string SaatjaNimi { get; private set; } = "Tooded pood";

        public static string Fail
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "ToodedPood", "postiseaded.json");
            }
        }

        /// <summary>Loads the settings. Returns false (and an explanation in viga) when they are not ready.</summary>
        public static bool Lae(out string viga)
        {
            viga = "";
            try
            {
                if (!File.Exists(Fail))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(Fail)!);
                    string mall = JsonSerializer.Serialize(new PostiAndmed(),
                        new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(Fail, mall, new UTF8Encoding(false));
                    AvaMarkmikus();

                    viga = "Saatja seadete faili ei olnud, lõin selle ja avasin Notepadis. Sisesta saatja Gmail-i " +
                           "aadress ja rakenduse parool, salvesta fail ning tee ost uuesti.\nFail: " + Fail;
                    return false;
                }

                PostiAndmed? andmed = JsonSerializer.Deserialize<PostiAndmed>(
                    File.ReadAllText(Fail, Encoding.UTF8),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (andmed == null || TuhiVoiMall(andmed.SmtpKasutaja) || TuhiVoiMall(andmed.Parool))
                {
                    viga = "Saatja e-posti seaded on täitmata. Ava fail, sisesta saatja Gmail-i aadress ja " +
                           "rakenduse parool ning salvesta.\nFail: " + Fail;
                    return false;
                }

                Server = andmed.Server.Trim();
                Port = andmed.Port;
                SmtpKasutaja = andmed.SmtpKasutaja.Trim();
                Parool = andmed.Parool.Replace(" ", "");   // Google shows app passwords in groups of 4
                SaatjaNimi = andmed.SaatjaNimi;
                return true;
            }
            catch (Exception ex)
            {
                viga = "Seadete faili lugemine ebaõnnestus.\nFail: " + Fail + "\n" + ex.GetBaseException().Message;
                return false;
            }
        }

        private static bool TuhiVoiMall(string? vaartus)
        {
            return string.IsNullOrWhiteSpace(vaartus) || vaartus.StartsWith("SIIA", StringComparison.Ordinal);
        }

        private static void AvaMarkmikus()
        {
            try
            {
                Process.Start(new ProcessStartInfo("notepad.exe", "\"" + Fail + "\"") { UseShellExecute = true });
            }
            catch
            {
                // Not important: the file path is in the message anyway.
            }
        }
    }
}