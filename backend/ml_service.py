"""
ML Tahmin Servisi — Random Forest modelini kullanarak kumaş kullanım alanı tahmini.

tahmin_yap.py mantığı doğrudan bu modüle taşındı; artık subprocess yerine
doğrudan Python fonksiyon çağrısı yapılır → ~10x daha hızlı.
"""
from __future__ import annotations

import warnings
import logging
from functools import lru_cache
from pathlib import Path
from typing import Optional

import joblib
import numpy as np
import pandas as pd

warnings.filterwarnings("ignore")
logger = logging.getLogger(__name__)

# ─────────────────────────────────────────────
# Yollar
# ─────────────────────────────────────────────
DIR = Path(__file__).parent
MODEL_PATH = DIR / "kumas_model_rf.pkl"
ENCODER_PATH = DIR / "label_encoder.pkl"
COLUMNS_PATH = DIR / "model_columns.pkl"

# Eğitim sırasında silinen sütunlar
COLUMNS_TO_DROP = [
    "ID", "Kumas_Ad", "Kumas_Renk",
    "Kumas_Uzunluk_m", "Kumas_En_cm", "Kumas_Fiyat_TLm", "Kullanim_Alani",
]

# One-Hot Encoding yapılacak kategorik sütunlar
CATEGORICAL_COLUMNS = [
    "Kumas_Tur", "Kumas_Likra_Yonu", "Kumas_SuItici", "Kullanim_Donemi",
]

# Üretim adet hesaplama metrajları (m² / adet)
TEKIL_METRAJLAR: dict[str, float] = {
    "Elbise": 2.6, "Spor Tisort": 0.8, "Gomlek": 1.8, "Etek": 2.0,
    "Pantolon": 1.8, "Tisort": 0.8, "Esofman": 1.0, "Spor Hirka": 1.8,
    "Sort": 0.4, "Tayt": 1.5, "Mont": 1.8, "TakimElbise": 3.0,
}

COKLU_METRAJLAR: dict[str, dict[str, float]] = {
    "Mayo": {"Erkek Mayosu": 0.3, "Kadın Takım Mayosu": 0.6},
    "IcGiyim": {"Atlet": 0.7, "Alt İçgiyim": 0.4},
}

FIRE_ORANI = 0.98  # %2 fire


# ─────────────────────────────────────────────
# Model yükleme (tek seferlik, önbelleğe alınır)
# ─────────────────────────────────────────────

class _ModelBundle:
    """ML model, encoder ve sütun listesini tek nesnede taşır."""
    __slots__ = ("model", "encoder", "columns", "loaded")

    def __init__(self):
        self.model = None
        self.encoder = None
        self.columns = None
        self.loaded = False


_bundle = _ModelBundle()


def _load_model_compat(path: Path):
    """sklearn sürüm uyumsuzluklarını tolere ederek model yükler."""
    try:
        return joblib.load(str(path))
    except AttributeError as exc:
        if "monotonic_cst" not in str(exc):
            raise
        # sklearn >= 1.4 ile eğitilmiş model, eski sürümde yükleniyorsa
        import pickle
        with warnings.catch_warnings():
            warnings.simplefilter("ignore")
            with open(str(path), "rb") as f:
                model = pickle.load(f)
        # Eksik özniteliği ekle
        if hasattr(model, "estimators_"):
            for est in model.estimators_:
                if not hasattr(est, "monotonic_cst"):
                    object.__setattr__(est, "monotonic_cst", None)
        logger.info("Model monotonic_cst uyumluluk düzeltmesiyle yüklendi.")
        return model


def ensure_models_loaded() -> tuple[bool, Optional[str]]:
    """
    Model dosyaları henüz yüklenmemişse yükler.
    (True, None) → başarılı; (False, hata_mesajı) → hata.
    """
    if _bundle.loaded:
        return True, None

    for path, label in [
        (MODEL_PATH, "Model"), (ENCODER_PATH, "Encoder"), (COLUMNS_PATH, "Sütun listesi")
    ]:
        if not path.exists():
            return False, f"{label} dosyası bulunamadı: {path}"

    try:
        _bundle.model = _load_model_compat(MODEL_PATH)
        _bundle.encoder = joblib.load(str(ENCODER_PATH))
        _bundle.columns = joblib.load(str(COLUMNS_PATH))
        _bundle.loaded = True
        logger.info("ML modelleri başarıyla yüklendi.")
        return True, None
    except Exception as exc:
        return False, f"Model yükleme hatası: {exc}"


# ─────────────────────────────────────────────
# Tahmin
# ─────────────────────────────────────────────

def _preprocess(df: pd.DataFrame) -> pd.DataFrame:
    """Veriyi model girdisi formatına dönüştürür."""
    # 1) Gereksiz sütunları kaldır
    drop_cols = [c for c in COLUMNS_TO_DROP if c in df.columns]
    df = df.drop(columns=drop_cols, errors="ignore")

    # 2) One-Hot Encoding
    cat_cols = [c for c in CATEGORICAL_COLUMNS if c in df.columns]
    df = pd.get_dummies(df, columns=cat_cols, drop_first=False, dtype=int)

    # 3) Eğitim sütun düzenine hizala (eksik → 0, fazla → at)
    df = df.reindex(columns=_bundle.columns, fill_value=0)

    # 4) Sayısala dönüştür
    df = df.apply(pd.to_numeric, errors="coerce").fillna(0)
    return df


def predict_kullanim_alani(kumas_data: dict) -> dict:
    """
    Kumaş verisinden kullanım alanı tahmini yapar.

    Args:
        kumas_data: Veritabanı satırına karşılık gelen dict
                    (Kumas_Tur, Kumas_Likra_%, ...).

    Returns:
        {
            "status": "success" | "error",
            "tahmin": str,
            "oran": float,        # %
            "tum_skorlar": {str: float}
        }
    """
    ok, err = ensure_models_loaded()
    if not ok:
        return {"status": "error", "message": err}

    try:
        df = pd.DataFrame([kumas_data])
        X = _preprocess(df)

        probas = _bundle.model.predict_proba(X)[0]
        best_idx = int(np.argmax(probas))
        tahmin = _bundle.encoder.classes_[best_idx]
        oran = float(probas[best_idx] * 100)

        tum_skorlar = {
            str(cls): round(float(p * 100), 4)
            for cls, p in zip(_bundle.encoder.classes_, probas)
        }

        return {
            "status": "success",
            "tahmin": tahmin,
            "oran": round(oran, 4),
            "tum_skorlar": tum_skorlar,
        }
    except Exception as exc:
        logger.exception("Tahmin hatası")
        return {"status": "error", "message": str(exc)}


# ─────────────────────────────────────────────
# Üretim adet hesaplama
# ─────────────────────────────────────────────

def hesapla_tahmini_adetler(
    uzunluk_m: float,
    en_cm: float,
    kullanim_alani: str,
) -> dict[str, float]:
    """
    Kumaş boyutlarına ve kullanım alanına göre üretilebilecek adet hesaplar.

    Returns:
        {"Elbise": 42.0, ...}  — her alt tür için tahmini adet sayısı
    """
    en_m = en_cm / 100.0
    toplam_alan = uzunluk_m * en_m

    if kullanim_alani in COKLU_METRAJLAR:
        return {
            ad: round(toplam_alan / metraj * FIRE_ORANI)
            for ad, metraj in COKLU_METRAJLAR[kullanim_alani].items()
        }

    if kullanim_alani in TEKIL_METRAJLAR:
        metraj = TEKIL_METRAJLAR[kullanim_alani]
        adet = round(toplam_alan / metraj * FIRE_ORANI)
        return {kullanim_alani: adet}

    return {}
