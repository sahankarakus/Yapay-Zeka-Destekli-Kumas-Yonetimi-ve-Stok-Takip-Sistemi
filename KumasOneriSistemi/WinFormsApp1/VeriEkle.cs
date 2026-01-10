using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using System.IO;
using System.Diagnostics;
using System.Text.Json;

namespace WinFormsApp1
{
    public partial class VeriEkle : Form
    {
        private const string DATABASE_PATH = @"C:\Users\sahan\OneDrive\Desktop\Dersler\4. sınıf\Mühendislik\sona doğru\kumasVerileri.db";
        private const double FIRE_ORANI = 0.98;

        // Tekil ürünler için metrajlar (m2)
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

        // Alt türü olan ürünler için metrajlar (m2)
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

        public VeriEkle()
        {
            InitializeComponent();
        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Tüm alanların dolu olup olmadığını kontrol et
            if (string.IsNullOrWhiteSpace(Ad_textBox1.Text))
            {
                MessageBox.Show("Lütfen kumaş adı girin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (Renk_comboBox4.SelectedIndex == -1)
            {
                MessageBox.Show("Lütfen renk seçin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (Tur_comboBox8.SelectedIndex == -1)
            {
                MessageBox.Show("Lütfen kumaş türü seçin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(LikraMik_textBox2.Text) || !decimal.TryParse(LikraMik_textBox2.Text, out _))
            {
                MessageBox.Show("Lütfen geçerli bir likra miktarı girin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (LikraYon_comboBox2.SelectedIndex == -1)
            {
                MessageBox.Show("Lütfen likra yönünü seçin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(Uzunluk_textBox3.Text) || !decimal.TryParse(Uzunluk_textBox3.Text, out _))
            {
                MessageBox.Show("Lütfen geçerli bir uzunluk girin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(En_textBox4.Text) || !decimal.TryParse(En_textBox4.Text, out _))
            {
                MessageBox.Show("Lütfen geçerli bir en girin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(Gramaj_textBox5.Text) || !decimal.TryParse(Gramaj_textBox5.Text, out _))
            {
                MessageBox.Show("Lütfen geçerli bir gramaj girin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (Suiticilik_comboBox10.SelectedIndex == -1)
            {
                MessageBox.Show("Lütfen su iticilik seçin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (Dönem_comboBox1.SelectedIndex == -1)
            {
                MessageBox.Show("Lütfen kullanım dönemini seçin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var tahminSonucu = VeriTabanınaEkle();
                
                string mesaj = OlusturBasariMesaji(tahminSonucu);
                MessageBox.Show(mesaj, "Başarı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                // Tahminleme hatası ise özel mesaj göster
                if (ex.Message.Contains("Tahminleme hatası:"))
                {
                    MessageBox.Show(ex.Message + "\n\nLütfen model dosyalarını ve sklearn versiyonunu kontrol edin.", 
                        "Tahminleme Hatası", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show($"Veri eklerken hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private (bool basarili, string tahmin, double skor)? VeriTabanınaEkle()
        {
            if (!File.Exists(DATABASE_PATH))
            {
                throw new Exception("Veritabanı dosyası bulunamadı: " + DATABASE_PATH);
            }

            string connectionString = $"Data Source={DATABASE_PATH};Version=3;";
            long lastInsertId = 0;

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string query = @"
                    INSERT INTO KullaniciniKumaslari 
                    (Kumas_Ad, Kumas_Tur, Kumas_Renk, [Kumas_Likra_%], Kumas_Likra_Yonu, 
                     Kumas_Uzunluk_m, Kumas_En_cm, Kumas_Gramaj_gm2, Kumas_SuItici, Kullanim_Donemi, Kullanim_Alani)
                    VALUES 
                    (@ad, @tur, @renk, @likra, @likraYonu, @uzunluk, @en, @gramaj, @suItici, @donem, @alani)";

                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ad", Ad_textBox1.Text);
                    command.Parameters.AddWithValue("@tur", Tur_comboBox8.SelectedItem?.ToString() ?? "");
                    command.Parameters.AddWithValue("@renk", Renk_comboBox4.SelectedItem?.ToString() ?? "");
                    command.Parameters.AddWithValue("@likra", decimal.Parse(LikraMik_textBox2.Text));
                    command.Parameters.AddWithValue("@likraYonu", LikraYon_comboBox2.SelectedItem?.ToString() ?? "");
                    command.Parameters.AddWithValue("@uzunluk", decimal.Parse(Uzunluk_textBox3.Text));
                    command.Parameters.AddWithValue("@en", decimal.Parse(En_textBox4.Text));
                    command.Parameters.AddWithValue("@gramaj", decimal.Parse(Gramaj_textBox5.Text));
                    command.Parameters.AddWithValue("@suItici", Suiticilik_comboBox10.SelectedItem?.ToString() ?? "");
                    command.Parameters.AddWithValue("@donem", Dönem_comboBox1.SelectedItem?.ToString() ?? "");
                    command.Parameters.AddWithValue("@alani", "Bekleniyor...");

                    command.ExecuteNonQuery();
                }

                // Son eklenen veri'nin ID'sini al
                using (SQLiteCommand idCommand = new SQLiteCommand("SELECT last_insert_rowid()", connection))
                {
                    lastInsertId = (long)idCommand.ExecuteScalar();
                }
            }

            // Tahminlemeyi yap ve sonucu döndür
            if (lastInsertId > 0)
            {
                return TahminlemeYap(lastInsertId);
            }

            return null;
        }

        private (bool basarili, string tahmin, double skor)? TahminlemeYap(long kumasId)
        {
            try
            {
                string pythonScriptPath = @"C:\Users\sahan\OneDrive\Desktop\Dersler\4. sınıf\Mühendislik\sona doğru\tahmin_yap.py";
                
                // Python script dosyasının varlığını kontrol et
                if (!File.Exists(pythonScriptPath))
                {
                    System.Diagnostics.Debug.WriteLine($"Python script bulunamadı: {pythonScriptPath}");
                    return null;
                }
                
                // Python scriptini çalıştır
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = $"\"{pythonScriptPath}\" {kumasId}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };

                using (Process process = Process.Start(psi))
                {
                    if (process == null)
                    {
                        System.Diagnostics.Debug.WriteLine("Python process başlatılamadı");
                        throw new Exception("Python process başlatılamadı");
                    }

                    // Çıktıları senkron oku (daha güvenilir)
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    
                    // Process'in bitmesini bekle (maksimum 30 saniye)
                    bool finished = process.WaitForExit(30000);
                    
                    if (!finished)
                    {
                        process.Kill();
                        System.Diagnostics.Debug.WriteLine("Python script timeout (30 saniye)");
                        throw new Exception("Python script timeout (30 saniye)");
                    }
                    
                    output = output.Trim();
                    error = error.Trim();

                    System.Diagnostics.Debug.WriteLine($"Python Output: {output}");
                    if (!string.IsNullOrEmpty(error))
                    {
                        System.Diagnostics.Debug.WriteLine($"Python Error: {error}");
                    }

                    if (string.IsNullOrEmpty(output))
                    {
                        System.Diagnostics.Debug.WriteLine("Python script çıktı vermedi");
                        if (!string.IsNullOrEmpty(error))
                        {
                            System.Diagnostics.Debug.WriteLine($"Hata detayı: {error}");
                            throw new Exception($"Python script hatası: {error}");
                        }
                        throw new Exception("Python script çıktı vermedi. Lütfen Python ve gerekli kütüphanelerin yüklü olduğundan emin olun.");
                    }

                    try
                    {
                        // JSON sonucunu parse et
                        var jsonDoc = JsonDocument.Parse(output);
                        var root = jsonDoc.RootElement;

                        if (root.TryGetProperty("status", out var statusProp))
                        {
                            string status = statusProp.GetString();
                            if (status == "success" && root.TryGetProperty("tahmin", out var tahminProp))
                            {
                                string tahmin = tahminProp.GetString();
                                double skor = 0;
                                
                                // Tahmin skorunu al (oran özelliği)
                                if (root.TryGetProperty("oran", out var oranProp))
                                {
                                    skor = oranProp.GetDouble();
                                }
                                
                                System.Diagnostics.Debug.WriteLine($"Tahmin başarılı: {tahmin} (Skor: {skor:F2}%)");
                                
                                // Veritabanını güncelle
                                GuncelleTahmin(kumasId, tahmin);
                                
                                // Sonucu döndür
                                return (true, tahmin, skor);
                            }
                            else if (status == "error" && root.TryGetProperty("message", out var msgProp))
                            {
                                string errorMsg = msgProp.GetString();
                                System.Diagnostics.Debug.WriteLine($"Tahmin hatası: {errorMsg}");
                                // Hata mesajını exception olarak fırlat ki kullanıcıya gösterilebilsin
                                throw new Exception($"Tahminleme hatası: {errorMsg}");
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("JSON'da 'status' property bulunamadı");
                            throw new Exception($"Python script geçersiz yanıt döndürdü. Çıktı: {output}");
                        }
                    }
                    catch (JsonException parseEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"JSON parse hatası: {parseEx.Message}");
                        System.Diagnostics.Debug.WriteLine($"Ham çıktı: {output}");
                        throw new Exception($"Python script yanıtı parse edilemedi: {parseEx.Message}. Çıktı: {output}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Tahminleme hatası: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                // Exception'ı yukarı fırlat ki kullanıcıya gösterilebilsin
                throw;
            }
            
            throw new Exception("Tahminleme tamamlanamadı. Bilinmeyen bir hata oluştu.");
        }

        private void GuncelleTahmin(long kumasId, string tahmin)
        {
            try
            {
                string connectionString = $"Data Source={DATABASE_PATH};Version=3;";

                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    string updateQuery = "UPDATE KullaniciniKumaslari SET Kullanim_Alani = @tahmin WHERE ID = @id";

                    using (SQLiteCommand command = new SQLiteCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@tahmin", tahmin);
                        command.Parameters.AddWithValue("@id", kumasId);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Tahmin güncellemesi hatası: {ex.Message}");
            }
        }

        private string OlusturBasariMesaji((bool basarili, string tahmin, double skor)? tahminSonucu)
        {
            var sb = new StringBuilder("Kumaş başarıyla eklendi");

            if (tahminSonucu == null || !tahminSonucu.Value.basarili)
            {
                return sb.ToString();
            }

            string tahmin = tahminSonucu.Value.tahmin;
            double skor = tahminSonucu.Value.skor;
            string tahminTrim = tahmin?.Trim() ?? string.Empty;

            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("-----------------------------------------------");
            sb.AppendLine($"Tahmin Edilen Kullanım Alanı: {tahminTrim}");
            sb.AppendLine($"Doğruluk Skoru: {skor:F2}%");

            double toplamAlan = HesaplaToplamAlan();
            if (double.IsNaN(toplamAlan))
            {
                return sb.ToString();
            }

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
                sb.AppendLine("Bu kumaştan  tahmini ");
                sb.AppendLine($" {tahminTrim}: ~ {adet} adet üretilebilir");
            }
            else
            {
                sb.AppendLine("Bu sınıf için metraj tanımı bulunamadı, adet hesaplanamadı.");
            }

            return sb.ToString();
        }

        private double HesaplaToplamAlan()
        {
            if (!double.TryParse(Uzunluk_textBox3.Text, out double uzunlukMetre))
            {
                return double.NaN;
            }

            if (!double.TryParse(En_textBox4.Text, out double enSantimetre))
            {
                return double.NaN;
            }

            // En santimetre, uzunluk metre -> alan m2
            double enMetre = enSantimetre / 100d;
            return enMetre * uzunlukMetre;
        }

        private string FormatAltTurAd(string tahmin, string altTur)
        {
            if (tahmin.Equals("Mayo", StringComparison.OrdinalIgnoreCase))
            {
                return $"{altTur} Mayosu";
            }

            return $"{altTur} {tahmin}";
        }
    }
}
