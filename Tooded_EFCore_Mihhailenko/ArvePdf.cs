using PdfSharpCore.Drawing;
using PdfSharpCore.Fonts;
using PdfSharpCore.Pdf;
using System.Drawing;

namespace Tooded_EFCore_Mihhailenko;

public static class ArvePdf
{
    private sealed class ArveFont(string folder) : IFontResolver
    {
        public string DefaultFontName => "ArveArial";

        public FontResolverInfo ResolveTypeface(
            string name, bool bold, bool italic)
        {
            return new FontResolverInfo(
                bold ? "ArveBold" : "ArveRegular");
        }

        public byte[] GetFont(string name)
        {
            string file = name == "ArveBold"
                ? "arialbd.ttf"
                : "arial.ttf";

            return File.ReadAllBytes(Path.Combine(folder, file));
        }
    }

    private static readonly object fontLock = new();
    private static bool fontsReady;

    public static string ArveteKaust => Path.Combine(
        Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData),
        "ToodedApp",
        "Arved");

    public static string Loo(
        Kasutaja buyer,
        Tellimus order,
        string? fontFolder = null)
    {
        if (order.Read.Count == 0)
            throw new ArgumentException("Arvel puuduvad read.");

        if (order.Read.Sum(r => r.Hind * r.Kogus) != order.Summa)
            throw new ArgumentException(
                "Arve kogusumma ei ühti ridadega.");

        lock (fontLock)
        {
            if (!fontsReady)
            {
                string folder = fontFolder ??
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.Fonts);

                GlobalFontSettings.FontResolver =
                    new ArveFont(folder);

                fontsReady = true;
            }
        }

        Directory.CreateDirectory(ArveteKaust);

        string path = Path.Combine(
            ArveteKaust,
            $"Arve_{order.Id}_{DateTime.Now:yyyyMMdd_HHmmss_fff}_{Guid.NewGuid():N}.pdf");

        using var document = new PdfDocument();
        document.Info.Title = $"Arve {order.Id}";

        var normal = new XFont(
            "ArveArial", 10, XFontStyle.Regular,
            new XPdfFontOptions(PdfFontEncoding.Unicode));

        var bold = new XFont(
            "ArveArial", 12, XFontStyle.Bold,
            new XPdfFontOptions(PdfFontEncoding.Unicode));

        XGraphics? graphics = null;
        double y = 0;

        void NewPage()
        {
            graphics?.Dispose();

            var page = document.AddPage();
            page.Size = PdfSharpCore.PageSize.A4;
            graphics = XGraphics.FromPdfPage(page);

            graphics.DrawString(
                $"ARVE nr {order.Id}", bold, XBrushes.Black, 40, 42);

            graphics.DrawString(
                $"Kuupäev: {order.Aeg:dd.MM.yyyy HH:mm}",
                normal, XBrushes.Black, 40, 64);

            graphics.DrawString(
                $"Ostja: {buyer.TaisNimi}",
                normal, XBrushes.Black, 40, 84);

            graphics.DrawString(
                $"E-post: {buyer.Epost}",
                normal, XBrushes.Black, 40, 104);

            graphics.DrawString(
                "Toode", bold, XBrushes.Black, 40, 140);
            graphics.DrawString(
                "Kogus", bold, XBrushes.Black, 335, 140);
            graphics.DrawString(
                "Hind €", bold, XBrushes.Black, 405, 140);
            graphics.DrawString(
                "Summa €", bold, XBrushes.Black, 485, 140);

            y = 160;
        }

        try
        {
            NewPage();

            foreach (var row in order.Read)
            {
                // Wrap long product names.
                var lines = new List<string>();
                string remaining = row.ToodeNimi;

                while (remaining.Length > 0)
                {
                    int count = 1;

                    while (count < remaining.Length &&
                           graphics!.MeasureString(
                               remaining[..(count + 1)], normal).Width <= 275)
                    {
                        count++;
                    }

                    if (count < remaining.Length)
                    {
                        int space = remaining.LastIndexOf(
                            ' ', count - 1, count);

                        if (space > 0)
                            count = space;
                    }

                    lines.Add(remaining[..count].Trim());
                    remaining = remaining[count..].TrimStart();
                }

                double height = Math.Max(
                    30, lines.Count * 14 + 10);

                if (y + height > 760)
                    NewPage();

                for (int i = 0; i < lines.Count; i++)
                {
                    graphics!.DrawString(
                        lines[i], normal, XBrushes.Black,
                        40, y + i * 14);
                }

                graphics!.DrawString(
                    row.Kogus.ToString(),
                    normal, XBrushes.Black, 340, y);

                graphics.DrawString(
                    row.Hind.ToString("F2"),
                    normal, XBrushes.Black, 405, y);

                graphics.DrawString(
                    (row.Kogus * row.Hind).ToString("F2"),
                    normal, XBrushes.Black, 485, y);

                y += height;
            }

            if (y > 750)
                NewPage();

            graphics!.DrawString(
                $"Kokku: {order.Summa:F2} €",
                bold, XBrushes.Black, 340, y + 12);
        }
        finally
        {
            graphics?.Dispose();
        }

        document.Save(path);
        return path;
    }
}