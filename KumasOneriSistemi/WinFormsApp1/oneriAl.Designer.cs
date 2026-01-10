namespace WinFormsApp1
{
    partial class oneriAl
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(oneriAl));
            button1 = new Button();
            label11 = new Label();
            label6 = new Label();
            label9 = new Label();
            label4 = new Label();
            label3 = new Label();
            Tur_comboBox1 = new ComboBox();
            LikraYon_comboBox2 = new ComboBox();
            Suitici_comboBox3 = new ComboBox();
            LikraMik_comboBox1 = new ComboBox();
            Gramaj_comboBox2 = new ComboBox();
            label1 = new Label();
            Mevsim_comboBox1 = new ComboBox();
            label2 = new Label();
            KullanimAlani_comboBox2 = new ComboBox();
            SuspendLayout();
            // 
            // button1
            // 
            button1.BackColor = Color.FromArgb(33, 150, 243);
            button1.FlatAppearance.BorderSize = 0;
            button1.FlatStyle = FlatStyle.Flat;
            button1.Font = new Font("Microsoft Sans Serif", 10.2F);
            button1.ForeColor = Color.White;
            button1.Location = new Point(129, 294);
            button1.Margin = new Padding(4, 3, 4, 3);
            button1.Name = "button1";
            button1.Size = new Size(182, 66);
            button1.TabIndex = 49;
            button1.Text = "Önerileri Gör";
            button1.UseVisualStyleBackColor = false;
            button1.Click += button1_Click;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Font = new Font("Microsoft Sans Serif", 10.2F);
            label11.Location = new Point(76, 126);
            label11.Margin = new Padding(4, 0, 4, 0);
            label11.Name = "label11";
            label11.Size = new Size(60, 20);
            label11.TabIndex = 54;
            label11.Text = "Su İtici";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Microsoft Sans Serif", 10.2F);
            label6.Location = new Point(301, 18);
            label6.Margin = new Padding(4, 0, 4, 0);
            label6.Name = "label6";
            label6.Size = new Size(101, 20);
            label6.TabIndex = 53;
            label6.Text = "Likra Miktarı";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Font = new Font("Microsoft Sans Serif", 10.2F);
            label9.Location = new Point(311, 72);
            label9.Margin = new Padding(4, 0, 4, 0);
            label9.Name = "label9";
            label9.Size = new Size(64, 20);
            label9.TabIndex = 52;
            label9.Text = "Gramaj";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Microsoft Sans Serif", 10.2F);
            label4.Location = new Point(48, 72);
            label4.Margin = new Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new Size(88, 20);
            label4.TabIndex = 51;
            label4.Text = "Likra Yönü";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Microsoft Sans Serif", 10.2F);
            label3.Location = new Point(76, 18);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new Size(34, 20);
            label3.TabIndex = 50;
            label3.Text = "Tür";
            // 
            // Tur_comboBox1
            // 
            Tur_comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            Tur_comboBox1.FormattingEnabled = true;
            Tur_comboBox1.Items.AddRange(new object[] { "", "Polyester", "Pamuk", "Naylon", "Viskon", "Yun", "Keten", "Ipek", "Kot" });
            Tur_comboBox1.Location = new Point(15, 41);
            Tur_comboBox1.Margin = new Padding(4, 3, 4, 3);
            Tur_comboBox1.Name = "Tur_comboBox1";
            Tur_comboBox1.Size = new Size(188, 28);
            Tur_comboBox1.TabIndex = 55;
            // 
            // LikraYon_comboBox2
            // 
            LikraYon_comboBox2.DropDownStyle = ComboBoxStyle.DropDownList;
            LikraYon_comboBox2.FormattingEnabled = true;
            LikraYon_comboBox2.Items.AddRange(new object[] { "", "Her Iki Yonde", "Enine", "Boyuna" });
            LikraYon_comboBox2.Location = new Point(15, 95);
            LikraYon_comboBox2.Margin = new Padding(4, 3, 4, 3);
            LikraYon_comboBox2.Name = "LikraYon_comboBox2";
            LikraYon_comboBox2.Size = new Size(188, 28);
            LikraYon_comboBox2.TabIndex = 56;
            // 
            // Suitici_comboBox3
            // 
            Suitici_comboBox3.DropDownStyle = ComboBoxStyle.DropDownList;
            Suitici_comboBox3.FormattingEnabled = true;
            Suitici_comboBox3.Items.AddRange(new object[] { "", "Hayir", "Evet" });
            Suitici_comboBox3.Location = new Point(15, 149);
            Suitici_comboBox3.Margin = new Padding(4, 3, 4, 3);
            Suitici_comboBox3.Name = "Suitici_comboBox3";
            Suitici_comboBox3.Size = new Size(188, 28);
            Suitici_comboBox3.TabIndex = 57;
            // 
            // LikraMik_comboBox1
            // 
            LikraMik_comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            LikraMik_comboBox1.Font = new Font("Segoe UI", 10F);
            LikraMik_comboBox1.FormattingEnabled = true;
            LikraMik_comboBox1.Items.AddRange(new object[] { "", "0-2", "2-5", "5-10", "10-15", "15-20", "20+" });
            LikraMik_comboBox1.Location = new Point(262, 42);
            LikraMik_comboBox1.Margin = new Padding(4, 3, 4, 3);
            LikraMik_comboBox1.Name = "LikraMik_comboBox1";
            LikraMik_comboBox1.Size = new Size(188, 31);
            LikraMik_comboBox1.TabIndex = 58;
            // 
            // Gramaj_comboBox2
            // 
            Gramaj_comboBox2.DropDownStyle = ComboBoxStyle.DropDownList;
            Gramaj_comboBox2.Font = new Font("Segoe UI", 10F);
            Gramaj_comboBox2.FormattingEnabled = true;
            Gramaj_comboBox2.Items.AddRange(new object[] { "", "0-100", "100-150", "150-200", "200-250", "250+" });
            Gramaj_comboBox2.Location = new Point(262, 95);
            Gramaj_comboBox2.Margin = new Padding(4, 3, 4, 3);
            Gramaj_comboBox2.Name = "Gramaj_comboBox2";
            Gramaj_comboBox2.Size = new Size(188, 31);
            Gramaj_comboBox2.TabIndex = 59;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Microsoft Sans Serif", 10.2F);
            label1.Location = new Point(320, 128);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(67, 20);
            label1.TabIndex = 60;
            label1.Text = "Mevsim";
            // 
            // Mevsim_comboBox1
            // 
            Mevsim_comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            Mevsim_comboBox1.FormattingEnabled = true;
            Mevsim_comboBox1.Items.AddRange(new object[] { "", "Dort Mevsim", "Yazlik", "Kislik" });
            Mevsim_comboBox1.Location = new Point(262, 151);
            Mevsim_comboBox1.Margin = new Padding(4, 3, 4, 3);
            Mevsim_comboBox1.Name = "Mevsim_comboBox1";
            Mevsim_comboBox1.Size = new Size(188, 28);
            Mevsim_comboBox1.TabIndex = 61;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Microsoft Sans Serif", 10.2F);
            label2.Location = new Point(48, 180);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(115, 20);
            label2.TabIndex = 62;
            label2.Text = "Kullanım Alanı";
            // 
            // KullanimAlani_comboBox2
            // 
            KullanimAlani_comboBox2.DropDownStyle = ComboBoxStyle.DropDownList;
            KullanimAlani_comboBox2.FormattingEnabled = true;
            KullanimAlani_comboBox2.Items.AddRange(new object[] { "", "Tayt", "IcGiyim", "Spor Tisort", "Tisort", "Esofman", "Mayo", "Spor Hirka", "Pantolon", "Sort", "Mont", "Gomlek", "Etek", "TakimElbise", "Elbise" });
            KullanimAlani_comboBox2.Location = new Point(15, 203);
            KullanimAlani_comboBox2.Margin = new Padding(4, 3, 4, 3);
            KullanimAlani_comboBox2.Name = "KullanimAlani_comboBox2";
            KullanimAlani_comboBox2.Size = new Size(188, 28);
            KullanimAlani_comboBox2.TabIndex = 63;
            // 
            // oneriAl
            // 
            AutoScaleDimensions = new SizeF(10F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.WhiteSmoke;
            ClientSize = new Size(475, 460);
            Controls.Add(KullanimAlani_comboBox2);
            Controls.Add(label2);
            Controls.Add(Mevsim_comboBox1);
            Controls.Add(label1);
            Controls.Add(Gramaj_comboBox2);
            Controls.Add(LikraMik_comboBox1);
            Controls.Add(Suitici_comboBox3);
            Controls.Add(LikraYon_comboBox2);
            Controls.Add(Tur_comboBox1);
            Controls.Add(label11);
            Controls.Add(label6);
            Controls.Add(label9);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(button1);
            Font = new Font("Microsoft Sans Serif", 10.2F);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 3, 4, 3);
            Name = "oneriAl";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Kumaş Önerisi";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private Label label11;
        private Label label6;
        private Label label9;
        private Label label4;
        private Label label3;
        private ComboBox Tur_comboBox1;
        private ComboBox LikraYon_comboBox2;
        private ComboBox Suitici_comboBox3;
        private ComboBox LikraMik_comboBox1;
        private ComboBox Gramaj_comboBox2;
        private Label label1;
        private ComboBox Mevsim_comboBox1;
        private Label label2;
        private ComboBox KullanimAlani_comboBox2;
    }
}