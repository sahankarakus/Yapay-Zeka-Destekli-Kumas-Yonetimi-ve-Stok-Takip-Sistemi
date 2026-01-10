# 🧵 Yapay Zeka Destekli Kumaş Öneri ve Stok Takip Sistemi
**(AI-Supported Fabric Recommendation and Stock Tracking System)**

## 📖 Proje Özeti (Summary)
Bu proje, tekstil endüstrisindeki manuel karar verme süreçlerini dijitalleştirmek, operasyonel hataları minimize etmek ve stok yönetimini optimize etmek amacıyla geliştirilmiş bir **Karar Destek Sistemidir **.

Muğla Sıtkı Koçman Üniversitesi **Bilişim Sistemleri Mühendisliği** bitirme projesi kapsamında geliştirilen sistem; kumaşların fiziksel özelliklerini (gramaj, likra, hammadde vb.) analiz ederek en uygun kullanım alanını **Yapay Zeka (Random Forest)** ile tahmin eder ve işletme stoklarını dinamik olarak yönetir.

---

## 🚀 Temel Özellikler (Key Features)

### 🧠 1. Yapay Zeka ile Kumaş Analizi
* **Algoritma:** Random Forest (Rastgele Orman) Sınıflandırıcısı.
* **Başarı Oranı:** Test verileri üzerinde **%79.5** doğruluk.
* **İşlev:** Girilen teknik parametrelere göre kumaşın en uygun olduğu giysi türünü (Pantolon, Gömlek, Elbise vb.) ve **Güven Skorunu** (Confidence Score) tahmin eder.

### 📊 2. Akıllı Stok ve Kapasite Yönetimi
* **Dinamik Hesaplama:** Depodaki kumaş metrajından kaç adet ürün çıkabileceğini endüstriyel "Birim Sarfiyat Katsayıları"nı kullanarak otomatik hesaplar.
* **Örnek:** *100 metre kumaşım var -> Bundan 55 adet Pantolon üretilebilir.*

### 🔄 3. Hibrit ve Güvenli Mimari
* **Save-First Prensibi:** Veri kaybını önlemek için sistem "Önce Kaydet, Sonra İşle" mantığıyla çalışır.
* **Entegrasyon:** C# Windows Forms arayüzü ve Python analiz motoru, SQLite veritabanı üzerinde asenkron olarak haberleşir.

---

## 🛠️ Kullanılan Teknolojiler (Tech Stack)

| Kategori | Teknoloji / Kütüphane |
| :--- | :--- |
| **Arayüz (Frontend)** | C# .NET Framework (Windows Forms) |
| **Yapay Zeka (AI)** | Python 3.x, Scikit-learn, Joblib |
| **Veri İşleme** | Pandas, NumPy |
| **Veritabanı** | SQLite (İlişkisel Veritabanı) |
| **IDE & Araçlar** | Visual Studio 2022, Spyder |

---

## ⚙️ Sistem Mimarisi ve Akış (Workflow)

Sistemin çalışma mantığı, verinin kullanıcıdan alınıp işlenmesi ve sonucun gösterilmesi döngüsüne dayanır:

1.  **Veri Girişi:** Kullanıcı C# arayüzünden kumaş verilerini girer.
2.  **Kayıt (Insert):** Veriler ham haliyle SQLite veritabanına kaydedilir.
3.  **Analiz (Process):** C#, arka planda Python scriptini tetikler. Eğitilmiş model veriyi analiz eder.
4.  **Güncelleme (Update):** Tahmin sonucu ve güven skoru veritabanına geri yazılır.
5.  **Sonuç:** Kullanıcı ekranında öneri görüntülenir.

---

## 📷 Ekran Görüntüleri (Screenshots)

*(Buraya projenin arayüzünden, veri giriş ekranından veya sonuç ekranından 1-2 görsel ekleyebilirsiniz. Görselleri 'screenshots' klasörüne atıp buraya linkleyebilirsiniz.)*

<div align="center">
  <img src="screenshots/ana_ekran.png" alt="Ana Ekran" width="600">
</div>

---

## 💻 Kurulum ve Çalıştırma (Installation)

Projeyi yerel makinenizde çalıştırmak için aşağıdaki adımları izleyin:

1.  **Repoyu Klonlayın:**
    ```bash
    git clone [https://github.com/kullaniciadi/proje-ismi.git](https://github.com/kullaniciadi/proje-ismi.git)
    ```

2.  **Gerekli Python Kütüphanelerini Yükleyin:**
    Modelin çalışması için bilgisayarınızda Python yüklü olmalıdır.
    ```bash
    pip install pandas scikit-learn joblib
    ```

3.  **Projeyi Başlatın:**
    * `KumasOneriSistemi.sln` dosyasını Visual Studio ile açın.
    * `Baslat` (Start) butonuna basarak uygulamayı çalıştırın.
    * *Not: C# kodundaki Python yolu (path) ayarının kendi bilgisayarınıza uygun olduğundan emin olun.*

---

## 📬 İletişim (Contact)

**Geliştirici:** Şahan Karakuş  
**Bölüm:** Bilişim Sistemleri Mühendisliği  
**Üniversite:** Muğla Sıtkı Koçman Üniversitesi  
**LinkedIn:** [Linkedin Profil Linkin]  
