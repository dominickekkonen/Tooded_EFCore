using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Tooded_EFCore_Mihhailenko
{
    /// <summary>
    /// Builds the receipt (all items, prices and the total) and e-mails it to the buyer.
    /// Needs the NuGet package "MailKit" (Package Manager Console: Install-Package MailKit).
    /// </summary>
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
            if (string.IsNullOrWhiteSpace(ostja.Epost))
            {
                return "Kasutajal " + ostja.Kasutajanimi + " puudub e-posti aadress.";
            }

            // The sender's login is read from a settings file outside the project (see PostiSeaded.cs).
            string seadeteViga;
            if (!PostiSeaded.Lae(out seadeteViga))
            {
                return "Ostja aadress on olemas (" + ostja.Epost + "), aga saatja seaded ei ole valmis.\n" + seadeteViga;
            }

            MimeMessage kiri;
            try
            {
                kiri = new MimeMessage();
                kiri.From.Add(new MailboxAddress(PostiSeaded.SaatjaNimi, PostiSeaded.SmtpKasutaja));
                kiri.To.Add(new MailboxAddress(ostja.TaisNimi, ostja.Epost));
                kiri.Subject = "Tšekk nr " + tellimus.Id + " – Tooded pood";
                kiri.Body = new TextPart("html") { Text = EhitaHtml(ostja, tellimus) };
            }
            catch (Exception ex)
            {
                return "Kirja koostamine ebaõnnestus: " + ex.GetBaseException().Message;
            }

            // First the configured port, then the other usual secure port of the same server.
            var katsed = new List<(int port, SecureSocketOptions turva)>
            {
                (PostiSeaded.Port, PostiSeaded.Port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls)
            };
            if (PostiSeaded.Port == 587) katsed.Add((465, SecureSocketOptions.SslOnConnect));
            else if (PostiSeaded.Port == 465) katsed.Add((587, SecureSocketOptions.StartTls));

            var vead = new List<string>();

            foreach (var (port, turva) in katsed)
            {
                try
                {
                    using (var klient = new SmtpClient())
                    {
                        klient.Timeout = 10000;
                        await klient.ConnectAsync(PostiSeaded.Server, port, turva);
                        await klient.AuthenticateAsync(PostiSeaded.SmtpKasutaja, PostiSeaded.Parool);
                        await klient.SendAsync(kiri);
                        await klient.DisconnectAsync(true);
                    }
                    return null;
                }
                catch (AuthenticationException ex)
                {
                    // Connected fine, but the server did not accept the login: no point trying another port.
                    return "Saatja kontole sisselogimine ebaõnnestus (kontrolli Gmail-i aadressi ja rakenduse parooli " +
                           "seadete failis):\n" + ex.Message;
                }
                catch (SmtpCommandException ex)
                {
                    // Connected and logged in, but the server refused the message (e.g. bad recipient address).
                    return "Meiliserver keeldus kirja vastu võtmast (" + ex.StatusCode + "):\n" + ex.Message;
                }
                catch (Exception ex)
                {
                    string tekst = ex.GetBaseException().Message;

                    // Google answers a wrong login with "5.7.8 Username and Password not accepted" (MailKit sometimes
                    // reports it as a "disconnected" error). That is a login problem, not a network problem.
                    if (tekst.Contains("5.7.8") || tekst.Contains("BadCredentials") ||
                        tekst.Contains("Username and Password not accepted"))
                    {
                        return "Google ei võtnud saatja sisselogimist vastu (aadress või parool on vale).\n" +
                               "Kontrolli seadete failis Gmail-i aadressi ja 16-kohalist rakenduse parooli " +
                               "(mitte tavalist parooli).\nFail: " + PostiSeaded.Fail + "\n\n" + tekst;
                    }

                    vead.Add("port " + port + ": " + tekst);
                }
            }

            return "Ühendus meiliserveriga " + PostiSeaded.Server + " ebaõnnestus.\n" +
                   string.Join("\n", vead) +
                   "\n\nKui kooli võrk blokeerib e-posti, proovi kodus või telefoni mobiilse leviala kaudu.";
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