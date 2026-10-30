using System;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Tooded_EFCore_Mihhailenko
{
    /// <summary>
    /// Login + registration in one window, shown before the store opens.
    /// DialogResult.OK means a user is logged in; the user is in the Kasutaja property.
    /// The first registered user becomes "Omanik", everybody after that becomes "Müüja".
    /// </summary>
    public class LoginForm : Form
    {
        public Kasutaja? Kasutaja { get; private set; }

        private readonly Panel _loginPaneel = new Panel();
        private readonly Panel _regPaneel = new Panel();

        private readonly TextBox _lNimi = new TextBox();
        private readonly TextBox _lParool = new TextBox();
        private readonly Label _lViga = new Label();
        private Button _loginNupp = new Button();

        private readonly TextBox _rEesnimi = new TextBox();
        private readonly TextBox _rPerenimi = new TextBox();
        private readonly TextBox _rEpost = new TextBox();
        private readonly TextBox _rNimi = new TextBox();
        private readonly TextBox _rParool = new TextBox();
        private readonly TextBox _rParool2 = new TextBox();
        private readonly Label _rViga = new Label();
        private Button _regNupp = new Button();

        public LoginForm()
        {
            Ehita();
        }
        private void Ehita()
        {
            Text = "Tooded pood";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(860, 560);
            BackColor = Teema.Kaart;
            Font = Teema.F(10);

            var vasak = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(340, 560),
                BackColor = Teema.Aksent
            };
            vasak.Controls.Add(Teema.Silt("Tooded" + Environment.NewLine + "pood", 36, 150, 280, 110,
                Teema.F(32, FontStyle.Bold), Color.White));
            vasak.Controls.Add(Teema.Silt("Värsked tooted," + Environment.NewLine + "õiglased hinnad.", 40, 290, 280, 60,
                Teema.F(13), Color.White));
            Controls.Add(vasak);

            EhitaLogin();
            EhitaRegistreerimine();
            Controls.Add(_loginPaneel);
            Controls.Add(_regPaneel);

            NaitaLogin();
        }

        private static void LisaVali(Control vanem, string silt, TextBox tb, int x, int y, int laius, bool parool)
        {
            vanem.Controls.Add(Teema.Silt(silt, x, y, laius, 20, Teema.F(9), Teema.TekstHall));
            tb.SetBounds(x, y + 22, laius, 30);
            tb.Font = Teema.F(11);
            tb.BorderStyle = BorderStyle.FixedSingle;
            tb.UseSystemPasswordChar = parool;
            vanem.Controls.Add(tb);
        }

        private void EhitaLogin()
        {
            _loginPaneel.SetBounds(340, 0, 520, 560);
            _loginPaneel.BackColor = Teema.Kaart;

            _loginPaneel.Controls.Add(Teema.Silt("Logi sisse", 60, 90, 400, 44, Teema.F(22, FontStyle.Bold), Teema.Tekst));
            _loginPaneel.Controls.Add(Teema.Silt("Poe avamiseks logi sisse oma kontoga.", 62, 140, 400, 24,
                Teema.F(10), Teema.TekstHall));

            _lNimi.MaxLength = 50;
            _lParool.MaxLength = 100;
            LisaVali(_loginPaneel, "Kasutajanimi", _lNimi, 60, 190, 400, false);
            LisaVali(_loginPaneel, "Parool", _lParool, 60, 262, 400, true);

            _lViga.SetBounds(60, 322, 400, 22);
            _lViga.ForeColor = Color.Firebrick;
            _lViga.Font = Teema.F(9);
            _loginPaneel.Controls.Add(_lViga);

            _loginNupp = new Button { Text = "Logi sisse", Location = new Point(60, 352), Size = new Size(400, 46) };
            Teema.NupuStiil(_loginNupp);
            _loginNupp.Click += LoginKlikk;
            _loginPaneel.Controls.Add(_loginNupp);

            var link = new LinkLabel
            {
                Text = "Pole veel kontot? Registreeru",
                AutoSize = true,
                Location = new Point(60, 416),
                LinkColor = Teema.Aksent,
                ActiveLinkColor = Teema.AksentTume
            };
            link.LinkClicked += (s, e) => NaitaRegistreerimine();
            _loginPaneel.Controls.Add(link);
        }

        private void EhitaRegistreerimine()
        {
            _regPaneel.SetBounds(340, 0, 520, 560);
            _regPaneel.BackColor = Teema.Kaart;

            _regPaneel.Controls.Add(Teema.Silt("Registreeru", 60, 36, 400, 44, Teema.F(22, FontStyle.Bold), Teema.Tekst));
            _regPaneel.Controls.Add(Teema.Silt("Esimene registreeritud kasutaja saab rolli Omanik, ülejäänud Müüja.",
                62, 84, 400, 24, Teema.F(9), Teema.TekstHall));

            _rEesnimi.MaxLength = 50;
            _rPerenimi.MaxLength = 50;
            _rEpost.MaxLength = 100;
            _rNimi.MaxLength = 50;
            _rParool.MaxLength = 100;
            _rParool2.MaxLength = 100;

            LisaVali(_regPaneel, "Eesnimi", _rEesnimi, 60, 120, 190, false);
            LisaVali(_regPaneel, "Perekonnanimi", _rPerenimi, 270, 120, 190, false);
            LisaVali(_regPaneel, "E-post", _rEpost, 60, 188, 400, false);
            LisaVali(_regPaneel, "Kasutajanimi (vähemalt 3 märki)", _rNimi, 60, 256, 400, false);
            LisaVali(_regPaneel, "Parool (vähemalt 6 märki)", _rParool, 60, 324, 190, true);
            LisaVali(_regPaneel, "Korda parooli", _rParool2, 270, 324, 190, true);

            _rViga.SetBounds(60, 386, 400, 38);
            _rViga.ForeColor = Color.Firebrick;
            _rViga.Font = Teema.F(9);
            _regPaneel.Controls.Add(_rViga);

            _regNupp = new Button { Text = "Loo konto", Location = new Point(60, 430), Size = new Size(400, 46) };
            Teema.NupuStiil(_regNupp);
            _regNupp.Click += RegistreeriKlikk;
            _regPaneel.Controls.Add(_regNupp);

            var link = new LinkLabel
            {
                Text = "Konto on juba olemas? Logi sisse",
                AutoSize = true,
                Location = new Point(60, 494),
                LinkColor = Teema.Aksent,
                ActiveLinkColor = Teema.AksentTume
            };
            link.LinkClicked += (s, e) => NaitaLogin();
            _regPaneel.Controls.Add(link);
        }

        private void NaitaLogin()
        {
            _regPaneel.Visible = false;
            _loginPaneel.Visible = true;
            _lViga.Text = "";
            AcceptButton = _loginNupp;
            _lNimi.Focus();
        }

        private void NaitaRegistreerimine()
        {
            _loginPaneel.Visible = false;
            _regPaneel.Visible = true;
            _rViga.Text = "";
            AcceptButton = _regNupp;
            _rEesnimi.Focus();
        }

        // ------------------------------------------------------------- logic

        private void LoginKlikk(object? sender, EventArgs e)
        {
            string nimi = _lNimi.Text.Trim();
            if (nimi.Length == 0 || _lParool.Text.Length == 0)
            {
                _lViga.Text = "Sisesta kasutajanimi ja parool.";
                return;
            }

            try
            {
                using (var db = new ToodedContext())
                {
                    var k = db.Kasutajatabel.FirstOrDefault(x => x.Kasutajanimi == nimi);
                    if (k == null || !Salasona.Kontrolli(_lParool.Text, k.ParoolHash, k.Sool))
                    {
                        _lViga.Text = "Vale kasutajanimi või parool.";
                        return;
                    }

                    Kasutaja = k;
                }
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                _lViga.Text = "Andmebaasi viga: " + ex.GetBaseException().Message;
            }
        }

        private void RegistreeriKlikk(object? sender, EventArgs e)
        {
            string eesnimi = _rEesnimi.Text.Trim();
            string perenimi = _rPerenimi.Text.Trim();
            string epost = _rEpost.Text.Trim();
            string nimi = _rNimi.Text.Trim();

            if (eesnimi.Length == 0 || perenimi.Length == 0)
            {
                _rViga.Text = "Sisesta ees- ja perekonnanimi.";
                return;
            }
            if (!Regex.IsMatch(epost, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                _rViga.Text = "E-posti aadress ei ole korrektne.";
                return;
            }
            if (nimi.Length < 3)
            {
                _rViga.Text = "Kasutajanimi peab olema vähemalt 3 märki.";
                return;
            }
            if (_rParool.Text.Length < 6)
            {
                _rViga.Text = "Parool peab olema vähemalt 6 märki.";
                return;
            }
            if (_rParool.Text != _rParool2.Text)
            {
                _rViga.Text = "Paroolid ei kattu.";
                return;
            }

            try
            {
                Kasutaja uus;
                using (var db = new ToodedContext())
                {
                    if (db.Kasutajatabel.Any(x => x.Kasutajanimi == nimi))
                    {
                        _rViga.Text = "See kasutajanimi on juba kasutusel.";
                        return;
                    }

                    bool esimene = !db.Kasutajatabel.Any();

                    string hash;
                    string sool;
                    Salasona.Loo(_rParool.Text, out hash, out sool);

                    uus = new Kasutaja
                    {
                        Eesnimi = eesnimi,
                        Perenimi = perenimi,
                        Epost = epost,
                        Kasutajanimi = nimi,
                        ParoolHash = hash,
                        Sool = sool,
                        Roll = esimene ? "Omanik" : "Müüja",
                        Loodud = DateTime.Now
                    };
                    db.Kasutajatabel.Add(uus);
                    db.SaveChanges();
                }

                MessageBox.Show("Konto on loodud. Sinu roll: " + uus.Roll + ".", "Registreerimine",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                Kasutaja = uus;
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                _rViga.Text = "Andmebaasi viga: " + ex.GetBaseException().Message;
            }
        }
    }
}