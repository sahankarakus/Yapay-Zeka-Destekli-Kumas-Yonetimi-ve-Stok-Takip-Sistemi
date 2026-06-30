"""
FastAPI — Kumaş Yönetim ve Stok Takip Sistemi API
"""
from __future__ import annotations

import logging
from contextlib import asynccontextmanager
from typing import Any, Optional

from fastapi import FastAPI, HTTPException, Query, BackgroundTasks
from fastapi.middleware.cors import CORSMiddleware

import database as db
import ml_service as ml
from models import (
    KumasCreate,
    KumasResponse,
    MetrajGuncelle,
    TahminResponse,
    AdetHesaplamaResponse,
    KUMAS_TURLERI,
    RENKLER,
    LIKRA_YONLERI,
    SU_ITICI_SECENEKLER,
    KULLANIM_DONEMLERI,
    KULLANIM_ALANLARI,
)

logging.basicConfig(level=logging.INFO, format="%(levelname)s | %(name)s | %(message)s")
logger = logging.getLogger(__name__)


# ─────────────────────────────────────────────
# Yaşam döngüsü
# ─────────────────────────────────────────────

@asynccontextmanager
async def lifespan(app: FastAPI):
    """Uygulama başlatılırken ML modellerini önceden yükle."""
    logger.info("ML modelleri yükleniyor...")
    ok, err = ml.ensure_models_loaded()
    if ok:
        logger.info("✅ ML modelleri hazır.")
    else:
        logger.warning(f"⚠️  ML modelleri yüklenemedi: {err}")
    yield
    logger.info("Uygulama kapatılıyor.")


# ─────────────────────────────────────────────
# Uygulama
# ─────────────────────────────────────────────

app = FastAPI(
    title="Kumaş Yönetim API",
    description="Yapay Zeka Destekli Kumaş Yönetimi ve Stok Takip Sistemi",
    version="2.0.0",
    lifespan=lifespan,
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["http://localhost:3000", "http://127.0.0.1:3000"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


# ─────────────────────────────────────────────
# Yardımcılar
# ─────────────────────────────────────────────

def _not_found(kumas_id: int) -> HTTPException:
    return HTTPException(status_code=404, detail=f"Kumaş bulunamadı (ID={kumas_id})")


def _predict_and_update(kumas_id: int, kumas_data: dict) -> None:
    """Arka planda ML tahmini yapıp veritabanını günceller."""
    result = ml.predict_kullanim_alani(kumas_data)
    if result.get("status") == "success":
        db.update_kullanim_alani(kumas_id, result["tahmin"])
        logger.info(f"ID={kumas_id} → tahmin: {result['tahmin']} ({result['oran']:.1f}%)")


# ─────────────────────────────────────────────
# Endpoints: Meta / Enum
# ─────────────────────────────────────────────

@app.get("/api/meta/secenekler", tags=["Meta"])
def get_secenekler():
    """Form dropdown seçeneklerini döndürür."""
    return {
        "tur": KUMAS_TURLERI,
        "renk": RENKLER,
        "likra_yonu": LIKRA_YONLERI,
        "su_itici": SU_ITICI_SECENEKLER,
        "kullanim_donemi": KULLANIM_DONEMLERI,
        "kullanim_alani": KULLANIM_ALANLARI,
    }


@app.get("/api/istatistik", tags=["Meta"])
def get_istatistik():
    """Dashboard özet istatistikleri."""
    return db.get_stats()


# ─────────────────────────────────────────────
# Endpoints: CRUD
# ─────────────────────────────────────────────

@app.get("/api/kumaslar", tags=["Kumaşlar"])
def list_kumaslar(
    tur: Optional[str] = Query(None),
    likra_yonu: Optional[str] = Query(None),
    su_itici: Optional[str] = Query(None),
    kullanim_donemi: Optional[str] = Query(None),
    kullanim_alani: Optional[str] = Query(None),
    min_likra: Optional[float] = Query(None),
    max_likra: Optional[float] = Query(None),
    min_gramaj: Optional[float] = Query(None),
    max_gramaj: Optional[float] = Query(None),
    search: Optional[str] = Query(None, description="Kumaş adı / tür / kullanım alanında arama"),
):
    """Tüm kumaşları listeler. Tüm parametreler isteğe bağlıdır."""
    return db.get_all_kumaslar(
        tur=tur, likra_yonu=likra_yonu, su_itici=su_itici,
        kullanim_donemi=kullanim_donemi, kullanim_alani=kullanim_alani,
        min_likra=min_likra, max_likra=max_likra,
        min_gramaj=min_gramaj, max_gramaj=max_gramaj,
        search=search,
    )


@app.get("/api/kumaslar/{kumas_id}", tags=["Kumaşlar"])
def get_kumas(kumas_id: int):
    """Tek kumaş döndürür."""
    row = db.get_kumas_by_id(kumas_id)
    if row is None:
        raise _not_found(kumas_id)
    return row


@app.post("/api/kumaslar", status_code=201, tags=["Kumaşlar"])
def create_kumas(payload: KumasCreate, background_tasks: BackgroundTasks):
    """
    Yeni kumaş ekler.
    ML tahmini arka planda çalışır; Kullanim_Alani önce 'Bekleniyor...' olarak kaydedilir.
    """
    data = payload.model_dump(by_alias=True)
    # alias'ı düzelt: Kumas_Likra_Yuzde → Kumas_Likra_%
    data["Kumas_Likra_%"] = data.pop("Kumas_Likra_%", data.pop("Kumas_Likra_Yuzde", 0))

    kumas_id = db.create_kumas(data)
    row = db.get_kumas_by_id(kumas_id)

    # ML tahmini arka planda
    background_tasks.add_task(_predict_and_update, kumas_id, dict(row))

    return {"id": kumas_id, "message": "Kumaş eklendi, ML tahmini işleniyor...", "data": row}


@app.delete("/api/kumaslar/{kumas_id}", tags=["Kumaşlar"])
def delete_kumas(kumas_id: int):
    """Kumaş siler."""
    success = db.delete_kumas(kumas_id)
    if not success:
        raise _not_found(kumas_id)
    return {"message": f"Kumaş silindi (ID={kumas_id})"}


@app.patch("/api/kumaslar/{kumas_id}/metraj", tags=["Kumaşlar"])
def update_metraj(kumas_id: int, payload: MetrajGuncelle):
    """Kumaş uzunluğunu günceller ve tahmini adet hesaplar."""
    success = db.update_metraj(kumas_id, payload.uzunluk)
    if not success:
        raise _not_found(kumas_id)

    row = db.get_kumas_by_id(kumas_id)
    en_cm = float(row.get("Kumas_En_cm", 0))
    kullanim_alani = row.get("Kullanim_Alani", "")

    hesaplama = ml.hesapla_tahmini_adetler(payload.uzunluk, en_cm, kullanim_alani)

    return {
        "message": "Metraj güncellendi",
        "uzunluk_m": payload.uzunluk,
        "kullanim_alani": kullanim_alani,
        "tahmini_uretim": hesaplama,
        "data": row,
    }


# ─────────────────────────────────────────────
# Endpoints: ML
# ─────────────────────────────────────────────

@app.get("/api/kumaslar/{kumas_id}/tahmin", response_model=TahminResponse, tags=["ML"])
def get_tahmin(kumas_id: int):
    """
    Seçili kumaş için kullanım alanı tahminini ve tüm sınıf olasılıklarını döndürür.
    Sonuç veritabanına da yazılır.
    """
    row = db.get_kumas_by_id(kumas_id)
    if row is None:
        raise _not_found(kumas_id)

    result = ml.predict_kullanim_alani(dict(row))

    if result.get("status") == "success":
        db.update_kullanim_alani(kumas_id, result["tahmin"])
        return TahminResponse(
            status="success",
            kumas_id=kumas_id,
            tahmin=result["tahmin"],
            oran=result["oran"],
            tum_skorlar=result["tum_skorlar"],
        )

    return TahminResponse(status="error", message=result.get("message", "Bilinmeyen hata"))


@app.get("/api/kumaslar/{kumas_id}/uretim", tags=["ML"])
def get_uretim_hesaplama(kumas_id: int):
    """Seçili kumaşın mevcut boyutlarıyla üretilebilecek adetleri hesaplar."""
    row = db.get_kumas_by_id(kumas_id)
    if row is None:
        raise _not_found(kumas_id)

    uzunluk = float(row.get("Kumas_Uzunluk_m", 0))
    en_cm = float(row.get("Kumas_En_cm", 0))
    kullanim_alani = row.get("Kullanim_Alani", "")

    hesaplama = ml.hesapla_tahmini_adetler(uzunluk, en_cm, kullanim_alani)

    return {
        "kumas_id": kumas_id,
        "uzunluk_m": uzunluk,
        "en_cm": en_cm,
        "kullanim_alani": kullanim_alani,
        "tahmini_uretim": hesaplama,
    }


# ─────────────────────────────────────────────
# Sağlık kontrolü
# ─────────────────────────────────────────────

@app.get("/health", tags=["Meta"])
def health_check():
    """Servis sağlık durumu."""
    return {
        "status": "ok",
        "ml_model_loaded": ml._bundle.loaded,
        "db_path": str(db.DB_PATH),
        "db_exists": db.DB_PATH.exists(),
    }
