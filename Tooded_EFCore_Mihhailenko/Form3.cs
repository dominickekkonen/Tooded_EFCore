using Microsoft.EntityFrameworkCore;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Tooded_EFCore_Mihhailenko
{
    public partial class Form3 : Form
    {
        private readonly ToodedContext _db;

        public Form3()
        {
            InitializeComponent();
            _db = new ToodedContext();
            LaeKategooriadJaPildid();
        }

        private void LaeKategooriadJaPildid()
        {
            tabPood.TabPages.Clear();

            var kategooriad = _db.Kategooriatabel.ToList();

            foreach (var kat in kategooriad)
            {
                // Loo uus tab selle kategooria jaoks
                var page = new TabPage(kat.Kategooria_nimetus);
                page.AutoScroll = true;

                // Too kategooria tooted andmebaasist
                var tooted = _db.Toodetabel
                                .Where(t => t.KategooriaId == kat.Id)
                                .ToList();

                int x = 10;
                int y = 10;
                int count = 0;

                foreach (var t in tooted)
                {
                    // Loo paneel iga toote jaoks
                    Panel p = new Panel();
                    p.Width = 150;
                    p.Height = 200;
                    p.BorderStyle = BorderStyle.FixedSingle;

                    // Pildikast
                    PictureBox pb = new PictureBox();
                    pb.Width = 140;
                    pb.Height = 140;
                    pb.SizeMode = PictureBoxSizeMode.Zoom;
                    pb.Location = new Point(5, 5);

                    // lae pilt failist, kui olemas
                    var piltPath = Path.Combine("Images", t.Pilt ?? "puudub.png");
                    if (File.Exists(piltPath))
                        pb.Image = Image.FromFile(piltPath);

                    // Nimetus
                    Label lbl = new Label();
                    lbl.Text = t.Toodenimetus;
                    lbl.AutoSize = false;
                    lbl.TextAlign = ContentAlignment.MiddleCenter;
                    lbl.Location = new Point(5, 150);
                    lbl.Width = 140;

                    // Lisa elemendid paneelile
                    p.Controls.Add(pb);
                    p.Controls.Add(lbl);

                    // Paiguta paneel TabPage-s
                    p.Location = new Point(x, y);
                    page.Controls.Add(p);

                    // Järgmise positsioon
                    x += 160;
                    count++;

                    // iga 4 pildi järel uus rida
                    if (count % 4 == 0)
                    {
                        x = 10;
                        y += 210;
                    }
                }

                // Lisa TabPage TabControli sisse
                tabPood.TabPages.Add(page);
            }
        }
    }
}
