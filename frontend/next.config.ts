import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  // API isteklerini backend'e yönlendir (CORS bypass)
  async rewrites() {
    return [
      {
        source: "/api/:path*",
        destination: "http://localhost:8000/api/:path*",
      },
      {
        source: "/health",
        destination: "http://localhost:8000/health",
      },
    ];
  },
};

export default nextConfig;
