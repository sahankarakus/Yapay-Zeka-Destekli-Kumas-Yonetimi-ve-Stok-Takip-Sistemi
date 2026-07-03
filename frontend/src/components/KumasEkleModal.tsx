"use client";

import { useState, useEffect } from "react";
import { api, type KumasCreate, type Secenekler } from "@/lib/api";
import styles from "./KumasEkleModal.module.css";

interface Props {
  onClose: () => void;
  onSuccess: () => void;
}

const EMPTY: any = {
  Kumas_Ad: "",
  Kumas_Tur: "",
  Kumas_Renk: "",
  "Kumas_Likra_%": "",
  Kumas_Likra_Yonu: "",
  Kumas_Uzunluk_m: "",
  Kumas_En_cm: "",
  Kumas_Gramaj_gm2: "",
  Kumas_SuItici: "",
  Kullanim_Donemi: "",
};

export default function KumasEkleModal({ onClose, onSuccess }: Props) {
  const [form, setForm] = useState<KumasCreate>(EMPTY);
  const [secenekler, setSecenekler] = useState<Secenekler | null>(null);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    api.getSecenekler().then(setSecenekler).catch(() => {});
  }, []);

  function set(field: keyof KumasCreate, value: string | number) {
    setForm(f => ({ ...f, [field]: value }));
    setError(null);
  }

  function validate(): string | null {
    if (!form.Kumas_Ad.trim()) return "Kumaş adı zorunludur.";
    if (!form.Kumas_Tur) return "Kumaş türü seçin.";
    if (!form.Kumas_Renk) return "Renk seçin.";
    if (!form.Kumas_Likra_Yonu) return "Likra yönü seçin.";
    
    const likra = Number(form["Kumas_Likra_%"]);
    if (isNaN(likra) || likra < 0 || likra > 100 || form["Kumas_Likra_%"] === "") return "Likra % 0–100 arası olmalı.";
    
    const uzunluk = Number(form.Kumas_Uzunluk_m);
    if (isNaN(uzunluk) || uzunluk <= 0) return "Uzunluk 0'dan büyük olmalı.";
    
    const en = Number(form.Kumas_En_cm);
    if (isNaN(en) || en <= 0) return "En 0'dan büyük olmalı.";
    
    const gramaj = Number(form.Kumas_Gramaj_gm2);
    if (isNaN(gramaj) || gramaj <= 0) return "Gramaj 0'dan büyük olmalı.";
    
    if (!form.Kumas_SuItici) return "Su iticilik seçin.";
    if (!form.Kullanim_Donemi) return "Kullanım dönemi seçin.";
    return null;
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    const err = validate();
    if (err) { setError(err); return; }

    const payload: KumasCreate = {
      ...form,
      "Kumas_Likra_%": Number(form["Kumas_Likra_%"]),
      Kumas_Uzunluk_m: Number(form.Kumas_Uzunluk_m),
      Kumas_En_cm: Number(form.Kumas_En_cm),
      Kumas_Gramaj_gm2: Number(form.Kumas_Gramaj_gm2),
    };

    setSaving(true);
    setError(null);
    try {
      await api.createKumas(payload);
      onSuccess();
      onClose();
    } catch (ex: unknown) {
      setError(ex instanceof Error ? ex.message : "Kayıt sırasında hata oluştu.");
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="overlay">
      <div className="modal">
        <div className="modal-header">
          <h2>✚ Yeni Kumaş Ekle</h2>
          <button className="btn btn-ghost btn-icon" onClick={onClose}>✕</button>
        </div>

        <form onSubmit={handleSubmit}>
          <div className={styles.grid}>
            {/* Kumaş Adı */}
            <div className={`field ${styles.full}`}>
              <label className="label">Kumaş Adı *</label>
              <input
                className="input"
                placeholder="Örn: Beyaz Viskon Dokuma"
                value={form.Kumas_Ad}
                onChange={e => set("Kumas_Ad", e.target.value)}
                autoFocus
              />
            </div>

            {/* Tür */}
            <div className="field">
              <label className="label">Tür *</label>
              <select className="select" value={form.Kumas_Tur} onChange={e => set("Kumas_Tur", e.target.value)}>
                <option value="">Seçin...</option>
                {(secenekler?.tur ?? []).map(t => <option key={t}>{t}</option>)}
              </select>
            </div>

            {/* Renk */}
            <div className="field">
              <label className="label">Renk *</label>
              <select className="select" value={form.Kumas_Renk} onChange={e => set("Kumas_Renk", e.target.value)}>
                <option value="">Seçin...</option>
                {(secenekler?.renk ?? []).map(r => <option key={r}>{r}</option>)}
              </select>
            </div>

            {/* Likra % */}
            <div className="field">
              <label className="label">Likra % *</label>
              <input
                className="input"
                type="number"
                min={0}
                max={100}
                step={0.1}
                value={form["Kumas_Likra_%"]}
                onChange={e => set("Kumas_Likra_%", e.target.value)}
              />
            </div>

            {/* Likra Yönü */}
            <div className="field">
              <label className="label">Likra Yönü *</label>
              <select className="select" value={form.Kumas_Likra_Yonu} onChange={e => set("Kumas_Likra_Yonu", e.target.value)}>
                <option value="">Seçin...</option>
                {(secenekler?.likra_yonu ?? []).map(l => <option key={l}>{l}</option>)}
              </select>
            </div>

            {/* Uzunluk */}
            <div className="field">
              <label className="label">Uzunluk (m) *</label>
              <input
                className="input"
                type="number"
                min={0}
                step={0.1}
                value={form.Kumas_Uzunluk_m}
                onChange={e => set("Kumas_Uzunluk_m", e.target.value)}
              />
            </div>

            {/* En */}
            <div className="field">
              <label className="label">En (cm) *</label>
              <input
                className="input"
                type="number"
                min={0}
                step={0.5}
                value={form.Kumas_En_cm}
                onChange={e => set("Kumas_En_cm", e.target.value)}
              />
            </div>

            {/* Gramaj */}
            <div className="field">
              <label className="label">Gramaj (g/m²) *</label>
              <input
                className="input"
                type="number"
                min={0}
                step={1}
                value={form.Kumas_Gramaj_gm2}
                onChange={e => set("Kumas_Gramaj_gm2", e.target.value)}
              />
            </div>

            {/* Su İtici */}
            <div className="field">
              <label className="label">Su İtici *</label>
              <select className="select" value={form.Kumas_SuItici} onChange={e => set("Kumas_SuItici", e.target.value)}>
                <option value="">Seçin...</option>
                {(secenekler?.su_itici ?? []).map(s => <option key={s}>{s}</option>)}
              </select>
            </div>

            {/* Kullanım Dönemi */}
            <div className="field">
              <label className="label">Kullanım Dönemi *</label>
              <select className="select" value={form.Kullanim_Donemi} onChange={e => set("Kullanim_Donemi", e.target.value)}>
                <option value="">Seçin...</option>
                {(secenekler?.kullanim_donemi ?? []).map(d => <option key={d}>{d}</option>)}
              </select>
            </div>
          </div>

          {/* ML Bilgi notu */}
          <div className={styles.mlNote}>
            🤖 Kumaş kaydedildikten sonra ML modeli kullanım alanını otomatik tahmin edecek.
          </div>

          {/* Hata */}
          {error && (
            <div className={styles.errorBox}>
              ⚠️ {error}
            </div>
          )}

          {/* Aksiyonlar */}
          <div className={styles.footer}>
            <button type="button" className="btn btn-ghost" onClick={onClose} disabled={saving}>
              İptal
            </button>
            <button type="submit" className="btn btn-primary" disabled={saving}>
              {saving ? <><span className="spinner" style={{ width: 16, height: 16 }} />Kaydediliyor...</> : "✚ Kumaş Ekle"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
