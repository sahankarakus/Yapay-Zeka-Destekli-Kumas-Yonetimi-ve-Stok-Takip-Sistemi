# YAPAY ZEKA DESTEKLİ KUMAŞ ÖNERİ VE STOK TAKİP SİSTEMİ
**(AI-Supported Fabric Recommendation and Stock Tracking System)**

Muğla Sıtkı Koçman Üniversitesi **Bilişim Sistemleri Mühendisliği** Bölümü

---

## 📖 Proje Özeti
Bu proje, tekstil endüstrisindeki manuel karar verme süreçlerini dijitalleştirmek ve hataları minimize etmek amacıyla geliştirilmiştir. Sistem, kumaş özelliklerini analiz ederek en uygun giysi türünü **Yapay Zeka (Random Forest)** ile tahmin eder.

## 🚀 Temel Özellikler

### 1. Yapay Zeka ile Kumaş Analizi
* **Algoritma:** Random Forest (Rastgele Orman)
* **Başarı Oranı:** %79.5 Doğruluk
* **İşlev:** Kumaşın gramaj ve hammadde verisine göre "Pantolonluk", "Gömleklik" gibi öneriler sunar.

### 2. Akıllı Stok Yönetimi
* Depodaki kumaş metrajından kaç adet ürün çıkacağını otomatik hesaplar.
* Örnek: *100 metre kumaştan -> 55 adet Pantolon üretilebilir.*

### 3. Veri Güvenliği
* "Önce Kaydet, Sonra İşle" prensibi ile elektrik kesilse bile veri kaybı olmaz.

---

## 🛠️ Kullanılan Teknolojiler
* **Arayüz:** C# (Windows Forms)
* **Yapay Zeka:** Python (Scikit-learn)
* **Veritabanı:** SQLite
* **Araçlar:** Visual Studio 2022

---

## ⚙️ Nasıl Çalışır?
1. **Veri Girişi:** Kullanıcı kumaş bilgilerini girer.
2. **Kayıt:** Veriler veritabanına kaydedilir.
3. **Analiz:** Python arka planda çalışır ve tahmin yapar.
4. **Sonuç:** Tahmin sonucu ve güven skoru ekrana yansır.

---

## 📬 İletişim
**Geliştirici:** [Adın Soyadın]
**Bölüm:** Bilişim Sistemleri Mühendisliği
**Üniversite:** Muğla Sıtkı Koçman Üniversitesi
