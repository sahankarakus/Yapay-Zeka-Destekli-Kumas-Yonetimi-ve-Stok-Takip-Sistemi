"""
Veritabanı katmanı — SQLite CRUD işlemleri.
Thread-safe bağlantı havuzu yerine bağlam yöneticisi kullanılır.
"""
from __future__ import annotations

import sqlite3
from contextlib import contextmanager
from pathlib import Path
from typing import Any, Generator, Optional

# ─────────────────────────────────────────────
# Yollar
# ─────────────────────────────────────────────
ROOT_DIR = Path(__file__).parent.parent
DB_PATH = ROOT_DIR / "kumasVerileri.db"

TABLE = "KullaniciniKumaslari"

# Orijinal SQLite sütun adı (Kumas_Likra_%) köşeli parantez gerektiriyor
LIKRA_COL = "[Kumas_Likra_%]"


# ─────────────────────────────────────────────
# Bağlantı yöneticisi
# ─────────────────────────────────────────────

@contextmanager
def get_connection() -> Generator[sqlite3.Connection, None, None]:
    """Thread-safe SQLite bağlantısı sağlar."""
    conn = sqlite3.connect(str(DB_PATH), check_same_thread=False)
    conn.row_factory = sqlite3.Row  # sözlük benzeri erişim
    conn.execute("PRAGMA journal_mode=WAL")  # eş zamanlı okuma/yazma
    conn.execute("PRAGMA foreign_keys=ON")
    try:
        yield conn
        conn.commit()
    except Exception:
        conn.rollback()
        raise
    finally:
        conn.close()


# ─────────────────────────────────────────────
# Yardımcı dönüştürücü
# ─────────────────────────────────────────────

def row_to_dict(row: sqlite3.Row) -> dict[str, Any]:
    """sqlite3.Row'u JSON-serileştirilebilir dict'e çevirir."""
    return dict(row)


# ─────────────────────────────────────────────
# READ
# ─────────────────────────────────────────────

def get_all_kumaslar(
    tur: Optional[str] = None,
    likra_yonu: Optional[str] = None,
    su_itici: Optional[str] = None,
    kullanim_donemi: Optional[str] = None,
    kullanim_alani: Optional[str] = None,
    min_likra: Optional[float] = None,
    max_likra: Optional[float] = None,
    min_gramaj: Optional[float] = None,
    max_gramaj: Optional[float] = None,
    search: Optional[str] = None,
) -> list[dict[str, Any]]:
    """Filtreleme destekli kumaş listesi döndürür."""
    conditions: list[str] = []
    params: list[Any] = []

    if tur:
        conditions.append("Kumas_Tur = ?")
        params.append(tur)
    if likra_yonu:
        conditions.append("Kumas_Likra_Yonu = ?")
        params.append(likra_yonu)
    if su_itici:
        conditions.append("Kumas_SuItici = ?")
        params.append(su_itici)
    if kullanim_donemi:
        conditions.append("Kullanim_Donemi = ?")
        params.append(kullanim_donemi)
    if kullanim_alani:
        conditions.append("Kullanim_Alani = ?")
        params.append(kullanim_alani)
    if min_likra is not None:
        conditions.append(f"{LIKRA_COL} >= ?")
        params.append(min_likra)
    if max_likra is not None:
        conditions.append(f"{LIKRA_COL} <= ?")
        params.append(max_likra)
    if min_gramaj is not None:
        conditions.append("Kumas_Gramaj_gm2 >= ?")
        params.append(min_gramaj)
    if max_gramaj is not None:
        conditions.append("Kumas_Gramaj_gm2 <= ?")
        params.append(max_gramaj)
    if search:
        conditions.append("(Kumas_Ad LIKE ? OR Kumas_Tur LIKE ? OR Kullanim_Alani LIKE ?)")
        like = f"%{search}%"
        params.extend([like, like, like])

    where = ("WHERE " + " AND ".join(conditions)) if conditions else ""
    query = f"SELECT * FROM {TABLE} {where} ORDER BY ID DESC"

    with get_connection() as conn:
        rows = conn.execute(query, params).fetchall()
    return [row_to_dict(r) for r in rows]


def get_kumas_by_id(kumas_id: int) -> Optional[dict[str, Any]]:
    """ID'ye göre tek kumaş döndürür."""
    with get_connection() as conn:
        row = conn.execute(
            f"SELECT * FROM {TABLE} WHERE ID = ?", (kumas_id,)
        ).fetchone()
    return row_to_dict(row) if row else None


# ─────────────────────────────────────────────
# CREATE
# ─────────────────────────────────────────────

def create_kumas(data: dict[str, Any]) -> int:
    """Yeni kumaş ekler; eklenen satırın ID'sini döndürür."""
    query = f"""
        INSERT INTO {TABLE}
            (Kumas_Ad, Kumas_Tur, Kumas_Renk, {LIKRA_COL},
             Kumas_Likra_Yonu, Kumas_Uzunluk_m, Kumas_En_cm,
             Kumas_Gramaj_gm2, Kumas_SuItici, Kullanim_Donemi, Kullanim_Alani)
        VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
    """
    params = (
        data["Kumas_Ad"],
        data["Kumas_Tur"],
        data["Kumas_Renk"],
        data.get("Kumas_Likra_%", data.get("Kumas_Likra_Yuzde", 0)),
        data["Kumas_Likra_Yonu"],
        data["Kumas_Uzunluk_m"],
        data["Kumas_En_cm"],
        data["Kumas_Gramaj_gm2"],
        data["Kumas_SuItici"],
        data["Kullanim_Donemi"],
        data.get("Kullanim_Alani", "Bekleniyor..."),
    )
    with get_connection() as conn:
        cursor = conn.execute(query, params)
        return cursor.lastrowid


def update_kullanim_alani(kumas_id: int, kullanim_alani: str) -> None:
    """ML tahmini sonrası kullanım alanını günceller."""
    with get_connection() as conn:
        conn.execute(
            f"UPDATE {TABLE} SET Kullanim_Alani = ? WHERE ID = ?",
            (kullanim_alani, kumas_id),
        )


# ─────────────────────────────────────────────
# UPDATE
# ─────────────────────────────────────────────

def update_metraj(kumas_id: int, uzunluk: float) -> bool:
    """Kumaş uzunluğunu günceller. Başarıysa True döner."""
    with get_connection() as conn:
        cursor = conn.execute(
            f"UPDATE {TABLE} SET Kumas_Uzunluk_m = ? WHERE ID = ?",
            (uzunluk, kumas_id),
        )
    return cursor.rowcount > 0


# ─────────────────────────────────────────────
# DELETE
# ─────────────────────────────────────────────

def delete_kumas(kumas_id: int) -> bool:
    """Kumaşı siler. Başarıysa True döner."""
    with get_connection() as conn:
        cursor = conn.execute(
            f"DELETE FROM {TABLE} WHERE ID = ?", (kumas_id,)
        )
    return cursor.rowcount > 0


# ─────────────────────────────────────────────
# İSTATİSTİK
# ─────────────────────────────────────────────

def get_stats() -> dict[str, Any]:
    """Özet istatistikleri döndürür."""
    with get_connection() as conn:
        total = conn.execute(f"SELECT COUNT(*) FROM {TABLE}").fetchone()[0]
        tur_dagilim = conn.execute(
            f"SELECT Kumas_Tur, COUNT(*) as adet FROM {TABLE} GROUP BY Kumas_Tur ORDER BY adet DESC"
        ).fetchall()
        alan_dagilim = conn.execute(
            f"SELECT Kullanim_Alani, COUNT(*) as adet FROM {TABLE} GROUP BY Kullanim_Alani ORDER BY adet DESC LIMIT 5"
        ).fetchall()
        toplam_stok = conn.execute(
            f"SELECT COALESCE(SUM(Kumas_Uzunluk_m), 0) FROM {TABLE}"
        ).fetchone()[0]
    return {
        "toplam_kumas": total,
        "toplam_stok_m": round(float(toplam_stok), 2),
        "tur_dagilimi": [dict(r) for r in tur_dagilim],
        "kullanim_alani_dagilimi": [dict(r) for r in alan_dagilim],
    }
