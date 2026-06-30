"use client";

import { useState, useEffect, useCallback } from "react";
import Navbar from "@/components/Navbar";
import { api, type Kumas, type Secenekler } from "@/lib/api";
import styles from "./page.module.css";

export default function OneriPage() {
  const [secenekler, setSecenekler] = useState<Secenekler | null>(null);
  const [kumaslar, setKumaslar] = useState<Kumas[]>([]);
  const [loading, setLoading] = useState(false);
  const [searched, setSearched] = useState(false);

  // Filtreler
  const [tur, setTur] = useState("");
  const [likraYonu, setLikraYonu] = useState("");
  const [suItici, setSuItici] = useState("");
  const [kullanim_donemi, setKullanimDonemi] = useState("");
  const [kullanimAlani, setKullanimAlani] = useState("");
  const [minLikra, setMinLikra] = useState("");
  const [maxLikra, setMaxLikra] = useState("");
  const [minGramaj, setMinGramaj] = useState("");
  const [maxGramaj, setMaxGramaj] = useState("");

  useEffect(() => {
    api.getSecenekler().then(setSecenekler).catch(() => {});
  }, []);

  async function handleSearch() {
    setLoading(true);
    setSearched(true);
    try {
      const data = await api.listKumaslar({
        tur: tur || undefined,
        likra_yonu: likraYonu || undefined,
        su_itici: suItici || undefined,
        kullanim_donemi: kullanim_donemi || undefined,
        kullanim_alani: kullanimAlani || undefined,
        min_likra: minLikra ? parseFloat(minLikra) : undefined,
        max_likra: maxLikra ? parseFloat(maxLikra) : undefined,
        min_gramaj: minGramaj ? parseFloat(minGramaj) : undefined,
        max_gramaj: maxGramaj ? parseFloat(maxGramaj) : undefined,
      });
      setKumaslar(data);
    } catch {
      setKumaslar([]);
    } finally {
      setLoading(false);
    }
  }

  function handleReset() {
    setTur(""); setLikraYonu(""); setSuItici(""); setKullanimDonemi(""); setKullanimAlani("");
    setMinLikra(""); setMaxLikra(""); setMinGramaj(""); setMaxGramaj("");
    setKumaslar([]); setSearched(false);
  }

  return (
    <div className={styles.root}>
      <Navbar />

      <main className={styles.main}>
        <div className={styles.pageHeader}>
          <div>
            <h1 style={{ display: "flex", alignItems: "center", gap: 10 }}>
              <span className="material-symbols-outlined" style={{ fontSize: 32, color: "var(--on-surface)" }}>manage_search</span>
              Kumaş Öneri & Arama
            </h1>
            <p>Özelliklerine göre en uygun kumaşları filtreleyin</p>
          </div>
        </div>

        <div className={styles.layout}>
          {/* Filtre paneli */}
          <aside className={styles.filterPanel}>
            <div className="card-flat" style={{ padding: 24 }}>
              <h3 style={{ marginBottom: 20, display: "flex", alignItems: "center", gap: 8, color: "var(--on-surface)" }}>
                <span className="material-symbols-outlined" style={{ color: "var(--secondary)", fontSize: 20 }}>tune</span>
                Filtreler
              </h3>

              <div className={styles.filterGrid}>
                <div className="field">
                  <label className="label">Kumaş Türü</label>
                  <select className="select" value={tur} onChange={e => setTur(e.target.value)}>
                    <option value="">Tümü</option>
                    {(secenekler?.tur ?? []).map(t => <option key={t}>{t}</option>)}
                  </select>
                </div>

                <div className="field">
                  <label className="label">Likra Yönü</label>
                  <select className="select" value={likraYonu} onChange={e => setLikraYonu(e.target.value)}>
                    <option value="">Tümü</option>
                    {(secenekler?.likra_yonu ?? []).map(l => <option key={l}>{l}</option>)}
                  </select>
                </div>

                <div className="field">
                  <label className="label">Su İtici</label>
                  <select className="select" value={suItici} onChange={e => setSuItici(e.target.value)}>
                    <option value="">Tümü</option>
                    {(secenekler?.su_itici ?? []).map(s => <option key={s}>{s}</option>)}
                  </select>
                </div>

                <div className="field">
                  <label className="label">Kullanım Dönemi</label>
                  <select className="select" value={kullanim_donemi} onChange={e => setKullanimDonemi(e.target.value)}>
                    <option value="">Tümü</option>
                    {(secenekler?.kullanim_donemi ?? []).map(d => <option key={d}>{d}</option>)}
                  </select>
                </div>

                <div className="field">
                  <label className="label">Kullanım Alanı</label>
                  <select className="select" value={kullanimAlani} onChange={e => setKullanimAlani(e.target.value)}>
                    <option value="">Tümü</option>
                    {(secenekler?.kullanim_alani ?? []).map(a => <option key={a}>{a}</option>)}
                  </select>
                </div>

                <div className="divider" style={{ gridColumn: "1/-1" }} />

                <div className={styles.rangeGroup}>
                  <label className="label">Likra % Aralığı</label>
                  <div className={styles.rangeRow}>
                    <input className="input" type="number" placeholder="Min" value={minLikra} onChange={e => setMinLikra(e.target.value)} min={0} max={100} />
                    <span>—</span>
                    <input className="input" type="number" placeholder="Max" value={maxLikra} onChange={e => setMaxLikra(e.target.value)} min={0} max={100} />
                  </div>
                </div>

                <div className={styles.rangeGroup}>
                  <label className="label">Gramaj (g/m²) Aralığı</label>
                  <div className={styles.rangeRow}>
                    <input className="input" type="number" placeholder="Min" value={minGramaj} onChange={e => setMinGramaj(e.target.value)} min={0} />
                    <span>—</span>
                    <input className="input" type="number" placeholder="Max" value={maxGramaj} onChange={e => setMaxGramaj(e.target.value)} min={0} />
                  </div>
                </div>
              </div>

              <div className={styles.filterActions}>
                <button className="btn btn-ghost" onClick={handleReset}>Temizle</button>
                <button className="btn btn-primary" onClick={handleSearch} disabled={loading}>
                  {loading ? (<><span className="spinner" style={{ width: 16, height: 16 }} />Aranıyor...</>) : (
                    <><span className="material-symbols-outlined" style={{ fontSize: 18 }}>search</span>Ara</>
                  )}
                </button>
              </div>
            </div>
          </aside>

          {/* Sonuçlar */}
          <div className={styles.results}>
            {!searched && (
              <div className={styles.startPrompt}>
                <div className={styles.startIcon}>
                  <span className="material-symbols-outlined" style={{ fontSize: 48, color: "var(--outline-variant)" }}>manage_search</span>
                </div>
                <h3>Filtrelerinizi seçin ve arama yapın</h3>
                <p>Özelliklerine göre stoktan en uygun kumaşları bulun.</p>
              </div>
            )}

            {searched && loading && (
              <div className={styles.startPrompt}>
                <div className="spinner" style={{ width: 32, height: 32, borderWidth: 3 }} />
                <p>Kumaşlar aranıyor...</p>
              </div>
            )}

            {searched && !loading && kumaslar.length === 0 && (
              <div className={styles.startPrompt}>
                <div className={styles.startIcon}>
                  <span className="material-symbols-outlined" style={{ fontSize: 48, color: "var(--outline-variant)" }}>search_off</span>
                </div>
                <h3>Eşleşen kumaş bulunamadı</h3>
                <p>Farklı filtreler deneyin.</p>
              </div>
            )}

            {searched && !loading && kumaslar.length > 0 && (
              <>
                <div className={styles.resultsHeader}>
                  <h3>{kumaslar.length} kumaş bulundu</h3>
                </div>
                <div className={styles.resultGrid}>
                  {kumaslar.map(k => (
                    <div key={k.ID} className={`card ${styles.resultCard}`}>
                      <div className={styles.cardTop}>
                        <span className={styles.cardName}>{k.Kumas_Ad}</span>
                        <span className="badge badge-primary">{k.Kumas_Tur}</span>
                      </div>
                      <div className={styles.cardBody}>
                        <div className={styles.cardRow}>
                          <span className="text-muted">Renk</span>
                          <span>{k.Kumas_Renk}</span>
                        </div>
                        <div className={styles.cardRow}>
                          <span className="text-muted">Stok</span>
                          <span className="font-mono">{k.Kumas_Uzunluk_m} m</span>
                        </div>
                        <div className={styles.cardRow}>
                          <span className="text-muted">En</span>
                          <span className="font-mono">{k.Kumas_En_cm} cm</span>
                        </div>
                        <div className={styles.cardRow}>
                          <span className="text-muted">Gramaj</span>
                          <span className="font-mono">{k.Kumas_Gramaj_gm2} g/m²</span>
                        </div>
                        <div className={styles.cardRow}>
                          <span className="text-muted">Likra</span>
                          <span className="font-mono">{k["Kumas_Likra_%"]}% — {k.Kumas_Likra_Yonu}</span>
                        </div>
                        <div className={styles.cardRow}>
                          <span className="text-muted">Su İtici</span>
                          <span className={k.Kumas_SuItici === "Evet" ? "badge badge-success" : "badge badge-neutral"}>
                            {k.Kumas_SuItici}
                          </span>
                        </div>
                        <div className={styles.cardRow}>
                          <span className="text-muted">Dönem</span>
                          <span>{k.Kullanim_Donemi}</span>
                        </div>
                      </div>
                      {k.Kullanim_Alani && k.Kullanim_Alani !== "Bekleniyor..." && (
                        <div className={styles.cardFooter}>
                          <span style={{ display: "flex", alignItems: "center", gap: 4 }}>
                            <span className="material-symbols-outlined" style={{ fontSize: 14, color: "var(--primary)" }}>auto_awesome</span>
                            Önerilen Kullanım:
                          </span>
                          <span className="badge badge-primary">{k.Kullanim_Alani}</span>
                        </div>
                      )}
                    </div>
                  ))}
                </div>
              </>
            )}
          </div>
        </div>
      </main>
    </div>
  );
}
