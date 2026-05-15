# Yapay Zeka Destekli Kumaş Yönetimi ve Stok Takip Sistemi 🧶🤖

Bu proje, tekstil endüstrisindeki karar verme süreçlerini dijitalleştirmek ve optimize etmek amacıyla geliştirilmiş, hibrit bir yazılım çözümüdür.

## 🌟 Projenin Amacı
Tekstil üretiminde doğru kumaş seçimi ve stok yönetimi kritik bir öneme sahiptir. Bu sistem, geçmiş verileri analiz ederek yeni siparişler için en uygun kumaş önerilerini sunar ve stok durumunu yapay zeka desteğiyle takip eder.

## 🧠 Teknik Derinlik & Algoritma
Projenin kalbinde **Random Forest (Rastgele Orman)** algoritması yer almaktadır.

### Neden Random Forest?
- **Ensemble Learning:** Çok sayıda karar ağacının (Decision Trees) çıktılarını birleştirerek daha kararlı ve doğru tahminler üretir.
- **Overfitting Engelleme:** Rastgele özellik seçimi sayesinde modelin verilere aşırı uyum sağlamasını önler.
- **Önem Derecesi:** Hangi kumaş özelliklerinin (gramaj, içerik, doku vb.) tahminde daha etkili olduğunu analiz etmemize olanak tanır.

### Model Performansı
Model, `kumas_verisi_1000.xlsx` veri seti üzerinden eğitilmiş ve çapraz doğrulama (cross-validation) yöntemleriyle optimize edilmiştir.

## 🛠️ Kullanılan Teknolojiler
- **Backend (ML):** Python, Scikit-learn, Pandas, NumPy
- **Frontend / UI:** C# Windows Forms (.NET Core)
- **Veritabanı:** SQLite (Hızlı ve taşınabilir veri yönetimi)
- **Entegrasyon:** Python scriptleri, C# uygulaması üzerinden dinamik olarak tetiklenerek tahmin sonuçlarını arayüze döner.

## 📋 Özellikler
- ✅ **Akıllı Öneri:** Sipariş detaylarına göre en uygun kumaş türünü tahmin etme.
- ✅ **Stok Takibi:** Gerçek zamanlı stok giriş-çıkış yönetimi.
- ✅ **Veri Görselleştirme:** Stok ve tahmin sonuçlarının raporlanması.
- ✅ **Hata Analizi:** Manuel süreçlerdeki insan hatasını minimize eden doğrulama katmanı.

## 🚀 Kurulum ve Çalıştırma
1. Gerekli Python kütüphanelerini yükleyin: `pip install scikit-learn pandas openpyxl`
2. SQLite veritabanını kontrol edin.
3. C# projesini Visual Studio üzerinden derleyip çalıştırın.
