namespace Tooded_EFCore_Mihhailenko
{
    partial class Form3
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private System.Windows.Forms.TabControl tabPood;

        private void InitializeComponent()
        {
            this.tabPood = new System.Windows.Forms.TabControl();
            this.SuspendLayout();

            // tabPood
            this.tabPood.Location = new System.Drawing.Point(10, 10);
            this.tabPood.Name = "tabPood";
            this.tabPood.Size = new System.Drawing.Size(900, 600);

            // Form3
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(930, 630);
            this.Controls.Add(this.tabPood);
            this.Name = "Form3";
            this.Text = "Pood";
            this.ResumeLayout(false);
        }
    }
}
