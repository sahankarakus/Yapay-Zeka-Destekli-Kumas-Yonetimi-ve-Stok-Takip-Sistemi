"use client";

import { useRouter } from "next/navigation";
import { useEffect } from "react";

/**
 * /ekle rotası — ana sayfaya yönlendirir ve ekleme modalını açar.
 * Kumaş ekleme işlemi ana sayfa üzerindeki modal ile yapılır.
 */
export default function EklePage() {
  const router = useRouter();
  useEffect(() => {
    router.replace("/?ekle=1");
  }, [router]);
  return null;
}
