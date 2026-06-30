"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import styles from "./Navbar.module.css";

const NAV_ITEMS = [
  { href: "/",      label: "Stok Listesi", icon: "inventory_2" },
  { href: "/oneri", label: "Öneri & Ara", icon: "manage_search" },
];

export default function Navbar() {
  const pathname = usePathname();

  return (
    <header className={styles.header}>
      <div className={styles.inner}>
        {/* Logo */}
        <Link href="/" className={styles.logo}>
          <span className={`material-symbols-outlined ${styles.logoIcon}`}>blur_on</span>
          <span className={styles.logoText}>
            Kumaş<span className={styles.logoAccent}>AI</span>
          </span>
        </Link>

        {/* Navigation */}
        <nav className={styles.nav}>
          {NAV_ITEMS.map(({ href, label, icon }) => (
            <Link
              key={href}
              href={href}
              className={`${styles.navLink} ${pathname === href ? styles.active : ""}`}
            >
              <span className={`material-symbols-outlined ${styles.navIcon}`}>{icon}</span>
              {label}
            </Link>
          ))}
        </nav>

        {/* Status badge */}
        <div className={styles.status}>
          <span className={styles.statusDot} />
          <span className={styles.statusText}>Sistem Aktif</span>
        </div>
      </div>
    </header>
  );
}
