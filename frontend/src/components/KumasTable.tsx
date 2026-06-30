"use client";

import { useState, useMemo, useEffect } from "react";
import type { Kumas } from "@/lib/api";
import styles from "./KumasTable.module.css";

interface Props {
  kumaslar: Kumas[];
  loading: boolean;
  selectedId: number | null;
  onSelect: (k: Kumas) => void;
  onDelete: (id: number) => void;
  onMetrajGuncelle: (id: number) => void;
  onTahminAl: (id: number) => void;
}

type SortKey = keyof Kumas;
type SortDir = "asc" | "desc";

const COL_MAP: { key: SortKey; label: string; width?: string }[] = [
  // Sütun genişliklerini buradan değiştirebilirsiniz (örn: "150px" veya "%10" gibi)
  { key: "ID", label: "#", width: "50px" },
  { key: "Kumas_Ad", label: "Kumaş Adı", width: "120px" },
  { key: "Kumas_Tur", label: "Tür", width: "85px" },
  { key: "Kumas_Renk", label: "Renk", width: "90px" },
  { key: "Kumas_Likra_%", label: "Likra %", width: "60px" },
  { key: "Kumas_Likra_Yonu", label: "Likra Yönü", width: "95px" },
  { key: "Kumas_Uzunluk_m", label: "Uzunluk (m)", width: "80px" },
  { key: "Kumas_En_cm", label: "En (cm)", width: "80px" },
  { key: "Kumas_Gramaj_gm2", label: "Gramaj", width: "100px" },
  { key: "Kumas_SuItici", label: "Su İtici", width: "75px" },
  { key: "Kullanim_Donemi", label: "Dönem", width: "100px" },
  { key: "Kullanim_Alani", label: "Kullanım Alanı", width: "110px" },
];

function SortIcon({ dir }: { dir: SortDir | null }) {
  if (!dir) return <span className={styles.sortIcon}>⇅</span>;
  return <span className={styles.sortIcon}>{dir === "asc" ? "↑" : "↓"}</span>;
}

export default function KumasTable({
  kumaslar,
  loading,
  selectedId,
  onSelect,
  onDelete,
  onMetrajGuncelle,
  onTahminAl,
}: Props) {
  const [sortKey, setSortKey] = useState<SortKey>("ID");
  const [sortDir, setSortDir] = useState<SortDir>("desc");
  const [deleteConfirm, setDeleteConfirm] = useState<number | null>(null);
  const [openDropdown, setOpenDropdown] = useState<number | null>(null);

  useEffect(() => {
    function handleClickOutside() {
      setOpenDropdown(null);
    }
    document.addEventListener("click", handleClickOutside);
    return () => document.removeEventListener("click", handleClickOutside);
  }, []);

  const sorted = useMemo(() => {
    return [...kumaslar].sort((a, b) => {
      const va = a[sortKey] ?? "";
      const vb = b[sortKey] ?? "";
      const cmp = typeof va === "number" && typeof vb === "number"
        ? va - vb
        : String(va).localeCompare(String(vb), "tr");
      return sortDir === "asc" ? cmp : -cmp;
    });
  }, [kumaslar, sortKey, sortDir]);

  function handleSort(key: SortKey) {
    if (key === sortKey) {
      setSortDir(d => d === "asc" ? "desc" : "asc");
    } else {
      setSortKey(key);
      setSortDir("asc");
    }
  }

  function handleDeleteClick(e: React.MouseEvent, id: number) {
    e.stopPropagation();
    setDeleteConfirm(id);
  }

  function confirmDelete(id: number) {
    onDelete(id);
    setDeleteConfirm(null);
  }

  if (loading) {
    return (
      <div className={styles.loading}>
        <div className="spinner" />
        <span>Veriler yükleniyor...</span>
      </div>
    );
  }

  if (kumaslar.length === 0) {
    return (
      <div className={styles.empty}>
        <div className={styles.emptyIcon}>🧶</div>
        <h3>Kumaş bulunamadı</h3>
        <p>Henüz kayıt yok veya filtrelerinizi değiştirin.</p>
      </div>
    );
  }

  return (
    <>
      {/* Delete confirm overlay */}
      {deleteConfirm !== null && (
        <div className="overlay" onClick={() => setDeleteConfirm(null)}>
          <div className="modal" style={{ maxWidth: 400 }} onClick={e => e.stopPropagation()}>
            <div className={styles.deleteModal}>
              <div className={styles.deleteIcon}>🗑️</div>
              <h3>Kumaşı Sil</h3>
              <p>Bu kumaşı kalıcı olarak silmek istediğinize emin misiniz?</p>
              <div className={styles.deleteActions}>
                <button className="btn btn-ghost" onClick={() => setDeleteConfirm(null)}>
                  İptal
                </button>
                <button className="btn btn-danger" onClick={() => confirmDelete(deleteConfirm)}>
                  Evet, Sil
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      <div className="table-wrapper">
        <table className="table">
          <thead>
            <tr>
              {COL_MAP.map(({ key, label, width }) => (
                <th key={key} onClick={() => handleSort(key)} style={{ width: width, minWidth: width }}>
                  {label}
                  <SortIcon dir={sortKey === key ? sortDir : null} />
                </th>
              ))}
              <th style={{ width: "140px", minWidth: "140px" }}>İşlemler</th>
            </tr>
          </thead>
          <tbody>
            {sorted.map((k, i) => (
              <tr
                key={k.ID}
                onClick={() => onTahminAl(k.ID)}
                className={selectedId === k.ID ? "selected" : ""}
                style={{ animationDelay: `${i * 20}ms` }}
              >
                <td>
                  <span className={styles.rowNum}>{k.ID}</span>
                </td>
                <td>
                  <span className={styles.kumasAd}>{k.Kumas_Ad}</span>
                </td>
                <td>
                  <span className="badge badge-indigo">{k.Kumas_Tur}</span>
                </td>
                <td>
                  <div className={styles.renkCell}>
                    <span className={styles.renkDot} style={{ background: RENK_MAP[k.Kumas_Renk] ?? "#94a3b8" }} />
                    {k.Kumas_Renk}
                  </div>
                </td>
                <td>
                  <span className="font-mono">{k["Kumas_Likra_%"]}%</span>
                </td>
                <td>{k.Kumas_Likra_Yonu}</td>
                <td>
                  <span className="font-mono">{k.Kumas_Uzunluk_m} m</span>
                </td>
                <td>
                  <span className="font-mono">{k.Kumas_En_cm} cm</span>
                </td>
                <td>
                  <span className="font-mono">{k.Kumas_Gramaj_gm2} g/m²</span>
                </td>
                <td>
                  <span className={k.Kumas_SuItici === "Evet" ? "badge badge-green" : "badge badge-gray"}>
                    {k.Kumas_SuItici}
                  </span>
                </td>
                <td>{k.Kullanim_Donemi}</td>
                <td>
                  {k.Kullanim_Alani && k.Kullanim_Alani !== "Bekleniyor..." ? (
                    <span className="badge badge-gold">{k.Kullanim_Alani}</span>
                  ) : (
                    <span className="badge badge-gray animate-pulse">
                      {k.Kullanim_Alani ?? "—"}
                    </span>
                  )}
                </td>
                <td onClick={e => e.stopPropagation()} style={{ overflow: "visible" }}>
                  <div className={styles.actions}>
                    <button
                      className="btn btn-ghost btn-sm btn-icon"
                      title="İşlemler"
                      onClick={(e) => {
                        e.stopPropagation();
                        e.nativeEvent.stopImmediatePropagation();
                        setOpenDropdown(openDropdown === k.ID ? null : k.ID);
                      }}
                    >
                      <span className="material-symbols-outlined">more_vert</span>
                    </button>
                    {openDropdown === k.ID && (
                      <div className={styles.dropdownMenu}>
                        <button
                          className={styles.dropdownItem}
                          onClick={(e) => {
                            e.stopPropagation();
                            setOpenDropdown(null);
                            onMetrajGuncelle(k.ID);
                          }}
                        >
                          <span className="material-symbols-outlined" style={{ fontSize: 16 }}>edit</span>
                          Düzenle
                        </button>
                        <button
                          className={`${styles.dropdownItem} ${styles.danger}`}
                          onClick={(e) => {
                            e.stopPropagation();
                            setOpenDropdown(null);
                            handleDeleteClick(e, k.ID);
                          }}
                        >
                          <span className="material-symbols-outlined" style={{ fontSize: 16 }}>delete</span>
                          Sil
                        </button>
                      </div>
                    )}
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <div className={styles.footer}>
        <span>{kumaslar.length} kayıt</span>
      </div>
    </>
  );
}

// Renk → CSS renk haritası
const RENK_MAP: Record<string, string> = {
  Beyaz: "#f8fafc", Siyah: "#1e293b", Kırmızı: "#ef4444", Mavi: "#3b82f6",
  Yeşil: "#22c55e", Sarı: "#eab308", Mor: "#a855f7", Turuncu: "#f97316",
  Pembe: "#ec4899", Gri: "#94a3b8", Kahverengi: "#92400e", Lacivert: "#1e3a5f",
  Bordo: "#9f1239", Bej: "#d6c89a", Krem: "#fef3c7", Turkuaz: "#06b6d4",
};
