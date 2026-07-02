/**
 * API istemci katmanı — tüm backend istekleri buradan geçer.
 */

// Browser'da Next.js proxy üzerinden gider (/api/... → http://localhost:8000/api/...)
// Server-side render sırasında doğrudan backend'e bağlanır
const API_BASE = process.env.NEXT_PUBLIC_API_URL
  ?? (typeof window !== "undefined" ? "" : "http://localhost:8000");

// ─────────────────────────────────────────────
// Tipler
// ─────────────────────────────────────────────

export interface Kumas {
  ID: number;
  Kumas_Ad: string;
  Kumas_Tur: string;
  Kumas_Renk: string;
  "Kumas_Likra_%": number;
  Kumas_Likra_Yonu: string;
  Kumas_Uzunluk_m: number;
  Kumas_En_cm: number;
  Kumas_Gramaj_gm2: number;
  Kumas_SuItici: string;
  Kullanim_Donemi: string;
  Kullanim_Alani: string | null;
}

export interface KumasCreate {
  Kumas_Ad: string;
  Kumas_Tur: string;
  Kumas_Renk: string;
  "Kumas_Likra_%": number;
  Kumas_Likra_Yonu: string;
  Kumas_Uzunluk_m: number;
  Kumas_En_cm: number;
  Kumas_Gramaj_gm2: number;
  Kumas_SuItici: string;
  Kullanim_Donemi: string;
}

export interface TahminResult {
  status: "success" | "error";
  kumas_id?: number;
  tahmin?: string;
  oran?: number;
  tum_skorlar?: Record<string, number>;
  message?: string;
}

export interface Secenekler {
  tur: string[];
  renk: string[];
  likra_yonu: string[];
  su_itici: string[];
  kullanim_donemi: string[];
  kullanim_alani: string[];
}

export interface Istatistik {
  toplam_kumas: number;
  toplam_stok_m: number;
  tur_dagilimi: Array<{ Kumas_Tur: string; adet: number }>;
  kullanim_alani_dagilimi: Array<{ Kullanim_Alani: string; adet: number }>;
}

export interface UretimHesaplama {
  kumas_id: number;
  uzunluk_m: number;
  en_cm: number;
  kullanim_alani: string;
  tahmini_uretim: Record<string, number>;
}

export interface ListeFiltre {
  tur?: string;
  likra_yonu?: string;
  su_itici?: string;
  kullanim_donemi?: string;
  kullanim_alani?: string;
  min_likra?: number;
  max_likra?: number;
  min_gramaj?: number;
  max_gramaj?: number;
  search?: string;
}

// ─────────────────────────────────────────────
// Temel fetch sarmalayıcı
// ─────────────────────────────────────────────

async function apiFetch<T>(
  path: string,
  init?: RequestInit
): Promise<T> {
  const res = await fetch(`${API_BASE}${path}`, {
    headers: { "Content-Type": "application/json" },
    ...init,
  });

  if (!res.ok) {
    const err = await res.json().catch(() => ({ detail: res.statusText }));
    throw new Error(err.detail ?? "API hatası");
  }

  return res.json() as Promise<T>;
}

function buildQuery(params: Record<string, string | number | undefined>): string {
  const qs = Object.entries(params)
    .filter(([, v]) => v !== undefined && v !== "" && v !== null)
    .map(([k, v]) => `${encodeURIComponent(k)}=${encodeURIComponent(String(v))}`)
    .join("&");
  return qs ? `?${qs}` : "";
}

// ─────────────────────────────────────────────
// API fonksiyonları
// ─────────────────────────────────────────────

export const api = {
  /** Tüm kumaşları listele (isteğe bağlı filtreler ile). */
  listKumaslar: (filtre: ListeFiltre = {}): Promise<Kumas[]> =>
    apiFetch<Kumas[]>(`/api/kumaslar${buildQuery(filtre as Record<string, string | number | undefined>)}`),

  /** Tek kumaş getir. */
  getKumas: (id: number): Promise<Kumas> =>
    apiFetch<Kumas>(`/api/kumaslar/${id}`),

  /** Yeni kumaş ekle. */
  createKumas: (data: KumasCreate): Promise<{ id: number; message: string; data: Kumas }> =>
    apiFetch(`/api/kumaslar`, {
      method: "POST",
      body: JSON.stringify(data),
    }),

  /** Kumaş sil. */
  deleteKumas: (id: number): Promise<{ message: string }> =>
    apiFetch(`/api/kumaslar/${id}`, { method: "DELETE" }),

  /** Metraj güncelle. */
  updateMetraj: (
    id: number,
    uzunluk: number
  ): Promise<{ message: string; uzunluk_m: number; tahmini_uretim: Record<string, number>; data: Kumas }> =>
    apiFetch(`/api/kumaslar/${id}/metraj`, {
      method: "PATCH",
      body: JSON.stringify({ uzunluk }),
    }),

  /** ML ile kullanım alanı tahmini al. */
  getTahmin: (id: number): Promise<TahminResult> =>
    apiFetch<TahminResult>(`/api/kumaslar/${id}/tahmin`),

  /** Üretim adet hesaplama. */
  getUretim: (id: number): Promise<UretimHesaplama> =>
    apiFetch<UretimHesaplama>(`/api/kumaslar/${id}/uretim`),

  /** Form dropdown seçenekleri. */
  getSecenekler: (): Promise<Secenekler> =>
    apiFetch<Secenekler>(`/api/meta/secenekler`),

  /** Dashboard istatistikleri. */
  getIstatistik: (): Promise<Istatistik> =>
    apiFetch<Istatistik>(`/api/istatistik`),
};
