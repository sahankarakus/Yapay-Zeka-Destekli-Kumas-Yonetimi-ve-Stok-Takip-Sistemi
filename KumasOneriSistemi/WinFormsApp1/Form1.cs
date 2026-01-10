namespace WinFormsApp1
{
    using System.Data;
    using System.Data.SQLite;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using Microsoft.VisualBasic;

    public partial class AnaEkran : Form
    {
        private const string DATABASE_PATH = @"C:\Users\sahan\OneDrive\Desktop\Dersler\4. sınıf\Mühendislik\sona doğru\kumasVerileri.db";
        private static AnaEkran? instanceReference;
        private System.Windows.Forms.Timer resizeTimer;
        private bool isResizing = false;

        public AnaEkran()
        {
            InitializeComponent();
            instanceReference = this;
            
            // Resize timer'ı ayarla (250ms debounce)
            resizeTimer = new System.Windows.Forms.Timer();
            resizeTimer.Interval = 250;
            resizeTimer.Tick += ResizeTimer_Tick;
        }
    
        private void button2_MouseHover(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            VeriEkle verieklemeform = new VeriEkle();
            verieklemeform.ShowDialog();
            // Yeni veri eklendikten sonra DataGridView'i güncelle
            VerileriYukle();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            oneriAl OneriAlmaForm = new oneriAl(this);
            OneriAlmaForm.Show();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen bir kumaş seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];
            object? idValue = selectedRow.Cells["ID"].Value;

            if (idValue == null)
            {
                MessageBox.Show("Seçilen satırda ID bulunamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                KullanimAlaniSkorlariAl(Convert.ToInt32(idValue));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kullanım alanı skorları alınırken hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            // Eğer sağ panelde (flowLayoutPanel1) yazı varsa temizle
            try
            {
                if (flowLayoutPanel1 != null)
                {
                    foreach (Control c in flowLayoutPanel1.Controls)
                    {
                        if (c is Label lbl && !string.IsNullOrWhiteSpace(lbl.Text))
                        {
                            lbl.Text = string.Empty;
                        }
                    }
                }
            }
            catch
            {
                // Panel temizleme hatası uygulamayı etkilemesin
            }

            // Veritabanındaki tüm verileri filtre olmadan tekrar yükle
            VerileriYukle();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen bir kumaş seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];
            object? idValue = selectedRow.Cells["ID"].Value;

            if (idValue == null)
            {
                MessageBox.Show("Seçilen satırda ID bulunamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                int id = Convert.ToInt32(idValue);

                // Mevcut en bilgisini veritabanından al
                double enCm = 0;
                string kullanimAlani = "";
                string connectionString = $"Data Source={DATABASE_PATH};Version=3;";
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand("SELECT Kumas_En_cm, Kullanim_Alani FROM KullaniciniKumaslari WHERE ID = @id", connection))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        using (SQLiteDataReader rdr = cmd.ExecuteReader())
                        {
                            if (rdr.Read())
                            {
                                double.TryParse(rdr[0]?.ToString(), out enCm);
                                kullanimAlani = rdr[1]?.ToString() ?? "";
                            }
                        }
                    }
                }

                // Kullanıcıdan yeni uzunluğu al
                string input = Interaction.InputBox("Yeni uzunluğu metre cinsinden girin:", "Metraj Güncelle", selectedRow.Cells["Kumas_Uzunluk_m"].Value?.ToString() ?? "0");
                if (string.IsNullOrWhiteSpace(input))
                {
                    return; // kullanıcı iptal etti
                }

                if (!double.TryParse(input.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double yeniUzunluk))
                {
                    MessageBox.Show("Geçerli bir sayı giriniz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Veritabanını güncelle
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand("UPDATE KullaniciniKumaslari SET Kumas_Uzunluk_m = @uzunluk WHERE ID = @id", connection))
                    {
                        cmd.Parameters.AddWithValue("@uzunluk", yeniUzunluk);
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                }

                // Tahmini adetleri hesapla
                string adetlerText = HesaplaTahminiAdetler(yeniUzunluk, enCm, kullanimAlani);

                // DataGridView'i güncelle
                VerileriYukle();

                // Mesaj kutusunda göster
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Yeni uzunluk: {yeniUzunluk} m");
                sb.AppendLine();
                sb.AppendLine(adetlerText);

                MessageBox.Show(sb.ToString(), "Metraj Güncellendi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Güncelleme sırasında hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen silmek istediğiniz satırı seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];
            object? idValue = selectedRow.Cells["ID"].Value;

            if (idValue == null)
            {
                MessageBox.Show("Seçilen satırda ID bulunamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult result = MessageBox.Show(
                "Bu kumaşı silmek istediğinize emin misiniz?",
                "Silme Onayı",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    SatirSil(Convert.ToInt32(idValue));
                    MessageBox.Show("Kumaş başarıyla silindi.", "Başarı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    VerileriYukle();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Veri silinirken hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void SatirSil(int id)
        {
            if (!File.Exists(DATABASE_PATH))
            {
                throw new Exception("Veritabanı dosyası bulunamadı.");
            }

            string connectionString = $"Data Source={DATABASE_PATH};Version=3;";

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string query = "DELETE FROM KullaniciniKumaslari WHERE ID = @id";

                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.ExecuteNonQuery();
                }
            }
        }

        private const double FIRE_ORANI = 0.98;

        private static readonly Dictionary<string, double> TekilMetrajlar = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Elbise", 2.6 },
            { "Spor Tisort", 0.8 },
            { "Gomlek", 1.8 },
            { "Etek", 2 },
            { "Pantolon", 1.8 },
            { "Tisort", 0.8 },
            { "Esofman", 1 },
            { "Spor Hirka", 1.8 },
            { "Sort", 0.4 },
            { "Tayt", 1.5 },
            { "Mont", 1.8 },
            { "TakimElbise", 3 }
        };

        private static readonly Dictionary<string, Dictionary<string, double>> CokluMetrajlar = new(StringComparer.OrdinalIgnoreCase)
        {
            {
                "Mayo", new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                {
                    { "erkek", 0.3 },
                    { "kadin takim", 0.6 }
                }
            },
            {
                "IcGiyim", new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                {
                    { "atlet", 0.7 },
                    { "alt", 0.4 }
                }
            }
        };

        private string HesaplaTahminiAdetler(double uzunlukMetre, double enSantimetre, string tahmin)
        {
            StringBuilder sb = new StringBuilder();

            if (double.IsNaN(uzunlukMetre) || uzunlukMetre <= 0 || double.IsNaN(enSantimetre) || enSantimetre <= 0)
            {
                return "En veya uzunluk bilgisi eksik olduğu için adet hesaplanamadı.";
            }

            double enMetre = enSantimetre / 100d;
            double toplamAlan = enMetre * uzunlukMetre;

            string tahminTrim = tahmin?.Trim() ?? string.Empty;

            if (CokluMetrajlar.TryGetValue(tahminTrim, out var altTurMetrajlari))
            {
                sb.AppendLine("Bu kumaştan üretilebilir tahmini adetler:");
                foreach (var altTur in altTurMetrajlari)
                {
                    double adet = Math.Floor((toplamAlan / altTur.Value) * FIRE_ORANI);
                    sb.AppendLine($"- {FormatAltTurAd(tahminTrim, altTur.Key)}: Yaklaşık {adet} adet");
                }
            }
            else if (TekilMetrajlar.TryGetValue(tahminTrim, out double metraj))
            {
                double adet = Math.Floor((toplamAlan / metraj) * FIRE_ORANI);
                sb.AppendLine($"Bu kumaştan {tahminTrim}: ~ {adet} adet üretilebilir");
            }
            else
            {
                sb.AppendLine("Bu sınıf için metraj tanımı bulunamadı, adet hesaplanamadı.");
            }

            return sb.ToString();
        }

        private string FormatAltTurAd(string tahmin, string altTur)
        {
            if (tahmin.Equals("Mayo", StringComparison.OrdinalIgnoreCase))
            {
                return $"{altTur} Mayosu";
            }

            return $"{altTur} {tahmin}";
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        public void yazidegis(string oneri)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadEkran loadekrani = new LoadEkran();
            loadekrani.Close();
            VerileriYukle();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            // Timer'ı resetle
            resizeTimer.Stop();
            isResizing = true;
            resizeTimer.Start();
        }

        private void ResizeTimer_Tick(object? sender, EventArgs e)
        {
            resizeTimer.Stop();
            isResizing = false;

            // Kontrollerin konumlarını güncelle
            AdjustLayout();
        }

        private void AdjustLayout()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(AdjustLayout));
                return;
            }

            try
            {
                // Minimum boyutları kontrol et
                if (this.ClientSize.Width < 600 || this.ClientSize.Height < 400)
                    return;

                int buttonHeight = 49;
                int buttonY = this.ClientSize.Height - buttonHeight - 10; // Butonları en alta sabitle

                // Butonları en altda konumlandır
                // place buttons in a row with consistent spacing so they remain grouped
                int startX = 15;
                int spacing = 155; // matches designer spacing
                if (button1 != null)
                {
                    button1.Top = buttonY;
                    button1.Left = startX;
                }
                if (button2 != null)
                {
                    button2.Top = buttonY;
                    button2.Left = startX + spacing;
                }
                if (button4 != null)
                {
                    button4.Top = buttonY;
                    button4.Left = startX + spacing * 2;
                }
                if (button3 != null)
                {
                    button3.Top = buttonY;
                    button3.Left = startX + spacing * 3;
                }
                if (button5 != null)
                {
                    button5.Top = buttonY;
                    button5.Left = startX + spacing * 4;
                }
                if (button6 != null)
                {
                    button6.Top = buttonY;
                    button6.Left = startX + spacing * 5;
                }

                // Sağ paneli dinamik olarak pozisyonlandır
                if (flowLayoutPanel1 != null)
                {
                    int panelWidth = Math.Min(300, this.ClientSize.Width / 4);
                    int padding = 15;
                    int newX = this.ClientSize.Width - panelWidth - padding;
                    int panelHeight = buttonY - 50; // Butonların üstüne kadar uzat
                    
                    if (flowLayoutPanel1.Location.X != newX || flowLayoutPanel1.Width != panelWidth || flowLayoutPanel1.Height != panelHeight)
                    {
                        flowLayoutPanel1.Location = new Point(newX, 40);
                        flowLayoutPanel1.Size = new Size(panelWidth, Math.Max(100, panelHeight));
                    }

                    // DataGridView'i dinamik boyutlandır
                    if (dataGridView1 != null)
                    {
                        int gridWidth = Math.Max(200, newX - 30);
                        int gridHeight = Math.Max(100, buttonY - 60);
                        
                        if (dataGridView1.Width != gridWidth || dataGridView1.Height != gridHeight)
                        {
                            dataGridView1.Size = new Size(gridWidth, gridHeight);
                        }

                        // Ensure columns fill the available space after resize
                        if (dataGridView1.ColumnCount > 0)
                        {
                            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                        }
                    }
                }
            }
            catch
            {
                // Hata durumunda sessiz devam et
            }
        }

        private void VerileriYukle()
        {
            if (!File.Exists(DATABASE_PATH))
            {
                MessageBox.Show("Veritabanı dosyası bulunamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                string connectionString = $"Data Source={DATABASE_PATH};Version=3;";

                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    using (SQLiteDataAdapter adapter = new SQLiteDataAdapter("SELECT * FROM KullaniciniKumaslari", connection))
                    {
                        System.Data.DataTable dataTable = new System.Data.DataTable();
                        adapter.Fill(dataTable);

                            dataGridView1.AutoGenerateColumns = true;
                            dataGridView1.DataSource = dataTable;
                            dataGridView1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
                            // Columns should expand to fill available width so grid doesn't look empty
                            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                        dataGridView1.RowHeadersVisible = true;
                        dataGridView1.RowHeadersWidth = 70;
                        dataGridView1.TopLeftHeaderCell.Value = "#";

                        // Show sequential row numbers in the row header instead of the DB ID
                        for (int i = 0; i < dataGridView1.Rows.Count; i++)
                        {
                            DataGridViewRow row = dataGridView1.Rows[i];
                            if (!row.IsNewRow)
                            {
                                row.HeaderCell.Value = (i + 1).ToString();
                            }
                        }

                        foreach (DataGridViewColumn column in dataGridView1.Columns)
                        {
                            switch (column.Name)
                            {
                                case "ID":
                                    column.Width = 50;
                                    column.Visible = false;
                                    column.HeaderText = "ID";
                                    break;
                                case "Kumas_Ad":
                                    column.Width = 130;
                                    column.HeaderText = "Kumaş Adı";
                                    break;
                                case "Kumas_Tur":
                                    column.Width = 90;
                                    column.HeaderText = "Kumaş Türü";
                                    break;
                                case "Kumas_Renk":
                                    column.Width = 80;
                                    column.HeaderText = "Renk";
                                    break;
                                case "Kumas_Likra_%":
                                    column.Width = 40;
                                    column.HeaderText = "Likra (%)";
                                    break;
                                case "Kumas_Likra_Yonu":
                                    column.Width = 93;
                                    column.HeaderText = "Likra Yönü";
                                    break;
                                case "Kumas_Uzunluk_m":
                                    column.Width = 60;
                                    column.HeaderText = "Uzunluk (m)";
                                    break;
                                case "Kumas_En_cm":
                                    column.Width = 60;
                                    column.HeaderText = "En (cm)";
                                    break;
                                case "Kumas_Gramaj_gm2":
                                    column.Width = 70;
                                    column.HeaderText = "Gramaj (g/m²)";
                                    break;
                                case "Kumas_SuItici":
                                    column.Width = 70;
                                    column.HeaderText = "Su İtici";
                                    break;
                                case "Kullanim_Donemi":
                                    column.Width = 90;
                                    column.HeaderText = "Kullanım Dönemi";
                                    break;
                                case "Kullanim_Alani":
                                    column.Width = 100;
                                    column.HeaderText = "Kullanım Alanı";
                                    break;
                                default:
                                    column.Width = 100;
                                    column.HeaderText = column.HeaderText?.Replace("_", " ");
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Veritabanından veriler okunurken bir hata oluştu.\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Static metod: oneriAl formundan Form1'deki label3'i güncellemek için çağırılır
        /// AI ile metni iyileştirir (async olarak arka planda)
        /// </summary>
        public static void UpdateSuggestions(string text)
        {
            if (instanceReference != null && instanceReference.label3 != null)
            {
                // Önce orijinal metni göster (hızlı geri bildirim için)
                instanceReference.label3.Text = text;
                
                // Arka planda AI ile iyileştir
                _ = Task.Run(async () =>
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine("Form1: AI metin iyileştirme başlatılıyor...");
                        string improvedText = await AITextService.ImproveSuggestionsTextAsync(text);
                        
                        System.Diagnostics.Debug.WriteLine($"Form1: AI yanıtı alındı. Orijinal: {text.Length} karakter, İyileştirilmiş: {improvedText.Length} karakter");
                        
                        // Eğer metin değişmediyse (AI çalışmadı), kullanıcıya bilgi ver
                        if (improvedText == text)
                        {
                            System.Diagnostics.Debug.WriteLine("Form1: AI metni iyileştiremedi (Ollama çalışmıyor olabilir)");
                        }
                        
                        // UI thread'inde güncelle
                        if (instanceReference != null && instanceReference.label3 != null && !instanceReference.IsDisposed)
                        {
                            instanceReference.Invoke(new Action(() =>
                            {
                                if (instanceReference.label3 != null && !instanceReference.IsDisposed)
                                {
                                    instanceReference.label3.Text = improvedText;
                                }
                            }));
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Form1: AI metin iyileştirme hatası: {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"Form1: Stack trace: {ex.StackTrace}");
                        // Hata durumunda orijinal metin zaten gösteriliyor
                    }
                });
            }
        }

        /// <summary>
        /// Seçili kumaş için kullanım alanı skorlarını alır ve label3'e yazdırır
        /// </summary>
        private void KullanimAlaniSkorlariAl(int kumasId)
        {
            try
            {
                string pythonScriptPath = @"C:\Users\sahan\OneDrive\Desktop\Dersler\4. sınıf\Mühendislik\sona doğru\tahmin_yap.py";
                
                // Python scriptini çalıştır
                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = $"\"{pythonScriptPath}\" {kumasId}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (System.Diagnostics.Process process = System.Diagnostics.Process.Start(psi))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (!string.IsNullOrEmpty(error))
                        System.Diagnostics.Debug.WriteLine($"Python Error: {error}");

                    if (!string.IsNullOrEmpty(output))
                    {
                        try
                        {
                            // JSON sonucunu parse et
                            var jsonDoc = System.Text.Json.JsonDocument.Parse(output);
                            var root = jsonDoc.RootElement;

                            if (root.TryGetProperty("status", out var statusProp) && statusProp.GetString() == "success")
                            {
                                if (root.TryGetProperty("tum_skorlar", out var skorlarProp))
                                {
                                    // Skorları sırala ve formatla
                                    var skorlar = new List<(string alan, double skor)>();
                                    
                                    foreach (var skor in skorlarProp.EnumerateObject())
                                    {
                                        skorlar.Add((skor.Name, skor.Value.GetDouble()));
                                    }
                                    
                                    // Skorlara göre azalan sırada sırala ve %15 üzerinde olanları filtrele
                                    skorlar = skorlar.Where(s => s.skor > 15.0).OrderByDescending(s => s.skor).ToList();
                                    
                                    // label3'e yazdır (önce orijinal metin)
                                    StringBuilder sb = new StringBuilder();
                                    sb.AppendLine("--- Kullanım Alanı Skorları ---\n");
                                    
                                    if (skorlar.Count > 0)
                                    {
                                        foreach (var (alan, skor) in skorlar)
                                        {
                                            sb.AppendLine($"{alan}: {skor:F2}%");
                                        }
                                    }
                                    else
                                    {
                                        sb.AppendLine("15% üzerinde skor bulunamadı.");
                                    }
                                    
                                    string originalText = sb.ToString();
                                    label3.Text = originalText;
                                    
                                    // Arka planda AI ile iyileştir
                                    _ = Task.Run(async () =>
                                    {
                                        try
                                        {
                                            System.Diagnostics.Debug.WriteLine("Form1: Skor metni AI ile iyileştiriliyor...");
                                            string improvedText = await AITextService.ImproveScoresTextAsync(originalText);
                                            
                                            System.Diagnostics.Debug.WriteLine($"Form1: Skor metni AI yanıtı alındı. Orijinal: {originalText.Length} karakter, İyileştirilmiş: {improvedText.Length} karakter");
                                            
                                            // Eğer metin değişmediyse (AI çalışmadı), kullanıcıya bilgi ver
                                            if (improvedText == originalText)
                                            {
                                                System.Diagnostics.Debug.WriteLine("Form1: AI skor metnini iyileştiremedi (Ollama çalışmıyor olabilir)");
                                            }
                                            
                                            // UI thread'inde güncelle
                                            if (this != null && !this.IsDisposed && label3 != null)
                                            {
                                                this.Invoke(new Action(() =>
                                                {
                                                    if (label3 != null && !this.IsDisposed)
                                                    {
                                                        label3.Text = improvedText;
                                                    }
                                                }));
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            System.Diagnostics.Debug.WriteLine($"Form1: AI skor metni iyileştirme hatası: {ex.Message}");
                                            System.Diagnostics.Debug.WriteLine($"Form1: Stack trace: {ex.StackTrace}");
                                            // Hata durumunda orijinal metin zaten gösteriliyor
                                        }
                                    });
                                }
                                else
                                {
                                    label3.Text = "Kullanım alanı skorları alınamadı.";
                                }
                            }
                            else if (root.TryGetProperty("message", out var msgProp))
                            {
                                label3.Text = $"Hata: {msgProp.GetString()}";
                            }
                        }
                        catch (Exception parseEx)
                        {
                            label3.Text = $"Sonuç işlenirken hata: {parseEx.Message}";
                            System.Diagnostics.Debug.WriteLine($"JSON parse hatası: {parseEx.Message}");
                        }
                    }
                    else
                    {
                        label3.Text = "Python scriptinden çıktı alınamadı.";
                    }
                }
            }
            catch (Exception ex)
            {
                label3.Text = $"Hata: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Kullanım alanı skorları hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// oneriAl formundan gelen verileri Form1'deki DataGridView'de gösterir
        /// </summary>
        public void DisplayFilteredResults(DataTable data)
        {
            if (data == null || data.Rows.Count == 0)
            {
                MessageBox.Show("Eşleşen kumaş bulunamadı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                dataGridView1.AutoGenerateColumns = true;
                dataGridView1.DataSource = data;
                dataGridView1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
                // Make columns expand to use available width so grid doesn't look empty
                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dataGridView1.RowHeadersVisible = true;
                dataGridView1.RowHeadersWidth = 70;
                dataGridView1.TopLeftHeaderCell.Value = "#";

                // ID sütununu gizle
                if (dataGridView1.Columns.Contains("ID"))
                    dataGridView1.Columns["ID"].Visible = false;

                // Show sequential row numbers in the row header
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    DataGridViewRow row = dataGridView1.Rows[i];
                    if (!row.IsNewRow)
                    {
                        row.HeaderCell.Value = (i + 1).ToString();
                    }
                }

                // Sütun başlıklarını ve genişliklerini ayarla
                foreach (DataGridViewColumn column in dataGridView1.Columns)
                {
                    switch (column.Name)
                    {
                        case "ID":
                            column.Width = 50;
                            column.Visible = false;
                            column.HeaderText = "ID";
                            break;
                        case "Kumas_Ad":
                            column.Width = 130;
                            column.HeaderText = "Kumaş Adı";
                            break;
                        case "Kumas_Tur":
                            column.Width = 90;
                            column.HeaderText = "Kumaş Türü";
                            break;
                        case "Kumas_Renk":
                            column.Width = 80;
                            column.HeaderText = "Renk";
                            break;
                        case "Kumas_Likra_%":
                            column.Width = 40;
                            column.HeaderText = "Likra (%)";
                            break;
                        case "Kumas_Likra_Yonu":
                            column.Width = 93;
                            column.HeaderText = "Likra Yönü";
                            break;
                        case "Kumas_Uzunluk_m":
                            column.Width = 60;
                            column.HeaderText = "Uzunluk (m)";
                            break;
                        case "Kumas_En_cm":
                            column.Width = 60;
                            column.HeaderText = "En (cm)";
                            break;
                        case "Kumas_Gramaj_gm2":
                            column.Width = 70;
                            column.HeaderText = "Gramaj (g/m²)";
                            break;
                        case "Kumas_SuItici":
                            column.Width = 70;
                            column.HeaderText = "Su İtici";
                            break;
                        case "Kullanim_Donemi":
                            column.Width = 90;
                            column.HeaderText = "Kullanım Dönemi";
                            break;
                        case "Kullanim_Alani":
                            column.Width = 100;
                            column.HeaderText = "Kullanım Alanı";
                            break;
                        default:
                            column.Width = 100;
                            column.HeaderText = column.HeaderText?.Replace("_", " ");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Veriler gösterilirken bir hata oluştu.\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
