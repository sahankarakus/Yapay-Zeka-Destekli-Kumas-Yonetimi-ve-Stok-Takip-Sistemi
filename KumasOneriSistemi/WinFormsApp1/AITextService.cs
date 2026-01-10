using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsApp1
{
    /// <summary>
    /// Ollama AI servisi ile metinleri daha doğal Türkçe cümlelere dönüştüren servis
    /// </summary>
    public class AITextService
    {
        private const string OLLAMA_BASE_URL = "http://localhost:11434";
        private const string DEFAULT_MODEL = "llama3.2:1b"; // Düşük sistem gereksinimli model (alternatif: tinyllama, llama3.2:3b)
        private const int TIMEOUT_SECONDS = 10;

        private static HttpClient? _httpClient;

        static AITextService()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(TIMEOUT_SECONDS),
                BaseAddress = new Uri(OLLAMA_BASE_URL)
            };
        }

        /// <summary>
        /// Ollama'nın çalışıp çalışmadığını kontrol eder
        /// </summary>
        public static async Task<bool> IsOllamaAvailableAsync()
        {
            try
            {
                if (_httpClient == null) 
                {
                    System.Diagnostics.Debug.WriteLine("AITextService: HttpClient null");
                    return false;
                }

                var response = await _httpClient.GetAsync("/api/tags");
                bool isAvailable = response.IsSuccessStatusCode;
                
                if (!isAvailable)
                {
                    System.Diagnostics.Debug.WriteLine($"AITextService: Ollama yanıt vermedi. Status: {response.StatusCode}");
                }
                
                return isAvailable;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AITextService: Ollama kontrolü hatası: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Belirtilen modelin yüklü olup olmadığını kontrol eder
        /// </summary>
        public static async Task<bool> IsModelInstalledAsync(string modelName = DEFAULT_MODEL)
        {
            try
            {
                if (_httpClient == null) 
                {
                    System.Diagnostics.Debug.WriteLine("AITextService: IsModelInstalledAsync - HttpClient null");
                    return false;
                }

                var response = await _httpClient.GetAsync("/api/tags");
                if (!response.IsSuccessStatusCode) 
                {
                    System.Diagnostics.Debug.WriteLine($"AITextService: IsModelInstalledAsync - API yanıt vermedi. Status: {response.StatusCode}");
                    return false;
                }

                var content = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"AITextService: Model listesi alındı: {content.Substring(0, Math.Min(200, content.Length))}...");
                
                var jsonDoc = JsonDocument.Parse(content);
                
                if (jsonDoc.RootElement.TryGetProperty("models", out var models))
                {
                    foreach (var model in models.EnumerateArray())
                    {
                        if (model.TryGetProperty("name", out var name))
                        {
                            string? modelNameStr = name.GetString();
                            System.Diagnostics.Debug.WriteLine($"AITextService: Kontrol edilen model: {modelNameStr}");
                            
                            // Model adını kontrol et (tam eşleşme veya içeriyor mu)
                            if (!string.IsNullOrEmpty(modelNameStr))
                            {
                                // Tam eşleşme veya model adı içeriyor mu kontrol et
                                if (modelNameStr.Equals(modelName, StringComparison.OrdinalIgnoreCase) || 
                                    modelNameStr.Contains(modelName, StringComparison.OrdinalIgnoreCase) ||
                                    modelName.Contains(modelNameStr.Split(':')[0], StringComparison.OrdinalIgnoreCase))
                                {
                                    System.Diagnostics.Debug.WriteLine($"AITextService: Model bulundu: {modelNameStr}");
                                    return true;
                                }
                            }
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"AITextService: Model bulunamadı: {modelName}");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AITextService: IsModelInstalledAsync hatası: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Teknik metni daha doğal Türkçe cümlelere dönüştürür
        /// </summary>
        public static async Task<string> ImproveTextAsync(string originalText, string context = "kumaş önerileri")
        {
            try
            {
                // Ollama kontrolü
                if (!await IsOllamaAvailableAsync())
                {
                    System.Diagnostics.Debug.WriteLine("AITextService: Ollama çalışmıyor. Orijinal metin gösteriliyor.");
                    return originalText; // Fallback: orijinal metin
                }

                // Model kontrolü
                if (!await IsModelInstalledAsync())
                {
                    System.Diagnostics.Debug.WriteLine($"AITextService: Model ({DEFAULT_MODEL}) yüklü değil. Orijinal metin gösteriliyor.");
                    return originalText; // Fallback: orijinal metin
                }
                
                System.Diagnostics.Debug.WriteLine("AITextService: Ollama ve model hazır, metin iyileştirme başlıyor...");

                if (_httpClient == null) return originalText;

                // Prompt oluştur
                string prompt = CreatePrompt(originalText, context);

                // Ollama API isteği
                var requestBody = new
                {
                    model = DEFAULT_MODEL,
                    prompt = prompt,
                    stream = false,
                    options = new
                    {
                        temperature = 0.3, // Daha düşük temperature = daha tutarlı ve doğru yanıtlar
                        top_p = 0.9,
                        num_predict = 500 // Maksimum token sayısı
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/generate", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    return originalText; // Fallback: orijinal metin
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var responseDoc = JsonDocument.Parse(responseContent);

                if (responseDoc.RootElement.TryGetProperty("response", out var aiResponse))
                {
                    string improvedText = aiResponse.GetString() ?? originalText;
                    
                    // AI yanıtını temizle (gereksiz boşluklar, satır sonları)
                    improvedText = improvedText.Trim();
                    
                    // Eğer AI yanıtı çok kısa veya anlamsızsa orijinal metni döndür
                    if (string.IsNullOrWhiteSpace(improvedText) || improvedText.Length < originalText.Length / 2)
                    {
                        return originalText;
                    }

                    return improvedText;
                }

                return originalText; // Fallback: orijinal metin
            }
            catch (Exception ex)
            {
                // Hata durumunda orijinal metni döndür
                System.Diagnostics.Debug.WriteLine($"AITextService: AI metin iyileştirme hatası: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"AITextService: Stack trace: {ex.StackTrace}");
                return originalText;
            }
        }

        /// <summary>
        /// Kullanım alanı skorları için özel prompt oluşturur
        /// </summary>
        public static async Task<string> ImproveScoresTextAsync(string scoresText)
        {
            System.Diagnostics.Debug.WriteLine($"AITextService: ImproveScoresTextAsync çağrıldı. Orijinal metin: '{scoresText}'");
            
            string prompt = $@"Aşağıdaki kumaş kullanım alanı skorlarını daha doğal, insansı Türkçe cümlelere dönüştür.

ÖNEMLİ KURALLAR:
1. Kumaş isimlerini (Etek, Gomlek, Mont, Pantolon vb.) ASLA değiştirme, olduğu gibi bırak
2. Sayısal değerleri (yüzdeler) ASLA değiştirme, olduğu gibi bırak
3. Sadece cümle yapısını ve ifadeyi iyileştir
4. Başlığı koru ama daha doğal bir şekilde ifade et

Örnek:
Orijinal: === Kullanım Alanı Skorları ===
Etek: 23,10%
Gomlek: 21,82%

İyileştirilmiş: Bu kumaş için en uygun kullanım alanları:
• Etek üretimi için %23,10 oranında uygun
• Gömlek üretimi için %21,82 oranında uygun

Orijinal metin:
{scoresText}

İyileştirilmiş metin (sadece iyileştirilmiş metni yaz, açıklama ekleme):";
            
            System.Diagnostics.Debug.WriteLine($"AITextService: ImproveScoresTextAsync - Prompt oluşturuldu: {prompt.Substring(0, Math.Min(200, prompt.Length))}...");

            try
            {
                if (!await IsOllamaAvailableAsync() || !await IsModelInstalledAsync())
                {
                    return scoresText;
                }

                if (_httpClient == null) return scoresText;

                var requestBody = new
                {
                    model = DEFAULT_MODEL,
                    prompt = prompt,
                    stream = false,
                    options = new
                    {
                        temperature = 0.3, // Daha düşük temperature = daha tutarlı ve doğru yanıtlar
                        top_p = 0.9,
                        num_predict = 500 // Maksimum token sayısı
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/generate", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    return scoresText;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"AITextService: ImproveScoresTextAsync - Tam API yanıtı: {responseContent}");
                var responseDoc = JsonDocument.Parse(responseContent);

                if (responseDoc.RootElement.TryGetProperty("response", out var aiResponse))
                {
                    string improvedText = aiResponse.GetString() ?? scoresText;
                    System.Diagnostics.Debug.WriteLine($"AITextService: ImproveScoresTextAsync - AI ham yanıtı: '{improvedText}'");
                    improvedText = improvedText.Trim();
                    System.Diagnostics.Debug.WriteLine($"AITextService: ImproveScoresTextAsync - AI trimmed yanıtı: '{improvedText}'");
                    System.Diagnostics.Debug.WriteLine($"AITextService: ImproveScoresTextAsync - Orijinal: {scoresText.Length} karakter, İyileştirilmiş: {improvedText.Length} karakter");
                    
                    // Metinlerin aynı olup olmadığını kontrol et
                    if (improvedText == scoresText)
                    {
                        System.Diagnostics.Debug.WriteLine($"AITextService: ImproveScoresTextAsync - AI yanıtı orijinal metinle aynı!");
                    }
                    
                    if (string.IsNullOrWhiteSpace(improvedText) || improvedText.Length < scoresText.Length / 2)
                    {
                        System.Diagnostics.Debug.WriteLine($"AITextService: ImproveScoresTextAsync - Yanıt çok kısa, orijinal metin döndürülüyor");
                        return scoresText;
                    }

                    System.Diagnostics.Debug.WriteLine($"AITextService: ImproveScoresTextAsync - İyileştirilmiş metin döndürülüyor: '{improvedText}'");
                    return improvedText;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"AITextService: ImproveScoresTextAsync - JSON'da 'response' property bulunamadı. Tüm JSON: {responseContent}");
                }

                return scoresText;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AI skor metni iyileştirme hatası: {ex.Message}");
                return scoresText;
            }
        }

        /// <summary>
        /// Öneri metni için özel prompt oluşturur
        /// </summary>
        public static async Task<string> ImproveSuggestionsTextAsync(string suggestionsText)
        {
            string prompt = $@"Aşağıdaki kumaş önerileri metnini daha doğal, samimi ve anlaşılır Türkçe cümlelere dönüştür. 
Kumaş isimlerini ve teknik bilgileri koru, sadece ifadeyi iyileştir. Daha sıcak ve kullanıcı dostu bir dil kullan.

Orijinal metin:
{suggestionsText}

İyileştirilmiş metin:";

            try
            {
                if (!await IsOllamaAvailableAsync() || !await IsModelInstalledAsync())
                {
                    return suggestionsText;
                }

                if (_httpClient == null) return suggestionsText;

                var requestBody = new
                {
                    model = DEFAULT_MODEL,
                    prompt = prompt,
                    stream = false,
                    options = new
                    {
                        temperature = 0.3, // Daha düşük temperature = daha tutarlı ve doğru yanıtlar
                        top_p = 0.9,
                        num_predict = 500 // Maksimum token sayısı
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/generate", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"AITextService: ImproveSuggestionsTextAsync - API yanıt vermedi. Status: {response.StatusCode}");
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"AITextService: Hata içeriği: {errorContent}");
                    return suggestionsText;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"AITextService: ImproveSuggestionsTextAsync - Yanıt alındı: {responseContent.Substring(0, Math.Min(500, responseContent.Length))}...");
                var responseDoc = JsonDocument.Parse(responseContent);

                if (responseDoc.RootElement.TryGetProperty("response", out var aiResponse))
                {
                    string improvedText = aiResponse.GetString() ?? suggestionsText;
                    improvedText = improvedText.Trim();
                    
                    if (string.IsNullOrWhiteSpace(improvedText) || improvedText.Length < suggestionsText.Length / 2)
                    {
                        return suggestionsText;
                    }

                    return improvedText;
                }

                return suggestionsText;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AI öneri metni iyileştirme hatası: {ex.Message}");
                return suggestionsText;
            }
        }

        /// <summary>
        /// Genel amaçlı prompt oluşturur
        /// </summary>
        private static string CreatePrompt(string text, string context)
        {
            return $@"Aşağıdaki {context} metnini daha doğal, samimi ve anlaşılır Türkçe cümlelere dönüştür. 
Teknik bilgileri ve sayısal değerleri koru, sadece ifadeyi iyileştir. Daha sıcak ve kullanıcı dostu bir dil kullan.

Orijinal metin:
{text}

İyileştirilmiş metin:";
        }

        /// <summary>
        /// Kaynakları temizle
        /// </summary>
        public static void Dispose()
        {
            _httpClient?.Dispose();
            _httpClient = null;
        }
    }
}

