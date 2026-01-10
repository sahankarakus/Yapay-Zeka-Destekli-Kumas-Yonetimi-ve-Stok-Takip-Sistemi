"""
Kumaş Kullanım Alanı Tahmin Scripti

Bu script, veritabanından kumaş verilerini alır, model ile tahmin yapar
ve sonuçları JSON formatında döndürür.
"""

import pandas as pd
import joblib
import sqlite3
import sys
import json
import warnings
import sklearn
from pathlib import Path

# sklearn uyarılarını yoksay
warnings.filterwarnings('ignore')

# Sabitler
DB_PATH = r"C:\Users\sahan\OneDrive\Desktop\Dersler\4. sınıf\Mühendislik\sona doğru\kumasVerileri.db"
MODEL_DIR = r"C:\Users\sahan\OneDrive\Desktop\Dersler\4. sınıf\Mühendislik\sona doğru"
MODEL_FILE = "kumas_model_rf.pkl"
ENCODER_FILE = "label_encoder.pkl"
COLUMNS_FILE = "model_columns.pkl"

# Eğitim sırasında silinen sütunlar
COLUMNS_TO_DROP = ['ID', 'Kumas_Ad', 'Kumas_Renk', 'Kumas_Uzunluk_m', 'Kumas_En_cm', 'Kumas_Fiyat_TLm', 'Kullanim_Alani']

# One-Hot Encoding yapılacak kategorik sütunlar
CATEGORICAL_COLUMNS = ['Kumas_Tur', 'Kumas_Likra_Yonu', 'Kumas_SuItici', 'Kullanim_Donemi']


def get_version_info():
    """Kütüphane sürüm bilgilerini döndürür."""
    return {
        "python": sys.version.split()[0],
        "sklearn": sklearn.__version__,
        "pandas": pd.__version__,
        "joblib": joblib.__version__
    }


def load_model_with_compatibility(model_path):
    """
    Modeli sklearn sürüm uyumluluğu ile yükler.
    monotonic_cst hatası için özel işleme yapar.
    """
    try:
        # Normal yükleme dene
        model = joblib.load(model_path)
        return model, None
    except AttributeError as e:
        error_msg = str(e)
        if 'monotonic_cst' in error_msg:
            # monotonic_cst hatası - pickle ile manuel yükleme dene
            try:
                import pickle
                import warnings
                
                # Uyarıları geçici olarak kapat
                with warnings.catch_warnings():
                    warnings.simplefilter("ignore")
                    
                    # Pickle ile yükle
                    with open(model_path, 'rb') as f:
                        model = pickle.load(f)
                    
                    # Model yüklendikten sonra eksik özellikleri ekle
                    # RandomForestClassifier içindeki DecisionTreeClassifier'ları düzelt
                    if hasattr(model, 'estimators_'):
                        for estimator in model.estimators_:
                            # DecisionTreeClassifier'da monotonic_cst özelliğini ekle
                            if not hasattr(estimator, 'monotonic_cst'):
                                try:
                                    # setattr ile özellik ekle
                                    setattr(estimator, 'monotonic_cst', None)
                                except:
                                    pass
                    
                    return model, "monotonic_cst uyumluluk düzeltmesi uygulandı"
            except Exception as compat_error:
                # Pickle yükleme de başarısız oldu
                return None, (
                    f"Model yüklenemedi: {error_msg}. "
                    f"Uyumluluk düzeltmesi başarısız: {str(compat_error)}. "
                    f"Lütfen modeli mevcut sklearn sürümü ({sklearn.__version__}) ile yeniden eğitin."
                )
        else:
            # Diğer AttributeError'lar
            return None, f"Model yüklenirken AttributeError: {error_msg}"
    except FileNotFoundError:
        return None, f"Model dosyası bulunamadı: {model_path}"
    except Exception as e:
        return None, f"Model yüklenirken beklenmeyen hata: {str(e)}"


def load_models():
    """
    Tüm model dosyalarını yükler (model, encoder, columns).
    """
    model_path = Path(MODEL_DIR) / MODEL_FILE
    encoder_path = Path(MODEL_DIR) / ENCODER_FILE
    columns_path = Path(MODEL_DIR) / COLUMNS_FILE
    
    # Model dosyalarının varlığını kontrol et
    if not model_path.exists():
        return None, None, None, f"Model dosyası bulunamadı: {model_path}"
    if not encoder_path.exists():
        return None, None, None, f"Encoder dosyası bulunamadı: {encoder_path}"
    if not columns_path.exists():
        return None, None, None, f"Columns dosyası bulunamadı: {columns_path}"
    
    # Modeli yükle (uyumluluk düzeltmesi ile)
    model, compat_msg = load_model_with_compatibility(str(model_path))
    if model is None:
        return None, None, None, compat_msg
    
    # Encoder'ı yükle
    try:
        encoder = joblib.load(str(encoder_path))
    except Exception as e:
        return None, None, None, f"Encoder yüklenirken hata: {str(e)}"
    
    # Columns'ı yükle
    try:
        columns = joblib.load(str(columns_path))
    except Exception as e:
        return None, None, None, f"Columns yüklenirken hata: {str(e)}"
    
    return model, encoder, columns, compat_msg


def load_fabric_data(kumas_id):
    """
    Veritabanından belirtilen ID'deki kumaş verisini yükler.
    """
    if not Path(DB_PATH).exists():
        return None, f"Veritabanı dosyası bulunamadı: {DB_PATH}"
    
    try:
        conn = sqlite3.connect(DB_PATH)
        query = f"SELECT * FROM KullaniciniKumaslari WHERE ID = {kumas_id}"
        df = pd.read_sql(query, conn)
        conn.close()
        
        if df.empty:
            return None, f"ID {kumas_id} bulunamadı"
        
        return df, None
    except sqlite3.Error as e:
        return None, f"Veritabanı hatası: {str(e)}"
    except Exception as e:
        return None, f"Veri yüklenirken beklenmeyen hata: {str(e)}"


def preprocess_data(df, training_columns):
    """
    Veriyi model için hazırlar (eğitim scripti ile aynı işlemler).
    """
    try:
        # Gereksiz sütunları kaldır
        df_features = df.drop(columns=[col for col in COLUMNS_TO_DROP if col in df.columns], errors='ignore')
        
        # Kategorik verileri One-Hot Encoding yap
        # Eğitim scriptinde drop_first=False kullanılmış
        df_processed = pd.get_dummies(df_features, columns=CATEGORICAL_COLUMNS, drop_first=False, dtype=int)
        
        # Eğitim sırasında oluşan sütun yapısına göre reindex et
        # Eksik sütunlar 0 ile doldurulur
        df_processed = df_processed.reindex(columns=training_columns, fill_value=0)
        
        # Tüm özellikleri sayısallaştır: tarih/datetime gibi string değerler float'a dönüşemez
        # Bu durumda hatayı önlemek için coercion kullanılarak sayısal olmayanlar NaN'a çevrilir ve 0 ile doldurulur.
        try:
            df_processed = df_processed.apply(pd.to_numeric, errors='coerce').fillna(0)
        except Exception:
            # Eğer dönüşümde beklenmeyen bir hata olursa, önceki df_processed'ı döndür (model çağrısı yine başarısız olabilir)
            pass
        
        return df_processed, None
    except Exception as e:
        return None, f"Veri işleme hatası: {str(e)}"


def make_prediction(model, encoder, processed_data):
    """
    Model ile tahmin yapar ve sonuçları döndürür.
    """
    try:
        # Tahmin olasılıklarını al
        tahmin_proba = model.predict_proba(processed_data)
        
        # En yüksek olasılıklı sınıfı bul
        tahmin_index = tahmin_proba[0].argmax()
        tahmin_adi = encoder.classes_[tahmin_index]
        tahmin_orani = float(tahmin_proba[0][tahmin_index] * 100)
        
        # Tüm sınıflar için olasılıkları al
        tum_skorlar = {}
        for i, sinif in enumerate(encoder.classes_):
            tum_skorlar[sinif] = float(tahmin_proba[0][i] * 100)
        
        return {
            "tahmin": tahmin_adi,
            "oran": tahmin_orani,
            "tum_skorlar": tum_skorlar
        }, None
    except Exception as e:
        return None, f"Tahmin yapılırken hata: {str(e)}"


def tahmin_yap(kumas_id):
    """
    Ana tahmin fonksiyonu.
    Verilen kumaş ID'si için kullanım alanı tahminini yapar.
    """
    try:
        # 1. Modelleri yükle
        model, encoder, training_columns, error_msg = load_models()
        if model is None:
            return {
                "status": "error",
                "message": error_msg
            }
        
        # 2. Veritabanından veriyi yükle
        df, error_msg = load_fabric_data(kumas_id)
        if df is None:
            return {
                "status": "error",
                "message": error_msg
            }
        
        # 3. Veriyi işle
        processed_data, error_msg = preprocess_data(df, training_columns)
        if processed_data is None:
            return {
                "status": "error",
                "message": error_msg
            }
        
        # 4. Tahmin yap
        prediction_result, error_msg = make_prediction(model, encoder, processed_data)
        if prediction_result is None:
            return {
                "status": "error",
                "message": error_msg
            }
        
        # 5. Başarılı sonuç döndür
        return {
            "status": "success",
            "kumas_id": kumas_id,
            "tahmin": prediction_result["tahmin"],
            "oran": prediction_result["oran"],
            "tum_skorlar": prediction_result["tum_skorlar"]
        }
    
    except Exception as e:
        return {
            "status": "error",
            "message": f"Beklenmeyen hata: {str(e)}"
        }


def main():
    """Script ana giriş noktası."""
    if len(sys.argv) < 2:
        result = {
            "status": "error",
            "message": "Kumaş ID'si gereklidir. Kullanım: python tahmin_yap.py <kumas_id>"
        }
        print(json.dumps(result, ensure_ascii=False))
        sys.exit(1)
    
    try:
        kumas_id = int(sys.argv[1])
    except ValueError:
        result = {
            "status": "error",
            "message": f"Geçersiz kumaş ID'si: {sys.argv[1]}. ID bir tam sayı olmalıdır."
        }
        print(json.dumps(result, ensure_ascii=False))
        sys.exit(1)
    
    # Tahmin yap
    result = tahmin_yap(kumas_id)
    
    # JSON çıktı
    print(json.dumps(result, ensure_ascii=False))
    
    # Hata durumunda exit code 1
    if result["status"] == "error":
        sys.exit(1)
    else:
        sys.exit(0)


if __name__ == "__main__":
    main()

