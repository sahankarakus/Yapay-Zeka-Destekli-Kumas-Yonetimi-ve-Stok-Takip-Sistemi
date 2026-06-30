"use client";

import type { TahminResult } from "@/lib/api";
import styles from "./TahminPanel.module.css";

interface Props {
  kumasAd: string | null;
  tahmin: TahminResult | null;
  uretim: Record<string, number> | null;
  loading: boolean;
}

export default function TahminPanel({ kumasAd, tahmin, uretim, loading }: Props) {
  if (!kumasAd && !loading) {
    return (
      <div className={styles.empty}>
        <div className={styles.emptyIcon}>🤖</div>
        <p className={styles.emptyText}>
          Tablodan bir kumaş seçin ve <strong>ML Tahmin</strong> butonuna basın.
        </p>
      </div>
    );
  }

  if (loading) {
    return (
      <div className={styles.loading}>
        <div className="spinner" />
        <p>ML modeli hesaplıyor...</p>
      </div>
    );
  }

  if (!tahmin) return null;

  if (tahmin.status === "error") {
    return (
      <div className={styles.error}>
        <span>⚠️</span>
        <p>{tahmin.message ?? "Bilinmeyen hata"}</p>
      </div>
    );
  }

  const skorlar = tahmin.tum_skorlar
    ? Object.entries(tahmin.tum_skorlar)
        .filter(([, v]) => v > 5)
        .sort(([, a], [, b]) => b - a)
        .slice(0, 8)
    : [];

  return (
    <div className={styles.panel}>
      {/* Header */}
      <div className={styles.header}>
        <span className={styles.headerIcon}>🤖</span>
        <div>
          <h4 className={styles.headerTitle}>ML Tahmin Sonucu</h4>
          <p className={styles.headerSub}>{kumasAd}</p>
        </div>
      </div>

      {/* Ana tahmin */}
      <div className={styles.mainResult}>
        <span className={styles.mainLabel}>Önerilen Kullanım Alanı</span>
        <span className={styles.mainValue}>{tahmin.tahmin}</span>
        <span className={styles.mainOran}>
          %{tahmin.oran?.toFixed(1)} güven
        </span>
      </div>

      {/* Skor listesi */}
      {skorlar.length > 0 && (
        <div className={styles.skorList}>
          <p className={styles.skorTitle}>Tüm Kullanım Alanı Olasılıkları</p>
          {skorlar.map(([alan, skor]) => (
            <div key={alan} className={styles.skorItem}>
              <div className={styles.skorRow}>
                <span className={styles.skorLabel}>{alan}</span>
                <span className={styles.skorValue}>{skor.toFixed(1)}%</span>
              </div>
              <div className="progress-bar">
                <div
                  className="progress-fill"
                  style={{ width: `${Math.min(skor, 100)}%` }}
                />
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Üretim hesaplama */}
      {uretim && Object.keys(uretim).length > 0 && (
        <div className={styles.uretim}>
          <p className={styles.uretimTitle}>📦 Tahmini Üretim Kapasitesi</p>
          {Object.entries(uretim).map(([ad, adet]) => (
            <div key={ad} className={styles.uretimItem}>
              <span>{ad}</span>
              <span className={styles.uretimAdet}>~{adet} adet</span>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
