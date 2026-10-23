using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tooded_EFCore_Mihhailenko
{
    /// <summary>
    /// The store: category list on the left, product cards in the middle, basket on the right.
    /// "Omanik" also gets a "Haldus" button (the product management form, Form2); "Müüja" sees only the store.
    /// </summary>
    public class PoodForm : Form
    {
        private readonly Kasutaja _kasutaja;
        public bool LogiValja { get; private set; }

        private List<Kategooria> _kategooriad = new List<Kategooria>();
        private List<Toode> _tooted = new List<Toode>();
        private int? _kategooriaId;
        private readonly Dictionary<int, int> _korv = new Dictionary<int, int>();   // product id -> quantity
        private readonly List<(Button nupp, int? id)> _kategooriaNupud = new List<(Button nupp, int? id)>();

        private readonly TextBox _otsing = new TextBox();
        private readonly FlowLayoutPanel _kategooriaPaneel = new FlowLayoutPanel();
        private readonly FlowLayoutPanel _ruudustik = new FlowLayoutPanel();
        private readonly Label _tulemusSilt = new Label();
        private readonly Label _korvPealkiri = new Label();
        private readonly FlowLayoutPanel _korvRead = new FlowLayoutPanel();
        private readonly Label _kokku = new Label();
        private readonly Button _maksaNupp = new Button();

        public PoodForm(Kasutaja kasutaja)
        {
            _kasutaja = kasutaja;
            Ehita();
            LaeAndmed();
            JoonistaKategooriad();
            JoonistaToted();
            JoonistaKorv();
        }

        // ------------------------------------------------------------ layout

        private void Ehita()
        {
            Text = "Tooded pood";
            ClientSize = new Size(1280, 780);
            MinimumSize = new Size(1120, 660);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Teema.Taust;
            Font = Teema.F(10);

            // ---- header (green bar: name, search, user, buttons)
            var pais = new Panel { Dock = DockStyle.Top, Height = 72, BackColor = Teema.Aksent };
            pais.Controls.Add(Teema.Silt("Tooded pood", 20, 16, 230, 40, Teema.F(20, FontStyle.Bold), Color.White));

            _otsing.PlaceholderText = "Otsi tooteid…";
            _otsing.Font = Teema.F(12);
            _otsing.BorderStyle = BorderStyle.FixedSingle;
            _otsing.SetBounds(260, 20, 380, 32);
            _otsing.TextChanged += (s, e) => JoonistaToted();
            pais.Controls.Add(_otsing);

            var kasutajaPaneel = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0, 16, 16, 0),
                BackColor = Teema.Aksent
            };
            var tere = new Label
            {
                Text = _kasutaja.TaisNimi + "  ·  " + _kasutaja.Roll,
                ForeColor = Color.White,
                AutoSize = true,
                Font = Teema.F(10, FontStyle.Bold),
                Margin = new Padding(0, 9, 16, 0)
            };
            kasutajaPaneel.Controls.Add(tere);

            if (_kasutaja.Roll == "Omanik")
            {
                var haldus = new Button { Text = "Haldus", Size = new Size(90, 36), Margin = new Padding(0, 0, 8, 0) };
                Teema.NupuStiil(haldus, false);
                haldus.Click += HaldusKlikk;
                kasutajaPaneel.Controls.Add(haldus);
            }

            var valja = new Button { Text = "Logi välja", Size = new Size(100, 36), Margin = new Padding(0) };
            Teema.NupuStiil(valja, false);
            valja.Click += (s, e) => { LogiValja = true; Close(); };
            kasutajaPaneel.Controls.Add(valja);
            pais.Controls.Add(kasutajaPaneel);

            // ---- left: categories
            var vasak = new Panel { Dock = DockStyle.Left, Width = 220, BackColor = Teema.Kaart, Padding = new Padding(14, 14, 0, 0) };
            _kategooriaPaneel.Dock = DockStyle.Fill;
            _kategooriaPaneel.FlowDirection = FlowDirection.TopDown;
            _kategooriaPaneel.WrapContents = false;
            _kategooriaPaneel.AutoScroll = true;
            vasak.Controls.Add(_kategooriaPaneel);
            var kategooriaPealkiri = Teema.Silt("KATEGOORIAD", 0, 0, 100, 28, Teema.F(9, FontStyle.Bold), Teema.TekstHall);
            kategooriaPealkiri.Dock = DockStyle.Top;
            vasak.Controls.Add(kategooriaPealkiri);

            // ---- right: basket
            var korv = new Panel { Dock = DockStyle.Right, Width = 340, BackColor = Teema.Kaart, Padding = new Padding(16) };
            _korvRead.Dock = DockStyle.Fill;
            _korvRead.FlowDirection = FlowDirection.TopDown;
            _korvRead.WrapContents = false;
            _korvRead.AutoScroll = true;
            korv.Controls.Add(_korvRead);

            var alumine = new Panel { Dock = DockStyle.Bottom, Height = 140 };
            _kokku.SetBounds(0, 8, 308, 36);
            _kokku.Font = Teema.F(16, FontStyle.Bold);
            _kokku.ForeColor = Teema.Tekst;
            _kokku.TextAlign = ContentAlignment.MiddleRight;
            alumine.Controls.Add(_kokku);

            _maksaNupp.Text = "Maksa";
            _maksaNupp.SetBounds(0, 52, 308, 48);
            Teema.NupuStiil(_maksaNupp);
            _maksaNupp.Font = Teema.F(12, FontStyle.Bold);
            _maksaNupp.Click += async (s, e) => await Maksa();
            alumine.Controls.Add(_maksaNupp);

            var tuhjenda = new LinkLabel
            {
                Text = "Tühjenda korv",
                AutoSize = true,
                Location = new Point(0, 110),
                LinkColor = Teema.TekstHall,
                ActiveLinkColor = Teema.Tekst
            };
            tuhjenda.LinkClicked += (s, e) => { _korv.Clear(); JoonistaKorv(); };
            alumine.Controls.Add(tuhjenda);
            korv.Controls.Add(alumine);

            _korvPealkiri.Dock = DockStyle.Top;
            _korvPealkiri.Height = 44;
            _korvPealkiri.Font = Teema.F(15, FontStyle.Bold);
            _korvPealkiri.ForeColor = Teema.Tekst;
            korv.Controls.Add(_korvPealkiri);

            // ---- centre: product cards
            var keskel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16, 12, 8, 8) };
            _ruudustik.Dock = DockStyle.Fill;
            _ruudustik.AutoScroll = true;
            _ruudustik.FlowDirection = FlowDirection.LeftToRight;
            _ruudustik.WrapContents = true;
            keskel.Controls.Add(_ruudustik);
            _tulemusSilt.Dock = DockStyle.Top;
            _tulemusSilt.Height = 34;
            _tulemusSilt.Font = Teema.F(12, FontStyle.Bold);
            _tulemusSilt.ForeColor = Teema.Tekst;
            keskel.Controls.Add(_tulemusSilt);

            // Order matters for docking: Fill first, header last.
            Controls.Add(keskel);
            Controls.Add(korv);
            Controls.Add(vasak);
            Controls.Add(pais);
        }

        // -------------------------------------------------------------- data

        private void LaeAndmed()
        {
            try
            {
                using (var db = new ToodedContext())
                {
                    _kategooriad = db.Kategooriatabel.AsNoTracking().OrderBy(k => k.Kategooria_nimetus).ToList();
                    _tooted = db.Toodetabel.AsNoTracking().OrderBy(t => t.Toodenimetus).ToList();
                }
                PildiAbi.TuhjendaCache();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Andmete laadimine ebaõnnestus:\n" + ex.GetBaseException().Message, "Viga",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void HaldusKlikk(object? sender, EventArgs e)
        {
            using (var vorm = new Form2())
            {
                vorm.ShowDialog(this);
            }

            LaeAndmed();
            if (_kategooriaId != null && !_kategooriad.Any(k => k.Id == _kategooriaId))
            {
                _kategooriaId = null;
            }
            JoonistaKategooriad();
            JoonistaToted();
            JoonistaKorv();
        }

        // -------------------------------------------------------- categories

        private void JoonistaKategooriad()
        {
            foreach (Control c in _kategooriaPaneel.Controls.Cast<Control>().ToList()) c.Dispose();
            _kategooriaNupud.Clear();

            LisaKategooriaNupp("Kõik tooted", null);
            foreach (var k in _kategooriad)
            {
                LisaKategooriaNupp(k.Kategooria_nimetus, k.Id);
            }
            UuendaKategooriaStiil();
        }

        private void LisaKategooriaNupp(string tekst, int? id)
        {
            var nupp = new Button
            {
                Text = tekst,
                Size = new Size(184, 40),
                Margin = new Padding(0, 0, 0, 2),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            nupp.FlatAppearance.BorderSize = 0;
            nupp.FlatAppearance.MouseOverBackColor = Teema.Taust;
            nupp.Click += (s, e) =>
            {
                _kategooriaId = id;
                UuendaKategooriaStiil();
                JoonistaToted();
            };
            _kategooriaNupud.Add((nupp, id));
            _kategooriaPaneel.Controls.Add(nupp);
        }

        private void UuendaKategooriaStiil()
        {
            foreach (var (nupp, id) in _kategooriaNupud)
            {
                bool aktiivne = id == _kategooriaId;
                nupp.BackColor = aktiivne ? Teema.AksentHele : Teema.Kaart;
                nupp.ForeColor = aktiivne ? Teema.Aksent : Teema.Tekst;
                nupp.Font = Teema.F(10.5f, aktiivne ? FontStyle.Bold : FontStyle.Regular);
            }
        }

        // ----------------------------------------------------- product cards

        private void JoonistaToted()
        {
            string otsing = _otsing.Text.Trim();
            var nahtavad = _tooted
                .Where(t => (_kategooriaId == null || t.KategooriaId == _kategooriaId) &&
                            (otsing.Length == 0 ||
                             t.Toodenimetus.Contains(otsing, StringComparison.CurrentCultureIgnoreCase)))
                .ToList();

            _ruudustik.SuspendLayout();
            foreach (Control c in _ruudustik.Controls.Cast<Control>().ToList()) c.Dispose();

            foreach (var t in nahtavad)
            {
                _ruudustik.Controls.Add(Kaart(t));
            }
            _ruudustik.ResumeLayout();

            _tulemusSilt.Text = nahtavad.Count == 0 ? "Ühtegi toodet ei leitud." : nahtavad.Count + " toodet";
        }

        private Control Kaart(Toode t)
        {
            bool otsas = t.Kogus <= 0;

            var kaart = new Panel { Size = new Size(200, 290), Margin = new Padding(8), BackColor = Teema.Kaart };
            kaart.Paint += (s, e) =>
            {
                using (var pen = new Pen(Teema.Joon))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, kaart.Width - 1, kaart.Height - 1);
                }
            };

            kaart.Controls.Add(new PictureBox
            {
                Location = new Point(10, 10),
                Size = new Size(180, 130),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = PildiAbi.Lae(t.Pilt) ?? PildiAbi.Kohahoidja
            });
            kaart.Controls.Add(Teema.Silt(t.Toodenimetus, 10, 148, 180, 40, Teema.F(11, FontStyle.Bold), Teema.Tekst));
            kaart.Controls.Add(Teema.Silt(otsas ? "Otsas" : "Laos: " + t.Kogus + " tk", 10, 190, 180, 18,
                Teema.F(9), otsas ? Color.Firebrick : Teema.TekstHall));
            kaart.Controls.Add(Teema.Silt(t.Hind.ToString("0.00") + " €", 10, 210, 180, 32,
                Teema.F(17, FontStyle.Bold), Teema.Hind));

            var nupp = new Button
            {
                Text = otsas ? "Otsas" : "Lisa korvi",
                Location = new Point(10, 248),
                Size = new Size(180, 32),
                Enabled = !otsas
            };
            Teema.NupuStiil(nupp);
            if (otsas)
            {
                nupp.BackColor = Teema.Joon;
                nupp.ForeColor = Teema.TekstHall;
            }
            else
            {
                nupp.Click += (s, e) => MuudaKorvi(t, +1);
            }
            kaart.Controls.Add(nupp);

            return kaart;
        }

        // ------------------------------------------------------------ basket

        // Changes the quantity of a product in the basket. You cannot put more in the basket than is in stock.
        private void MuudaKorvi(Toode t, int muutus)
        {
            int praegu;
            _korv.TryGetValue(t.Id, out praegu);
            int uus = praegu + muutus;

            if (uus > t.Kogus)
            {
                MessageBox.Show("Laos on ainult " + t.Kogus + " tk toodet „" + t.Toodenimetus + "“.",
                    "Laoseis", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (uus <= 0) _korv.Remove(t.Id);
            else _korv[t.Id] = uus;

            // Redraw after the click handler has finished (the clicked button is part of what is redrawn).
            BeginInvoke(new Action(JoonistaKorv));
        }

        private void JoonistaKorv()
        {
            _korvRead.SuspendLayout();
            foreach (Control c in _korvRead.Controls.Cast<Control>().ToList()) c.Dispose();

            decimal summa = 0;
            int tukke = 0;

            foreach (var paar in _korv.ToList())
            {
                var t = _tooted.FirstOrDefault(x => x.Id == paar.Key);
                if (t == null || t.Kogus <= 0)
                {
                    _korv.Remove(paar.Key);
                    continue;
                }

                int kogus = Math.Min(paar.Value, t.Kogus);
                _korv[paar.Key] = kogus;

                summa += t.Hind * kogus;
                tukke += kogus;
                _korvRead.Controls.Add(KorviRida(t, kogus));
            }
            _korvRead.ResumeLayout();

            _korvPealkiri.Text = "Ostukorv (" + tukke + ")";
            _kokku.Text = "Kokku: " + summa.ToString("0.00") + " €";
            _maksaNupp.Enabled = _korv.Count > 0;
        }

        private Control KorviRida(Toode t, int kogus)
        {
            var rida = new Panel { Size = new Size(284, 64), Margin = new Padding(0, 0, 0, 6), BackColor = Teema.Taust };

            rida.Controls.Add(Teema.Silt(t.Toodenimetus, 8, 6, 238, 22, Teema.F(10, FontStyle.Bold), Teema.Tekst));

            var eemalda = new Button { Text = "✕", Location = new Point(252, 4), Size = new Size(26, 24) };
            Teema.NupuStiil(eemalda, false);
            eemalda.Font = Teema.F(9, FontStyle.Bold);
            eemalda.Click += (s, e) => MuudaKorvi(t, -kogus);
            rida.Controls.Add(eemalda);

            var miinus = new Button { Text = "−", Location = new Point(8, 34), Size = new Size(26, 24) };
            Teema.NupuStiil(miinus, false);
            miinus.Click += (s, e) => MuudaKorvi(t, -1);
            rida.Controls.Add(miinus);

            rida.Controls.Add(Teema.Silt(kogus.ToString(), 36, 34, 30, 24, Teema.F(10, FontStyle.Bold), Teema.Tekst,
                ContentAlignment.MiddleCenter));

            var pluss = new Button { Text = "+", Location = new Point(68, 34), Size = new Size(26, 24) };
            Teema.NupuStiil(pluss, false);
            pluss.Click += (s, e) => MuudaKorvi(t, +1);
            rida.Controls.Add(pluss);

            rida.Controls.Add(Teema.Silt((t.Hind * kogus).ToString("0.00") + " €", 104, 34, 172, 24,
                Teema.F(11, FontStyle.Bold), Teema.Tekst, ContentAlignment.MiddleRight));

            return rida;
        }

        // ---------------------------------------------------------- checkout

        // Saves the order in the database and reduces the stock (one transaction),
        // then e-mails the receipt to the logged-in user.
        private async Task Maksa()
        {
            if (_korv.Count == 0) return;

            Tellimus? valmis = null;

            try
            {
                using (var db = new ToodedContext())
                using (var tx = db.Database.BeginTransaction())
                {
                    var idd = _korv.Keys.ToList();
                    var tooted = db.Toodetabel.Where(t => idd.Contains(t.Id)).ToList();

                    // Re-check the stock in the database (someone else may have bought it in the meantime).
                    var probleemid = new List<string>();
                    foreach (var paar in _korv)
                    {
                        var t = tooted.FirstOrDefault(x => x.Id == paar.Key);
                        if (t == null) probleemid.Add("Toode on kustutatud.");
                        else if (t.Kogus < paar.Value)
                            probleemid.Add(t.Toodenimetus + ": laos " + t.Kogus + " tk, korvis " + paar.Value + " tk");
                    }

                    if (probleemid.Count > 0)
                    {
                        MessageBox.Show("Kõike ei saa osta, laoseis on muutunud:\n\n" + string.Join("\n", probleemid),
                            "Laoseis", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        LaeAndmed();
                        JoonistaToted();
                        JoonistaKorv();
                        return;
                    }

                    var tellimus = new Tellimus { KasutajaId = _kasutaja.Id, Aeg = DateTime.Now };
                    decimal summa = 0;

                    foreach (var paar in _korv)
                    {
                        var t = tooted.First(x => x.Id == paar.Key);
                        t.Kogus -= paar.Value;
                        summa += t.Hind * paar.Value;
                        tellimus.Read.Add(new TellimuseRida
                        {
                            ToodeId = t.Id,
                            ToodeNimi = t.Toodenimetus,
                            Kogus = paar.Value,
                            Hind = t.Hind
                        });
                    }

                    tellimus.Summa = summa;
                    db.Tellimustabel.Add(tellimus);
                    db.SaveChanges();
                    tx.Commit();

                    valmis = tellimus;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ostu ei õnnestunud salvestada:\n" + ex.GetBaseException().Message, "Viga",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (valmis == null) return;

            // The order is saved. Refresh the shop, then send the receipt.
            _korv.Clear();
            LaeAndmed();
            JoonistaToted();
            JoonistaKorv();

            string sonum = "Aitäh! Tellimus nr " + valmis.Id + " on salvestatud.\nKokku: " +
                           valmis.Summa.ToString("0.00") + " €";

            Enabled = false;
            UseWaitCursor = true;
            string? viga = await TsekiPost.SaadaAsync(_kasutaja, valmis);
            UseWaitCursor = false;
            Enabled = true;

            if (viga == null)
            {
                MessageBox.Show(sonum + "\n\nTšekk saadeti aadressile " + _kasutaja.Epost + ".",
                    "Ost sooritatud", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(sonum + "\n\nTšekki ei õnnestunud saata:\n" + viga,
                    "Ost sooritatud", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}