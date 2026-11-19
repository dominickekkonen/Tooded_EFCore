using MailKit.Security;
using MimeKit;

namespace Tooded_EFCore_Mihhailenko;
internal static class TsekiPost
{
    public static async Task<string?> SaadaAsync(
    Kasutaja ostja,
    Tellimus tellimus,
    string pdfFail,
    string saaja)
    {
        if (!File.Exists(pdfFail))
            return "Genereeri enne PDF-arve.";

        if (!System.Net.Mail.MailAddress.TryCreate(saaja, out _))
            return "Sisesta korrektne saaja e-posti aadress.";

        if (!PostiSeaded.Lae(out string viga))
            return viga;

        try
        {
            using var kiri = KoostaPdfKiri(
                ostja,
                tellimus,
                pdfFail,
                saaja,
                PostiSeaded.SmtpKasutaja,
                PostiSeaded.SaatjaNimi);

            using var klient = new SmtpClient
            {
                Timeout = 15000
            };

            await klient.ConnectAsync(
                PostiSeaded.Server,
                PostiSeaded.Port,
                PostiSeaded.Port == 465
                    ? SecureSocketOptions.SslOnConnect
                    : SecureSocketOptions.StartTls);

            await klient.AuthenticateAsync(
                PostiSeaded.SmtpKasutaja,
                PostiSeaded.Parool);

            await klient.SendAsync(kiri);
            await klient.DisconnectAsync(true);

            return null;
        }
        catch (Exception ex)
        {
            return "Arve saatmine ebaõnnestus: " +
                   ex.GetBaseException().Message;
        }
    }

    public static MimeMessage KoostaPdfKiri(
        Kasutaja ostja,
        Tellimus tellimus,
        string pdfFail,
        string saaja,
        string saatja,
        string saatjaNimi)
    {
        var kiri = new MimeMessage();

        kiri.From.Add(new MailboxAddress(saatjaNimi, saatja));
        kiri.To.Add(MailboxAddress.Parse(saaja));

        kiri.Subject =
            "Arve nr " + tellimus.Id + " – Tooded pood";

        var sisu = new BodyBuilder
        {
            HtmlBody = EhitaHtml(ostja, tellimus)
        };

        sisu.Attachments.Add(
            pdfFail,
            new ContentType("application", "pdf"));

        kiri.Body = sisu.ToMessageBody();
        return kiri;
    }
}
