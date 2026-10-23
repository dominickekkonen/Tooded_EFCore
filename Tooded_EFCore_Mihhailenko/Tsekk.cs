using System;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Tooded_EFCore_Mihhailenko
{
    /// <summary>
    /// SMTP settings for the sending mailbox. Fill these in before running.
    /// Gmail: turn on 2-step verification, then create an "App password" (Google Account -> Security)
    /// and put that 16-character password below. NEVER use your normal password here.
    /// This file contains a secret, so add "PostiSeaded.cs" to .gitignore and do not commit it.
    /// </summary>
    internal static class PostiSeaded
    {
        public static readonly string Server = "smtp.gmail.com";
        public static readonly int Port = 587;                          // STARTTLS
        public static readonly string SmtpKasutaja = "SIIA-OMA-POOD@gmail.com";
        public static readonly string Parool = "SIIA-RAKENDUSE-PAROOL";
        public static readonly string SaatjaNimi = "Tooded pood";
    }

    /// <summary>Builds the receipt (all items, prices and the total) and e-mails it to the buyer.</summary>
    internal static class TsekiPost
    {
        private static readonly CultureInfo Et = new CultureInfo("et-EE");

        private static string Raha(decimal summa)
        {
            return summa.ToString("0.00", Et) + " €";
        }

        private static string Html(string tekst)
        {
            return WebUtility.HtmlEncode(tekst);
        }

        /// <summary>Returns null when the e-mail was sent, otherwise a short error text.</summary>
        public static async Task<string?> SaadaAsync(Kasutaja ostja, Tellimus tellimus)
        {
            if (PostiSeaded.SmtpKasutaja.StartsWith("SIIA", StringComparison.Ordinal) ||
                PostiSeaded.Parool.StartsWith("SIIA", StringComparison.Ordinal))
            {
                return "E-posti seaded on täitmata (vaata klassi PostiSeaded failis Tsekk.cs).";
            }

            try
            {
                using (var smtp = new SmtpClient(PostiSeaded.Server, PostiSeaded.Port))
                {
                    smtp.EnableSsl = true;
                    smtp.Credentials = new NetworkCredential(PostiSeaded.SmtpKasutaja, PostiSeaded.Parool);
                    smtp.Timeout = 20000;

                    using (var kiri = new MailMessage())
                    {
                        kiri.From = new MailAddress(PostiSeaded.SmtpKasutaja, PostiSeaded.SaatjaNimi, Encoding.UTF8);
                        kiri.To.Add(new MailAddress(ostja.Epost, ostja.TaisNimi, Encoding.UTF8));
                        kiri.Subject = "Tšekk nr " + tellimus.Id + " – Tooded pood";
                        kiri.SubjectEncoding = Encoding.UTF8;
                        kiri.BodyEncoding = Encoding.UTF8;
                        kiri.IsBodyHtml = true;
                        kiri.Body = EhitaHtml(ostja, tellimus);

                        await smtp.SendMailAsync(kiri);
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                return ex.GetBaseException().Message;
            }
        }

        private static string EhitaHtml(Kasutaja ostja, Tellimus tellimus)
        {
            string aeg = tellimus.Aeg.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);
            const string lahter = "padding:8px 10px;border-bottom:1px solid #DCE3DC;";

            var sb = new StringBuilder();
            sb.Append("<div style=\"font-family:'Segoe UI',Arial,sans-serif;color:#222B22;max-width:600px\">");
            sb.Append("<h2 style=\"color:#2E9E4F;margin:0 0 4px 0\">Tooded pood</h2>");
            sb.Append("<p style=\"margin:0 0 16px 0;color:#6B776B\">Tšekk nr " + tellimus.Id + " &middot; " + aeg + "</p>");
            sb.Append("<p>Tere, " + Html(ostja.Eesnimi) + "! Aitäh ostu eest. Sinu tellimus:</p>");

            sb.Append("<table style=\"border-collapse:collapse;width:100%;font-size:14px\">");
            sb.Append("<tr style=\"background:#E8F5EC;text-align:left\">");
            sb.Append("<th style=\"padding:8px 10px\">Toode</th>");
            sb.Append("<th style=\"padding:8px 10px;text-align:right\">Kogus</th>");
            sb.Append("<th style=\"padding:8px 10px;text-align:right\">Hind</th>");
            sb.Append("<th style=\"padding:8px 10px;text-align:right\">Summa</th>");
            sb.Append("</tr>");

            foreach (var rida in tellimus.Read)
            {
                sb.Append("<tr>");
                sb.Append("<td style=\"" + lahter + "\">" + Html(rida.ToodeNimi) + "</td>");
                sb.Append("<td style=\"" + lahter + "text-align:right\">" + rida.Kogus + " tk</td>");
                sb.Append("<td style=\"" + lahter + "text-align:right\">" + Raha(rida.Hind) + "</td>");
                sb.Append("<td style=\"" + lahter + "text-align:right\">" + Raha(rida.Hind * rida.Kogus) + "</td>");
                sb.Append("</tr>");
            }

            sb.Append("<tr>");
            sb.Append("<td colspan=\"3\" style=\"padding:12px 10px;text-align:right;font-weight:bold\">Kokku</td>");
            sb.Append("<td style=\"padding:12px 10px;text-align:right;font-weight:bold;font-size:16px;color:#E4572E\">" +
                      Raha(tellimus.Summa) + "</td>");
            sb.Append("</tr>");
            sb.Append("</table>");

            sb.Append("<p style=\"margin-top:20px;color:#6B776B;font-size:12px\">See on automaatselt saadetud tšekk.</p>");
            sb.Append("</div>");
            return sb.ToString();
        }
    }
}