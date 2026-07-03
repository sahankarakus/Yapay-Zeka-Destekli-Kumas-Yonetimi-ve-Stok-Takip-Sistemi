"""
Pydantic veri modelleri — kumaş verisi için tip güvenli şemalar.
"""
from __future__ import annotations

from typing import Optional
from pydantic import BaseModel, Field, field_validator


# ─────────────────────────────────────────────
# Veritabanı alanlarına birebir karşılık gelen şema
# ─────────────────────────────────────────────

KUMAS_TURLERI = [
    "Polyester", "Pamuk", "Naylon", "Yun", "Ipek", "Keten", "Kot", "Viskon"
]

RENKLER = [
    "Bej", "Krem", "Turuncu", "Beyaz", "Pembe", "Gri", "Lacivert",
    "Sari", "Mavi", "Kirmizi", "Siyah", "Kahverengi", "Yesil"
]

LIKRA_YONLERI = ["Enine", "Boyuna", "Her Iki Yonde", "Yok"]

SU_ITICI_SECENEKLER = ["Hayir", "Evet"]

KULLANIM_DONEMLERI = ["Dort Mevsim", "Yazlik", "Kislik"]

KULLANIM_ALANLARI = [
    "Elbise", "Spor Tisort", "Gomlek", "Etek", "Pantolon",
    "Tisort", "Esofman", "Spor Hirka", "Sort", "Tayt", "Mont",
    "TakimElbise", "Mayo", "IcGiyim",
]


class KumasBase(BaseModel):
    """Kumaş oluştururken / güncellerken kullanılan ortak alanlar."""
    Kumas_Ad: str = Field(..., min_length=1, max_length=200, description="Kumaş adı")
    Kumas_Tur: str = Field(..., description="Kumaş türü")
    Kumas_Renk: str = Field(..., description="Kumaş rengi")
    Kumas_Likra_Yuzde: float = Field(..., ge=0, le=100, alias="Kumas_Likra_%", description="Likra yüzdesi (%)")
    Kumas_Likra_Yonu: str = Field(..., description="Likra yönü")
    Kumas_Uzunluk_m: float = Field(..., gt=0, description="Uzunluk (metre)")
    Kumas_En_cm: float = Field(..., gt=0, description="En (santimetre)")
    Kumas_Gramaj_gm2: float = Field(..., gt=0, description="Gramaj (g/m²)")
    Kumas_SuItici: str = Field(..., description="Su iticilik")
    Kullanim_Donemi: str = Field(..., description="Kullanım dönemi")

    model_config = {"populate_by_name": True}

    @field_validator("Kumas_Uzunluk_m", "Kumas_En_cm", "Kumas_Gramaj_gm2", "Kumas_Likra_Yuzde", mode="before")
    @classmethod
    def parse_numeric(cls, v):
        if isinstance(v, str):
            return float(v.replace(",", "."))
        return v


class KumasCreate(KumasBase):
    """Yeni kumaş eklerken kullanılan şema."""
    pass


class KumasResponse(KumasBase):
    """Veritabanından okunan kumaşı temsil eden şema."""
    ID: int
    Kullanim_Alani: Optional[str] = Field(default=None, description="ML tarafından tahmin edilen kullanım alanı")

    model_config = {"populate_by_name": True, "from_attributes": True}


class MetrajGuncelle(BaseModel):
    """Metraj güncelleme isteği."""
    uzunluk: float = Field(..., gt=0, description="Yeni uzunluk (metre)")


class TahminResponse(BaseModel):
    """ML tahmin yanıtı."""
    status: str
    kumas_id: Optional[int] = None
    tahmin: Optional[str] = None
    oran: Optional[float] = None
    tum_skorlar: Optional[dict[str, float]] = None
    message: Optional[str] = None


class AdetHesaplamaResponse(BaseModel):
    """Tahmini üretim adedi hesaplama yanıtı."""
    uzunluk_m: float
    en_cm: float
    kullanim_alani: str
    hesaplama: dict[str, float]


class OneriFiltre(BaseModel):
    """Öneri/filtreleme isteği parametreleri."""
    tur: Optional[str] = None
    likra_yonu: Optional[str] = None
    su_itici: Optional[str] = None
    kullanim_donemi: Optional[str] = None
    kullanim_alani: Optional[str] = None
    min_likra: Optional[float] = None
    max_likra: Optional[float] = None
    min_gramaj: Optional[float] = None
    max_gramaj: Optional[float] = None
