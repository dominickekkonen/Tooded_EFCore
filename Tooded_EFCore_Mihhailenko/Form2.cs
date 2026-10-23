using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Tooded_EFCore_Mihhailenko
{
    public partial class Form2 : Form
    {
        private readonly ToodedContext _db;
        private string valitudPilt;

        public Form2()
        {
            InitializeComponent();
            _db = new ToodedContext();
            LaeKategooriad();
            LaeTooted();
        }

        private void LaeKategooriad()
        {
            comboKategooria.DataSource = _db.Kategooriatabel.ToList();
            comboKategooria.DisplayMember = "Kategooria_nimetus";
            comboKategooria.ValueMember = "Id";
        }

        private void LaeTooted()
        {
            dataGridViewTooted.DataSource = _db.Toodetabel
                .Include(t => t.Kategooria)
                .Select(t => new
                {
                    t.Id,
                    t.Toodenimetus,
                    t.Kogus,
                    t.Hind,
                    Kategooria = t.Kategooria.Kategooria_nimetus,
                    t.Pilt
                })
                .ToList();
        }

        private void btnValiPilt_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog();
            ofd.Filter = "Pildid|*.jpg;*.png;*.jpeg";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                valitudPilt = ofd.FileName;
                picturePilt.Image = Image.FromFile(valitudPilt);
            }
        }

        private void btnLisa_Click(object sender, EventArgs e)
        {
            // 1. Check that nothing is empty
            if (string.IsNullOrWhiteSpace(txtNimetus.Text) ||
                string.IsNullOrWhiteSpace(txtKogus.Text) ||
                string.IsNullOrWhiteSpace(txtHind.Text))
            {
                MessageBox.Show("Palun täida kõik väljad: toote nimetus, kogus ja hind!",
                                "Puuduvad andmed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Check that kogus and hind are valid numbers
            if (!int.TryParse(txtKogus.Text, out int kogus) || kogus < 0)
            {
                MessageBox.Show("Kogus peab olema täisarv (nt 10).",
                                "Vigane kogus", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtKogus.Focus();
                return;
            }

            if (!decimal.TryParse(txtHind.Text.Trim().Replace(',', '.'),
                    System.Globalization.NumberStyles.Number,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out decimal hind) || hind < 0)
            {
                MessageBox.Show("Hind peab olema number (nt 2,50).",
                                "Vigane hind", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtHind.Focus();
                return;
            }

            // 3. Check that a category and a picture were chosen
            if (comboKategooria.SelectedValue == null)
            {
                MessageBox.Show("Palun vali kategooria!",
                                "Kategooria puudub", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(valitudPilt))
            {
                MessageBox.Show("Palun vali toote pilt!",
                                "Pilt puudub", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 4. Create the product
            var uus = new Toode
            {
                Toodenimetus = txtNimetus.Text.Trim(),
                Kogus = kogus,
                Hind = hind,
                KategooriaId = (int)comboKategooria.SelectedValue,
                Pilt = Path.GetFileName(valitudPilt)
            };

            // 5. Copy the picture into the Images folder
            Directory.CreateDirectory("Images");
            string siht = Path.Combine("Images", uus.Pilt);
            if (!string.Equals(Path.GetFullPath(valitudPilt), Path.GetFullPath(siht),
                    StringComparison.OrdinalIgnoreCase))
            {
                File.Copy(valitudPilt, siht, true);
            }

            _db.Toodetabel.Add(uus);
            _db.SaveChanges();
            LaeTooted();

            // 6. Clear the fields for the next product
            txtNimetus.Clear();
            txtKogus.Clear();
            txtHind.Clear();
            picturePilt.Image = null;
            valitudPilt = "";
        }

        private void dataGridViewTooted_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            txtNimetus.Text = dataGridViewTooted.Rows[e.RowIndex].Cells["Toodenimetus"].Value.ToString();
            txtKogus.Text = dataGridViewTooted.Rows[e.RowIndex].Cells["Kogus"].Value.ToString();
            txtHind.Text = dataGridViewTooted.Rows[e.RowIndex].Cells["Hind"].Value.ToString();

            string pilt = dataGridViewTooted.Rows[e.RowIndex].Cells["Pilt"].Value?.ToString();

            if (!string.IsNullOrEmpty(pilt) && File.Exists(Path.Combine("Images", pilt)))
                picturePilt.Image = Image.FromFile(Path.Combine("Images", pilt));
        }

        private void btnUuenda_Click(object sender, EventArgs e)
        {
            if (dataGridViewTooted.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vali tabelist toode, mida uuendada.",
                                "Toode valimata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtNimetus.Text) ||
                !int.TryParse(txtKogus.Text, out int kogus) || kogus < 0)
            {
                MessageBox.Show("Sisesta toote nimetus ja kogus (täisarv, mitte negatiivne).",
                                "Vigased andmed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(txtHind.Text.Trim().Replace(',', '.'),
                    System.Globalization.NumberStyles.Number,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out decimal hind) || hind < 0)
            {
                MessageBox.Show("Hind peab olema number (nt 2,50).",
                                "Vigane hind", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (comboKategooria.SelectedValue == null)
            {
                MessageBox.Show("Palun vali kategooria!",
                                "Kategooria puudub", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int id = (int)dataGridViewTooted.SelectedRows[0].Cells["Id"].Value;
            var toode = _db.Toodetabel.Find(id);
            if (toode == null) return;

            toode.Toodenimetus = txtNimetus.Text.Trim();
            toode.Kogus = kogus;
            toode.Hind = hind;
            toode.KategooriaId = (int)comboKategooria.SelectedValue;

            if (!string.IsNullOrEmpty(valitudPilt))
            {
                toode.Pilt = Path.GetFileName(valitudPilt);
                Directory.CreateDirectory("Images");
                string siht = Path.Combine("Images", toode.Pilt);
                if (!string.Equals(Path.GetFullPath(valitudPilt), Path.GetFullPath(siht),
                        StringComparison.OrdinalIgnoreCase))
                {
                    File.Copy(valitudPilt, siht, true);
                }
            }

            _db.SaveChanges();
            LaeTooted();
            valitudPilt = "";
        }

        private void btnKustuta_Click(object sender, EventArgs e)
        {
            int id = (int)dataGridViewTooted.SelectedRows[0].Cells["Id"].Value;

            var toode = _db.Toodetabel.Find(id);

            _db.Toodetabel.Remove(toode);
            _db.SaveChanges();
            LaeTooted();
        }
    }
}
