using System.Diagnostics;
using System.Net.Mail;

namespace Tooded_EFCore_Mihhailenko;

public class ArveForm : Form
{
    private readonly Kasutaja buyer;
    private readonly Dictionary<int, int> cart;
    private readonly List<Toode> products;

    private readonly ListBox list = new();
    private readonly Label total = new();
    private readonly Label status = new();
    private readonly TextBox email = new();

    private readonly Button remove = new();
    private readonly Button send = new();
    private readonly Button open = new();

    private Tellimus? savedOrder;
    private string? pdf;

    public ArveForm(
        Kasutaja kasutaja,
        Dictionary<int, int> korv,
        List<Toode> tooted)
    {
        buyer = kasutaja;
        cart = korv;
        products = tooted;

        Text = "Ostukorv ja PDF-arve";
        ClientSize = new Size(850, 440);
        StartPosition = FormStartPosition.CenterParent;

        list.SetBounds(20, 20, 510, 330);
        list.HorizontalScrollbar = true;

        total.SetBounds(20, 365, 510, 30);
        total.Font = new Font(Font, FontStyle.Bold);

        email.SetBounds(550, 20, 280, 28);
        email.PlaceholderText = "Saaja e-post";
        email.Text = buyer.Epost;

        AddButton(
            remove, "Eemalda valitud rida", 70, RemoveRow);

        AddButton(
            new Button(),
            "Kinnita ost / genereeri PDF",
            120,
            Generate);

        AddButton(open, "Ava PDF", 170, () =>
        {
            if (pdf != null)
            {
                Process.Start(new ProcessStartInfo(pdf)
                {
                    UseShellExecute = true
                });
            }
        });

        AddButton(send, "Saada e-postiga", 220, null);
        send.Click += async (_, _) => await Send();

        status.SetBounds(550, 280, 280, 130);
        status.AutoEllipsis = true;

        Controls.AddRange(new Control[]
        {
            list, total, email, status
        });

        RefreshRows();
    }

    private void AddButton(
        Button button,
        string text,
        int y,
        Action? action)
    {
        button.Text = text;
        button.SetBounds(550, y, 280, 38);

        if (action != null)
        {
            button.Click += (_, _) =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    RefreshRows();
                    MessageBox.Show(
                        ex.GetBaseException().Message, "Viga");
                }
            };
        }

        Controls.Add(button);
    }

    private void RefreshRows()
    {
        list.Items.Clear();

        if (savedOrder != null)
        {
            foreach (var row in savedOrder.Read)
            {
                list.Items.Add(
                    $"{row.ToodeNimi} – {row.Kogus} tk × " +
                    $"{row.Hind:F2} € = " +
                    $"{row.Kogus * row.Hind:F2} €");
            }

            total.Text = $"Kokku: {savedOrder.Summa:F2} €";
        }
        else
        {
            decimal sum = 0;

            foreach (var item in cart)
            {
                var product = products.FirstOrDefault(
                    t => t.Id == item.Key);

                if (product == null)
                    continue;

                list.Items.Add(new CartRow(
                    product.Id,
                    $"{product.Toodenimetus} – {item.Value} tk × " +
                    $"{product.Hind:F2} € = " +
                    $"{item.Value * product.Hind:F2} €"));

                sum += item.Value * product.Hind;
            }

            total.Text = $"Kokku: {sum:F2} €";
        }

        remove.Enabled = savedOrder == null;
        send.Enabled = open.Enabled = pdf != null;
    }

    private sealed record CartRow(int Id, string Text)
    {
        public override string ToString() => Text;
    }

    private void RemoveRow()
    {
        if (savedOrder == null &&
            list.SelectedItem is CartRow row)
        {
            cart.Remove(row.Id);
            RefreshRows();
        }
    }

    private void Generate()
    {
        if (savedOrder == null)
        {
            if (cart.Count == 0)
            {
                MessageBox.Show("Ostukorv on tühi.");
                return;
            }

            if (MessageBox.Show(
                "Kas kinnitan ostu ja vähendan laoseisu?",
                "Kinnitus",
                MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return;
            }

            using var db = new ToodedContext();

            savedOrder = ArveOst.Salvesta(
                db, buyer, cart, products);

            cart.Clear();

            status.Text =
                $"Tellimus nr {savedOrder.Id} on salvestatud. " +
                "PDF-i saab vajadusel uuesti genereerida.";
        }

        // Repeated generation reuses the saved order.
        pdf = ArvePdf.Loo(buyer, savedOrder);

        status.Text = "PDF on loodud:\n" + pdf;
        RefreshRows();
    }

    private async Task Send()
    {
        if (pdf == null || savedOrder == null)
            return;

        if (!MailAddress.TryCreate(email.Text.Trim(), out _))
        {
            MessageBox.Show(
                "Sisesta korrektne e-posti aadress.");
            return;
        }

        Enabled = false;

        try
        {
            string? error = await TsekiPost.SaadaAsync(
                buyer, savedOrder, pdf, email.Text.Trim());

            if (!IsDisposed)
            {
                MessageBox.Show(
                    error ?? "Arve saadetud PDF-manusena!");
            }
        }
        catch (Exception ex)
        {
            if (!IsDisposed)
                MessageBox.Show(ex.GetBaseException().Message);
        }
        finally
        {
            if (!IsDisposed)
                Enabled = true;
        }
    }
}