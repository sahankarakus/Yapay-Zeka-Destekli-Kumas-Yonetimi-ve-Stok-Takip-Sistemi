"use client";

import { useState, useEffect, useCallback, useRef } from "react";
import Navbar from "@/components/Navbar";
import KumasTable from "@/components/KumasTable";
import TahminPanel from "@/components/TahminPanel";
import KumasEkleModal from "@/components/KumasEkleModal";
import { api, type Kumas, type TahminResult, type Istatistik } from "@/lib/api";
import styles from "./page.module.css";

interface Toast {
  id: number;
  type: "success" | "error" | "info";
  message: string;
}

let toastId = 0;

export default function HomePage() {
  const [kumaslar, setKumaslar] = useState<Kumas[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState("");
  const [istatistik, setIstatistik] = useState<Istatistik | null>(null);

  // Seçili kumaş & ML
  const [selectedKumas, setSelectedKumas] = useState<Kumas | null>(null);
  const [tahmin, setTahmin] = useState<TahminResult | null>(null);
  const [tahminLoading, setTahminLoading] = useState(false);
  const [uretim, setUretim] = useState<Record<string, number> | null>(null);

  // Modals
  const [showEkle, setShowEkle] = useState(false);
  const [metrajModal, setMetrajModal] = useState<{ id: number; current: number } | null>(null);
  const [metrajInput, setMetrajInput] = useState("");

  // Toasts
  const [toasts, setToasts] = useState<Toast[]>([]);

  function addToast(type: Toast["type"], message: string) {
    const id = ++toastId;
    setToasts(t => [...t, { id, type, message }]);
    setTimeout(() => setToasts(t => t.filter(x => x.id !== id)), 4000);
  }

  // ─── Veri yükleme ───────────────────────────────────────────────────────────

  const fetchKumaslar = useCallback(async () => {
    setLoading(true);
    try {
      const [data, stats] = await Promise.all([
        api.listKumaslar({ search: search || undefined }),
        api.getIstatistik(),
      ]);
      setKumaslar(data);
      setIstatistik(stats);
    } catch {
      addToast("error", "Veriler yüklenemedi. Backend çalışıyor mu?");
    } finally {
      setLoading(false);
    }
  }, [search]);

  useEffect(() => {
    const timer = setTimeout(fetchKumaslar, 300); // debounce
    return () => clearTimeout(timer);
  }, [fetchKumaslar]);

  // ─── Silme ──────────────────────────────────────────────────────────────────

  async function handleDelete(id: number) {
    try {
      await api.deleteKumas(id);
      addToast("success", "Kumaş silindi.");
      if (selectedKumas?.ID === id) {
        setSelectedKumas(null);
        setTahmin(null);
        setUretim(null);
      }
      fetchKumaslar();
    } catch (ex: unknown) {
      addToast("error", ex instanceof Error ? ex.message : "Silme başarısız.");
    }
  }

  // ─── ML Tahmin ──────────────────────────────────────────────────────────────

  async function handleTahminAl(id: number) {
    const k = kumaslar.find(x => x.ID === id);
    setSelectedKumas(k ?? null);
    setTahminLoading(true);
    setTahmin(null);
    setUretim(null);
    try {
      const [t, u] = await Promise.all([
        api.getTahmin(id),
        api.getUretim(id),
      ]);
      setTahmin(t);
      if (u.tahmini_uretim) setUretim(u.tahmini_uretim);
      if (t.status === "success") {
        addToast("success", `Tahmin: ${t.tahmin} (%${t.oran?.toFixed(1)} güven)`);
        fetchKumaslar(); // Kullanim_Alani güncellenmiş olabilir
      }
    } catch (ex: unknown) {
      addToast("error", ex instanceof Error ? ex.message : "Tahmin alınamadı.");
    } finally {
      setTahminLoading(false);
    }
  }

  // ─── Metraj güncelleme ──────────────────────────────────────────────────────

  function openMetraj(id: number) {
    const k = kumaslar.find(x => x.ID === id);
    setMetrajModal({ id, current: k?.Kumas_Uzunluk_m ?? 0 });
    setMetrajInput(String(k?.Kumas_Uzunluk_m ?? ""));
  }

  async function saveMetraj() {
    if (!metrajModal) return;
    const val = parseFloat(metrajInput.replace(",", "."));
    if (isNaN(val) || val <= 0) {
      addToast("error", "Geçerli bir uzunluk girin.");
      return;
    }
    try {
      const res = await api.updateMetraj(metrajModal.id, val);
      addToast("success", `Metraj güncellendi: ${val} m`);
      if (res.tahmini_uretim) setUretim(res.tahmini_uretim);
      setMetrajModal(null);
      fetchKumaslar();
    } catch (ex: unknown) {
      addToast("error", ex instanceof Error ? ex.message : "Güncelleme başarısız.");
    }
  }

  // ─── Render ─────────────────────────────────────────────────────────────────

  return (
    <div className={styles.root}>
      <Navbar />

      <main className={styles.main}>
        {/* Başlık */}
        <div className={styles.pageHeader}>
          <div>
            <h1>
              Kumaş Stok Listesi
              <span className="material-symbols-outlined" style={{ fontSize: 32, color: "var(--primary)" }}>inventory_2</span>
            </h1>
            <p>Tüm kumaşlarınızı yönetin, ML tahmini alın</p>
          </div>
          <button className="btn btn-primary btn-lg" onClick={() => setShowEkle(true)}>
            <span className="material-symbols-outlined" style={{ fontSize: 18 }}>add</span>
            Kumaş Ekle
          </button>
        </div>

        {/* İstatistik kartları */}
        {istatistik && (
          <div className={styles.statRow}>
            <div className="card stat-card">
              <span className="stat-label">Toplam Kumaş</span>
              <span className="stat-value">{istatistik.toplam_kumas}</span>
              <span className="stat-sub">kayıt</span>
            </div>
            <div className="card stat-card">
              <span className="stat-label">Toplam Stok</span>
              <span className="stat-value">{istatistik.toplam_stok_m.toLocaleString("tr-TR")}</span>
              <span className="stat-sub">metre</span>
            </div>
            <div className="card stat-card">
              <span className="stat-label">En Çok Tür</span>
              <span className="stat-value" style={{ fontSize: "1.25rem" }}>
                {istatistik.tur_dagilimi[0]?.Kumas_Tur ?? "—"}
              </span>
              <span className="stat-sub">{istatistik.tur_dagilimi[0]?.adet ?? 0} adet</span>
            </div>
            <div className="card stat-card">
              <span className="stat-label">En Çok Kullanım</span>
              <span className="stat-value" style={{ fontSize: "1rem" }}>
                {istatistik.kullanim_alani_dagilimi[0]?.Kullanim_Alani ?? "—"}
              </span>
              <span className="stat-sub">{istatistik.kullanim_alani_dagilimi[0]?.adet ?? 0} adet</span>
            </div>
          </div>
        )}

        {/* İçerik alanı */}
        <div className={styles.contentArea}>
          {/* Sol: tablo */}
          <div className={styles.tableSection}>
            {/* Arama */}
            <div className={styles.searchBar}>
              <span className={`material-symbols-outlined ${styles.searchIcon}`}>search</span>
              <input
                className="input"
                style={{ paddingLeft: 44 }}
                placeholder="Kumaş adı, tür veya kullanım alanı ara..."
                value={search}
                onChange={e => setSearch(e.target.value)}
              />
              {search && (
                <button
                  className="btn btn-ghost btn-sm btn-icon"
                  style={{ position: "absolute", right: 24 }}
                  onClick={() => setSearch("")}
                >
                  <span className="material-symbols-outlined" style={{ fontSize: 18 }}>close</span>
                </button>
              )}
            </div>

            <KumasTable
              kumaslar={kumaslar}
              loading={loading}
              selectedId={selectedKumas?.ID ?? null}
              onSelect={k => { setSelectedKumas(k); setTahmin(null); setUretim(null); }}
              onDelete={handleDelete}
              onMetrajGuncelle={openMetraj}
              onTahminAl={handleTahminAl}
            />
          </div>

          {/* Sağ: ML Panel */}
          <aside className={styles.panel}>
            <div className="card-flat" style={{ height: "100%", overflow: "auto" }}>
              <div className={styles.aiAccentBar} />
              <div className={styles.panelHeader}>
                <span className={`material-symbols-outlined ${styles.panelHeaderIcon}`}>smart_toy</span>
                <h3>AI Analiz</h3>
              </div>
              <TahminPanel
                kumasAd={selectedKumas?.Kumas_Ad ?? null}
                tahmin={tahmin}
                uretim={uretim}
                loading={tahminLoading}
              />
            </div>
          </aside>
        </div>
      </main>

      {/* Modal: Kumaş Ekle */}
      {showEkle && (
        <KumasEkleModal
          onClose={() => setShowEkle(false)}
          onSuccess={() => {
            addToast("success", "Kumaş eklendi! ML tahmini arka planda çalışıyor...");
            fetchKumaslar();
          }}
        />
      )}

      {/* Modal: Metraj Güncelle */}
      {metrajModal && (
        <div className="overlay" onClick={() => setMetrajModal(null)}>
          <div className="modal" style={{ maxWidth: 400 }} onClick={e => e.stopPropagation()}>
            <div className="modal-header">
              <h3 style={{ display: "flex", alignItems: "center", gap: 8 }}>
                <span className="material-symbols-outlined" style={{ color: "var(--primary)", fontSize: 20 }}>edit</span>
                Metraj Güncelle
              </h3>
              <button className="btn btn-ghost btn-icon" onClick={() => setMetrajModal(null)}>
                <span className="material-symbols-outlined" style={{ fontSize: 18 }}>close</span>
              </button>
            </div>
            <div className="field" style={{ marginBottom: 20 }}>
              <label className="label">Yeni Uzunluk (metre)</label>
              <input
                className="input"
                type="number"
                min={0}
                step={0.1}
                value={metrajInput}
                onChange={e => setMetrajInput(e.target.value)}
                autoFocus
                onKeyDown={e => e.key === "Enter" && saveMetraj()}
              />
              <span style={{ fontSize: "0.8125rem", color: "var(--text-muted)" }}>
                Mevcut: {metrajModal.current} m
              </span>
            </div>
            <div style={{ display: "flex", justifyContent: "flex-end", gap: 12 }}>
              <button className="btn btn-ghost" onClick={() => setMetrajModal(null)}>İptal</button>
              <button className="btn btn-primary" onClick={saveMetraj}>Kaydet</button>
            </div>
          </div>
        </div>
      )}

      {/* Toasts */}
      <div className="toast-container">
        {toasts.map(t => (
          <div key={t.id} className={`toast toast-${t.type}`}>
            {t.message}
          </div>
        ))}
      </div>
    </div>
  );
}
