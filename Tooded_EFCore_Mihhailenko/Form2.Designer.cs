namespace Tooded_EFCore_Mihhailenko
{
    partial class Form2
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private System.Windows.Forms.DataGridView dataGridViewTooted;
        private System.Windows.Forms.TextBox txtNimetus;
        private System.Windows.Forms.TextBox txtKogus;
        private System.Windows.Forms.TextBox txtHind;
        private System.Windows.Forms.ComboBox comboKategooria;
        private System.Windows.Forms.PictureBox picturePilt;
        private System.Windows.Forms.Button btnLisa;
        private System.Windows.Forms.Button btnUuenda;
        private System.Windows.Forms.Button btnKustuta;
        private System.Windows.Forms.Button btnValiPilt;

        private void InitializeComponent()
        {
            this.dataGridViewTooted = new System.Windows.Forms.DataGridView();
            this.txtNimetus = new System.Windows.Forms.TextBox();
            this.txtKogus = new System.Windows.Forms.TextBox();
            this.txtHind = new System.Windows.Forms.TextBox();
            this.comboKategooria = new System.Windows.Forms.ComboBox();
            this.picturePilt = new System.Windows.Forms.PictureBox();
            this.btnLisa = new System.Windows.Forms.Button();
            this.btnUuenda = new System.Windows.Forms.Button();
            this.btnKustuta = new System.Windows.Forms.Button();
            this.btnValiPilt = new System.Windows.Forms.Button();

            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewTooted)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picturePilt)).BeginInit();
            this.SuspendLayout();

            // dataGridViewTooted
            this.dataGridViewTooted.Location = new System.Drawing.Point(12, 12);
            this.dataGridViewTooted.Size = new System.Drawing.Size(600, 300);
            this.dataGridViewTooted.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewTooted.MultiSelect = false;
            this.dataGridViewTooted.ReadOnly = true;
            this.dataGridViewTooted.CellClick += dataGridViewTooted_CellClick;

            // txtNimetus
            this.txtNimetus.Location = new System.Drawing.Point(630, 30);
            this.txtNimetus.Width = 200;
            this.txtNimetus.PlaceholderText = "Toote nimetus";

            // txtKogus
            this.txtKogus.Location = new System.Drawing.Point(630, 70);
            this.txtKogus.Width = 200;
            this.txtKogus.PlaceholderText = "Kogus";

            // txtHind
            this.txtHind.Location = new System.Drawing.Point(630, 110);
            this.txtHind.Width = 200;
            this.txtHind.PlaceholderText = "Hind €";

            // comboKategooria
            this.comboKategooria.Location = new System.Drawing.Point(630, 150);
            this.comboKategooria.Width = 200;

            // picturePilt
            this.picturePilt.Location = new System.Drawing.Point(630, 190);
            this.picturePilt.Size = new System.Drawing.Size(150, 150);
            this.picturePilt.SizeMode = PictureBoxSizeMode.Zoom;
            this.picturePilt.BorderStyle = BorderStyle.FixedSingle;

            // btnValiPilt
            this.btnValiPilt.Location = new System.Drawing.Point(630, 350);
            this.btnValiPilt.Text = "Vali pilt";
            this.btnValiPilt.Click += btnValiPilt_Click;

            // btnLisa
            this.btnLisa.Location = new System.Drawing.Point(630, 390);
            this.btnLisa.Text = "Lisa";
            this.btnLisa.Click += btnLisa_Click;

            // btnUuenda
            this.btnUuenda.Location = new System.Drawing.Point(630, 430);
            this.btnUuenda.Text = "Uuenda";
            this.btnUuenda.Click += btnUuenda_Click;

            // btnKustuta
            this.btnKustuta.Location = new System.Drawing.Point(630, 470);
            this.btnKustuta.Text = "Kustuta";
            this.btnKustuta.Click += btnKustuta_Click;

            // Form2
            this.ClientSize = new System.Drawing.Size(860, 520);
            this.Controls.Add(this.dataGridViewTooted);
            this.Controls.Add(this.txtNimetus);
            this.Controls.Add(this.txtKogus);
            this.Controls.Add(this.txtHind);
            this.Controls.Add(this.comboKategooria);
            this.Controls.Add(this.picturePilt);
            this.Controls.Add(this.btnValiPilt);
            this.Controls.Add(this.btnLisa);
            this.Controls.Add(this.btnUuenda);
            this.Controls.Add(this.btnKustuta);
            this.Text = "Toodete haldus";

            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewTooted)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picturePilt)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

    }
}
