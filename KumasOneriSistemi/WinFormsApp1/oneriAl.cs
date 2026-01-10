using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public partial class oneriAl : Form
    {
        private const string DATABASE_PATH = @"C:\Users\sahan\OneDrive\Desktop\Dersler\4. sınıf\Mühendislik\sona doğru\kumasVerileri.db";
        private AnaEkran parentForm;

        public oneriAl()
        {
            InitializeComponent();
        }

        public oneriAl(AnaEkran parent) : this()
        {
            parentForm = parent;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Filtreleme kriterleri oluştur
            var filter = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(Tur_comboBox1.Text))
                filter["Kumas_Tur"] = Tur_comboBox1.Text;

            if (!string.IsNullOrEmpty(LikraYon_comboBox2.Text))
                filter["Kumas_Likra_Yonu"] = LikraYon_comboBox2.Text;

            if (!string.IsNullOrEmpty(Suitici_comboBox3.Text))
                filter["Kumas_SuItici"] = Suitici_comboBox3.Text;

            if (!string.IsNullOrEmpty(Mevsim_comboBox1.Text))
                filter["Kullanim_Donemi"] = Mevsim_comboBox1.Text;

            if (!string.IsNullOrEmpty(KullanimAlani_comboBox2.Text))
                filter["Kullanim_Alani"] = KullanimAlani_comboBox2.Text;

            // Sayısal filtreler
            // Parse selected ranges from comboboxes (e.g. "5-10", "20+", "")
            double? minLikra = null;
            double? maxLikra = null;
            if (!string.IsNullOrEmpty(LikraMik_comboBox1.Text))
            {
                ParseRange(LikraMik_comboBox1.Text, out minLikra, out maxLikra);
            }

            double? minGramaj = null;
            double? maxGramaj = null;
            if (!string.IsNullOrEmpty(Gramaj_comboBox2.Text))
            {
                ParseRange(Gramaj_comboBox2.Text, out minGramaj, out maxGramaj);
            }

            // Veritabanından uygun kumaşları getir
            var matchedFabrics = GetMatchedFabrics(filter, minLikra, maxLikra, minGramaj, maxGramaj);

            // Form1'deki DataGridView'i güncelle
            if (parentForm != null)
            {
                parentForm.DisplayFilteredResults(matchedFabrics);
            }

            // Form1'deki label3'i güncellemek için öneriler oluştur
            if (parentForm != null && matchedFabrics.Rows.Count > 0)
            {
                UpdateForm1Suggestions(matchedFabrics);
            }
        }

        private DataTable GetMatchedFabrics(Dictionary<string, object> filters, double? minLikra, double? maxLikra, double? minGramaj, double? maxGramaj)
        {
            DataTable result = new DataTable();

            if (!File.Exists(DATABASE_PATH))
            {
                MessageBox.Show("Veritabanı dosyası bulunamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return result;
            }

            try
            {
                string connectionString = $"Data Source={DATABASE_PATH};Version=3;";

                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT * FROM KullaniciniKumaslari WHERE 1=1";

                    // SQL WHERE şartlarını dinamik olarak ekle
                    if (filters.ContainsKey("Kumas_Tur"))
                        query += $" AND Kumas_Tur = '{filters["Kumas_Tur"]}'";
                    if (filters.ContainsKey("Kumas_Likra_Yonu"))
                        query += $" AND Kumas_Likra_Yonu = '{filters["Kumas_Likra_Yonu"]}'";
                    if (filters.ContainsKey("Kumas_SuItici"))
                        query += $" AND Kumas_SuItici = '{filters["Kumas_SuItici"]}'";
                    if (filters.ContainsKey("Kullanim_Donemi"))
                        query += $" AND Kullanim_Donemi = '{filters["Kullanim_Donemi"]}'";
                    if (filters.ContainsKey("Kullanim_Alani"))
                        query += $" AND Kullanim_Alani = '{filters["Kullanim_Alani"]}'";
                    
                    if (minLikra.HasValue && maxLikra.HasValue)
                        query += $" AND [Kumas_Likra_%] BETWEEN {minLikra} AND {maxLikra}";
                    else if (minLikra.HasValue)
                        query += $" AND [Kumas_Likra_%] >= {minLikra}";
                    else if (maxLikra.HasValue)
                        query += $" AND [Kumas_Likra_%] <= {maxLikra}";

                    if (minGramaj.HasValue && maxGramaj.HasValue)
                        query += $" AND Kumas_Gramaj_gm2 BETWEEN {minGramaj} AND {maxGramaj}";
                    else if (minGramaj.HasValue)
                        query += $" AND Kumas_Gramaj_gm2 >= {minGramaj}";
                    else if (maxGramaj.HasValue)
                        query += $" AND Kumas_Gramaj_gm2 <= {maxGramaj}";

                    using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(query, connection))
                    {
                        adapter.Fill(result);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Veritabanı hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return result;
        }

        private void ParseRange(string text, out double? min, out double? max)
        {
            min = null;
            max = null;
            if (string.IsNullOrWhiteSpace(text)) return;

            text = text.Trim();
            if (text.EndsWith("+"))
            {
                if (double.TryParse(text.TrimEnd('+'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double v))
                {
                    min = v;
                }
                return;
            }

            var parts = text.Split('-', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
            {
                if (double.TryParse(parts[0], out double p0)) min = p0;
                if (double.TryParse(parts[1], out double p1)) max = p1;
            }
        }


        /// <summary>
        /// Form1'deki label3'e önerileri yazar. AI ile metin iyileştirmesi Form1.UpdateSuggestions içinde yapılır.
        /// </summary>
        private void UpdateForm1Suggestions(DataTable fabrics)
        {
            if (fabrics.Rows.Count == 0)
            {
                AnaEkran.UpdateSuggestions("Eşleşen kumaş bulunamadı.");
                return;
            }

            StringBuilder suggestions = new StringBuilder();
            suggestions.AppendLine("--- Önerilen Kumaşlar ---\n");

            int count = 0;
            foreach (DataRow row in fabrics.Rows)
            {
                if (count >= 5) break; // En fazla 5 öneriye sınırla

                string name = row["Kumas_Ad"]?.ToString() ?? "Bilinmiyor";
                string type = row["Kumas_Tur"]?.ToString() ?? "";
                string usage = row["Kullanim_Alani"]?.ToString() ?? "";

                suggestions.AppendLine($"{count + 1}. {name} ({type})");
                suggestions.AppendLine($"   Kullanım Alanı: {usage}");
                suggestions.AppendLine();

                count++;
            }

            if (fabrics.Rows.Count > 5)
                suggestions.AppendLine($"... ve {fabrics.Rows.Count - 5} daha");

            // Form1.UpdateSuggestions metodu AI ile metni iyileştirecek
            AnaEkran.UpdateSuggestions(suggestions.ToString());
        }
    }
}
