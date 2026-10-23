using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace Tooded_EFCore_Mihhailenko
{
    /// <summary>Colours, fonts and small styling helpers. Change the colours here to re-theme the whole shop.</summary>
    internal static class Teema
    {
        public static readonly Color Aksent = ColorTranslator.FromHtml("#2E9E4F");
        public static readonly Color AksentTume = ColorTranslator.FromHtml("#237A3B");
        public static readonly Color AksentHele = ColorTranslator.FromHtml("#E8F5EC");
        public static readonly Color Hind = ColorTranslator.FromHtml("#E4572E");
        public static readonly Color Taust = ColorTranslator.FromHtml("#F3F5F3");
        public static readonly Color Kaart = Color.White;
        public static readonly Color Joon = ColorTranslator.FromHtml("#DCE3DC");
        public static readonly Color Tekst = ColorTranslator.FromHtml("#222B22");
        public static readonly Color TekstHall = ColorTranslator.FromHtml("#6B776B");

        public static Font F(float suurus, FontStyle stiil = FontStyle.Regular)
        {
            return new Font("Segoe UI", suurus, stiil);
        }

        public static void NupuStiil(Button b, bool esmane = true)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = esmane ? 0 : 1;
            b.FlatAppearance.BorderColor = Aksent;
            b.BackColor = esmane ? Aksent : Color.White;
            b.ForeColor = esmane ? Color.White : Aksent;
            b.FlatAppearance.MouseOverBackColor = esmane ? AksentTume : AksentHele;
            b.Font = F(10, FontStyle.Bold);
            b.Cursor = Cursors.Hand;
            b.UseVisualStyleBackColor = false;
        }

        /// <summary>Fixed-size label (AutoSize off) so long text is cut with "…" instead of growing.</summary>
        public static Label Silt(string tekst, int x, int y, int w, int h, Font font, Color varv,
                                 ContentAlignment joondus = ContentAlignment.TopLeft)
        {
            return new Label
            {
                Text = tekst,
                Location = new Point(x, y),
                Size = new Size(w, h),
                Font = font,
                ForeColor = varv,
                TextAlign = joondus,
                AutoEllipsis = true,
                AutoSize = false
            };
        }
    }

    /// <summary>Loads product pictures from the Images folder next to the .exe (without locking the files).</summary>
    internal static class PildiAbi
    {
        public static string Kaust
        {
            get { return Path.Combine(Application.StartupPath, "Images"); }
        }

        private static readonly Dictionary<string, Image> _cache =
            new Dictionary<string, Image>(StringComparer.OrdinalIgnoreCase);
        private static Image? _kohahoidja;

        public static Image Kohahoidja
        {
            get { return _kohahoidja ??= TeeKohahoidja(); }
        }

        public static Image? Lae(string? failinimi)
        {
            if (string.IsNullOrWhiteSpace(failinimi)) return null;

            Image? olemas;
            if (_cache.TryGetValue(failinimi, out olemas)) return olemas;

            string tee = Path.Combine(Kaust, failinimi);
            if (!File.Exists(tee)) return null;

            try
            {
                using (var fs = new FileStream(tee, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var originaal = Image.FromStream(fs))
                {
                    var koopia = new Bitmap(originaal);
                    _cache[failinimi] = koopia;
                    return koopia;
                }
            }
            catch
            {
                return null;
            }
        }

        public static void TuhjendaCache()
        {
            _cache.Clear();
        }

        private static Image TeeKohahoidja()
        {
            var bmp = new Bitmap(180, 130);
            using (var g = Graphics.FromImage(bmp))
            using (var font = Teema.F(10))
            using (var pintsel = new SolidBrush(Teema.TekstHall))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                g.Clear(ColorTranslator.FromHtml("#EEF2EE"));
                g.DrawString("Pilt puudub", font, pintsel, new RectangleF(0, 0, 180, 130), sf);
            }
            return bmp;
        }
    }

    /// <summary>Password hashing (PBKDF2 + random salt). Passwords are never stored as plain text.</summary>
    internal static class Salasona
    {
        public static void Loo(string parool, out string hash, out string sool)
        {
            byte[] soolBaidid = RandomNumberGenerator.GetBytes(16);
            sool = Convert.ToBase64String(soolBaidid);
            hash = Arvuta(parool, soolBaidid);
        }

        public static bool Kontrolli(string parool, string hash, string sool)
        {
            try
            {
                byte[] oodatud = Convert.FromBase64String(hash);
                byte[] saadud = Convert.FromBase64String(Arvuta(parool, Convert.FromBase64String(sool)));
                return CryptographicOperations.FixedTimeEquals(oodatud, saadud);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static string Arvuta(string parool, byte[] sool)
        {
            byte[] baidid = Rfc2898DeriveBytes.Pbkdf2(parool, sool, 100000, HashAlgorithmName.SHA256, 32);
            return Convert.ToBase64String(baidid);
        }
    }
}